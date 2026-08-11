using UnityEngine;

/// <summary>
/// One fighter identity for demos / AI: which move library they use, look, hurtbox,
/// and which moveIds the six attack buttons should fire.
/// Data only until <see cref="FightDirector"/> applies it at join time.
/// </summary>
[CreateAssetMenu(
    fileName = "Character_",
    menuName = "Patient Zero/Combat/Character Definition",
    order = 30)]
public class CharacterDefinition : ScriptableObject
{
    public string characterId;
    public string displayName;
    public MoveLibrary moveLibrary;
    public Color capsuleColor = Color.white;
    public Vector2 hurtboxSize = new Vector2(1f, 2f);

    [Header("Attack button → moveId (must exist in moveLibrary)")]
    public string lightPunchMoveId = "light_punch";
    public string heavyPunchMoveId = "heavy_punch";
    public string lightKickMoveId = "light_kick";
    public string heavyKickMoveId = "heavy_kick";
    public string crouchJabMoveId = "crouch_jab";
    public string specialMoveId = "hadoken";

    public override string ToString() =>
        $"{characterId} ({displayName}) lib={(moveLibrary != null ? moveLibrary.libraryId : "null")}";
}
