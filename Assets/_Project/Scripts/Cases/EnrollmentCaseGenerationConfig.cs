using System;
using System.Collections.Generic;
using UnityEngine;

public enum NpcVisualProfile
{
    Masculino,
    Feminino
}

public enum EnrollmentGeneratedCaseType
{
    Valid,
    MissingDocument,
    NameMismatch,
    RaMismatch,
    MissingRequiredField,
    ExpiredDocument
}

[Serializable]
public class EnrollmentMinimumCaseRule
{
    public EnrollmentGeneratedCaseType caseType = EnrollmentGeneratedCaseType.Valid;
    public int minimumCount = 1;
}

[Serializable]
public class EnrollmentCaseTypeWeight
{
    public EnrollmentGeneratedCaseType caseType = EnrollmentGeneratedCaseType.Valid;
    public int weight = 10;
}

[CreateAssetMenu(menuName = "Ultimo Dia Util/Cases/Enrollment Generation Config", fileName = "EnrollmentGen_")]
public class EnrollmentCaseGenerationConfig : ScriptableObject
{
    public int totalGeneratedCases = 6;
    public bool shuffleGeneratedCases = true;
    public bool useFreshRandomSeed;
    public int randomSeed = 12345;

    [Header("Documents")]
    public DocumentDefinition identityCardDefinition;
    public DocumentDefinition schoolTranscriptDefinition;
    public DocumentDefinition enrollmentProofDefinition;

    [Header("Rules")]
    public string referenceDateIso = "2026-05-21";
    public int firstRaNumber = 2026001;
    public int lastRaNumber = 2026999;

    [Header("Guaranteed Case Mix")]
    public List<EnrollmentMinimumCaseRule> minimumCases = new List<EnrollmentMinimumCaseRule>();

    [Header("Weighted Random Cases")]
    public List<EnrollmentCaseTypeWeight> caseTypeWeights = new List<EnrollmentCaseTypeWeight>();

    [Header("Generated Student Data")]
    public List<string> firstNames = new List<string>();
    public List<string> lastNames = new List<string>();
    public List<string> courses = new List<string>();

    [Header("Profiled Student Data")]
    public bool useProfiledNpcGeneration = true;
    public List<string> maleFirstNames = new List<string>();
    public List<string> femaleFirstNames = new List<string>();
    public List<NpcDefinition> maleNpcDefinitions = new List<NpcDefinition>();
    public List<NpcDefinition> femaleNpcDefinitions = new List<NpcDefinition>();
}
