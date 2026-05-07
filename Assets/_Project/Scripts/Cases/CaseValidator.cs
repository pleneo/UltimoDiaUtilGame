using System;
using System.Collections.Generic;
using System.Linq;

public static class CaseValidator
{
    public static CaseValidationResult Evaluate(StudentCaseDefinition caseDefinition, IReadOnlyList<DocumentRecord> documents)
    {
        var result = new CaseValidationResult();

        if (caseDefinition == null)
        {
            result.recommendedDecision = DecisionType.Reject;
            result.summary = "Caso inexistente.";
            result.issues.Add(new ValidationIssue
            {
                issueType = ValidationIssueType.InvalidRequest,
                message = "O caso atual nao foi carregado corretamente."
            });
            return result;
        }

        if (caseDefinition.hasDecisionOverride)
        {
            result.recommendedDecision = caseDefinition.overriddenCorrectDecision;
            result.summary = string.IsNullOrWhiteSpace(caseDefinition.overrideReason)
                ? "Decisao definida por narrativa."
                : caseDefinition.overrideReason;
        }

        var activeDocuments = documents ?? Array.Empty<DocumentRecord>();
        var referenceDate = ResolveReferenceDate(caseDefinition.referenceDateIso);

        EvaluateRequiredDocuments(caseDefinition, activeDocuments, referenceDate, result);
        EvaluateComparisons(caseDefinition, activeDocuments, result);

        result.hasSupervisorReview = caseDefinition.requiresSupervisorReview;

        if (!caseDefinition.hasDecisionOverride)
        {
            if (result.issues.Count > 0)
            {
                result.recommendedDecision = caseDefinition.requiresSupervisorReview
                    ? DecisionType.Forward
                    : DecisionType.Reject;
                result.summary = "Existem problemas na documentacao.";
            }
            else
            {
                result.recommendedDecision = caseDefinition.requiresSupervisorReview
                    ? DecisionType.Forward
                    : DecisionType.Approve;
                result.summary = "Documentacao em ordem.";
            }
        }

        return result;
    }

    private static void EvaluateRequiredDocuments(
        StudentCaseDefinition caseDefinition,
        IReadOnlyList<DocumentRecord> documents,
        DateTime referenceDate,
        CaseValidationResult result)
    {
        if (caseDefinition.requiredDocuments == null)
        {
            return;
        }

        for (var index = 0; index < caseDefinition.requiredDocuments.Count; index++)
        {
            var requirement = caseDefinition.requiredDocuments[index];
            if (requirement == null || !requirement.required)
            {
                continue;
            }

            var document = FindDocument(documents, requirement.documentType);
            if (document == null)
            {
                result.issues.Add(new ValidationIssue
                {
                    issueType = ValidationIssueType.MissingDocument,
                    message = $"Documento obrigatorio ausente: {requirement.documentType}",
                    sourceDocumentType = requirement.documentType
                });
                continue;
            }

            if (document.isFake)
            {
                result.issues.Add(new ValidationIssue
                {
                    issueType = ValidationIssueType.SuspiciousDocument,
                    message = $"{document.GetDisplayName()} foi marcado como falso.",
                    sourceDocumentType = document.GetDocumentType()
                });
            }

            if (document.isSuspicious)
            {
                result.issues.Add(new ValidationIssue
                {
                    issueType = ValidationIssueType.SuspiciousDocument,
                    message = $"{document.GetDisplayName()} esta suspeito.",
                    sourceDocumentType = document.GetDocumentType()
                });
            }

            if (requirement.requiresValidStamp && !document.hasValidStamp)
            {
                result.issues.Add(new ValidationIssue
                {
                    issueType = ValidationIssueType.InvalidStamp,
                    message = $"{document.GetDisplayName()} precisa de carimbo valido.",
                    sourceDocumentType = document.GetDocumentType()
                });
            }

            if (requirement.requiresNonExpiredDate && document.IsExpired(referenceDate))
            {
                result.issues.Add(new ValidationIssue
                {
                    issueType = ValidationIssueType.ExpiredDocument,
                    message = $"{document.GetDisplayName()} esta vencido.",
                    sourceDocumentType = document.GetDocumentType()
                });
            }

            if (requirement.requiredFieldKeys == null)
            {
                continue;
            }

            for (var fieldIndex = 0; fieldIndex < requirement.requiredFieldKeys.Count; fieldIndex++)
            {
                var requiredFieldKey = requirement.requiredFieldKeys[fieldIndex];
                if (string.IsNullOrWhiteSpace(requiredFieldKey))
                {
                    continue;
                }

                if (!document.TryGetFieldValue(requiredFieldKey, out var fieldValue) || string.IsNullOrWhiteSpace(fieldValue))
                {
                    result.issues.Add(new ValidationIssue
                    {
                        issueType = ValidationIssueType.MissingField,
                        message = $"{document.GetDisplayName()} precisa do campo '{requiredFieldKey}'.",
                        sourceDocumentType = document.GetDocumentType()
                    });
                }
            }
        }
    }

    private static void EvaluateComparisons(
        StudentCaseDefinition caseDefinition,
        IReadOnlyList<DocumentRecord> documents,
        CaseValidationResult result)
    {
        if (caseDefinition.comparisonRules == null)
        {
            return;
        }

        for (var index = 0; index < caseDefinition.comparisonRules.Count; index++)
        {
            var rule = caseDefinition.comparisonRules[index];
            if (rule == null)
            {
                continue;
            }

            var firstDocument = FindDocument(documents, rule.firstDocumentType);
            var secondDocument = FindDocument(documents, rule.secondDocumentType);

            if (firstDocument == null || secondDocument == null)
            {
                result.issues.Add(new ValidationIssue
                {
                    issueType = ValidationIssueType.MissingDocument,
                    message = $"Comparacao impossivel entre {rule.firstDocumentType} e {rule.secondDocumentType}.",
                    sourceDocumentType = rule.firstDocumentType,
                    targetDocumentType = rule.secondDocumentType
                });
                continue;
            }

            var firstValueExists = firstDocument.TryGetFieldValue(rule.firstFieldKey, out var firstValue);
            var secondValueExists = secondDocument.TryGetFieldValue(rule.secondFieldKey, out var secondValue);

            if (!firstValueExists || !secondValueExists)
            {
                result.issues.Add(new ValidationIssue
                {
                    issueType = ValidationIssueType.MissingField,
                    message = string.IsNullOrWhiteSpace(rule.description)
                        ? "Campo de comparacao ausente."
                        : rule.description,
                    sourceDocumentType = rule.firstDocumentType,
                    targetDocumentType = rule.secondDocumentType
                });
                continue;
            }

            if (!ValuesMatch(firstValue, secondValue))
            {
                result.issues.Add(new ValidationIssue
                {
                    issueType = ValidationIssueType.FieldMismatch,
                    message = string.IsNullOrWhiteSpace(rule.description)
                        ? $"'{rule.firstFieldKey}' nao bate com '{rule.secondFieldKey}'."
                        : rule.description,
                    sourceDocumentType = rule.firstDocumentType,
                    targetDocumentType = rule.secondDocumentType
                });
            }
        }
    }

    private static DocumentRecord FindDocument(IReadOnlyList<DocumentRecord> documents, DocumentType documentType)
    {
        if (documents == null)
        {
            return null;
        }

        for (var index = 0; index < documents.Count; index++)
        {
            var document = documents[index];
            if (document == null)
            {
                continue;
            }

            if (document.GetDocumentType() == documentType)
            {
                return document;
            }
        }

        return null;
    }

    private static bool ValuesMatch(string firstValue, string secondValue)
    {
        return string.Equals(NormalizeValue(firstValue), NormalizeValue(secondValue), StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeValue(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    private static DateTime ResolveReferenceDate(string referenceDateIso)
    {
        return DateUtility.ResolveReferenceDate(referenceDateIso);
    }
}
