using System.Text;
using TMPro;
using UnityEngine;

public class CaseDebugPanel : MonoBehaviour
{
    [SerializeField] private CaseManager caseManager;
    [SerializeField] private DocumentManager documentManager;
    [SerializeField] private DayManager dayManager;
    [SerializeField] private TMP_Text debugText;

    private string lastResultMessage = "Nenhuma decisao enviada ainda.";

    private void Awake()
    {
        if (caseManager == null)
        {
            caseManager = FindObjectOfType<CaseManager>();
        }

        if (documentManager == null)
        {
            documentManager = FindObjectOfType<DocumentManager>();
        }

        if (dayManager == null)
        {
            dayManager = FindObjectOfType<DayManager>();
        }

        if (debugText == null)
        {
            debugText = GetComponentInChildren<TMP_Text>();
        }
    }

    private void OnEnable()
    {
        if (caseManager != null)
        {
            caseManager.CaseLoaded += HandleCaseLoaded;
            caseManager.CaseResolved += HandleCaseResolved;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (caseManager != null)
        {
            caseManager.CaseLoaded -= HandleCaseLoaded;
            caseManager.CaseResolved -= HandleCaseResolved;
        }
    }

    private void HandleCaseLoaded(StudentCaseDefinition caseDefinition)
    {
        lastResultMessage = "Aguardando decisao.";
        Refresh();
    }

    private void HandleCaseResolved(CaseResolutionResult result)
    {
        if (result == null)
        {
            lastResultMessage = "Resultado indisponivel.";
            Refresh();
            return;
        }

        var status = result.isCorrectDecision ? "CORRETA" : "INCORRETA";
        var expected = result.validationResult != null
            ? result.validationResult.recommendedDecision.ToString()
            : "Indisponivel";

        lastResultMessage = $"Ultima decisao: {result.chosenDecision} | Esperado: {expected} | {status}";
        Refresh(result.caseDefinition, result.validationResult);
    }

    public void Refresh()
    {
        var currentCase = caseManager != null ? caseManager.CurrentCase : null;
        var validation = currentCase != null
            ? CaseValidator.Evaluate(currentCase, documentManager != null ? documentManager.CurrentDocuments : currentCase.documents)
            : null;

        Refresh(currentCase, validation);
    }

    private void Refresh(StudentCaseDefinition currentCase, CaseValidationResult validation)
    {
        if (debugText == null)
        {
            return;
        }

        var builder = new StringBuilder();
        builder.AppendLine("DEBUG MATRICULA");
        builder.AppendLine();

        if (currentCase == null)
        {
            builder.AppendLine("Caso atual: nenhum");
            builder.AppendLine(lastResultMessage);
            debugText.text = builder.ToString();
            return;
        }

        builder.AppendLine($"Caso: {currentCase.caseTitle}");
        builder.AppendLine($"Aluno: {currentCase.applicantName}");
        builder.AppendLine($"Tipo: {currentCase.requestType}");
        builder.AppendLine($"Decisao recomendada: {(validation != null ? validation.recommendedDecision.ToString() : "Indisponivel")}");
        builder.AppendLine();
        builder.AppendLine("Documentos entregues:");

        var documents = documentManager != null ? documentManager.CurrentDocuments : currentCase.documents;
        if (documents == null || documents.Count == 0)
        {
            builder.AppendLine("- Nenhum documento entregue.");
        }
        else
        {
            for (var index = 0; index < documents.Count; index++)
            {
                var document = documents[index];
                if (document == null)
                {
                    continue;
                }

                builder.AppendLine($"- {document.GetDisplayName()}");
                if (document.fields != null)
                {
                    for (var fieldIndex = 0; fieldIndex < document.fields.Count; fieldIndex++)
                    {
                        var field = document.fields[fieldIndex];
                        if (field == null || string.IsNullOrWhiteSpace(field.key))
                        {
                            continue;
                        }

                        builder.AppendLine($"  {field.key}: {field.value}");
                    }
                }
            }
        }

        builder.AppendLine();
        builder.AppendLine("Problemas:");
        if (validation == null || validation.issues == null || validation.issues.Count == 0)
        {
            builder.AppendLine("- Nenhum problema encontrado.");
        }
        else
        {
            for (var index = 0; index < validation.issues.Count; index++)
            {
                var issue = validation.issues[index];
                if (issue == null)
                {
                    continue;
                }

                builder.AppendLine($"- {issue.issueType}: {issue.message}");
            }
        }

        builder.AppendLine();
        builder.AppendLine(lastResultMessage);

        if (dayManager != null && dayManager.IsWaitingForNextCase)
        {
            builder.AppendLine("Clique em Proximo aluno para continuar.");
        }

        debugText.text = builder.ToString();
    }
}
