using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DaySummaryPanel : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text continueButtonText;

    public void Configure(GameObject panelRoot, TMP_Text title, TMP_Text body, Button button = null, TMP_Text buttonText = null)
    {
        root = panelRoot;
        titleText = title;
        bodyText = body;
        continueButton = button;
        continueButtonText = buttonText;
    }

    private void OnEnable()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueAfterSummary);
        }
    }

    private void OnDisable()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(ContinueAfterSummary);
        }
    }

    public void Show(DaySummaryData summary)
    {
        if (root != null)
        {
            root.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }

        if (summary == null)
        {
            if (titleText != null)
            {
                titleText.text = "Resumo do dia";
            }

            if (bodyText != null)
            {
                bodyText.text = "Sem dados para exibir.";
            }

            return;
        }

        RefreshContinueButton(summary);

        if (titleText != null)
        {
            titleText.text = summary.headline;
        }

        if (bodyText == null)
        {
            return;
        }

        var builder = new StringBuilder();
        builder.AppendLine(summary.details);
        builder.AppendLine();
        builder.AppendLine($"Casos: {summary.completedCases}/{summary.totalCases}");
        builder.AppendLine($"Acertos: {summary.correctDecisions}");
        builder.AppendLine($"Erros: {summary.incorrectDecisions}");
        builder.AppendLine($"Ganhos por acertos: +{summary.dailyGrossPay}");
        builder.AppendLine($"Multas por erros: -{summary.dailyPenalty}");
        builder.AppendLine($"Despesas do dia: -{summary.dailyExpenses}");
        builder.AppendLine($"Pagamento da divida: -{summary.dailyDebtPayment}");
        builder.AppendLine($"Saldo liquido do dia: {summary.dailyNetBalance}");

        if (summary.economySnapshot != null)
        {
            builder.AppendLine($"Dinheiro final: {summary.economySnapshot.currentMoney}");
            builder.AppendLine($"Divida restante: {summary.economySnapshot.remainingDebt}");
            builder.AppendLine($"Advertencias: {summary.economySnapshot.warnings}/{summary.economySnapshot.warningLimit}");
        }

        if (summary.notes != null && summary.notes.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Notas:");
            for (var index = 0; index < summary.notes.Count; index++)
            {
                var note = summary.notes[index];
                if (string.IsNullOrWhiteSpace(note))
                {
                    continue;
                }

                builder.AppendLine($"- {note}");
            }
        }

        bodyText.text = builder.ToString().TrimEnd();
    }

    private void RefreshContinueButton(DaySummaryData summary)
    {
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
        }

        if (continueButtonText == null || summary == null)
        {
            return;
        }

        continueButtonText.text = summary.endReason == DayEndReason.GameOver
            ? "Encerrar"
            : "Proximo dia";
    }

    public void Hide()
    {
        if (root != null)
        {
            root.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void ContinueAfterSummary()
    {
        var gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.ContinueAfterSummary();
        }
    }
}
