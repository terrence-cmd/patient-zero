using UnityEngine;

/// <summary>
/// One fully abstracted attack definition.
/// Stable <see cref="moveId"/> is the lookup key for AI, tools, and future systems —
/// do not rename lightly once content references it.
/// This asset does not play animations, apply damage, or run hitstun.
/// </summary>
[CreateAssetMenu(
    fileName = "Move_",
    menuName = "Patient Zero/Combat/Move Definition",
    order = 10)]
public class MoveDefinition : ScriptableObject
{
    [Tooltip("Stable machine id, e.g. light_punch. Used for lookups.")]
    public string moveId;

    [Tooltip("Human-readable name for demos and UI.")]
    public string displayName;

    public MoveCategory category = MoveCategory.Normal;
    public MoveHeight height = MoveHeight.Mid;

    public FrameTiming frames;
    public HitboxSpec hitbox;

    [Min(0)] public int damage;
    [Min(0)] public int hitstun;
    [Min(0)] public int blockstun;

    /// <summary>
    /// Optional cancel targets by moveId (empty = no cancels defined yet).
    /// Stored as ids so libraries stay data-driven and reorder-safe.
    /// </summary>
    public string[] cancelIntoMoveIds = System.Array.Empty<string>();

    public override string ToString()
    {
        string label = string.IsNullOrEmpty(displayName) ? name : displayName;
        return $"{moveId} ({label}): {frames}, dmg={damage}, hitstun={hitstun}, " +
               $"{category}/{height}, hitbox={hitbox}";
    }
}
