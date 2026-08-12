/// <summary>
/// Proposed default timings from the Game Flow design (Patient Zero spine).
/// Constants only — tune here rather than scattering magic numbers.
/// </summary>
public static class GameFlowTimings
{
    /// <summary>Boot / splash — non-skippable.</summary>
    public const float BootSeconds = 2.0f;

    /// <summary>Opening / VS card total hold.</summary>
    public const float OpeningSeconds = 4.0f;

    /// <summary>Opening becomes skippable after this many seconds.</summary>
    public const float OpeningSkipAfterSeconds = 1.0f;

    /// <summary>"READY" banner before fight unlock.</summary>
    public const float ReadySeconds = 1.0f;

    /// <summary>"FIGHT" banner after READY; input still locked until it ends.</summary>
    public const float FightBannerSeconds = 0.6f;

    /// <summary>Round clock (best of 1).</summary>
    public const float RoundClockSeconds = 99f;

    /// <summary>Match end freeze + outro before Results.</summary>
    public const float MatchEndFreezeSeconds = 2.5f;
}
