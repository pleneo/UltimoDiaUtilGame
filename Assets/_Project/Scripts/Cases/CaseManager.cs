using System;
using UnityEngine;

public class CaseManager : MonoBehaviour
{
    [SerializeField] private DocumentManager documentManager;
    [SerializeField] private bool logDecisionDebug = true;

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

        var resolvedCase = CurrentCase;
        var validation = CaseValidator.Evaluate(CurrentCase, documentManager != null ? documentManager.CurrentDocuments : null);
        var result = new CaseResolutionResult
        {
            caseDefinition = resolvedCase,
            chosenDecision = decision,
            validationResult = validation,
            isCorrectDecision = decision == validation.recommendedDecision,
            warningDelta = decision == validation.recommendedDecision
                ? 0
                : (resolvedCase.mistakeIsCritical ? 1 : 0),
            feedbackMessage = BuildFeedbackMessage(validation, decision)
        };

        ClearCurrentCase();
        LogDecisionDebug(result);
        CaseResolved?.Invoke(result);
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

    private void LogDecisionDebug(CaseResolutionResult result)
    {
        if (!logDecisionDebug || result == null)
        {
            return;
        }

        var caseTitle = result.caseDefinition != null ? result.caseDefinition.caseTitle : "Caso desconhecido";
        var applicantName = result.caseDefinition != null ? result.caseDefinition.applicantName : "Aluno desconhecido";
        var expectedDecision = result.validationResult != null
            ? result.validationResult.recommendedDecision.ToString()
            : "Indisponivel";
        var status = result.isCorrectDecision ? "CORRETA" : "INCORRETA";

        Debug.Log(
            $"[CaseManager] Caso: {caseTitle} | Aluno: {applicantName} | Escolha: {result.chosenDecision} | Esperado: {expectedDecision} | Decisao {status}");

        if (result.validationResult == null || result.validationResult.issues == null || result.validationResult.issues.Count == 0)
        {
            Debug.Log("[CaseManager] Validacao: nenhum problema encontrado.");
            return;
        }

        for (var index = 0; index < result.validationResult.issues.Count; index++)
        {
            var issue = result.validationResult.issues[index];
            if (issue == null)
            {
                continue;
            }

            Debug.Log(
                $"[CaseManager] Problema: {issue.issueType} | {issue.message} | Origem: {issue.sourceDocumentType} | Alvo: {issue.targetDocumentType}");
        }
    }
}
