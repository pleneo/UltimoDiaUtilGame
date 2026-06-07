using System.Text;
using TMPro;
using UnityEngine;

public class DaySummaryPanel : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    public void Configure(GameObject panelRoot, TMP_Text title, TMP_Text body)
    {
        root = panelRoot;
        titleText = title;
        bodyText = body;
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
}
