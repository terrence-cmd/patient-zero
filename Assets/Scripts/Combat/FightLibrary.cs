using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Catalog of fight packages.
/// </summary>
[CreateAssetMenu(
    fileName = "FightLibrary",
    menuName = "Patient Zero/Combat/Fight Library",
    order = 41)]
public class FightLibrary : ScriptableObject
{
    public string libraryId = "demo_fights";
    public FightDefinition[] fights = System.Array.Empty<FightDefinition>();

    public int Count => fights != null ? fights.Length : 0;

    public FightDefinition GetById(string fightId)
    {
        if (fights == null || string.IsNullOrEmpty(fightId))
            return null;

        for (int i = 0; i < fights.Length; i++)
        {
            FightDefinition f = fights[i];
            if (f != null && f.fightId == fightId)
                return f;
        }

        return null;
    }

    public IEnumerable<FightDefinition> Enumerate()
    {
        if (fights == null)
            yield break;
        for (int i = 0; i < fights.Length; i++)
        {
            if (fights[i] != null)
                yield return fights[i];
        }
    }
}
