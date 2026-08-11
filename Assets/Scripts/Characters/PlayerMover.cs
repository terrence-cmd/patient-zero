using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Character locomotion for joined players. Supports free XZ (Gate 0) or
/// side-view X-only when a fight package asks for it.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class PlayerMover : MonoBehaviour
{
    [SerializeField] private float speed = 6f;
    [SerializeField] private bool sideViewOnly;

    private static readonly Color[] PlayerColors =
    {
        new Color(0.90f, 0.30f, 0.25f), // P1 red
        new Color(0.25f, 0.50f, 0.95f)  // P2 blue
    };

    private PlayerInput playerInput;
    private InputAction moveAction;
    private FighterCombat combat;
    private FighterHealth health;
    private Vector2 stageBoundsX = new Vector2(-8f, 8f);
    private bool hasStageBounds;

    public void SetSideViewOnly(bool enabled) => sideViewOnly = enabled;

    public void SetStageBoundsX(float minX, float maxX)
    {
        stageBoundsX = new Vector2(minX, maxX);
        hasStageBounds = true;
    }

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        combat = GetComponent<FighterCombat>();
        health = GetComponent<FighterHealth>();
    }

    private void OnEnable()
    {
        // Same timing as FighterCombat: bind after PlayerInput has enabled its map.
        BindMoveAction();
    }

    private void Start()
    {
        BindMoveAction();

        int index = Mathf.Clamp(playerInput.playerIndex, 0, PlayerColors.Length - 1);
        transform.position = new Vector3(index == 0 ? -3f : 3f, 1f, 0f);

        var rend = GetComponent<Renderer>();
        if (rend != null)
        {
            // PropertyBlock tints without instantiating a material copy (avoids
            // leaks when players join/leave repeatedly).
            var block = new MaterialPropertyBlock();
            rend.GetPropertyBlock(block);
            block.SetColor("_Color", PlayerColors[index]);
            rend.SetPropertyBlock(block);
        }
    }

    private void OnDisable()
    {
        moveAction = null;
    }

    private void BindMoveAction()
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

        moveAction = map.FindAction("Move", throwIfNotFound: false);
    }

    private void Update()
    {
        if (health == null)
            health = GetComponent<FighterHealth>();
        if (health != null && health.IsKnockedOut)
            return;

        // Attacks and hitstun lock movement so frame data is readable on-screen.
        if (combat != null && combat.LocksMovement)
            return;

        if (moveAction == null)
            BindMoveAction();
        if (moveAction == null)
            return;

        Vector2 move = moveAction.ReadValue<Vector2>();
        float x = move.x * speed * Time.deltaTime;
        float z = sideViewOnly ? 0f : move.y * speed * Time.deltaTime;
        transform.Translate(new Vector3(x, 0f, z), Space.World);

        if (hasStageBounds)
        {
            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, stageBoundsX.x, stageBoundsX.y);
            p.z = sideViewOnly ? 0f : p.z;
            transform.position = p;
        }
    }
}
