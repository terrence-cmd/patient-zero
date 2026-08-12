/// <summary>
/// High-level match flow states for Patient Zero (1v1, 1 stage, best of 1).
/// Identifiers match the Claude Design "Game Flow" spine (Boot → Results + Pause).
/// Left open by design docs as application-layer flow; timings live in
/// <see cref="GameFlowTimings"/>.
/// </summary>
public enum GameState
{
    Boot,
    Title,
    PlayerJoin,
    CharacterSelect,
    Opening,
    RoundStart,
    Fight,
    MatchEnd,
    Results,
    Paused
}
