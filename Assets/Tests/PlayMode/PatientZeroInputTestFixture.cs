using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.TestTools;

/// <summary>
/// Shared Play Mode fixture. PlayerInputManager.OnDisable → DisableJoining()
/// decrements InputUser.listenForUnpairedDeviceActivity. InputTestFixture can
/// reset that counter to 0 while a manager still thinks joining is hooked, so
/// a naive DisableJoining / OnDisable throws ArgumentOutOfRangeException and
/// fails the next test. This tear-down unhooks safely first.
/// </summary>
public abstract class PatientZeroInputTestFixture : InputTestFixture
{
    [UnityTearDown]
    public IEnumerator SilenceJoiningBeforeInputTeardown()
    {
        PlayerInputManager[] managers = Object.FindObjectsByType<PlayerInputManager>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < managers.Length; i++)
        {
            PlayerInputManager manager = managers[i];
            if (manager != null)
                SilenceJoining(manager);
        }

        yield return null;
    }

    private static void SilenceJoining(PlayerInputManager manager)
    {
        // Happy path: counter still positive — use the public API.
        if (manager.joiningEnabled && InputUser.listenForUnpairedDeviceActivity > 0)
        {
            manager.DisableJoining();
            return;
        }

        // Counter already 0 (or joining already off): clear private flags so a
        // later OnDisable does not call DisableJoining / decrement again.
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        FieldInfo hookedField = typeof(PlayerInputManager)
            .GetField("m_UnpairedDeviceUsedDelegateHooked", flags);
        FieldInfo delegateField = typeof(PlayerInputManager)
            .GetField("m_UnpairedDeviceUsedDelegate", flags);
        FieldInfo allowField = typeof(PlayerInputManager)
            .GetField("m_AllowJoining", flags);

        if (hookedField != null && (bool)hookedField.GetValue(manager))
        {
            object del = delegateField?.GetValue(manager);
            if (del is System.Action<InputControl, UnityEngine.InputSystem.LowLevel.InputEventPtr> unpaired)
                InputUser.onUnpairedDeviceUsed -= unpaired;
            hookedField.SetValue(manager, false);
        }

        allowField?.SetValue(manager, false);
    }
}
