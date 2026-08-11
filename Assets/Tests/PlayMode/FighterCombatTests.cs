using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Proves the demo move library is executable: a joined fighter can start a
/// move by id, walk startup→active→recovery, and apply hitstun on overlap.
/// </summary>
public class FighterCombatTests : PatientZeroInputTestFixture
{
    [UnityTest]
    public IEnumerator LightPunch_RunsFramePhases_ThenReturnsToIdle()
    {
        SceneManager.LoadScene("Main");
        yield return null;
        yield return null;

        var pad = InputSystem.AddDevice<Gamepad>();
        Press(pad.buttonWest);
        yield return null;
        Release(pad.buttonWest);
        yield return null;

        Assert.AreEqual(1, PlayerInput.all.Count, "need one joined player");
        var combat = PlayerInput.all[0].GetComponent<FighterCombat>();
        Assert.IsNotNull(combat, "Player prefab should have FighterCombat");
        Assert.IsNotNull(combat.Library, "MoveLibrary should be assigned on prefab");

        string punchId = combat.Library.GetById("high_punch") != null ? "high_punch" : "light_punch";

        // Bypass join grace + input map: start by id.
        Assert.IsTrue(combat.TryStartMove(punchId), $"{punchId} should start from Idle");
        Assert.AreEqual(CombatPhase.Startup, combat.Phase);

        // Normals are short (~12f at 60 FPS); allow a little headroom.
        float guard = 0f;
        while (combat.Phase != CombatPhase.Idle && guard < 1.5f)
        {
            guard += Time.deltaTime;
            yield return null;
        }

        Assert.AreEqual(CombatPhase.Idle, combat.Phase, "move should finish back at Idle");
        Assert.IsNull(combat.CurrentMove);
    }

    [UnityTest]
    public IEnumerator LightPunch_OnOverlappingOpponent_AppliesHitstun()
    {
        SceneManager.LoadScene("Main");
        yield return null;
        yield return null;

        var pad1 = InputSystem.AddDevice<Gamepad>();
        var pad2 = InputSystem.AddDevice<Gamepad>();

        Press(pad1.buttonWest);
        yield return null;
        Release(pad1.buttonWest);
        yield return null;
        Press(pad2.buttonNorth);
        yield return null;
        Release(pad2.buttonNorth);
        yield return null;

        Assert.AreEqual(2, PlayerInput.all.Count);

        FighterCombat attacker = null;
        FighterCombat defender = null;
        foreach (var p in PlayerInput.all)
        {
            var c = p.GetComponent<FighterCombat>();
            Assert.IsNotNull(c);
            if (p.playerIndex == 0) attacker = c;
            else defender = c;
        }

        Assert.IsNotNull(attacker);
        Assert.IsNotNull(defender);

        // Let FightDirector finish character/stage configure (one frame after join).
        yield return null;

        // Stand on top of each other so the active hitbox must overlap.
        attacker.transform.position = new Vector3(0f, 1f, 0f);
        defender.transform.position = new Vector3(0.6f, 1f, 0f);

        string punchId = attacker.Library.GetById("high_punch") != null ? "high_punch" : "light_punch";
        Assert.IsTrue(attacker.TryStartMove(punchId));

        float guard = 0f;
        while (defender.Phase != CombatPhase.Hitstun && guard < 1.5f)
        {
            guard += Time.deltaTime;
            yield return null;
        }

        Assert.AreEqual(CombatPhase.Hitstun, defender.Phase, "defender should enter hitstun on hit");

        guard = 0f;
        while (defender.Phase == CombatPhase.Hitstun && guard < 2f)
        {
            guard += Time.deltaTime;
            yield return null;
        }

        Assert.AreEqual(CombatPhase.Idle, defender.Phase, "hitstun should expire back to Idle");
    }
}
