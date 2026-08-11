using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Applies a <see cref="FightDefinition"/> so a joined 2-player session can
/// render a basic fight: stage spawns/bounds, character move libraries, HP,
/// side-view movement, and a simple on-screen HP readout.
///
/// Place on the same object as PlayerInputManager (Main scene PlayerManager).
/// </summary>
[RequireComponent(typeof(PlayerInputManager))]
public class FightDirector : MonoBehaviour
{
    [SerializeField] private FightDefinition fight;
    [SerializeField] private bool drawHud = true;

    private PlayerInputManager manager;

    public FightDefinition Fight => fight;

    public void SetFight(FightDefinition definition) => fight = definition;

    private void Awake()
    {
        manager = GetComponent<PlayerInputManager>();
        ApplyStagePresentation();
    }

    private void OnEnable()
    {
        if (manager != null)
            manager.onPlayerJoined += HandlePlayerJoined;
    }

    private void OnDisable()
    {
        if (manager != null)
            manager.onPlayerJoined -= HandlePlayerJoined;
    }

    private void HandlePlayerJoined(PlayerInput player)
    {
        // Wait one frame so PlayerMover.Start default spawn/tint finishes first.
        StartCoroutine(ConfigureAfterStart(player));
    }

    private IEnumerator ConfigureAfterStart(PlayerInput player)
    {
        yield return null;
        if (fight == null || player == null)
            yield break;

        CharacterDefinition character = player.playerIndex == 0
            ? fight.player1Character
            : fight.player2Character;

        ConfigureFighter(player, character, player.playerIndex);
    }

    private void ConfigureFighter(PlayerInput player, CharacterDefinition character, int index)
    {
        var combat = player.GetComponent<FighterCombat>();
        var health = player.GetComponent<FighterHealth>();
        if (health == null)
            health = player.gameObject.AddComponent<FighterHealth>();
        var mover = player.GetComponent<PlayerMover>();

        health.Configure(fight.startingHealth);

        if (mover != null)
        {
            mover.SetSideViewOnly(fight.sideViewMovement);
            if (fight.stage != null)
                mover.SetStageBoundsX(fight.stage.boundsMin.x, fight.stage.boundsMax.x);
        }

        if (character != null)
        {
            if (combat != null)
                combat.ApplyCharacter(character);

            ApplyCapsuleColor(player.gameObject, character.capsuleColor);
            Debug.Log($"[Fight] P{index + 1} → {character.characterId}");
        }
        else if (combat != null)
        {
            Debug.LogWarning($"[Fight] P{index + 1} has no CharacterDefinition on the FightDefinition.");
        }

        if (fight.stage != null)
        {
            Vector2 spawn = index == 0 ? fight.stage.spawnP1 : fight.stage.spawnP2;
            player.transform.position = new Vector3(spawn.x, spawn.y + 1f, 0f);
        }
    }

    private void ApplyStagePresentation()
    {
        if (fight == null || fight.stage == null)
            return;

        StageDefinition stage = fight.stage;
        Transform ground = GameObject.Find("Ground")?.transform;
        if (ground != null)
        {
            float width = Mathf.Max(1f, stage.Width);
            float depth = fight.sideViewMovement ? 2f : 4f;
            ground.localScale = new Vector3(width / 10f, 1f, depth / 10f);
            ground.position = new Vector3(
                (stage.boundsMin.x + stage.boundsMax.x) * 0.5f,
                stage.groundY,
                0f);
        }

        Camera cam = Camera.main;
        if (cam != null)
        {
            float halfWidth = stage.Width * 0.5f;
            cam.transform.position = new Vector3(
                (stage.boundsMin.x + stage.boundsMax.x) * 0.5f,
                2.5f,
                -Mathf.Max(10f, halfWidth * 0.9f));
            cam.transform.rotation = Quaternion.Euler(10f, 0f, 0f);
        }

        Debug.Log($"[Fight] Stage '{stage.stageId}' applied ({fight})");
    }

    private static void ApplyCapsuleColor(GameObject go, Color color)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend == null)
            return;
        var block = new MaterialPropertyBlock();
        rend.GetPropertyBlock(block);
        block.SetColor("_Color", color);
        rend.SetPropertyBlock(block);
    }

    private void OnGUI()
    {
        if (!drawHud || fight == null)
            return;

        GUI.Label(new Rect(16, 12, 640, 22), $"Fight: {fight.displayName}");

        FighterHealth[] healths = FindObjectsByType<FighterHealth>(FindObjectsSortMode.None);
        float y = 36f;
        for (int i = 0; i < healths.Length; i++)
        {
            FighterHealth h = healths[i];
            if (h == null)
                continue;
            var pi = h.GetComponent<PlayerInput>();
            int slot = pi != null ? pi.playerIndex + 1 : i + 1;
            string ko = h.IsKnockedOut ? " — KO" : "";
            GUI.Label(new Rect(16, y, 420, 22),
                $"P{slot} HP {h.CurrentHealth}/{h.MaxHealth}{ko}");
            y += 22f;
        }

        if (healths.Length < 2)
            GUI.Label(new Rect(16, y + 8, 480, 22), "Join two pads to start the basic fight.");
    }
}
