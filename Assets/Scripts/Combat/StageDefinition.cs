using UnityEngine;

/// <summary>
/// Abstract stage layout: playable bounds and spawn points in 2D fighter space.
/// This is not a Unity scene — it is data a future stage loader / camera / spawn
/// system can consume. Keep art/scene wiring out of this asset.
/// </summary>
[CreateAssetMenu(
    fileName = "Stage_",
    menuName = "Patient Zero/Combat/Stage Definition",
    order = 20)]
public class StageDefinition : ScriptableObject
{
    [Tooltip("Stable machine id, e.g. training_room.")]
    public string stageId;

    [Tooltip("Human-readable name for demos and UI.")]
    public string displayName;

    [Tooltip("World Y of the walkable floor.")]
    public float groundY;

    [Tooltip("Inclusive-ish playable rectangle in XZ-ignored 2D fighter plane (X,Y).")]
    public Vector2 boundsMin;

    public Vector2 boundsMax;

    public Vector2 spawnP1;
    public Vector2 spawnP2;

    public float Width => boundsMax.x - boundsMin.x;
    public float Height => boundsMax.y - boundsMin.y;

    public override string ToString()
    {
        string label = string.IsNullOrEmpty(displayName) ? name : displayName;
        return $"{stageId} ({label}): groundY={groundY}, " +
               $"bounds=[{boundsMin} .. {boundsMax}], " +
               $"spawns P1={spawnP1} P2={spawnP2}";
    }
}
