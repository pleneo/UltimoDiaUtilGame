using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class NoticeBoardPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    public void SetEntries(IReadOnlyList<NoticeBoardEntry> entries)
    {
        if (titleText != null)
        {
            titleText.text = "Quadro de Avisos";
        }

        if (bodyText == null)
        {
            return;
        }

        if (entries == null || entries.Count == 0)
        {
            bodyText.text = "Sem avisos para encaminhamento.";
            return;
        }

        var builder = new StringBuilder();
        for (var index = 0; index < entries.Count; index++)
        {
            var entry = entries[index];
            if (entry == null)
            {
                continue;
            }

            builder.AppendLine($"{index + 1}. {entry.entryTitle}");
            if (!string.IsNullOrWhiteSpace(entry.entryBody))
            {
                builder.AppendLine(entry.entryBody);
            }

            if (entry.requiresForward)
            {
                builder.AppendLine("Encaminhamento obrigatorio.");
            }

            builder.AppendLine();
        }

        bodyText.text = builder.ToString().TrimEnd();
    }
}
