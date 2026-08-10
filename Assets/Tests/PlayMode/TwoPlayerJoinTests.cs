using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Verifies Gate 0 acceptance criterion 5 with virtual gamepads:
/// two gamepads can join via button press, each controls a distinct
/// player object, and joining is capped at 2.
/// (Hardware-free equivalent of "two gamepads connected, both join".)
/// </summary>
public class TwoPlayerJoinTests : InputTestFixture
{
    [UnityTest]
    public IEnumerator TwoGamepads_BothJoin_EachControlsDistinctPlayer_CapIsTwo()
    {
        SceneManager.LoadScene("Main");
        yield return null; // scene activation
        yield return null;

        Assert.AreEqual(0, PlayerInput.all.Count, "no players before any join press");

        var pad1 = InputSystem.AddDevice<Gamepad>();
        var pad2 = InputSystem.AddDevice<Gamepad>();

        // Player 1 joins on button press
        Press(pad1.buttonSouth);
        yield return null;
        Release(pad1.buttonSouth);
        yield return null;
        Assert.AreEqual(1, PlayerInput.all.Count, "player 1 joined on gamepad 1 button press");

        // Player 2 joins on the second gamepad
        Press(pad2.buttonSouth);
        yield return null;
        Release(pad2.buttonSouth);
        yield return null;
        Assert.AreEqual(2, PlayerInput.all.Count, "player 2 joined on gamepad 2 button press");

        var p1 = PlayerInput.all[0];
        var p2 = PlayerInput.all[1];
        Assert.AreNotEqual(p1.gameObject, p2.gameObject, "players are distinct objects");
        Assert.AreNotEqual(p1.playerIndex, p2.playerIndex, "players have distinct indices");
        CollectionAssert.Contains(p1.devices, (InputDevice)pad1, "gamepad 1 paired to player 1");
        CollectionAssert.Contains(p2.devices, (InputDevice)pad2, "gamepad 2 paired to player 2");

        // Moving gamepad 1's stick moves ONLY player 1
        Vector3 p1Start = p1.transform.position;
        Vector3 p2Start = p2.transform.position;
        Set(pad1.leftStick, new Vector2(1f, 0f));
        yield return new WaitForSeconds(0.3f);
        Set(pad1.leftStick, Vector2.zero);
        yield return null;
        Assert.Greater(p1.transform.position.x, p1Start.x + 0.05f, "player 1 moved with gamepad 1 stick");
        Assert.AreEqual(p2Start.x, p2.transform.position.x, 0.01f, "player 2 unaffected by gamepad 1 stick");

        // A third gamepad cannot join — local 2-player cap
        var pad3 = InputSystem.AddDevice<Gamepad>();
        Press(pad3.buttonSouth);
        yield return null;
        Release(pad3.buttonSouth);
        yield return null;
        Assert.AreEqual(2, PlayerInput.all.Count, "join-in-progress capped at 2 players");
    }

    /// <summary>
    /// Mid-session disconnect: after two players have joined, removing one
    /// device must clean up that player's object, fire onPlayerLeft (so the
    /// join cap re-opens), and let a new pad take the free slot.
    /// </summary>
    [UnityTest]
    public IEnumerator MidSessionDisconnect_CleansUpPlayer_AndReopensJoinSlot()
    {
        SceneManager.LoadScene("Main");
        yield return null;
        yield return null;

        var manager = Object.FindFirstObjectByType<PlayerInputManager>();
        Assert.IsNotNull(manager, "Main scene should have a PlayerInputManager");

        int leftEvents = 0;
        void OnLeft(PlayerInput _) => leftEvents++;
        manager.onPlayerLeft += OnLeft;

        var pad1 = InputSystem.AddDevice<Gamepad>();
        var pad2 = InputSystem.AddDevice<Gamepad>();

        Press(pad1.buttonSouth);
        yield return null;
        Release(pad1.buttonSouth);
        yield return null;
        Press(pad2.buttonSouth);
        yield return null;
        Release(pad2.buttonSouth);
        yield return null;
        Assert.AreEqual(2, PlayerInput.all.Count, "both pads joined before disconnect");

        var disconnectedPlayer = PlayerInput.all[0];
        CollectionAssert.Contains(disconnectedPlayer.devices, (InputDevice)pad1,
            "test assumes PlayerInput.all[0] is paired to pad1");
        var disconnectedGo = disconnectedPlayer.gameObject;

        InputSystem.RemoveDevice(pad1);
        // Device-lost → Destroy is end-of-frame; give the manager a beat to update.
        yield return null;
        yield return null;

        Assert.GreaterOrEqual(leftEvents, 1, "HandlePlayerLeft / onPlayerLeft should fire on disconnect");
        Assert.IsTrue(disconnectedGo == null, "disconnected player's GameObject should be destroyed");
        Assert.AreEqual(1, PlayerInput.all.Count, "one player should remain after disconnect");

        // Free slot must accept a new device (the old 2-player cap re-opens).
        // Don't assume PlayerInput.all order — after a mid-list disconnect the
        // new joiner may not be at the end of the list.
        var pad3 = InputSystem.AddDevice<Gamepad>();
        Press(pad3.buttonSouth);
        yield return null;
        Release(pad3.buttonSouth);
        yield return null;
        Assert.AreEqual(2, PlayerInput.all.Count, "new pad should join the re-opened slot");

        bool pad3Paired = false;
        foreach (var player in PlayerInput.all)
        {
            foreach (var device in player.devices)
            {
                if (device == pad3)
                {
                    pad3Paired = true;
                    break;
                }
            }
        }
        Assert.IsTrue(pad3Paired, "pad3 should be paired to the newly joined player");

        manager.onPlayerLeft -= OnLeft;
    }
}
