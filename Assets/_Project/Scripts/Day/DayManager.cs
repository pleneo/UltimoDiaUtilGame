using System;
using System.Collections.Generic;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    [SerializeField] private CaseManager caseManager;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private bool waitForPlayerToAdvanceCase = true;

    public event Action<DaySummaryData> DayEnded;

    public DayConfig CurrentDayConfig { get; private set; }
    public float RemainingTimeSeconds { get; private set; }
    public int CurrentCaseIndex { get; private set; }
    public int ResolvedCasesCount { get; private set; }
    public int CorrectDecisions { get; private set; }
    public int IncorrectDecisions { get; private set; }
    public bool IsDayActive { get; private set; }
    public bool IsWaitingForNextCase { get; private set; }

    private readonly List<StudentCaseDefinition> currentCases = new List<StudentCaseDefinition>();

    private void Awake()
    {
        if (caseManager == null)
        {
            caseManager = FindObjectOfType<CaseManager>();
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
        if (caseManager != null)
        {
            caseManager.CaseResolved += HandleCaseResolved;
        }
    }

    private void OnDisable()
    {
        if (caseManager != null)
        {
            caseManager.CaseResolved -= HandleCaseResolved;
        }
    }

    private void Update()
    {
        if (!IsDayActive || CurrentDayConfig == null)
        {
            return;
        }

        RemainingTimeSeconds -= Time.deltaTime;
        if (RemainingTimeSeconds <= 0f)
        {
            RemainingTimeSeconds = 0f;
            EndDay(DayEndReason.TimeExpired);
            return;
        }

        PushHudUpdate();
    }

    public void BeginDay(DayConfig dayConfig)
    {
        if (dayConfig == null)
        {
            Debug.LogWarning("DayManager.BeginDay foi chamado sem DayConfig.");
            return;
        }

        CurrentDayConfig = dayConfig;
        RemainingTimeSeconds = Mathf.Max(0f, dayConfig.workDurationSeconds);
        CurrentCaseIndex = 0;
        ResolvedCasesCount = 0;
        CorrectDecisions = 0;
        IncorrectDecisions = 0;
        IsDayActive = true;
        IsWaitingForNextCase = false;

        currentCases.Clear();
        if (dayConfig.cases != null)
        {
            currentCases.AddRange(dayConfig.cases);
        }

        if (economyManager != null)
        {
            economyManager.ApplyEconomyConfig(dayConfig.economyConfig);
        }

        if (uiManager != null)
        {
            uiManager.BindDay(dayConfig);
            uiManager.HideDaySummary();
        }

        PresentCurrentCase();
        PushHudUpdate();
    }

    public void EndDay(DayEndReason reason)
    {
        if (!IsDayActive)
        {
            return;
        }

        IsDayActive = false;
        IsWaitingForNextCase = false;

        if (caseManager != null)
        {
            caseManager.ClearCurrentCase();
        }

        if (economyManager != null)
        {
            economyManager.FinalizeDay();
        }

        var summary = BuildSummary(reason);

        DayEnded?.Invoke(summary);
    }

    private void PresentCurrentCase()
    {
        if (!IsDayActive || caseManager == null)
        {
            return;
        }

        if (CurrentCaseIndex >= currentCases.Count)
        {
            EndDay(DayEndReason.Completed);
            return;
        }

        var currentCase = currentCases[CurrentCaseIndex];
        caseManager.LoadCase(currentCase);

        if (uiManager != null)
        {
            uiManager.ShowCurrentCase(currentCase);
        }
    }

    private void HandleCaseResolved(CaseResolutionResult result)
    {
        if (!IsDayActive || result == null)
        {
            return;
        }

        if (result.isCorrectDecision)
        {
            CorrectDecisions++;
        }
        else
        {
            IncorrectDecisions++;
        }

        ResolvedCasesCount++;

        if (economyManager != null)
        {
            economyManager.ApplyCaseResolution(result);
            if (economyManager.IsGameOver)
            {
                EndDay(DayEndReason.GameOver);
                return;
            }
        }

        CurrentCaseIndex++;
        if (waitForPlayerToAdvanceCase)
        {
            IsWaitingForNextCase = true;
        }
        else
        {
            PresentCurrentCase();
        }

        PushHudUpdate();
    }

    public void ContinueToNextCase()
    {
        if (!IsDayActive)
        {
            Debug.LogWarning("[DayManager] Nao foi possivel avancar para o proximo aluno porque o dia nao esta ativo.");
            return;
        }

        if (!IsWaitingForNextCase)
        {
            Debug.LogWarning("[DayManager] Nao foi possivel avancar para o proximo aluno porque o jogo nao esta aguardando avancar caso.");
            return;
        }

        IsWaitingForNextCase = false;
        Debug.Log("[DayManager] Avancando para o proximo aluno.");
        PresentCurrentCase();
        PushHudUpdate();
    }

    private DaySummaryData BuildSummary(DayEndReason reason)
    {
        var summary = new DaySummaryData
        {
            dayNumber = CurrentDayConfig != null ? CurrentDayConfig.dayNumber : 1,
            dayLabel = CurrentDayConfig != null ? CurrentDayConfig.dayLabel : "Dia",
            totalCases = currentCases.Count,
            completedCases = Mathf.Clamp(ResolvedCasesCount, 0, currentCases.Count),
            correctDecisions = CorrectDecisions,
            incorrectDecisions = IncorrectDecisions,
            workDurationSeconds = CurrentDayConfig != null ? CurrentDayConfig.workDurationSeconds : 0f,
            remainingTimeSeconds = RemainingTimeSeconds,
            endReason = reason,
            economySnapshot = economyManager != null ? economyManager.GetSnapshot() : new EconomySnapshot()
        };

        summary.headline = reason == DayEndReason.Completed
            ? "Fim do dia"
            : reason == DayEndReason.TimeExpired
                ? "Tempo esgotado"
                : "Encerrado por erro";

        summary.details = reason == DayEndReason.Completed
            ? "O expediente foi encerrado sem interrupcoes."
            : reason == DayEndReason.TimeExpired
                ? "O horario acabou antes de concluir todos os casos."
                : "A sessão terminou por limite economico ou disciplinar.";

        if (CurrentDayConfig != null && !string.IsNullOrWhiteSpace(CurrentDayConfig.dayIntro))
        {
            summary.notes.Add(CurrentDayConfig.dayIntro);
        }

        return summary;
    }

    private void PushHudUpdate()
    {
        if (uiManager == null)
        {
            return;
        }

        var totalTime = CurrentDayConfig != null ? CurrentDayConfig.workDurationSeconds : 0f;
        var snapshot = economyManager != null ? economyManager.GetSnapshot() : new EconomySnapshot();
        uiManager.RefreshHUD(snapshot, RemainingTimeSeconds, totalTime);
    }
}
