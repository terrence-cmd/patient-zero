using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// One-shot batch-mode project setup for Gate 0. Run with:
///   Unity.exe -batchmode -quit -projectPath . -executeMethod ProjectSetup.ApplyAll
/// Idempotent: safe to re-run.
/// Applies (a) folder scaffold, (b) "console mode" player settings,
/// (c) player prefab + Main scene with 2-player PlayerInputManager wiring,
/// (d) build settings scene list.
/// </summary>
public static class ProjectSetup
{
    private const string InputActionsPath = "Assets/Scripts/Input/PlayerControls.inputactions";
    private const string PlayerPrefabPath = "Assets/Prefabs/Player.prefab";
    private const string MainScenePath = "Assets/Scenes/Main.unity";

    public static void ApplyAll()
    {
        try
        {
            CreateFolders();
            ApplyConsoleModeSettings();
            CreatePlayerPrefabAndScene();
            AssetDatabase.SaveAssets();
            Debug.Log("[ProjectSetup] ApplyAll completed OK.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ProjectSetup] FAILED: {ex}");
            EditorApplication.Exit(1);
        }
    }

    private static void CreateFolders()
    {
        string[] folders =
        {
            "Assets/Scenes",
            "Assets/Scripts/Characters",
            "Assets/Scripts/Combat",
            "Assets/Scripts/Input",
            "Assets/Scripts/UI",
            "Assets/Scripts/Netcode",
            "Assets/Prefabs",
            "Assets/Art",
            "Assets/Audio",
            "Assets/Editor",
            "Builds/WebGL",
            "Builds/Desktop",
            "Backend"
        };
        foreach (string f in folders)
        {
            Directory.CreateDirectory(f);
        }
        AssetDatabase.Refresh();
        Debug.Log("[ProjectSetup] Folder scaffold created.");
    }

    private static void ApplyConsoleModeSettings()
    {
        PlayerSettings.companyName = "PatientZero";
        PlayerSettings.productName = "PatientZero";

        // "Console mode": fullscreen borderless window, locked 1920x1080,
        // no window resize, no Alt+Enter mode switching.
        // Note: the old "Display Resolution Dialog" was removed from Unity
        // entirely (2019.3+), so there is no dialog to disable — launches go
        // straight into the game.
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.defaultIsNativeResolution = false;
        PlayerSettings.resizableWindow = false;
        PlayerSettings.allowFullscreenSwitch = false;

        // Active Input Handling -> Input System Package (new) only.
        // No public API for this; drive the serialized PlayerSettings asset
        // from script (still script-applied, not hand-edited).
        var playerSettingsAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0];
        var so = new SerializedObject(playerSettingsAsset);
        SerializedProperty handler = so.FindProperty("activeInputHandler");
        if (handler != null)
        {
            handler.intValue = 1; // 0 = old Input Manager, 1 = Input System, 2 = both
            so.ApplyModifiedProperties();
        }
        else
        {
            Debug.LogWarning("[ProjectSetup] activeInputHandler property not found; check manually.");
        }

        // WebGL: disable compression so the build runs from any static host
        // (S3/CloudFront or a local server) without Content-Encoding config.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;

        Debug.Log("[ProjectSetup] Console-mode player settings applied " +
                  "(FullScreenWindow, 1920x1080 locked, Input System only).");
    }

    private static void CreatePlayerPrefabAndScene()
    {
        var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
        if (actions == null)
        {
            throw new InvalidOperationException($"Input actions asset not found at {InputActionsPath}");
        }

        // --- Player prefab: placeholder capsule the PlayerInputManager spawns per join.
        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        temp.name = "Player";
        temp.transform.position = new Vector3(0f, 1f, 0f);
        var playerInput = temp.AddComponent<PlayerInput>();
        playerInput.actions = actions;
        playerInput.defaultActionMap = "Player";
        temp.AddComponent<PlayerMover>();
        var combat = temp.AddComponent<FighterCombat>();
        var moveLibrary = AssetDatabase.LoadAssetAtPath<MoveLibrary>(
            "Assets/Data/Combat/MoveLibrary_Demo.asset");
        if (moveLibrary != null)
            combat.SetMoveLibrary(moveLibrary);
        else
            Debug.LogWarning("[ProjectSetup] MoveLibrary_Demo.asset missing — run CombatLibrarySetup first.");
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, PlayerPrefabPath);
        UnityEngine.Object.DestroyImmediate(temp);

        // --- Main scene: camera+light, ground, PlayerInputManager (join on button press, cap 2).
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(2f, 1f, 2f);

        var managerGO = new GameObject("PlayerManager");
        var pim = managerGO.AddComponent<PlayerInputManager>();
        pim.playerPrefab = prefab;
        pim.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed;
        // C# events (not SendMessages) so TwoPlayerJoinController's
        // onPlayerJoined/onPlayerLeft subscriptions actually fire.
        pim.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
        managerGO.AddComponent<TwoPlayerJoinController>();

        EditorSceneManager.SaveScene(scene, MainScenePath);

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(MainScenePath, true) };

        Debug.Log($"[ProjectSetup] Player prefab ({PlayerPrefabPath}) and scene ({MainScenePath}) created; " +
                  "scene registered in build settings.");
    }
}
