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
        EvaluateExpectedFieldValues(caseDefinition, activeDocuments, result);

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
                    message = $"Faltando documento: {GetDocumentTypeLabel(requirement.documentType)}.",
                    sourceDocumentType = requirement.documentType
                });
                continue;
            }

            if (document.isFake)
            {
                result.issues.Add(new ValidationIssue
                {
                    issueType = ValidationIssueType.SuspiciousDocument,
                    message = $"{document.GetDisplayName()} aparenta ser falso.",
                    sourceDocumentType = document.GetDocumentType()
                });
            }

            if (document.isSuspicious)
            {
                result.issues.Add(new ValidationIssue
                {
                    issueType = ValidationIssueType.SuspiciousDocument,
                    message = $"{document.GetDisplayName()} apresenta informacoes suspeitas.",
                    sourceDocumentType = document.GetDocumentType()
                });
            }

            if (requirement.requiresValidStamp && !document.hasValidStamp)
            {
                result.issues.Add(new ValidationIssue
                {
                    issueType = ValidationIssueType.InvalidStamp,
                    message = $"O documento {document.GetDisplayName()} esta sem carimbo valido.",
                    sourceDocumentType = document.GetDocumentType()
                });
            }

            if (requirement.requiresNonExpiredDate && document.IsExpired(referenceDate))
            {
                result.issues.Add(new ValidationIssue
                {
                    issueType = ValidationIssueType.ExpiredDocument,
                    message = $"O documento {document.GetDisplayName()} esta vencido.",
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
                        message = $"Faltando o campo {GetFieldLabel(requiredFieldKey)} em {document.GetDisplayName()}.",
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
                    message = $"Nao foi possivel comparar {GetDocumentTypeLabel(rule.firstDocumentType)} com {GetDocumentTypeLabel(rule.secondDocumentType)}.",
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
                        ? $"Faltam campos para comparar {GetFieldLabel(rule.firstFieldKey)}."
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
                        ? BuildFieldMismatchMessage(rule)
                        : rule.description,
                    sourceDocumentType = rule.firstDocumentType,
                    targetDocumentType = rule.secondDocumentType
                });
            }
        }
    }

    private static void EvaluateExpectedFieldValues(
        StudentCaseDefinition caseDefinition,
        IReadOnlyList<DocumentRecord> documents,
        CaseValidationResult result)
    {
        if (caseDefinition.expectedFieldValueRules == null)
        {
            return;
        }

        for (var index = 0; index < caseDefinition.expectedFieldValueRules.Count; index++)
        {
            var rule = caseDefinition.expectedFieldValueRules[index];
            if (rule == null || rule.documentType == DocumentType.Unknown || string.IsNullOrWhiteSpace(rule.fieldKey))
            {
                continue;
            }

            var document = FindDocument(documents, rule.documentType);
            if (document == null)
            {
                continue;
            }

            if (!document.TryGetFieldValue(rule.fieldKey, out var actualValue) || string.IsNullOrWhiteSpace(actualValue))
            {
                result.issues.Add(new ValidationIssue
                {
                    issueType = ValidationIssueType.MissingField,
                    message = string.IsNullOrWhiteSpace(rule.description)
                        ? $"Faltando o campo {GetFieldLabel(rule.fieldKey)} em {document.GetDisplayName()}."
                        : rule.description,
                    sourceDocumentType = rule.documentType
                });
                continue;
            }

            if (ValuesMatch(actualValue, rule.expectedValue))
            {
                continue;
            }

            result.issues.Add(new ValidationIssue
            {
                issueType = ValidationIssueType.InvalidFieldValue,
                message = string.IsNullOrWhiteSpace(rule.description)
                    ? BuildUnexpectedFieldValueMessage(rule, document)
                    : rule.description,
                sourceDocumentType = rule.documentType
            });
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

    private static string BuildFieldMismatchMessage(DocumentComparisonRule rule)
    {
        var fieldLabel = GetFieldLabel(rule.firstFieldKey);
        var firstDocumentLabel = GetDocumentTypeLabel(rule.firstDocumentType);
        var secondDocumentLabel = GetDocumentTypeLabel(rule.secondDocumentType);

        return $"{fieldLabel} divergente entre {firstDocumentLabel} e {secondDocumentLabel}.";
    }

    private static string BuildUnexpectedFieldValueMessage(DocumentExpectedFieldValueRule rule, DocumentRecord document)
    {
        var fieldLabel = GetFieldLabel(rule.fieldKey);
        var documentLabel = document != null ? document.GetDisplayName() : GetDocumentTypeLabel(rule.documentType);
        var expectedValue = string.IsNullOrWhiteSpace(rule.expectedValue) ? "valor esperado" : rule.expectedValue;
        return $"{fieldLabel} invalido em {documentLabel}. Esperado: {expectedValue}.";
    }

    private static string GetDocumentTypeLabel(DocumentType documentType)
    {
        return documentType switch
        {
            DocumentType.IdentityCard => "Carteira de Identidade",
            DocumentType.SchoolTranscript => "Historico Escolar",
            DocumentType.EnrollmentProof => "Comprovante de Matricula",
            DocumentType.TuitionReceipt => "Comprovante de Mensalidade",
            DocumentType.WithdrawalForm => "Formulario de Trancamento",
            DocumentType.PaymentReceipt => "Via da Maquininha",
            DocumentType.SpecialNotice => "Aviso Especial",
            _ => "Documento"
        };
    }

    private static string GetFieldLabel(string fieldKey)
    {
        if (string.IsNullOrWhiteSpace(fieldKey))
        {
            return "informacao obrigatoria";
        }

        var normalizedKey = fieldKey.Trim().ToLowerInvariant();
        return normalizedKey switch
        {
            "nome" => "nome",
            "ra" => "RA",
            "curso" => "curso",
            "cpf" => "CPF",
            "data" => "data",
            "valor" => "valor",
            "status" => "status",
            _ => fieldKey
        };
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
