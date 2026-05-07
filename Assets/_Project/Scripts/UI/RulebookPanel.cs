using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class RulebookPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    public void SetRules(IReadOnlyList<RuleDefinition> rules)
    {
        if (titleText != null)
        {
            titleText.text = "Livro de Regras";
        }

        if (bodyText == null)
        {
            return;
        }

        if (rules == null || rules.Count == 0)
        {
            bodyText.text = "Sem regras cadastradas para este dia.";
            return;
        }

        var builder = new StringBuilder();
        for (var index = 0; index < rules.Count; index++)
        {
            var rule = rules[index];
            if (rule == null)
            {
                continue;
            }

            builder.AppendLine($"{index + 1}. {rule.ruleTitle}");
            if (!string.IsNullOrWhiteSpace(rule.ruleBody))
            {
                builder.AppendLine(rule.ruleBody);
            }

            builder.AppendLine();
        }

        bodyText.text = builder.ToString().TrimEnd();
    }
}
