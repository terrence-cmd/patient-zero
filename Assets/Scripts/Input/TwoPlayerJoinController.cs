using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

/// <summary>
/// Caps local join-in-progress at 2 players (shared screen, no split-screen).
/// Rejects a 3rd joiner by destroying that player object rather than calling
/// PlayerInputManager.DisableJoining — DisableJoining + scene teardown can
/// double-decrement InputUser.listenForUnpairedDeviceActivity and throw.
/// Also destroys a player's object on mid-session device loss — Input System
/// does not do that by default, which would leave an orphan and a stuck slot.
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
        // PlayerInputManager already counted this player before the callback.
        if (manager.playerCount > MaxPlayers)
        {
            Debug.Log($"[Join] Rejecting player {player.playerIndex + 1} — cap is {MaxPlayers}.");
            Destroy(player.gameObject);
            return;
        }

        Debug.Log($"[Join] Player {player.playerIndex + 1} joined " +
                  $"(devices: {string.Join(", ", player.devices)})");
    }

    private void HandlePlayerLeft(PlayerInput player)
    {
        Debug.Log($"[Join] Player {player.playerIndex + 1} left.");
    }

    private void HandleUserChange(InputUser user, InputUserChange change, InputDevice device)
    {
        if (change != InputUserChange.DeviceLost)
            return;

        // During an active flow-driven fight, device loss auto-pauses (Game Flow PDF).
        // Gate 0 / join-screen behavior stays destroy-and-reopen-slot.
        var flow = GameFlowDirector.Instance;
        if (flow != null && flow.State == GameState.Fight)
        {
            Debug.Log("[Join] Device lost during Fight — requesting pause.");
            flow.RequestPauseFromDeviceLoss();
            return;
        }

        // Find the PlayerInput still bound to this user and remove it. Destroying
        // the GameObject is what makes PlayerInputManager fire onPlayerLeft.
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
