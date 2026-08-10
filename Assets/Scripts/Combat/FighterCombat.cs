using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Executes <see cref="MoveDefinition"/> data on a joined player:
/// startup → active (hitbox + overlap) → recovery, plus hitstun when hit.
/// Movement is locked while not Idle (see <see cref="LocksMovement"/>).
///
/// Frame clock is a fixed 60 FPS accumulator so frame data matches the
/// library numbers even if rendering runs faster/slower.
///
/// Out of scope for this first executable pass: cancels, projectiles,
/// blocking, crouch state, animation, damage numbers / health bars.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class FighterCombat : MonoBehaviour
{
    public const float CombatFps = 60f;
    private const float FrameDuration = 1f / CombatFps;

    [SerializeField] private MoveLibrary moveLibrary;
    [SerializeField] private Vector2 hurtboxSize = new Vector2(1.0f, 2.0f);
    [Tooltip("Capsule half-height. Hitbox offsets in MoveDefinition are from the feet.")]
    [SerializeField] private float feetOffsetY = 1f;
    [SerializeField] private bool drawHitbox = true;

    private PlayerInput playerInput;
    private InputAction lightPunch;
    private InputAction heavyPunch;
    private InputAction lightKick;
    private InputAction heavyKick;
    private InputAction crouchJab;
    private InputAction special;

    private MoveDefinition currentMove;
    private CombatPhase phase = CombatPhase.Idle;
    private int phaseFrame;
    private int hitstunFramesRemaining;
    private bool hitLandedThisMove;
    private float frameAccumulator;
    private int facingSign = 1;
    private GameObject hitboxVisual;
    // Join uses "any button"; ignore attacks briefly so the join press doesn't fire a move.
    private float attackEnableAtTime;

    public CombatPhase Phase => phase;
    public MoveDefinition CurrentMove => currentMove;
    public bool LocksMovement => phase != CombatPhase.Idle;
    public int FacingSign => facingSign;
    public MoveLibrary Library => moveLibrary;

    public void SetMoveLibrary(MoveLibrary library) => moveLibrary = library;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        // Bind here (and again in Start) so we can see/log the live map after
        // PlayerInput has had a chance to enable it and pair devices. The
        // InputAction C# reference itself is fine if grabbed in Awake — what
        // matters is that the map is enabled and paired before Update polls it.
        BindActions();
        attackEnableAtTime = Time.time + 0.25f;
    }

    private void Start()
    {
        // Second bind + diagnostic: enabled/devices are what prove the
        // join → action-map handoff actually completed (Input Debugger equivalent).
        BindActions();

        if (moveLibrary == null)
            Debug.LogWarning($"[FighterCombat] P{playerInput.playerIndex + 1}: MoveLibrary is not assigned.");
        if (lightPunch == null)
            Debug.LogWarning($"[FighterCombat] P{playerInput.playerIndex + 1}: LightPunch action missing — check PlayerControls.");
        else
            Debug.Log($"[FighterCombat] P{playerInput.playerIndex + 1} ready " +
                      $"(LightPunch enabled={lightPunch.enabled}, devices={playerInput.devices.Count})");
    }

    private void OnDisable()
    {
        lightPunch = null;
        heavyPunch = null;
        lightKick = null;
        heavyKick = null;
        crouchJab = null;
        special = null;
        DestroyHitboxVisual();
    }

    private void BindActions()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();
        if (playerInput == null || playerInput.actions == null)
            return;

        InputActionMap map = playerInput.currentActionMap;
        if (map == null)
            map = playerInput.actions.FindActionMap("Player", throwIfNotFound: false);
        if (map == null)
            return;

        if (!map.enabled)
            map.Enable();

        lightPunch = map.FindAction("LightPunch", throwIfNotFound: false);
        heavyPunch = map.FindAction("HeavyPunch", throwIfNotFound: false);
        lightKick = map.FindAction("LightKick", throwIfNotFound: false);
        heavyKick = map.FindAction("HeavyKick", throwIfNotFound: false);
        crouchJab = map.FindAction("CrouchJab", throwIfNotFound: false);
        special = map.FindAction("Special", throwIfNotFound: false);
    }

    private void Update()
    {
        UpdateFacingFromOpponent();

        if (phase == CombatPhase.Idle)
        {
            TryStartFromInput();
            return;
        }

        frameAccumulator += Time.deltaTime;
        while (frameAccumulator >= FrameDuration)
        {
            frameAccumulator -= FrameDuration;
            TickCombatFrame();
            if (phase == CombatPhase.Idle)
                break;
        }
    }

    /// <summary>
    /// Start a move by stable moveId. Returns false if busy, unknown, or library missing.
    /// </summary>
    public bool TryStartMove(string moveId)
    {
        if (phase != CombatPhase.Idle || string.IsNullOrEmpty(moveId))
            return false;

        if (moveLibrary == null)
        {
            Debug.LogWarning("[FighterCombat] Cannot start move — MoveLibrary is not assigned on the prefab.");
            return false;
        }

        MoveDefinition move = moveLibrary.GetById(moveId);
        if (move == null)
        {
            Debug.LogWarning($"[FighterCombat] Unknown moveId '{moveId}'.");
            return false;
        }

        currentMove = move;
        phase = CombatPhase.Startup;
        phaseFrame = 0;
        hitLandedThisMove = false;
        frameAccumulator = 0f;
        Debug.Log($"[Combat] P{playerInput.playerIndex + 1} started {move.moveId} " +
                  $"({move.frames}) facing={(facingSign > 0 ? "+" : "-")}X");
        return true;
    }

    public void ApplyHitstun(int frames)
    {
        if (frames <= 0)
            return;

        currentMove = null;
        phase = CombatPhase.Hitstun;
        hitstunFramesRemaining = frames;
        phaseFrame = 0;
        frameAccumulator = 0f;
        DestroyHitboxVisual();
        Debug.Log($"[Combat] P{playerInput.playerIndex + 1} hitstun {frames}f");
    }

    public Bounds GetHurtboxBounds()
    {
        Vector3 center = transform.position;
        return new Bounds(center, new Vector3(hurtboxSize.x, hurtboxSize.y, 1f));
    }

    public Bounds GetActiveHitboxBounds()
    {
        if (phase != CombatPhase.Active || currentMove == null)
            return new Bounds(Vector3.zero, Vector3.zero);

        return BuildHitboxBounds(currentMove.hitbox);
    }

    private void TryStartFromInput()
    {
        if (Time.time < attackEnableAtTime)
            return;

        if (lightPunch == null)
            BindActions();

        // Face buttons first so a chord doesn't always prefer Special.
        if (lightPunch != null && lightPunch.WasPressedThisFrame())
            TryStartMove("light_punch");
        else if (heavyPunch != null && heavyPunch.WasPressedThisFrame())
            TryStartMove("heavy_punch");
        else if (lightKick != null && lightKick.WasPressedThisFrame())
            TryStartMove("light_kick");
        else if (heavyKick != null && heavyKick.WasPressedThisFrame())
            TryStartMove("heavy_kick");
        else if (crouchJab != null && crouchJab.WasPressedThisFrame())
            TryStartMove("crouch_jab");
        else if (special != null && special.WasPressedThisFrame())
            TryStartMove("hadoken");
    }

    private void TickCombatFrame()
    {
        if (phase == CombatPhase.Hitstun)
        {
            hitstunFramesRemaining--;
            if (hitstunFramesRemaining <= 0)
            {
                phase = CombatPhase.Idle;
                Debug.Log($"[Combat] P{playerInput.playerIndex + 1} recovered from hitstun");
            }
            return;
        }

        if (currentMove == null)
        {
            phase = CombatPhase.Idle;
            return;
        }

        phaseFrame++;

        switch (phase)
        {
            case CombatPhase.Startup:
                if (phaseFrame >= currentMove.frames.startup)
                {
                    phase = CombatPhase.Active;
                    phaseFrame = 0;
                    ShowHitboxVisual(true);
                }
                break;

            case CombatPhase.Active:
                TryHitOpponents();
                if (phaseFrame >= currentMove.frames.active)
                {
                    phase = CombatPhase.Recovery;
                    phaseFrame = 0;
                    DestroyHitboxVisual();
                }
                break;

            case CombatPhase.Recovery:
                if (phaseFrame >= currentMove.frames.recovery)
                {
                    currentMove = null;
                    phase = CombatPhase.Idle;
                    phaseFrame = 0;
                }
                break;
        }
    }

    private void TryHitOpponents()
    {
        if (hitLandedThisMove || currentMove == null)
            return;

        Bounds hitbox = GetActiveHitboxBounds();
        FighterCombat[] fighters = FindObjectsByType<FighterCombat>(FindObjectsSortMode.None);
        for (int i = 0; i < fighters.Length; i++)
        {
            FighterCombat other = fighters[i];
            if (other == null || other == this)
                continue;

            if (hitbox.Intersects(other.GetHurtboxBounds()))
            {
                hitLandedThisMove = true;
                other.ApplyHitstun(currentMove.hitstun);
                Debug.Log($"[Combat] P{playerInput.playerIndex + 1} hit " +
                          $"P{other.playerInput.playerIndex + 1} with {currentMove.moveId} " +
                          $"(dmg={currentMove.damage}, hitstun={currentMove.hitstun})");
                break;
            }
        }
    }

    private Bounds BuildHitboxBounds(HitboxSpec spec)
    {
        Vector3 feet = transform.position - Vector3.up * feetOffsetY;
        Vector3 center = feet + new Vector3(spec.offset.x * facingSign, spec.offset.y, 0f);
        return new Bounds(center, new Vector3(spec.size.x, spec.size.y, 1f));
    }

    private void UpdateFacingFromOpponent()
    {
        FighterCombat[] fighters = FindObjectsByType<FighterCombat>(FindObjectsSortMode.None);
        FighterCombat nearest = null;
        float best = float.MaxValue;
        for (int i = 0; i < fighters.Length; i++)
        {
            FighterCombat other = fighters[i];
            if (other == null || other == this)
                continue;
            float d = Mathf.Abs(other.transform.position.x - transform.position.x);
            if (d < best)
            {
                best = d;
                nearest = other;
            }
        }

        if (nearest != null)
        {
            float dx = nearest.transform.position.x - transform.position.x;
            if (Mathf.Abs(dx) > 0.05f)
                facingSign = dx > 0f ? 1 : -1;
        }
    }

    private void ShowHitboxVisual(bool on)
    {
        if (!drawHitbox || !on || currentMove == null)
        {
            DestroyHitboxVisual();
            return;
        }

        if (hitboxVisual == null)
        {
            hitboxVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hitboxVisual.name = "HitboxVisual";
            Object.Destroy(hitboxVisual.GetComponent<Collider>());
            var rend = hitboxVisual.GetComponent<Renderer>();
            if (rend != null)
            {
                // Unlit-ish tint via property block so we don't leak materials.
                var block = new MaterialPropertyBlock();
                rend.GetPropertyBlock(block);
                block.SetColor("_Color", new Color(1f, 0.2f, 0.2f, 0.55f));
                rend.SetPropertyBlock(block);
            }
        }

        Bounds b = BuildHitboxBounds(currentMove.hitbox);
        hitboxVisual.transform.position = b.center;
        hitboxVisual.transform.localScale = b.size;
    }

    private void DestroyHitboxVisual()
    {
        if (hitboxVisual != null)
        {
            Destroy(hitboxVisual);
            hitboxVisual = null;
        }
    }

    private void LateUpdate()
    {
        // Keep the cube locked to the hitbox while Active (fighter may still be pushed later).
        if (phase == CombatPhase.Active && hitboxVisual != null && currentMove != null)
        {
            Bounds b = BuildHitboxBounds(currentMove.hitbox);
            hitboxVisual.transform.position = b.center;
            hitboxVisual.transform.localScale = b.size;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
        Bounds hurt = Application.isPlaying
            ? GetHurtboxBounds()
            : new Bounds(transform.position, new Vector3(hurtboxSize.x, hurtboxSize.y, 1f));
        Gizmos.DrawWireCube(hurt.center, hurt.size);

        if (Application.isPlaying && phase == CombatPhase.Active && currentMove != null)
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.8f);
            Bounds hit = GetActiveHitboxBounds();
            Gizmos.DrawWireCube(hit.center, hit.size);
        }
    }
}
