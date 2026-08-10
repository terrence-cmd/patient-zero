using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates the demonstration move/stage library assets under Assets/Data/Combat.
/// Idempotent: re-running refreshes sample field values and library membership.
///
/// Menu: Patient Zero / Combat / Create Sample Libraries
/// Batch: Unity -executeMethod CombatLibrarySetup.CreateSampleLibraries
///
/// Out of scope here: hit detection, hitstun runtime, animations, scene loading,
/// wiring into PlayerMover, netcode.
/// </summary>
public static class CombatLibrarySetup
{
    private const string Root = "Assets/Data/Combat";
    private const string MovesFolder = Root + "/Moves";
    private const string StagesFolder = Root + "/Stages";
    private const string MoveLibraryPath = Root + "/MoveLibrary_Demo.asset";
    private const string StageLibraryPath = Root + "/StageLibrary_Demo.asset";

    [MenuItem("Patient Zero/Combat/Create Sample Libraries")]
    public static void CreateSampleLibraries()
    {
        try
        {
            EnsureFolders();

            MoveDefinition lightPunch = UpsertMove(
                MovesFolder + "/Move_LightPunch.asset",
                "light_punch", "Light Punch",
                MoveCategory.Normal, MoveHeight.High,
                new FrameTiming(3, 2, 7),
                new HitboxSpec(new Vector2(0.7f, 1.2f), new Vector2(0.6f, 0.4f)),
                damage: 5, hitstun: 12, blockstun: 8,
                cancelInto: new[] { "heavy_punch", "light_kick" });

            MoveDefinition heavyPunch = UpsertMove(
                MovesFolder + "/Move_HeavyPunch.asset",
                "heavy_punch", "Heavy Punch",
                MoveCategory.Normal, MoveHeight.Mid,
                new FrameTiming(8, 3, 16),
                new HitboxSpec(new Vector2(0.85f, 1.15f), new Vector2(0.75f, 0.5f)),
                damage: 12, hitstun: 18, blockstun: 14,
                cancelInto: System.Array.Empty<string>());

            MoveDefinition lightKick = UpsertMove(
                MovesFolder + "/Move_LightKick.asset",
                "light_kick", "Light Kick",
                MoveCategory.Normal, MoveHeight.Mid,
                new FrameTiming(5, 3, 10),
                new HitboxSpec(new Vector2(0.8f, 0.7f), new Vector2(0.7f, 0.45f)),
                damage: 7, hitstun: 14, blockstun: 10,
                cancelInto: new[] { "heavy_kick" });

            MoveDefinition heavyKick = UpsertMove(
                MovesFolder + "/Move_HeavyKick.asset",
                "heavy_kick", "Heavy Kick",
                MoveCategory.Normal, MoveHeight.Mid,
                new FrameTiming(10, 4, 18),
                new HitboxSpec(new Vector2(1.0f, 0.75f), new Vector2(0.9f, 0.5f)),
                damage: 14, hitstun: 20, blockstun: 16,
                cancelInto: System.Array.Empty<string>());

            MoveDefinition crouchJab = UpsertMove(
                MovesFolder + "/Move_CrouchJab.asset",
                "crouch_jab", "Crouch Jab",
                MoveCategory.Normal, MoveHeight.Low,
                new FrameTiming(4, 2, 8),
                new HitboxSpec(new Vector2(0.65f, 0.45f), new Vector2(0.55f, 0.35f)),
                damage: 4, hitstun: 11, blockstun: 7,
                cancelInto: new[] { "light_punch" });

            MoveDefinition hadoken = UpsertMove(
                MovesFolder + "/Move_Hadoken.asset",
                "hadoken", "Hadoken (demo special)",
                MoveCategory.Special, MoveHeight.Mid,
                new FrameTiming(12, 40, 18),
                new HitboxSpec(new Vector2(1.5f, 1.0f), new Vector2(0.8f, 0.8f)),
                damage: 10, hitstun: 16, blockstun: 12,
                cancelInto: System.Array.Empty<string>());

            StageDefinition training = UpsertStage(
                StagesFolder + "/Stage_TrainingRoom.asset",
                "training_room", "Training Room",
                groundY: 0f,
                boundsMin: new Vector2(-8f, 0f),
                boundsMax: new Vector2(8f, 4f),
                spawnP1: new Vector2(-3f, 0f),
                spawnP2: new Vector2(3f, 0f));

            StageDefinition rooftop = UpsertStage(
                StagesFolder + "/Stage_Rooftop.asset",
                "rooftop", "Rooftop",
                groundY: 0f,
                boundsMin: new Vector2(-6f, 0f),
                boundsMax: new Vector2(6f, 3.5f),
                spawnP1: new Vector2(-2.5f, 0f),
                spawnP2: new Vector2(2.5f, 0f));

            StageDefinition warehouse = UpsertStage(
                StagesFolder + "/Stage_Warehouse.asset",
                "warehouse", "Warehouse",
                groundY: 0f,
                boundsMin: new Vector2(-10f, 0f),
                boundsMax: new Vector2(10f, 4.5f),
                spawnP1: new Vector2(-4f, 0f),
                spawnP2: new Vector2(4f, 0f));

            MoveLibrary moveLibrary = UpsertAsset<MoveLibrary>(MoveLibraryPath);
            moveLibrary.libraryId = "demo_basics";
            moveLibrary.moves = new[]
            {
                lightPunch, heavyPunch, lightKick, heavyKick, crouchJab, hadoken
            };
            EditorUtility.SetDirty(moveLibrary);

            StageLibrary stageLibrary = UpsertAsset<StageLibrary>(StageLibraryPath);
            stageLibrary.libraryId = "demo_stages";
            stageLibrary.stages = new[] { training, rooftop, warehouse };
            EditorUtility.SetDirty(stageLibrary);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"[CombatLibrarySetup] Sample libraries ready.\n" +
                $"  Moves:  {MoveLibraryPath} ({moveLibrary.Count})\n" +
                $"  Stages: {StageLibraryPath} ({stageLibrary.Count})");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[CombatLibrarySetup] FAILED: {ex}");
            if (Application.isBatchMode)
                EditorApplication.Exit(1);
            throw;
        }

        if (Application.isBatchMode)
            EditorApplication.Exit(0);
    }

    /// <summary>
    /// Patches the existing Player prefab with FighterCombat + demo move library
    /// without recreating Main (safer than re-running ProjectSetup.ApplyAll).
    /// </summary>
    [MenuItem("Patient Zero/Combat/Wire Executable Combat Onto Player Prefab")]
    public static void WireExecutableCombatOntoPlayerPrefab()
    {
        try
        {
            const string playerPrefabPath = "Assets/Prefabs/Player.prefab";
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(playerPrefabPath);
            if (prefabRoot == null)
                throw new System.InvalidOperationException($"Missing prefab at {playerPrefabPath}");

            FighterCombat combat = prefabRoot.GetComponent<FighterCombat>();
            if (combat == null)
                combat = prefabRoot.AddComponent<FighterCombat>();

            MoveLibrary library = AssetDatabase.LoadAssetAtPath<MoveLibrary>(MoveLibraryPath);
            if (library == null)
                throw new System.InvalidOperationException(
                    $"Missing {MoveLibraryPath}. Run Create Sample Libraries first.");

            combat.SetMoveLibrary(library);
            EditorUtility.SetDirty(combat);
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, playerPrefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            AssetDatabase.SaveAssets();

            Debug.Log($"[CombatLibrarySetup] Wired FighterCombat + {MoveLibraryPath} onto {playerPrefabPath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[CombatLibrarySetup] Wire FAILED: {ex}");
            if (Application.isBatchMode)
                EditorApplication.Exit(1);
            throw;
        }

        if (Application.isBatchMode)
            EditorApplication.Exit(0);
    }

    private static void EnsureFolders()
    {
        Directory.CreateDirectory(MovesFolder);
        Directory.CreateDirectory(StagesFolder);
        AssetDatabase.Refresh();
    }

    private static MoveDefinition UpsertMove(
        string path,
        string moveId,
        string displayName,
        MoveCategory category,
        MoveHeight height,
        FrameTiming frames,
        HitboxSpec hitbox,
        int damage,
        int hitstun,
        int blockstun,
        string[] cancelInto)
    {
        MoveDefinition move = UpsertAsset<MoveDefinition>(path);
        move.moveId = moveId;
        move.displayName = displayName;
        move.category = category;
        move.height = height;
        move.frames = frames;
        move.hitbox = hitbox;
        move.damage = damage;
        move.hitstun = hitstun;
        move.blockstun = blockstun;
        move.cancelIntoMoveIds = cancelInto;
        EditorUtility.SetDirty(move);
        return move;
    }

    private static StageDefinition UpsertStage(
        string path,
        string stageId,
        string displayName,
        float groundY,
        Vector2 boundsMin,
        Vector2 boundsMax,
        Vector2 spawnP1,
        Vector2 spawnP2)
    {
        StageDefinition stage = UpsertAsset<StageDefinition>(path);
        stage.stageId = stageId;
        stage.displayName = displayName;
        stage.groundY = groundY;
        stage.boundsMin = boundsMin;
        stage.boundsMax = boundsMax;
        stage.spawnP1 = spawnP1;
        stage.spawnP2 = spawnP2;
        EditorUtility.SetDirty(stage);
        return stage;
    }

    private static T UpsertAsset<T>(string path) where T : ScriptableObject
    {
        T existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null)
            return existing;

        T created = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(created, path);
        return created;
    }
}
