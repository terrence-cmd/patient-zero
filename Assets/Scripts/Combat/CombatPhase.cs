/// <summary>
/// Where a fighter is in its combat timeline.
/// Idle = can move and start attacks. Hitstun = was hit; locked out.
/// </summary>
public enum CombatPhase
{
    Idle = 0,
    Startup = 1,
    Active = 2,
    Recovery = 3,
    Hitstun = 4,
}
