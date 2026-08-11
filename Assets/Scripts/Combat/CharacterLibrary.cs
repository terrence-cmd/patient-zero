using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Catalog of character definitions — same role as MoveLibrary / StageLibrary.
/// </summary>
[CreateAssetMenu(
    fileName = "CharacterLibrary",
    menuName = "Patient Zero/Combat/Character Library",
    order = 31)]
public class CharacterLibrary : ScriptableObject
{
    public string libraryId = "demo_roster";
    public CharacterDefinition[] characters = System.Array.Empty<CharacterDefinition>();

    public int Count => characters != null ? characters.Length : 0;

    public CharacterDefinition GetById(string characterId)
    {
        if (characters == null || string.IsNullOrEmpty(characterId))
            return null;

        for (int i = 0; i < characters.Length; i++)
        {
            CharacterDefinition c = characters[i];
            if (c != null && c.characterId == characterId)
                return c;
        }

        return null;
    }

    public IEnumerable<CharacterDefinition> Enumerate()
    {
        if (characters == null)
            yield break;
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null)
                yield return characters[i];
        }
    }
}
