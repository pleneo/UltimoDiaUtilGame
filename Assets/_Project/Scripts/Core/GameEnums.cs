public enum GameState
{
    Boot,
    DayActive,
    DaySummary,
    GameOver,
    Victory
}

public enum DayEndReason
{
    Completed,
    TimeExpired,
    GameOver
}

public enum RequestType
{
    Enrollment,
    TuitionPayment,
    ClassWithdrawal
}

public enum DecisionType
{
    Approve,
    Reject,
    Forward
}

public enum DocumentType
{
    Unknown,
    IdentityCard,
    SchoolTranscript,
    EnrollmentProof,
    TuitionReceipt,
    WithdrawalForm,
    PaymentReceipt,
    SpecialNotice
}

public enum StampType
{
    Aprovado,
    Negado
}

public enum ValidationIssueType
{
    MissingDocument,
    MissingField,
    FieldMismatch,
    InvalidStamp,
    ExpiredDocument,
    SuspiciousDocument,
    InvalidRequest,
    SupervisorReviewNeeded
}
