using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Catalog of move definitions. Characters (later) and demos reference a library,
/// not individual move assets, so content can grow without code changes.
/// </summary>
[CreateAssetMenu(
    fileName = "MoveLibrary",
    menuName = "Patient Zero/Combat/Move Library",
    order = 11)]
public class MoveLibrary : ScriptableObject
{
    [Tooltip("Stable id for this catalog, e.g. demo_basics.")]
    public string libraryId = "demo_basics";

    public MoveDefinition[] moves = System.Array.Empty<MoveDefinition>();

    public int Count => moves != null ? moves.Length : 0;

    public MoveDefinition GetById(string moveId)
    {
        if (moves == null || string.IsNullOrEmpty(moveId))
            return null;

        for (int i = 0; i < moves.Length; i++)
        {
            MoveDefinition move = moves[i];
            if (move != null && move.moveId == moveId)
                return move;
        }

        return null;
    }

    public IEnumerable<MoveDefinition> Enumerate()
    {
        if (moves == null)
            yield break;

        for (int i = 0; i < moves.Length; i++)
        {
            if (moves[i] != null)
                yield return moves[i];
        }
    }
}
