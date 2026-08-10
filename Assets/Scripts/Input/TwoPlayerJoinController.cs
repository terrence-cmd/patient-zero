using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Caps local join-in-progress at 2 players (shared screen, no split-screen).
/// PlayerInputManager itself has no player cap; this disables joining once
/// 2 players are in, and re-enables it if one leaves.
/// Handler names deliberately do NOT match the PlayerInputManager
/// SendMessages callback names (OnPlayerJoined/OnPlayerLeft) to avoid
/// double invocation via broadcast + C# event.
/// </summary>
[RequireComponent(typeof(PlayerInputManager))]
public class TwoPlayerJoinController : MonoBehaviour
{
    public const int MaxPlayers = 2;

    private PlayerInputManager manager;

    private void Awake()
    {
        manager = GetComponent<PlayerInputManager>();
    }

    private void OnEnable()
    {
        manager.onPlayerJoined += HandlePlayerJoined;
        manager.onPlayerLeft += HandlePlayerLeft;
    }

    private void OnDisable()
    {
        manager.onPlayerJoined -= HandlePlayerJoined;
        manager.onPlayerLeft -= HandlePlayerLeft;
    }

    private void HandlePlayerJoined(PlayerInput player)
    {
        Debug.Log($"[Join] Player {player.playerIndex + 1} joined " +
                  $"(devices: {string.Join(", ", player.devices)})");
        if (manager.playerCount >= MaxPlayers)
        {
            manager.DisableJoining();
            Debug.Log("[Join] 2 players in — joining disabled.");
        }
    }

    private void HandlePlayerLeft(PlayerInput player)
    {
        Debug.Log($"[Join] Player {player.playerIndex + 1} left.");
        if (manager.playerCount < MaxPlayers)
        {
            manager.EnableJoining();
        }
    }
}
