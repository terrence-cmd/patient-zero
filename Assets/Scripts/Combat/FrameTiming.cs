using System;
using UnityEngine;

/// <summary>
/// Classic fighting-game frame split for one attack.
/// All values are integer frames at the project's fixed tick (treat as 60 FPS).
/// Total length = Startup + Active + Recovery.
/// </summary>
[Serializable]
public struct FrameTiming
{
    [Tooltip("Frames before the hitbox turns on.")]
    [Min(0)] public int startup;

    [Tooltip("Frames the hitbox stays on.")]
    [Min(0)] public int active;

    [Tooltip("Frames after the hitbox turns off before the move ends.")]
    [Min(0)] public int recovery;

    public int TotalFrames => startup + active + recovery;

    public FrameTiming(int startupFrames, int activeFrames, int recoveryFrames)
    {
        startup = startupFrames;
        active = activeFrames;
        recovery = recoveryFrames;
    }

    public override string ToString() =>
        $"{startup}/{active}/{recovery} (total {TotalFrames})";
}
