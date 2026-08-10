using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

/// <summary>
/// Caps local join-in-progress at 2 players (shared screen, no split-screen).
/// PlayerInputManager itself has no player cap; this disables joining once
/// 2 players are in, and re-enables it if one leaves.
/// Also destroys a player's object on mid-session device loss — Input System
/// does not do that by default, which would leave an orphan and a stuck cap.
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
        // Listen at the InputUser level — PlayerInput.onDeviceLost only fires when
        // that component's notificationBehavior is InvokeCSharpEvents, and our
        // player prefab uses SendMessages (Unity's default).
        InputUser.onChange += HandleUserChange;
    }

    private void OnDisable()
    {
        manager.onPlayerJoined -= HandlePlayerJoined;
        manager.onPlayerLeft -= HandlePlayerLeft;
        InputUser.onChange -= HandleUserChange;
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

    private void HandleUserChange(InputUser user, InputUserChange change, InputDevice device)
    {
        if (change != InputUserChange.DeviceLost)
            return;

        // Find the PlayerInput still bound to this user and remove it. Destroying
        // the GameObject is what makes PlayerInputManager fire onPlayerLeft
        // (which re-enables joining via HandlePlayerLeft).
        foreach (var player in PlayerInput.all)
        {
            if (player.user == user)
            {
                Debug.Log($"[Join] Player {player.playerIndex + 1} device lost — removing player.");
                Destroy(player.gameObject);
                break;
            }
        }
    }
}
