using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Creates Boot/Title scenes, wires <see cref="GameFlowDirector"/> with the
/// basic fight libraries, and registers Boot → Title → Main in build settings.
///
/// Menu: Patient Zero / Flow / Setup Game Flow
/// Batch: Unity -executeMethod GameFlowSetup.Apply
/// Idempotent.
/// </summary>
public static class GameFlowSetup
{
    private const string BootScenePath = "Assets/Scenes/Boot.unity";
    private const string TitleScenePath = "Assets/Scenes/Title.unity";
    private const string MainScenePath = "Assets/Scenes/Main.unity";
    private const string CharacterLibraryPath = "Assets/Data/Combat/Fights/CharacterLibrary_Basic.asset";
    private const string FightPath = "Assets/Data/Combat/Fights/Fight_BasicSideScroller.asset";

    [MenuItem("Patient Zero/Flow/Setup Game Flow")]
    public static void Apply()
    {
        try
        {
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory("Assets/Scripts/Flow");
            Directory.CreateDirectory("Assets/Scripts/UI");
            AssetDatabase.Refresh();

            EnsureBootScene();
            EnsureTitleScene();
            WireLibrariesIntoBoot();
            RegisterBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("[GameFlowSetup] Boot/Title scenes + build settings ready. Enter Play from Boot.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GameFlowSetup] FAILED: {ex}");
            EditorApplication.Exit(1);
        }
    }

    private static void EnsureBootScene()
    {
        Scene scene;
        if (File.Exists(BootScenePath))
            scene = EditorSceneManager.OpenScene(BootScenePath, OpenSceneMode.Single);
        else
            scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        GameObject flowGo = GameObject.Find("GameFlow");
        if (flowGo == null)
        {
            flowGo = new GameObject("GameFlow");
            flowGo.AddComponent<GameFlowDirector>();
        }
        else if (flowGo.GetComponent<GameFlowDirector>() == null)
        {
            flowGo.AddComponent<GameFlowDirector>();
        }

        EditorSceneManager.SaveScene(scene, BootScenePath);
        Debug.Log($"[GameFlowSetup] Scene 'Boot' saved at {BootScenePath}");
    }

    private static void EnsureTitleScene()
    {
        Scene scene;
        if (File.Exists(TitleScenePath))
            scene = EditorSceneManager.OpenScene(TitleScenePath, OpenSceneMode.Single);
        else
            scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Title is presentation-only; GameFlowDirector persists from Boot via DontDestroyOnLoad.
        EditorSceneManager.SaveScene(scene, TitleScenePath);
        Debug.Log($"[GameFlowSetup] Scene 'Title' saved at {TitleScenePath}");
    }

    private static void WireLibrariesIntoBoot()
    {
        var scene = EditorSceneManager.OpenScene(BootScenePath, OpenSceneMode.Single);
        var director = Object.FindFirstObjectByType<GameFlowDirector>();
        if (director == null)
        {
            Debug.LogWarning("[GameFlowSetup] No GameFlowDirector in Boot.");
            return;
        }

        var roster = AssetDatabase.LoadAssetAtPath<CharacterLibrary>(CharacterLibraryPath);
        var fight = AssetDatabase.LoadAssetAtPath<FightDefinition>(FightPath);
        var so = new SerializedObject(director);
        so.FindProperty("characterLibrary").objectReferenceValue = roster;
        so.FindProperty("baseFight").objectReferenceValue = fight;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[GameFlowSetup] Wired roster={(roster != null)} fight={(fight != null)} into Boot.");
    }

    private static void RegisterBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(BootScenePath, true),
            new EditorBuildSettingsScene(TitleScenePath, true),
            new EditorBuildSettingsScene(MainScenePath, true)
        };
        Debug.Log("[GameFlowSetup] Build settings: Boot → Title → Main");
    }
}
