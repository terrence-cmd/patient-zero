using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Second demonstration library: Mortal Kombat–style side-scroller vocabulary.
/// Same ScriptableObject types as the SF-ish demo library — separate assets only.
///
/// Classic four-button normals (HP / LP / HK / LK), plus uppercut, sweep, and
/// two placeholder specials. Stages are wide on X for side-view spacing.
///
/// Menu: Patient Zero / Combat / Create Side-Scroller (MK-Style) Libraries
/// Batch: Unity -executeMethod SideScrollerCombatLibrarySetup.CreateLibraries
///
/// Out of scope: fatalities, juggling systems, stage hazards runtime, remapping
/// FighterCombat buttons onto these moveIds (data library only for now).
/// </summary>
public static class SideScrollerCombatLibrarySetup
{
    private const string Root = "Assets/Data/Combat/SideScroller";
    private const string MovesFolder = Root + "/Moves";
    private const string StagesFolder = Root + "/Stages";
    private const string MoveLibraryPath = Root + "/MoveLibrary_SideScroller.asset";
    private const string StageLibraryPath = Root + "/StageLibrary_SideScroller.asset";

    [MenuItem("Patient Zero/Combat/Create Side-Scroller (MK-Style) Libraries")]
    public static void CreateLibraries()
    {
        try
        {
            EnsureFolders();

            // --- Four-button normals (side-view, feet-origin hitboxes) ---
            MoveDefinition highPunch = UpsertMove(
                MovesFolder + "/Move_HighPunch.asset",
                "high_punch", "High Punch",
                MoveCategory.Normal, MoveHeight.High,
                new FrameTiming(4, 3, 10),
                new HitboxSpec(new Vector2(0.85f, 1.45f), new Vector2(0.65f, 0.4f)),
                damage: 8, hitstun: 14, blockstun: 10,
                cancelInto: new[] { "high_kick", "uppercut" });

            MoveDefinition lowPunch = UpsertMove(
                MovesFolder + "/Move_LowPunch.asset",
                "low_punch", "Low Punch",
                MoveCategory.Normal, MoveHeight.Low,
                new FrameTiming(3, 2, 8),
                new HitboxSpec(new Vector2(0.75f, 0.55f), new Vector2(0.6f, 0.35f)),
                damage: 6, hitstun: 12, blockstun: 8,
                cancelInto: new[] { "low_kick", "sweep" });

            MoveDefinition highKick = UpsertMove(
                MovesFolder + "/Move_HighKick.asset",
                "high_kick", "High Kick",
                MoveCategory.Normal, MoveHeight.High,
                new FrameTiming(7, 3, 14),
                new HitboxSpec(new Vector2(1.05f, 1.35f), new Vector2(0.85f, 0.45f)),
                damage: 11, hitstun: 16, blockstun: 12,
                cancelInto: System.Array.Empty<string>());

            MoveDefinition lowKick = UpsertMove(
                MovesFolder + "/Move_LowKick.asset",
                "low_kick", "Low Kick",
                MoveCategory.Normal, MoveHeight.Low,
                new FrameTiming(5, 3, 12),
                new HitboxSpec(new Vector2(0.95f, 0.4f), new Vector2(0.8f, 0.35f)),
                damage: 9, hitstun: 14, blockstun: 10,
                cancelInto: new[] { "sweep" });

            MoveDefinition uppercut = UpsertMove(
                MovesFolder + "/Move_Uppercut.asset",
                "uppercut", "Uppercut",
                MoveCategory.Normal, MoveHeight.High,
                new FrameTiming(6, 3, 22),
                new HitboxSpec(new Vector2(0.55f, 1.7f), new Vector2(0.55f, 0.9f)),
                damage: 16, hitstun: 24, blockstun: 14,
                cancelInto: System.Array.Empty<string>());

            MoveDefinition sweep = UpsertMove(
                MovesFolder + "/Move_Sweep.asset",
                "sweep", "Sweep",
                MoveCategory.Normal, MoveHeight.Low,
                new FrameTiming(9, 4, 18),
                new HitboxSpec(new Vector2(1.1f, 0.25f), new Vector2(1.1f, 0.3f)),
                damage: 12, hitstun: 20, blockstun: 16,
                cancelInto: System.Array.Empty<string>());

            // Placeholder specials — data only (no projectile / teleport runtime yet).
            MoveDefinition spearLine = UpsertMove(
                MovesFolder + "/Move_SpearLine.asset",
                "spear_line", "Spear Line (demo special)",
                MoveCategory.Special, MoveHeight.Mid,
                new FrameTiming(14, 36, 20),
                new HitboxSpec(new Vector2(2.2f, 1.15f), new Vector2(1.6f, 0.35f)),
                damage: 14, hitstun: 22, blockstun: 14,
                cancelInto: System.Array.Empty<string>());

            MoveDefinition teleportStrike = UpsertMove(
                MovesFolder + "/Move_TeleportStrike.asset",
                "teleport_strike", "Teleport Strike (demo special)",
                MoveCategory.Special, MoveHeight.Mid,
                new FrameTiming(18, 4, 16),
                new HitboxSpec(new Vector2(-0.9f, 1.1f), new Vector2(0.7f, 0.7f)),
                damage: 13, hitstun: 18, blockstun: 12,
                cancelInto: System.Array.Empty<string>());

            // Wide X bounds — side-scroller fight plane, P1 left / P2 right.
            StageDefinition courtyard = UpsertStage(
                StagesFolder + "/Stage_StoneCourtyard.asset",
                "stone_courtyard", "Stone Courtyard",
                groundY: 0f,
                boundsMin: new Vector2(-12f, 0f),
                boundsMax: new Vector2(12f, 4f),
                spawnP1: new Vector2(-4.5f, 0f),
                spawnP2: new Vector2(4.5f, 0f));

            StageDefinition bridge = UpsertStage(
                StagesFolder + "/Stage_TheBridge.asset",
                "the_bridge", "The Bridge",
                groundY: 0f,
                boundsMin: new Vector2(-14f, 0f),
                boundsMax: new Vector2(14f, 3.5f),
                spawnP1: new Vector2(-5f, 0f),
                spawnP2: new Vector2(5f, 0f));

            StageDefinition throne = UpsertStage(
                StagesFolder + "/Stage_ThroneRoom.asset",
                "throne_room", "Throne Room",
                groundY: 0f,
                boundsMin: new Vector2(-11f, 0f),
                boundsMax: new Vector2(11f, 4.5f),
                spawnP1: new Vector2(-4f, 0f),
                spawnP2: new Vector2(4f, 0f));

            StageDefinition pit = UpsertStage(
                StagesFolder + "/Stage_SpikePit.asset",
                "spike_pit", "Spike Pit",
                groundY: 0f,
                boundsMin: new Vector2(-10f, 0f),
                boundsMax: new Vector2(10f, 3.75f),
                spawnP1: new Vector2(-3.5f, 0f),
                spawnP2: new Vector2(3.5f, 0f));

            MoveLibrary moveLibrary = UpsertAsset<MoveLibrary>(MoveLibraryPath);
            moveLibrary.libraryId = "mk_side_scroller";
            moveLibrary.moves = new[]
            {
                highPunch, lowPunch, highKick, lowKick,
                uppercut, sweep, spearLine, teleportStrike
            };
            EditorUtility.SetDirty(moveLibrary);

            StageLibrary stageLibrary = UpsertAsset<StageLibrary>(StageLibraryPath);
            stageLibrary.libraryId = "mk_side_scroller_stages";
            stageLibrary.stages = new[] { courtyard, bridge, throne, pit };
            EditorUtility.SetDirty(stageLibrary);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"[SideScrollerCombatLibrarySetup] MK-style side-scroller libraries ready.\n" +
                $"  Moves:  {MoveLibraryPath} ({moveLibrary.Count})\n" +
                $"  Stages: {StageLibraryPath} ({stageLibrary.Count})");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SideScrollerCombatLibrarySetup] FAILED: {ex}");
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
