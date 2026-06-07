using TMPro;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    [SerializeField] private TMP_Text currentCaseText;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text debtText;
    [SerializeField] private TMP_Text warningsText;
    [SerializeField] private TMP_Text timerText;

    public void Configure(
        TMP_Text currentCase,
        TMP_Text money,
        TMP_Text debt,
        TMP_Text warnings,
        TMP_Text timer)
    {
        currentCaseText = currentCase;
        moneyText = money;
        debtText = debt;
        warningsText = warnings;
        timerText = timer;
    }

    public void SetCaseInfo(StudentCaseDefinition caseDefinition)
    {
        if (currentCaseText == null)
        {
            return;
        }

        currentCaseText.text = caseDefinition != null
            ? $"{caseDefinition.caseTitle}\n{caseDefinition.applicantName}"
            : "Nenhum caso ativo";
    }

    public void Refresh(EconomySnapshot snapshot, float remainingTimeSeconds, float totalTimeSeconds)
    {
        if (snapshot == null)
        {
            return;
        }

        if (moneyText != null)
        {
            moneyText.text = $"Dinheiro: {snapshot.currentMoney}";
        }

        if (debtText != null)
        {
            debtText.text = $"Divida: {snapshot.remainingDebt}";
        }

        if (warningsText != null)
        {
            warningsText.text = $"Advertencias: {snapshot.warnings}/{snapshot.warningLimit}";
        }

        if (timerText != null)
        {
            var safeTotal = Mathf.Max(0.01f, totalTimeSeconds);
            var percent = Mathf.Clamp01(remainingTimeSeconds / safeTotal) * 100f;
            timerText.text = $"Tempo: {Mathf.CeilToInt(remainingTimeSeconds)}s ({percent:0}%)";
        }
    }
}
