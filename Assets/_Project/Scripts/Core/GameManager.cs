using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private DayManager dayManager;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private List<DayConfig> daySequence = new List<DayConfig>();
    [SerializeField, Min(1)] private int expectedDayCount = 5;
    [SerializeField] private bool autoStartGame = true;

    public GameState CurrentState { get; private set; } = GameState.Boot;
    public int CurrentDayIndex { get; private set; }
    public DaySummaryData LastSummary { get; private set; }

    private void Awake()
    {
        if (dayManager == null)
        {
            dayManager = FindObjectOfType<DayManager>();
        }

        if (economyManager == null)
        {
            economyManager = FindObjectOfType<EconomyManager>();
        }

        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
    }

    private void OnEnable()
    {
        if (dayManager != null)
        {
            dayManager.DayEnded += HandleDayEnded;
        }
    }

    private void OnDisable()
    {
        if (dayManager != null)
        {
            dayManager.DayEnded -= HandleDayEnded;
        }
    }

    private void Start()
    {
        if (autoStartGame)
        {
            BeginGame();
        }
    }

    public void BeginGame()
    {
        if (daySequence == null || daySequence.Count == 0)
        {
            Debug.LogWarning("GameManager nao tem dias configurados.");
            CurrentState = GameState.Boot;
            return;
        }

        if (expectedDayCount > 0 && daySequence.Count != expectedDayCount)
        {
            Debug.LogWarning(
                $"GameManager esta configurado para {daySequence.Count} dia(s), mas o jogo espera {expectedDayCount}. " +
                "Preencha a lista Day Sequence com os 5 DayConfig no Inspector.",
                this);
        }

        CurrentDayIndex = 0;
        CurrentState = GameState.DayActive;
        StartCurrentDay();
    }

    public void RestartGame()
    {
        if (economyManager != null)
        {
            economyManager.ResetRun();
        }

        CurrentDayIndex = 0;
        CurrentState = GameState.DayActive;
        StartCurrentDay();
    }

    public void ContinueAfterSummary()
    {
        if (CurrentState != GameState.DaySummary)
        {
            return;
        }

        if (LastSummary != null && LastSummary.endReason == DayEndReason.GameOver)
        {
            CurrentState = GameState.GameOver;
            return;
        }

        if (CurrentDayIndex + 1 >= daySequence.Count)
        {
            if (economyManager != null && economyManager.RemainingDebt <= 0)
            {
                CurrentState = GameState.Victory;
            }
            else
            {
                CurrentState = GameState.GameOver;
            }

            return;
        }

        CurrentDayIndex++;
        CurrentState = GameState.DayActive;
        StartCurrentDay();
    }

    private void StartCurrentDay()
    {
        if (dayManager == null || daySequence == null || CurrentDayIndex < 0 || CurrentDayIndex >= daySequence.Count)
        {
            return;
        }

        dayManager.BeginDay(daySequence[CurrentDayIndex]);
    }

    private void HandleDayEnded(DaySummaryData summary)
    {
        LastSummary = summary;
        CurrentState = GameState.DaySummary;

        if (uiManager != null && summary != null)
        {
            uiManager.ShowDaySummary(summary);
        }
    }
}
