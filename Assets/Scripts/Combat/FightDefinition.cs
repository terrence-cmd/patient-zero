using UnityEngine;

/// <summary>
/// One playable fight package: stage + two characters + starting health.
/// Enough data for <see cref="FightDirector"/> to render a basic local match.
/// </summary>
[CreateAssetMenu(
    fileName = "Fight_",
    menuName = "Patient Zero/Combat/Fight Definition",
    order = 40)]
public class FightDefinition : ScriptableObject
{
    public string fightId = "basic_fight";
    public string displayName = "Basic Fight";

    public StageDefinition stage;
    public CharacterDefinition player1Character;
    public CharacterDefinition player2Character;

    [Min(1)] public int startingHealth = 100;

    [Tooltip("If true, PlayerMover only uses stick X (side-scroller plane).")]
    public bool sideViewMovement = true;

    public override string ToString() =>
        $"{fightId} ({displayName}): stage={(stage != null ? stage.stageId : "null")}, " +
        $"P1={(player1Character != null ? player1Character.characterId : "null")}, " +
        $"P2={(player2Character != null ? player2Character.characterId : "null")}, hp={startingHealth}";
}
