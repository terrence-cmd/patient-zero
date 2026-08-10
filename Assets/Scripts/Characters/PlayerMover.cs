using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Placeholder character controller for Gate 0: proves each joined player
/// controls a distinct object with their own device. Real character logic
/// is Gate 1 territory.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class PlayerMover : MonoBehaviour
{
    [SerializeField] private float speed = 6f;

    private static readonly Color[] PlayerColors =
    {
        new Color(0.90f, 0.30f, 0.25f), // P1 red
        new Color(0.25f, 0.50f, 0.95f)  // P2 blue
    };

    private PlayerInput playerInput;
    private InputAction moveAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
    }

    private void Start()
    {
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

    private void Update()
    {
        Vector2 move = moveAction.ReadValue<Vector2>();
        Vector3 delta = new Vector3(move.x, 0f, move.y) * (speed * Time.deltaTime);
        transform.Translate(delta, Space.World);
    }
}
