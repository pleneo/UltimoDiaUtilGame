using System.Collections.Generic;
using UnityEngine;

public enum DialogueSpeaker
{
    Npc,
    Player,
    Narrator
}

[System.Serializable]
public class CaseDialogueLine
{
    public DialogueSpeaker speaker = DialogueSpeaker.Npc;

    [TextArea(1, 4)]
    public string text;
}

[CreateAssetMenu(menuName = "Ultimo Dia Util/Cases/Student Case Definition", fileName = "Case_")]
public class StudentCaseDefinition : ScriptableObject
{
    public string caseId = "case_01";
    public string caseTitle = "Caso";
    public string applicantName = "Estudante";
    public RequestType requestType = RequestType.Enrollment;
    public NpcDefinition npcDefinition;

    [TextArea(2, 5)]
    public string npcDialogue;

    [Header("Dialogue Before Documents")]
    public List<CaseDialogueLine> preDocumentDialogue = new List<CaseDialogueLine>();

    [TextArea(2, 5)]
    public string caseSummary;

    [TextArea(2, 4)]
    public string referenceDateIso;

    public bool requiresSupervisorReview;
    public bool hasDecisionOverride;
    public DecisionType overriddenCorrectDecision = DecisionType.Approve;

    [TextArea(2, 4)]
    public string overrideReason;

    public bool mistakeIsCritical = true;

    public List<DocumentRequirement> requiredDocuments = new List<DocumentRequirement>();
    public List<DocumentComparisonRule> comparisonRules = new List<DocumentComparisonRule>();
    public List<DocumentRecord> documents = new List<DocumentRecord>();
}
