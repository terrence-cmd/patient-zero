using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Match-flow spine: Boot → Title → Join → Character Select → Opening/VS →
/// Round Start → Fight → Match End → Results, plus Pause overlay on Fight only.
///
/// Timings and state names follow the Claude Design Game Flow PDF.
/// Reuses existing combat join pieces (<see cref="FightDirector"/>,
/// <see cref="TwoPlayerJoinController"/>, libraries) without replacing Gate 0
/// cold-start into Main — open Main directly and fight still works without this.
///
/// Scene names: "Boot", "Title", "Main". Character select is an in-flow overlay
/// (no separate scene yet). Design left additive Select scene as a later option.
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameFlowDirector : MonoBehaviour
{
    public const string BootSceneName = "Boot";
    public const string TitleSceneName = "Title";
    public const string MainSceneName = "Main";

    public static GameFlowDirector Instance { get; private set; }

    [Header("Data")]
    [SerializeField] private CharacterLibrary characterLibrary;
    [SerializeField] private FightDefinition baseFight;

    [Header("Debug / stub")]
    [SerializeField] private bool drawHud = true;
    [SerializeField] private bool autoConfirmCharacterSelectInEditor;

    private GameState state = GameState.Boot;
    private GameState stateBeforePause = GameState.Fight;
    private float stateEnteredAt;
    private float roundTimeRemaining;
    private int winnerPlayerIndex = -1; // 0/1, or -1 time-over draw
    private bool matchEndedByKnockout;
    private bool mainSceneLoaded;
    private bool mainSceneLoading;
    private bool gameplayInputEnabled;
    private CharacterSelectSession selectSession = new CharacterSelectSession();
    private FightDefinition runtimeFight;
    private Coroutine stateRoutine;

    public GameState State => state;
    public float RoundTimeRemaining => roundTimeRemaining;
    public int WinnerPlayerIndex => winnerPlayerIndex;
    public bool MatchEndedByKnockout => matchEndedByKnockout;
    public bool GameplayInputEnabled => gameplayInputEnabled;
    public CharacterSelectSession SelectSession => selectSession;
    public CharacterLibrary CharacterLibrary => characterLibrary;
    public FightDefinition ActiveFight => runtimeFight != null ? runtimeFight : baseFight;
    public bool DrawHud => drawHud;
    public float StateElapsedUnscaled => Time.unscaledTime - stateEnteredAt;
    public bool IsShowingReadyBanner =>
        state == GameState.RoundStart && StateElapsedUnscaled < GameFlowTimings.ReadySeconds;

    /// <summary>
    /// True while the flow spine owns match lifecycle (not a Gate 0 Main-only session).
    /// </summary>
    public bool IsFlowDriven => isActiveAndEnabled;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (GetComponent<GameFlowHud>() == null)
            gameObject.AddComponent<GameFlowHud>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        // If someone dropped this into Main for debugging, still start at Boot
        // semantics only when Boot is the active scene; otherwise assume Title+.
        string active = SceneManager.GetActiveScene().name;
        if (active == BootSceneName || active == "Boot")
            EnterState(GameState.Boot);
        else if (active == TitleSceneName)
            EnterState(GameState.Title);
        else
            EnterState(GameState.Boot);
    }

    private void Update()
    {
        switch (state)
        {
            case GameState.Title:
                if (WasSubmitPressed())
                    EnterState(GameState.PlayerJoin);
                break;

            case GameState.PlayerJoin:
                if (CountJoinedPlayers() >= TwoPlayerJoinController.MaxPlayers)
                    EnterState(GameState.CharacterSelect);
                else if (!mainSceneLoaded)
                    EnsureMainLoadedForJoin();
                break;

            case GameState.CharacterSelect:
                TickCharacterSelectInput();
                if (selectSession.BothLocked)
                    EnterState(GameState.Opening);
                break;

            case GameState.Opening:
                if (Time.unscaledTime - stateEnteredAt >= GameFlowTimings.OpeningSkipAfterSeconds
                    && WasSubmitPressed())
                    EnterState(GameState.RoundStart);
                break;

            case GameState.Fight:
                TickFight();
                if (WasPausePressed())
                    EnterPause();
                break;

            case GameState.Paused:
                if (WasPausePressed() || WasSubmitPressed())
                    ResumeFromPause();
                else if (WasCancelPressed())
                    QuitToTitle();
                break;

            case GameState.Results:
                if (WasSubmitPressed())
                    Rematch();
                else if (WasCancelPressed())
                    QuitToTitle();
                break;
        }

        // Joined players enable their Player map on spawn — reassert lock outside Fight.
        if (!gameplayInputEnabled)
            ApplyGameplayInputMaps(false);
    }

    public void BindLibraries(CharacterLibrary roster, FightDefinition fight)
    {
        if (roster != null)
            characterLibrary = roster;
        if (fight != null)
            baseFight = fight;
    }

    public void RequestPauseFromDeviceLoss()
    {
        if (state == GameState.Fight)
            EnterPause();
    }

    public void SetGameplayInputEnabled(bool enabled)
    {
        gameplayInputEnabled = enabled;
        ApplyGameplayInputMaps(enabled);
    }

    private void EnterState(GameState next)
    {
        if (stateRoutine != null)
        {
            StopCoroutine(stateRoutine);
            stateRoutine = null;
        }

        // Leaving pause restores timescale elsewhere.
        if (state == GameState.Paused && next != GameState.Paused)
            Time.timeScale = 1f;

        state = next;
        stateEnteredAt = Time.unscaledTime;
        Debug.Log($"[Flow] → {state}");

        switch (state)
        {
            case GameState.Boot:
                SetGameplayInputEnabled(false);
                stateRoutine = StartCoroutine(BootRoutine());
                break;

            case GameState.Title:
                SetGameplayInputEnabled(false);
                LoadSingle(TitleSceneName);
                mainSceneLoaded = false;
                mainSceneLoading = false;
                break;

            case GameState.PlayerJoin:
                SetGameplayInputEnabled(false);
                selectSession.Reset();
                EnsureMainLoadedForJoin();
                break;

            case GameState.CharacterSelect:
                SetGameplayInputEnabled(false);
                selectSession.Reset();
                if (characterLibrary == null || characterLibrary.Count == 0)
                {
                    Debug.LogWarning("[Flow] No CharacterLibrary — auto-locking defaults from FightDefinition.");
                    ApplyDefaultsFromBaseFight();
                    selectSession.p1Locked = true;
                    selectSession.p2Locked = true;
                }
#if UNITY_EDITOR
                else if (autoConfirmCharacterSelectInEditor)
                {
                    selectSession.p1Locked = true;
                    selectSession.p2Locked = true;
                }
#endif
                break;

            case GameState.Opening:
                SetGameplayInputEnabled(false);
                ApplySelectedCharactersToRuntimeFight();
                EnsureMainLoadedForJoin();
                var director = FindFightDirector();
                if (director != null)
                {
                    director.SetFight(ActiveFight);
                    director.ApplyStagePresentationPublic();
                }
                stateRoutine = StartCoroutine(TimedAdvance(GameFlowTimings.OpeningSeconds, GameState.RoundStart));
                break;

            case GameState.RoundStart:
                SetGameplayInputEnabled(false);
                winnerPlayerIndex = -1;
                matchEndedByKnockout = false;
                roundTimeRemaining = GameFlowTimings.RoundClockSeconds;
                ReconfigureFightersForRound();
                stateRoutine = StartCoroutine(RoundStartRoutine());
                break;

            case GameState.Fight:
                SetGameplayInputEnabled(true);
                break;

            case GameState.MatchEnd:
                SetGameplayInputEnabled(false);
                stateRoutine = StartCoroutine(TimedAdvance(GameFlowTimings.MatchEndFreezeSeconds, GameState.Results));
                break;

            case GameState.Results:
                SetGameplayInputEnabled(false);
                break;

            case GameState.Paused:
                break;
        }
    }

    private IEnumerator BootRoutine()
    {
        // Boot scene should already be active when launched from build index 0.
        if (SceneManager.GetActiveScene().name != BootSceneName)
            LoadSingle(BootSceneName);

        yield return new WaitForSecondsRealtime(GameFlowTimings.BootSeconds);
        EnterState(GameState.Title);
    }

    private IEnumerator RoundStartRoutine()
    {
        yield return new WaitForSecondsRealtime(GameFlowTimings.ReadySeconds);
        yield return new WaitForSecondsRealtime(GameFlowTimings.FightBannerSeconds);
        EnterState(GameState.Fight);
    }

    private IEnumerator TimedAdvance(float seconds, GameState next)
    {
        yield return new WaitForSecondsRealtime(seconds);
        EnterState(next);
    }

    private void TickFight()
    {
        roundTimeRemaining -= Time.deltaTime;
        if (roundTimeRemaining < 0f)
            roundTimeRemaining = 0f;

        if (TryResolveKnockout(out int koWinner))
        {
            winnerPlayerIndex = koWinner;
            matchEndedByKnockout = true;
            EnterState(GameState.MatchEnd);
            return;
        }

        if (roundTimeRemaining <= 0f)
        {
            winnerPlayerIndex = ResolveTimeOverWinner();
            matchEndedByKnockout = false;
            EnterState(GameState.MatchEnd);
        }
    }

    private void EnterPause()
    {
        stateBeforePause = GameState.Fight;
        state = GameState.Paused;
        stateEnteredAt = Time.unscaledTime;
        Time.timeScale = 0f;
        SetGameplayInputEnabled(false);
        Debug.Log("[Flow] → Paused");
    }

    private void ResumeFromPause()
    {
        Time.timeScale = 1f;
        EnterState(stateBeforePause);
    }

    private void Rematch()
    {
        // PDF: rematch → Round Start; same stage; re-Configure.
        EnterState(GameState.RoundStart);
    }

    private void QuitToTitle()
    {
        Time.timeScale = 1f;
        DestroyJoinedPlayers();
        mainSceneLoaded = false;
        mainSceneLoading = false;
        EnterState(GameState.Title);
    }

    private void EnsureMainLoadedForJoin()
    {
        if (mainSceneLoaded || mainSceneLoading)
            return;

        if (SceneManager.GetActiveScene().name == MainSceneName)
        {
            mainSceneLoaded = true;
            var existing = FindFightDirector();
            if (existing != null && ActiveFight != null)
            {
                existing.SetFight(ActiveFight);
                existing.ApplyStagePresentationPublic();
            }
            return;
        }

        mainSceneLoading = true;
        // Load Main under the flow (Opening/Join). Keep director alive across loads.
        var op = SceneManager.LoadSceneAsync(MainSceneName, LoadSceneMode.Single);
        if (op != null)
        {
            op.completed += _ =>
            {
                mainSceneLoaded = true;
                mainSceneLoading = false;
                var director = FindFightDirector();
                if (director != null && ActiveFight != null)
                {
                    director.SetFight(ActiveFight);
                    director.ApplyStagePresentationPublic();
                }
            };
        }
        else
        {
            mainSceneLoaded = true;
            mainSceneLoading = false;
        }
    }

    private void LoadSingle(string sceneName)
    {
        if (SceneManager.GetActiveScene().name == sceneName)
            return;

        // Scene may not exist yet before GameFlowSetup runs in Editor.
        if (Application.CanStreamedLevelBeLoaded(sceneName))
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        else
            Debug.LogWarning($"[Flow] Scene '{sceneName}' not in build settings yet. Run Patient Zero / Flow / Setup Game Flow.");
    }

    private void TickCharacterSelectInput()
    {
        if (characterLibrary == null || characterLibrary.Count == 0)
            return;

        // Stub: keyboard/gamepad navigate + confirm. Full UI map comes later.
        if (WasLeftPressed(0)) selectSession.MoveCursor(0, -1, characterLibrary.Count);
        if (WasRightPressed(0)) selectSession.MoveCursor(0, 1, characterLibrary.Count);
        if (WasConfirmPressed(0)) selectSession.Confirm(0);

        if (WasLeftPressed(1)) selectSession.MoveCursor(1, -1, characterLibrary.Count);
        if (WasRightPressed(1)) selectSession.MoveCursor(1, 1, characterLibrary.Count);
        if (WasConfirmPressed(1)) selectSession.Confirm(1);

        // Allow a single device to lock P2 after P1 for solo stub testing.
        if (selectSession.p1Locked && !selectSession.p2Locked && WasConfirmPressed(0)
            && Keyboard.current != null && Keyboard.current.digit2Key.wasPressedThisFrame)
            selectSession.Confirm(1);
    }

    private void ApplySelectedCharactersToRuntimeFight()
    {
        FightDefinition source = baseFight;
        if (source == null)
        {
            Debug.LogError("[Flow] No base FightDefinition assigned.");
            return;
        }

        // Runtime copy so we don't mutate the project asset when players pick chars.
        runtimeFight = Instantiate(source);
        runtimeFight.name = source.name + " (Runtime)";

        CharacterDefinition p1 = selectSession.Resolve(characterLibrary, 0) ?? source.player1Character;
        CharacterDefinition p2 = selectSession.Resolve(characterLibrary, 1) ?? source.player2Character;
        runtimeFight.player1Character = p1;
        runtimeFight.player2Character = p2;
    }

    private void ApplyDefaultsFromBaseFight()
    {
        // No roster — Openings uses FightDefinition characters as-is.
        runtimeFight = baseFight != null ? Instantiate(baseFight) : null;
    }

    private void ReconfigureFightersForRound()
    {
        var director = FindFightDirector();
        if (director == null)
            return;

        director.SetFight(ActiveFight);
        director.ApplyStagePresentationPublic();
        director.ReconfigureAllJoinedFighters();
    }

    private static FightDirector FindFightDirector() =>
        FindFirstObjectByType<FightDirector>();

    private static int CountJoinedPlayers() => PlayerInput.all.Count;

    private static void DestroyJoinedPlayers()
    {
        // Copy because Destroy mutates PlayerInput.all.
        var players = new PlayerInput[PlayerInput.all.Count];
        for (int i = 0; i < PlayerInput.all.Count; i++)
            players[i] = PlayerInput.all[i];
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null)
                Destroy(players[i].gameObject);
        }
    }

    private static bool TryResolveKnockout(out int winnerPlayerIndex)
    {
        winnerPlayerIndex = -1;
        FighterHealth[] healths = FindObjectsByType<FighterHealth>(FindObjectsSortMode.None);
        if (healths == null || healths.Length < 2)
            return false;

        FighterHealth ko = null;
        FighterHealth alive = null;
        int koCount = 0;
        for (int i = 0; i < healths.Length; i++)
        {
            if (healths[i] == null)
                continue;
            if (healths[i].IsKnockedOut)
            {
                ko = healths[i];
                koCount++;
            }
            else
                alive = healths[i];
        }

        if (koCount == 0 || alive == null)
            return false;

        var pi = alive.GetComponent<PlayerInput>();
        winnerPlayerIndex = pi != null ? pi.playerIndex : -1;
        return winnerPlayerIndex >= 0;
    }

    private static int ResolveTimeOverWinner()
    {
        FighterHealth[] healths = FindObjectsByType<FighterHealth>(FindObjectsSortMode.None);
        if (healths == null || healths.Length == 0)
            return -1;

        FighterHealth best = null;
        bool tie = false;
        for (int i = 0; i < healths.Length; i++)
        {
            FighterHealth h = healths[i];
            if (h == null)
                continue;
            if (best == null)
            {
                best = h;
                continue;
            }

            if (h.CurrentHealth > best.CurrentHealth)
            {
                best = h;
                tie = false;
            }
            else if (h.CurrentHealth == best.CurrentHealth)
            {
                tie = true;
            }
        }

        if (tie || best == null)
            return -1;

        var pi = best.GetComponent<PlayerInput>();
        return pi != null ? pi.playerIndex : -1;
    }

    private static void ApplyGameplayInputMaps(bool enabled)
    {
        foreach (var player in PlayerInput.all)
        {
            if (player == null)
                continue;
            InputActionMap map = player.currentActionMap;
            if (map == null && player.actions != null)
                map = player.actions.FindActionMap("Player");
            if (map == null)
                continue;

            if (enabled && !map.enabled)
                map.Enable();
            else if (!enabled && map.enabled)
                map.Disable();
        }
    }

    // --- Stub input helpers (UI action map not wired yet) ---

    private static bool WasSubmitPressed()
    {
        if (Keyboard.current != null &&
            (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
            return true;
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            return true;
        return false;
    }

    private static bool WasCancelPressed()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            return true;
        if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
            return true;
        return false;
    }

    private static bool WasPausePressed()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            return true;
        if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
            return true;
        return false;
    }

    private static bool WasLeftPressed(int slot)
    {
        if (slot == 0 && Keyboard.current != null && Keyboard.current.aKey.wasPressedThisFrame)
            return true;
        if (slot == 1 && Keyboard.current != null && Keyboard.current.leftArrowKey.wasPressedThisFrame)
            return true;
        Gamepad pad = GamepadForSlot(slot);
        return pad != null && pad.dpad.left.wasPressedThisFrame;
    }

    private static bool WasRightPressed(int slot)
    {
        if (slot == 0 && Keyboard.current != null && Keyboard.current.dKey.wasPressedThisFrame)
            return true;
        if (slot == 1 && Keyboard.current != null && Keyboard.current.rightArrowKey.wasPressedThisFrame)
            return true;
        Gamepad pad = GamepadForSlot(slot);
        return pad != null && pad.dpad.right.wasPressedThisFrame;
    }

    private static bool WasConfirmPressed(int slot)
    {
        if (slot == 0 && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            return true;
        if (slot == 1 && Keyboard.current != null && Keyboard.current.periodKey.wasPressedThisFrame)
            return true;
        Gamepad pad = GamepadForSlot(slot);
        return pad != null && pad.buttonSouth.wasPressedThisFrame;
    }

    private static Gamepad GamepadForSlot(int slot)
    {
        var pads = Gamepad.all;
        if (pads.Count == 0)
            return null;
        if (slot < pads.Count)
            return pads[slot];
        return pads[0];
    }
}
