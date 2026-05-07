using System;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    [SerializeField] private EconomyConfig defaultEconomyConfig;

    public event Action<EconomySnapshot> EconomyChanged;

    public int CurrentMoney { get; private set; }
    public int RemainingDebt { get; private set; }
    public int Warnings { get; private set; }
    public int WarningLimit { get; private set; }
    public int PayPerCorrectDecision { get; private set; }
    public int PenaltyPerMistake { get; private set; }
    public int DailyExpenses { get; private set; }
    public bool AutoPayDebtFromMoney { get; private set; }
    public bool IsInitialized { get; private set; }

    public bool IsGameOver => CurrentMoney < 0 || Warnings >= WarningLimit;
    public string GameOverReason
    {
        get
        {
            if (CurrentMoney < 0)
            {
                return "Dinheiro negativo.";
            }

            if (Warnings >= WarningLimit)
            {
                return "Limite de advertencias atingido.";
            }

            return string.Empty;
        }
    }

    private void Awake()
    {
    }

    public void BeginRun(EconomyConfig config)
    {
        ApplyEconomyConfig(config != null ? config : defaultEconomyConfig, true);
    }

    public void ApplyEconomyConfig(EconomyConfig config)
    {
        ApplyEconomyConfig(config != null ? config : defaultEconomyConfig, false);
    }

    public void ResetRun()
    {
        if (defaultEconomyConfig != null)
        {
            ApplyEconomyConfig(defaultEconomyConfig, true);
        }
        else
        {
            ApplyFallbackEconomyConfig(true);
        }
    }

    public void ApplyCaseResolution(CaseResolutionResult result)
    {
        if (result == null)
        {
            return;
        }

        if (result.isCorrectDecision)
        {
            CurrentMoney += PayPerCorrectDecision;
        }
        else
        {
            CurrentMoney -= PenaltyPerMistake;
            Warnings += Mathf.Max(1, result.warningDelta);
        }

        NotifyChanged();
    }

    public void FinalizeDay()
    {
        CurrentMoney -= DailyExpenses;

        if (AutoPayDebtFromMoney && CurrentMoney > 0 && RemainingDebt > 0)
        {
            var payment = Mathf.Min(CurrentMoney, RemainingDebt);
            RemainingDebt -= payment;
            CurrentMoney -= payment;
        }

        NotifyChanged();
    }

    public EconomySnapshot GetSnapshot()
    {
        return new EconomySnapshot
        {
            currentMoney = CurrentMoney,
            remainingDebt = RemainingDebt,
            warnings = Warnings,
            warningLimit = WarningLimit,
            payPerCorrectDecision = PayPerCorrectDecision,
            penaltyPerMistake = PenaltyPerMistake,
            dailyExpenses = DailyExpenses,
            isGameOver = IsGameOver,
            gameOverReason = GameOverReason
        };
    }

    private void ApplyEconomyConfig(EconomyConfig config, bool resetMoneyAndDebt)
    {
        if (config == null)
        {
            if (defaultEconomyConfig != null)
            {
                config = defaultEconomyConfig;
            }
            else
            {
                ApplyFallbackEconomyConfig(resetMoneyAndDebt);
                return;
            }
        }

        PayPerCorrectDecision = config.payPerCorrectDecision;
        PenaltyPerMistake = config.penaltyPerMistake;
        DailyExpenses = config.dailyExpenses;
        WarningLimit = config.warningLimit;
        AutoPayDebtFromMoney = config.autoPayDebtFromMoney;

        if (resetMoneyAndDebt || !IsInitialized)
        {
            CurrentMoney = config.initialMoney;
            RemainingDebt = config.initialDebt;
            Warnings = 0;
            IsInitialized = true;
        }

        NotifyChanged();
    }

    private void ApplyFallbackEconomyConfig(bool resetMoneyAndDebt)
    {
        PayPerCorrectDecision = 10;
        PenaltyPerMistake = 5;
        DailyExpenses = 0;
        WarningLimit = 3;
        AutoPayDebtFromMoney = true;

        if (resetMoneyAndDebt || !IsInitialized)
        {
            CurrentMoney = 0;
            RemainingDebt = 0;
            Warnings = 0;
            IsInitialized = true;
        }

        NotifyChanged();
    }

    private void NotifyChanged()
    {
        EconomyChanged?.Invoke(GetSnapshot());
    }
}
