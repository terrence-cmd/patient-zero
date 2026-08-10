using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Catalog of stage definitions. Same role as <see cref="MoveLibrary"/>:
/// one asset AI and tools can extend without touching runtime systems.
/// </summary>
[CreateAssetMenu(
    fileName = "StageLibrary",
    menuName = "Patient Zero/Combat/Stage Library",
    order = 21)]
public class StageLibrary : ScriptableObject
{
    [Tooltip("Stable id for this catalog, e.g. demo_stages.")]
    public string libraryId = "demo_stages";

    public StageDefinition[] stages = System.Array.Empty<StageDefinition>();

    public int Count => stages != null ? stages.Length : 0;

    public StageDefinition GetById(string stageId)
    {
        if (stages == null || string.IsNullOrEmpty(stageId))
            return null;

        for (int i = 0; i < stages.Length; i++)
        {
            StageDefinition stage = stages[i];
            if (stage != null && stage.stageId == stageId)
                return stage;
        }

        return null;
    }

    public IEnumerable<StageDefinition> Enumerate()
    {
        if (stages == null)
            yield break;

        for (int i = 0; i < stages.Length; i++)
        {
            if (stages[i] != null)
                yield return stages[i];
        }
    }
}
