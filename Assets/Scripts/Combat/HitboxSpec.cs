using System;
using UnityEngine;

/// <summary>
/// Abstract axis-aligned hitbox in local attacker space.
/// Convention: attacker faces +X. Offset is the box center; size is full width/height.
/// No runtime collision lives here — this is data only.
/// </summary>
[Serializable]
public struct HitboxSpec
{
    public Vector2 offset;
    public Vector2 size;

    public HitboxSpec(Vector2 boxOffset, Vector2 boxSize)
    {
        offset = boxOffset;
        size = boxSize;
    }

    public override string ToString() =>
        $"offset={offset} size={size}";
}
