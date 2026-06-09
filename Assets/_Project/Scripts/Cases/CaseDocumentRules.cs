public static class CaseDocumentRules
{
    public static DocumentType ResolveDecisionDocumentType(StudentCaseDefinition caseDefinition)
    {
        if (caseDefinition != null && caseDefinition.decisionDocumentType != DocumentType.Unknown)
        {
            return caseDefinition.decisionDocumentType;
        }

        if (caseDefinition == null)
        {
            return DocumentType.Unknown;
        }

        return caseDefinition.requestType switch
        {
            RequestType.Enrollment => DocumentType.EnrollmentProof,
            RequestType.ClassWithdrawal => DocumentType.WithdrawalForm,
            RequestType.TuitionPayment => DocumentType.TuitionReceipt,
            _ => DocumentType.Unknown
        };
    }

    public static bool RequiresPayment(StudentCaseDefinition caseDefinition)
    {
        return caseDefinition != null && caseDefinition.requiresPaymentProcessing;
    }
}
