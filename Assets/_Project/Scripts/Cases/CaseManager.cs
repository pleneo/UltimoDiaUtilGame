using System;
using UnityEngine;

public class CaseManager : MonoBehaviour
{
    [SerializeField] private DocumentManager documentManager;

    public event Action<StudentCaseDefinition> CaseLoaded;
    public event Action<CaseResolutionResult> CaseResolved;

    public StudentCaseDefinition CurrentCase { get; private set; }
    public bool HasActiveCase => CurrentCase != null;

    private void Awake()
    {
        if (documentManager == null)
        {
            documentManager = FindObjectOfType<DocumentManager>();
        }
    }

    public void LoadCase(StudentCaseDefinition caseDefinition)
    {
        CurrentCase = caseDefinition;

        if (documentManager != null)
        {
            if (caseDefinition == null)
            {
                documentManager.ClearDocuments();
            }
            else
            {
                documentManager.LoadCase(caseDefinition);
            }
        }

        CaseLoaded?.Invoke(caseDefinition);
    }

    public void SubmitDecision(DecisionType decision)
    {
        if (CurrentCase == null)
        {
            return;
        }

        var validation = CaseValidator.Evaluate(CurrentCase, documentManager != null ? documentManager.CurrentDocuments : null);
        var result = new CaseResolutionResult
        {
            caseDefinition = CurrentCase,
            chosenDecision = decision,
            validationResult = validation,
            isCorrectDecision = decision == validation.recommendedDecision,
            warningDelta = decision == validation.recommendedDecision
                ? 0
                : (CurrentCase.mistakeIsCritical ? 1 : 0),
            feedbackMessage = BuildFeedbackMessage(validation, decision)
        };

        CaseResolved?.Invoke(result);
        ClearCurrentCase();
    }

    public void ClearCurrentCase()
    {
        CurrentCase = null;

        if (documentManager != null)
        {
            documentManager.ClearDocuments();
        }
    }

    private static string BuildFeedbackMessage(CaseValidationResult validation, DecisionType decision)
    {
        if (validation == null)
        {
            return "Sem validacao disponivel.";
        }

        if (decision == validation.recommendedDecision)
        {
            return string.IsNullOrWhiteSpace(validation.summary) ? "Decisao correta." : validation.summary;
        }

        return validation.HasIssues
            ? "Decisao incorreta. Verifique os documentos."
            : "Decisao incorreta para este caso.";
    }
}
