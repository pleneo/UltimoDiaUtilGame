using System;
using System.Collections.Generic;

[Serializable]
public class ValidationIssue
{
    public ValidationIssueType issueType;
    public string message;
    public DocumentType sourceDocumentType = DocumentType.Unknown;
    public DocumentType targetDocumentType = DocumentType.Unknown;
}

[Serializable]
public class CaseValidationResult
{
    public DecisionType recommendedDecision = DecisionType.Reject;
    public bool hasSupervisorReview;
    public string summary;
    public List<ValidationIssue> issues = new List<ValidationIssue>();

    public bool HasIssues => issues != null && issues.Count > 0;
}

[Serializable]
public class CaseResolutionResult
{
    public StudentCaseDefinition caseDefinition;
    public DecisionType chosenDecision;
    public CaseValidationResult validationResult;
    public bool isCorrectDecision;
    public int warningDelta;
    public string feedbackMessage;
}

[Serializable]
public class EconomySnapshot
{
    public int currentMoney;
    public int remainingDebt;
    public int warnings;
    public int warningLimit;
    public int payPerCorrectDecision;
    public int penaltyPerMistake;
    public int dailyExpenses;
    public bool isGameOver;
    public string gameOverReason;
}

[Serializable]
public class DaySummaryData
{
    public int dayNumber;
    public string dayLabel;
    public int totalCases;
    public int completedCases;
    public int correctDecisions;
    public int incorrectDecisions;
    public float workDurationSeconds;
    public float remainingTimeSeconds;
    public DayEndReason endReason;
    public string headline;
    public string details;
    public EconomySnapshot economySnapshot;
    public List<string> notes = new List<string>();
}
