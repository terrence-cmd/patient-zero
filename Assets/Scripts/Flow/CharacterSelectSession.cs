using System;
using UnityEngine;

/// <summary>
/// Runtime character-select session for two local players.
/// Cursor indices into <see cref="CharacterLibrary"/>; both must lock before Opening.
/// </summary>
[Serializable]
public class CharacterSelectSession
{
    public int p1Cursor;
    public int p2Cursor;
    public bool p1Locked;
    public bool p2Locked;

    public bool BothLocked => p1Locked && p2Locked;

    public void Reset()
    {
        p1Cursor = 0;
        p2Cursor = 0;
        p1Locked = false;
        p2Locked = false;
    }

    public void MoveCursor(int playerSlot, int delta, int rosterCount)
    {
        if (rosterCount <= 0)
            return;

        if (playerSlot == 0)
        {
            if (p1Locked)
                return;
            p1Cursor = Wrap(p1Cursor + delta, rosterCount);
        }
        else
        {
            if (p2Locked)
                return;
            p2Cursor = Wrap(p2Cursor + delta, rosterCount);
        }
    }

    public void Confirm(int playerSlot)
    {
        if (playerSlot == 0)
            p1Locked = true;
        else
            p2Locked = true;
    }

    public CharacterDefinition Resolve(CharacterLibrary library, int playerSlot)
    {
        if (library == null || library.Count == 0)
            return null;

        int index = playerSlot == 0 ? p1Cursor : p2Cursor;
        index = Mathf.Clamp(index, 0, library.Count - 1);
        return library.characters[index];
    }

    private static int Wrap(int value, int count)
    {
        if (count <= 0)
            return 0;
        int m = value % count;
        return m < 0 ? m + count : m;
    }
}
