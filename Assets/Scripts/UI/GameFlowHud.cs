using UnityEngine;

/// <summary>
/// Stub OnGUI banners for each <see cref="GameState"/> — splash, join, select,
/// VS, READY/FIGHT, HP+clock, K.O., results, pause. Replace with real UI later.
/// </summary>
public class GameFlowHud : MonoBehaviour
{
    private GameFlowDirector flow;

    private void Awake() => flow = GetComponent<GameFlowDirector>();

    private void OnGUI()
    {
        if (flow == null || !flow.DrawHud)
            return;

        GUIStyle title = new GUIStyle(GUI.skin.label)
        {
            fontSize = 28,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        GUIStyle body = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter
        };

        Rect banner = new Rect(0, Screen.height * 0.35f, Screen.width, 40);
        Rect sub = new Rect(0, Screen.height * 0.35f + 44, Screen.width, 28);

        switch (flow.State)
        {
            case GameState.Boot:
                GUI.Label(banner, "PATIENT ZERO", title);
                GUI.Label(sub, "STUDIO / ENGINE MARK", body);
                break;

            case GameState.Title:
                GUI.Label(banner, "PATIENT ZERO", title);
                GUI.Label(sub, "PRESS START", body);
                break;

            case GameState.PlayerJoin:
                GUI.Label(banner, "PLAYER JOIN", title);
                int joined = UnityEngine.InputSystem.PlayerInput.all.Count;
                GUI.Label(sub, $"P1/P2 — {joined}/{TwoPlayerJoinController.MaxPlayers} joined (press any button)", body);
                break;

            case GameState.CharacterSelect:
                DrawCharacterSelect(title, body);
                break;

            case GameState.Opening:
                GUI.Label(banner, "VS", title);
                var fight = flow.ActiveFight;
                string stage = fight != null && fight.stage != null ? fight.stage.stageId : "stage";
                GUI.Label(sub, $"{CharName(fight, 0)}  vs  {CharName(fight, 1)}  —  {stage}", body);
                break;

            case GameState.RoundStart:
                GUI.Label(banner, flow.IsShowingReadyBanner ? "READY" : "FIGHT", title);
                break;

            case GameState.Fight:
                DrawFightHud();
                break;

            case GameState.MatchEnd:
                GUI.Label(banner, flow.MatchEndedByKnockout ? "K.O." : "TIME OVER", title);
                GUI.Label(sub, WinnerLabel(), body);
                break;

            case GameState.Results:
                GUI.Label(banner, "RESULTS", title);
                GUI.Label(sub, $"{WinnerLabel()}  —  Submit: Rematch · Cancel: Quit to Title", body);
                break;

            case GameState.Paused:
                GUI.Label(banner, "PAUSED", title);
                GUI.Label(sub, "Submit/Start: Resume · Cancel: Quit to Title", body);
                break;
        }
    }

    private void DrawCharacterSelect(GUIStyle title, GUIStyle body)
    {
        GUI.Label(new Rect(0, 40, Screen.width, 36), "CHARACTER SELECT", title);
        var lib = flow.CharacterLibrary;
        var sel = flow.SelectSession;
        if (lib == null || lib.Count == 0)
        {
            GUI.Label(new Rect(0, 90, Screen.width, 24), "No roster — using FightDefinition defaults", body);
            return;
        }

        string p1 = NameAt(lib, sel.p1Cursor) + (sel.p1Locked ? " [LOCK]" : "");
        string p2 = NameAt(lib, sel.p2Cursor) + (sel.p2Locked ? " [LOCK]" : "");
        GUI.Label(new Rect(0, 100, Screen.width, 24), $"P1 ◀ {p1} ▶", body);
        GUI.Label(new Rect(0, 130, Screen.width, 24), $"P2 ◀ {p2} ▶", body);
        GUI.Label(new Rect(0, 170, Screen.width, 24),
            "P1: A/D + F · P2: ←/→ + . · or pads D-pad + South", body);
    }

    private void DrawFightHud()
    {
        GUI.Label(new Rect(Screen.width * 0.5f - 40, 12, 80, 24),
            Mathf.CeilToInt(flow.RoundTimeRemaining).ToString());

        FighterHealth[] healths = FindObjectsByType<FighterHealth>(FindObjectsSortMode.None);
        float y = 40f;
        for (int i = 0; i < healths.Length; i++)
        {
            FighterHealth h = healths[i];
            if (h == null) continue;
            var pi = h.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            int slot = pi != null ? pi.playerIndex + 1 : i + 1;
            GUI.Label(new Rect(16, y, 420, 22),
                $"P{slot} HP {h.CurrentHealth}/{h.MaxHealth}" + (h.IsKnockedOut ? " — KO" : ""));
            y += 22f;
        }
    }

    private string WinnerLabel()
    {
        if (flow.WinnerPlayerIndex < 0)
            return "DRAW";
        return $"P{flow.WinnerPlayerIndex + 1} WINS";
    }

    private static string CharName(FightDefinition fight, int slot)
    {
        if (fight == null) return "?";
        CharacterDefinition c = slot == 0 ? fight.player1Character : fight.player2Character;
        return c != null ? c.displayName : "?";
    }

    private static string NameAt(CharacterLibrary lib, int index)
    {
        if (lib == null || lib.characters == null || index < 0 || index >= lib.characters.Length)
            return "?";
        CharacterDefinition c = lib.characters[index];
        return c != null ? c.displayName : "?";
    }
}
