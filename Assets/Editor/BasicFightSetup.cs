using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Builds character + fight libraries on top of existing move/stage data, and
/// wires FightDirector onto Main so a basic local fight can render.
///
/// Prefers the MK side-scroller move/stage libraries when present; falls back
/// to the original demo libraries.
///
/// Menu: Patient Zero / Combat / Create Basic Fight Package
/// Batch: Unity -executeMethod BasicFightSetup.CreateAndWire
/// </summary>
public static class BasicFightSetup
{
    private const string Root = "Assets/Data/Combat/Fights";
    private const string CharactersFolder = Root + "/Characters";
    private const string CharacterLibraryPath = Root + "/CharacterLibrary_Basic.asset";
    private const string FightLibraryPath = Root + "/FightLibrary_Basic.asset";
    private const string FightPath = Root + "/Fight_BasicSideScroller.asset";
    private const string MainScenePath = "Assets/Scenes/Main.unity";
    private const string PlayerPrefabPath = "Assets/Prefabs/Player.prefab";

    [MenuItem("Patient Zero/Combat/Create Basic Fight Package")]
    public static void CreateAndWire()
    {
        try
        {
            Directory.CreateDirectory(CharactersFolder);
            AssetDatabase.Refresh();

            MoveLibrary moves = LoadFirstMoveLibrary(
                "Assets/Data/Combat/SideScroller/MoveLibrary_SideScroller.asset",
                "Assets/Data/Combat/MoveLibrary_Demo.asset");
            StageDefinition stage = LoadFirstStage(
                "Assets/Data/Combat/SideScroller/Stages/Stage_StoneCourtyard.asset",
                "Assets/Data/Combat/Stages/Stage_TrainingRoom.asset");

            if (moves == null)
                throw new System.InvalidOperationException(
                    "No MoveLibrary found. Run Create Sample Libraries or Side-Scroller setup first.");
            if (stage == null)
                throw new System.InvalidOperationException("No StageDefinition found.");

            bool mkStyle = moves.libraryId != null && moves.libraryId.Contains("side_scroller");

            CharacterDefinition warrior = UpsertCharacter(
                CharactersFolder + "/Character_Warrior.asset",
                "warrior", "Warrior",
                moves,
                new Color(0.85f, 0.2f, 0.2f),
                mkStyle);
            CharacterDefinition shadow = UpsertCharacter(
                CharactersFolder + "/Character_Shadow.asset",
                "shadow", "Shadow",
                moves,
                new Color(0.25f, 0.35f, 0.9f),
                mkStyle);

            CharacterLibrary roster = UpsertAsset<CharacterLibrary>(CharacterLibraryPath);
            roster.libraryId = "basic_roster";
            roster.characters = new[] { warrior, shadow };
            EditorUtility.SetDirty(roster);

            FightDefinition fight = UpsertAsset<FightDefinition>(FightPath);
            fight.fightId = "basic_side_scroller";
            fight.displayName = "Basic Side-Scroller Fight";
            fight.stage = stage;
            fight.player1Character = warrior;
            fight.player2Character = shadow;
            fight.startingHealth = 100;
            fight.sideViewMovement = true;
            EditorUtility.SetDirty(fight);

            FightLibrary fightLibrary = UpsertAsset<FightLibrary>(FightLibraryPath);
            fightLibrary.libraryId = "basic_fights";
            fightLibrary.fights = new[] { fight };
            EditorUtility.SetDirty(fightLibrary);

            EnsurePlayerHasHealthComponent();
            WireFightDirector(fight);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"[BasicFightSetup] Basic fight package ready.\n" +
                $"  Roster: {CharacterLibraryPath}\n" +
                $"  Fight:  {FightPath}\n" +
                $"  Wired FightDirector on Main → {fight.fightId}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[BasicFightSetup] FAILED: {ex}");
            if (Application.isBatchMode)
                EditorApplication.Exit(1);
            throw;
        }

        if (Application.isBatchMode)
            EditorApplication.Exit(0);
    }

    private static CharacterDefinition UpsertCharacter(
        string path,
        string id,
        string displayName,
        MoveLibrary moves,
        Color color,
        bool mkStyle)
    {
        CharacterDefinition c = UpsertAsset<CharacterDefinition>(path);
        c.characterId = id;
        c.displayName = displayName;
        c.moveLibrary = moves;
        c.capsuleColor = color;
        c.hurtboxSize = new Vector2(1f, 2f);

        if (mkStyle)
        {
            c.lightPunchMoveId = "high_punch";
            c.heavyPunchMoveId = "low_punch";
            c.lightKickMoveId = "high_kick";
            c.heavyKickMoveId = "low_kick";
            c.crouchJabMoveId = "sweep";
            c.specialMoveId = "spear_line";
        }
        else
        {
            c.lightPunchMoveId = "light_punch";
            c.heavyPunchMoveId = "heavy_punch";
            c.lightKickMoveId = "light_kick";
            c.heavyKickMoveId = "heavy_kick";
            c.crouchJabMoveId = "crouch_jab";
            c.specialMoveId = "hadoken";
        }

        EditorUtility.SetDirty(c);
        return c;
    }

    private static void EnsurePlayerHasHealthComponent()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
        if (root == null)
            throw new System.InvalidOperationException($"Missing {PlayerPrefabPath}");

        if (root.GetComponent<FighterHealth>() == null)
            root.AddComponent<FighterHealth>();

        PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static void WireFightDirector(FightDefinition fight)
    {
        var scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        PlayerInputManager manager = Object.FindFirstObjectByType<PlayerInputManager>();
        if (manager == null)
            throw new System.InvalidOperationException("Main scene has no PlayerInputManager.");

        FightDirector director = manager.GetComponent<FightDirector>();
        if (director == null)
            director = manager.gameObject.AddComponent<FightDirector>();

        director.SetFight(fight);
        EditorUtility.SetDirty(director);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static MoveLibrary LoadFirstMoveLibrary(params string[] paths)
    {
        for (int i = 0; i < paths.Length; i++)
        {
            MoveLibrary lib = AssetDatabase.LoadAssetAtPath<MoveLibrary>(paths[i]);
            if (lib != null)
                return lib;
        }
        return null;
    }

    private static StageDefinition LoadFirstStage(params string[] paths)
    {
        for (int i = 0; i < paths.Length; i++)
        {
            StageDefinition stage = AssetDatabase.LoadAssetAtPath<StageDefinition>(paths[i]);
            if (stage != null)
                return stage;
        }
        return null;
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
