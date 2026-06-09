using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FixedDayCaseEntry
{
    public StudentCaseDefinition caseDefinition;
    [Min(0)] public int triggerResolvedCaseNumber;
}

[CreateAssetMenu(menuName = "Ultimo Dia Util/Day/Day Config", fileName = "Day_")]
public class DayConfig : ScriptableObject
{ 
    public int dayNumber = 1;
    public string dayLabel = "Dia 1";

    [TextArea(2, 4)]
    public string dayIntro;

    [Header("Story Notice")]
    [TextArea(4, 10)]
    public string startOfDayNotice;

    public float workDurationSeconds = 300f;
    public EconomyConfig economyConfig;

    [Header("Generated Cases")]
    public bool useInfiniteGeneratedCases = false;
    public EnrollmentCaseGenerationConfig enrollmentGenerationConfig;

    [Header("Case Queue")]
    [Tooltip("Quantidade de atendimentos no expediente. Use 5 para o MVP.")]
    [Min(1)] public int maxCasesForDay = 5;
    public bool shuffleCaseQueue = true;
    public bool useFixedQueueSeed = false;
    public int queueRandomSeed = 1;

    [Header("Fixed Story Cases")]
    public List<FixedDayCaseEntry> fixedCases = new List<FixedDayCaseEntry>();

    [Header("Legacy Manual Cases")]
    public bool includeManualCases = true;
    public List<RequestType> availableRequestTypes = new List<RequestType>();
    public List<RuleDefinition> rulebookEntries = new List<RuleDefinition>();
    public List<NoticeBoardEntry> noticeBoardEntries = new List<NoticeBoardEntry>();
    public List<StudentCaseDefinition> cases = new List<StudentCaseDefinition>();
}
