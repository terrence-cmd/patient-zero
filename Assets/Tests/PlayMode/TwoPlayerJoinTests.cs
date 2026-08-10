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
}
