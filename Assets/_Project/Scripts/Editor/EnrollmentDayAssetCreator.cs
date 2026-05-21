using System.IO;
using UnityEditor;
using UnityEngine;

public static class EnrollmentDayAssetCreator
{
    private const string DocumentsFolder = "Assets/_Project/ScriptableObjects/Documents";
    private const string CasesFolder = "Assets/_Project/ScriptableObjects/Cases/Day01";
    private const string DaysFolder = "Assets/_Project/ScriptableObjects/Days";
    private const string RulesFolder = "Assets/_Project/ScriptableObjects/Rules";
    private const string EconomyFolder = "Assets/_Project/ScriptableObjects/Economy";

    [MenuItem("Tools/Ultimo Dia Util/Day 1/Create Enrollment Documents")]
    public static void CreateEnrollmentDocuments()
    {
        EnsureFolder("Assets/_Project/ScriptableObjects", "Documents");

        CreateOrUpdateDocument(
            "Document_IdentityCard.asset",
            DocumentType.IdentityCard,
            "Carteira de Identidade",
            "Documento oficial usado para conferir o nome do aluno.");

        CreateOrUpdateDocument(
            "Document_SchoolTranscript.asset",
            DocumentType.SchoolTranscript,
            "Historico Escolar",
            "Historico escolar usado para conferir nome e RA.");

        CreateOrUpdateDocument(
            "Document_EnrollmentProof.asset",
            DocumentType.EnrollmentProof,
            "Comprovante de Matricula",
            "Comprovante usado para conferir nome, RA e curso da matricula.");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Documentos base de matricula criados/atualizados em Assets/_Project/ScriptableObjects/Documents.");
    }

    [MenuItem("Tools/Ultimo Dia Util/Day 1/Create Enrollment Cases")]
    public static void CreateEnrollmentCases()
    {
        CreateEnrollmentDocuments();
        EnsureFolder("Assets/_Project/ScriptableObjects", "Cases");
        EnsureFolder("Assets/_Project/ScriptableObjects/Cases", "Day01");

        var identityCard = LoadDocument("Document_IdentityCard.asset");
        var schoolTranscript = LoadDocument("Document_SchoolTranscript.asset");
        var enrollmentProof = LoadDocument("Document_EnrollmentProof.asset");

        if (identityCard == null || schoolTranscript == null || enrollmentProof == null)
        {
            Debug.LogError("Nao foi possivel criar os casos de matricula porque algum DocumentDefinition nao foi encontrado.");
            return;
        }

        CreateOrUpdateValidCase(identityCard, schoolTranscript, enrollmentProof);
        CreateOrUpdateMissingDocumentCase(identityCard, enrollmentProof);
        CreateOrUpdateNameMismatchCase(identityCard, schoolTranscript, enrollmentProof);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Casos de matricula do Dia 1 criados/atualizados em Assets/_Project/ScriptableObjects/Cases/Day01.");
    }

    [MenuItem("Tools/Ultimo Dia Util/Day 1/Create Enrollment Day Config")]
    public static void CreateEnrollmentDayConfig()
    {
        CreateEnrollmentCases();
        EnsureFolder("Assets/_Project/ScriptableObjects", "Days");
        EnsureFolder("Assets/_Project/ScriptableObjects", "Rules");
        EnsureFolder("Assets/_Project/ScriptableObjects", "Economy");

        var validCase = LoadCase("Case_Day01_Enrollment_Valid.asset");
        var missingDocumentCase = LoadCase("Case_Day01_Enrollment_MissingDocument.asset");
        var nameMismatchCase = LoadCase("Case_Day01_Enrollment_NameMismatch.asset");

        if (validCase == null || missingDocumentCase == null || nameMismatchCase == null)
        {
            Debug.LogError("Nao foi possivel criar o DayConfig porque algum caso de matricula nao foi encontrado.");
            return;
        }

        var enrollmentRule = CreateOrUpdateEnrollmentRule();
        var economyConfig = CreateOrUpdateDayOneEconomyConfig();
        var dayConfig = LoadOrCreateDayConfig("Day_01_EnrollmentBasics.asset");

        dayConfig.dayNumber = 1;
        dayConfig.dayLabel = "Dia 1";
        dayConfig.dayIntro = "Primeiro dia de atendimento. Confira documentos de matricula com calma.";
        dayConfig.workDurationSeconds = 300f;
        dayConfig.economyConfig = economyConfig;

        dayConfig.availableRequestTypes.Clear();
        dayConfig.availableRequestTypes.Add(RequestType.Enrollment);

        dayConfig.rulebookEntries.Clear();
        dayConfig.rulebookEntries.Add(enrollmentRule);

        dayConfig.noticeBoardEntries.Clear();

        dayConfig.cases.Clear();
        dayConfig.cases.Add(validCase);
        dayConfig.cases.Add(missingDocumentCase);
        dayConfig.cases.Add(nameMismatchCase);

        EditorUtility.SetDirty(dayConfig);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("DayConfig do Dia 1 criado/atualizado em Assets/_Project/ScriptableObjects/Days/Day_01_EnrollmentBasics.asset.");
    }

    private static void CreateOrUpdateDocument(
        string fileName,
        DocumentType documentType,
        string displayName,
        string description)
    {
        var assetPath = Path.Combine(DocumentsFolder, fileName).Replace("\\", "/");
        var document = AssetDatabase.LoadAssetAtPath<DocumentDefinition>(assetPath);

        if (document == null)
        {
            document = ScriptableObject.CreateInstance<DocumentDefinition>();
            AssetDatabase.CreateAsset(document, assetPath);
        }

        document.documentType = documentType;
        document.displayName = displayName;
        document.description = description;

        EditorUtility.SetDirty(document);
    }

    private static void CreateOrUpdateValidCase(
        DocumentDefinition identityCard,
        DocumentDefinition schoolTranscript,
        DocumentDefinition enrollmentProof)
    {
        var studentCase = LoadOrCreateCase("Case_Day01_Enrollment_Valid.asset");

        ConfigureBaseEnrollmentCase(
            studentCase,
            "day01_enrollment_valid",
            "Matricula regular",
            "Plinio Gomes",
            "Aluno solicita matricula com todos os documentos em ordem.",
            "Todos os documentos obrigatorios foram entregues. Nome e RA batem entre os documentos.");

        AddEnrollmentRequirements(studentCase);
        AddEnrollmentComparisonRules(studentCase);

        studentCase.documents.Clear();
        studentCase.documents.Add(CreateIdentityCard(identityCard, "Plinio Gomes"));
        studentCase.documents.Add(CreateSchoolTranscript(schoolTranscript, "Plinio Gomes", "2026001", "Ciencia da Computacao"));
        studentCase.documents.Add(CreateEnrollmentProof(enrollmentProof, "Plinio Gomes", "2026001", "Ciencia da Computacao"));

        EditorUtility.SetDirty(studentCase);
    }

    private static void CreateOrUpdateMissingDocumentCase(
        DocumentDefinition identityCard,
        DocumentDefinition enrollmentProof)
    {
        var studentCase = LoadOrCreateCase("Case_Day01_Enrollment_MissingDocument.asset");

        ConfigureBaseEnrollmentCase(
            studentCase,
            "day01_enrollment_missing_document",
            "Matricula sem historico",
            "Plinio Gomes",
            "Aluno solicita matricula, mas nao trouxe o historico escolar.",
            "O historico escolar esta faltando, entao a matricula deve ser indeferida.");

        AddEnrollmentRequirements(studentCase);
        AddEnrollmentComparisonRules(studentCase);

        studentCase.documents.Clear();
        studentCase.documents.Add(CreateIdentityCard(identityCard, "Plinio Gomes"));
        studentCase.documents.Add(CreateEnrollmentProof(enrollmentProof, "Plinio Gomes", "2026001", "Ciencia da Computacao"));

        EditorUtility.SetDirty(studentCase);
    }

    private static void CreateOrUpdateNameMismatchCase(
        DocumentDefinition identityCard,
        DocumentDefinition schoolTranscript,
        DocumentDefinition enrollmentProof)
    {
        var studentCase = LoadOrCreateCase("Case_Day01_Enrollment_NameMismatch.asset");

        ConfigureBaseEnrollmentCase(
            studentCase,
            "day01_enrollment_name_mismatch",
            "Matricula com nome divergente",
            "Plinio Gomes",
            "Aluno solicita matricula, mas um dos documentos tem nome divergente.",
            "O comprovante de matricula usa um nome diferente, entao a matricula deve ser indeferida.");

        AddEnrollmentRequirements(studentCase);
        AddEnrollmentComparisonRules(studentCase);

        studentCase.documents.Clear();
        studentCase.documents.Add(CreateIdentityCard(identityCard, "Plinio Gomes"));
        studentCase.documents.Add(CreateSchoolTranscript(schoolTranscript, "Plinio Gomes", "2026001", "Ciencia da Computacao"));
        studentCase.documents.Add(CreateEnrollmentProof(enrollmentProof, "Plinio Gomis", "2026001", "Ciencia da Computacao"));

        EditorUtility.SetDirty(studentCase);
    }

    private static void ConfigureBaseEnrollmentCase(
        StudentCaseDefinition studentCase,
        string caseId,
        string caseTitle,
        string applicantName,
        string npcDialogue,
        string caseSummary)
    {
        studentCase.caseId = caseId;
        studentCase.caseTitle = caseTitle;
        studentCase.applicantName = applicantName;
        studentCase.requestType = RequestType.Enrollment;
        studentCase.npcDialogue = npcDialogue;
        studentCase.caseSummary = caseSummary;
        studentCase.referenceDateIso = "2026-05-21";
        studentCase.requiresSupervisorReview = false;
        studentCase.hasDecisionOverride = false;
        studentCase.overriddenCorrectDecision = DecisionType.Approve;
        studentCase.overrideReason = string.Empty;
        studentCase.mistakeIsCritical = true;
    }

    private static void AddEnrollmentRequirements(StudentCaseDefinition studentCase)
    {
        studentCase.requiredDocuments.Clear();
        studentCase.requiredDocuments.Add(CreateRequirement(DocumentType.IdentityCard, "nome"));
        studentCase.requiredDocuments.Add(CreateRequirement(DocumentType.SchoolTranscript, "nome", "ra"));
        studentCase.requiredDocuments.Add(CreateRequirement(DocumentType.EnrollmentProof, "nome", "ra"));
    }

    private static void AddEnrollmentComparisonRules(StudentCaseDefinition studentCase)
    {
        studentCase.comparisonRules.Clear();
        studentCase.comparisonRules.Add(CreateComparison(
            DocumentType.IdentityCard,
            "nome",
            DocumentType.SchoolTranscript,
            "nome",
            "O nome da Carteira de Identidade deve bater com o Historico Escolar."));

        studentCase.comparisonRules.Add(CreateComparison(
            DocumentType.IdentityCard,
            "nome",
            DocumentType.EnrollmentProof,
            "nome",
            "O nome da Carteira de Identidade deve bater com o Comprovante de Matricula."));

        studentCase.comparisonRules.Add(CreateComparison(
            DocumentType.SchoolTranscript,
            "ra",
            DocumentType.EnrollmentProof,
            "ra",
            "O RA do Historico Escolar deve bater com o Comprovante de Matricula."));
    }

    private static DocumentRequirement CreateRequirement(DocumentType documentType, params string[] requiredFieldKeys)
    {
        var requirement = new DocumentRequirement
        {
            documentType = documentType,
            required = true,
            requiresValidStamp = false,
            requiresNonExpiredDate = false
        };

        requirement.requiredFieldKeys.AddRange(requiredFieldKeys);
        return requirement;
    }

    private static DocumentComparisonRule CreateComparison(
        DocumentType firstDocumentType,
        string firstFieldKey,
        DocumentType secondDocumentType,
        string secondFieldKey,
        string description)
    {
        return new DocumentComparisonRule
        {
            firstDocumentType = firstDocumentType,
            firstFieldKey = firstFieldKey,
            secondDocumentType = secondDocumentType,
            secondFieldKey = secondFieldKey,
            description = description
        };
    }

    private static DocumentRecord CreateIdentityCard(DocumentDefinition definition, string name)
    {
        var record = CreateDocumentRecord(definition);
        record.fields.Add(CreateField("nome", name));
        return record;
    }

    private static DocumentRecord CreateSchoolTranscript(DocumentDefinition definition, string name, string ra, string course)
    {
        var record = CreateDocumentRecord(definition);
        record.fields.Add(CreateField("nome", name));
        record.fields.Add(CreateField("ra", ra));
        record.fields.Add(CreateField("curso", course));
        return record;
    }

    private static DocumentRecord CreateEnrollmentProof(DocumentDefinition definition, string name, string ra, string course)
    {
        var record = CreateDocumentRecord(definition);
        record.fields.Add(CreateField("nome", name));
        record.fields.Add(CreateField("ra", ra));
        record.fields.Add(CreateField("curso", course));
        return record;
    }

    private static DocumentRecord CreateDocumentRecord(DocumentDefinition definition)
    {
        return new DocumentRecord
        {
            definition = definition,
            hasValidStamp = true,
            isSuspicious = false,
            isFake = false,
            issueDateIso = "2026-05-21",
            expiryDateIso = string.Empty,
            notes = string.Empty
        };
    }

    private static DocumentField CreateField(string key, string value)
    {
        return new DocumentField
        {
            key = key,
            value = value
        };
    }

    private static DocumentDefinition LoadDocument(string fileName)
    {
        var assetPath = Path.Combine(DocumentsFolder, fileName).Replace("\\", "/");
        return AssetDatabase.LoadAssetAtPath<DocumentDefinition>(assetPath);
    }

    private static StudentCaseDefinition LoadOrCreateCase(string fileName)
    {
        var assetPath = Path.Combine(CasesFolder, fileName).Replace("\\", "/");
        var studentCase = AssetDatabase.LoadAssetAtPath<StudentCaseDefinition>(assetPath);

        if (studentCase == null)
        {
            studentCase = ScriptableObject.CreateInstance<StudentCaseDefinition>();
            AssetDatabase.CreateAsset(studentCase, assetPath);
        }

        return studentCase;
    }

    private static StudentCaseDefinition LoadCase(string fileName)
    {
        var assetPath = Path.Combine(CasesFolder, fileName).Replace("\\", "/");
        return AssetDatabase.LoadAssetAtPath<StudentCaseDefinition>(assetPath);
    }

    private static RuleDefinition CreateOrUpdateEnrollmentRule()
    {
        var assetPath = Path.Combine(RulesFolder, "Rule_Day01_EnrollmentDocuments.asset").Replace("\\", "/");
        var rule = AssetDatabase.LoadAssetAtPath<RuleDefinition>(assetPath);

        if (rule == null)
        {
            rule = ScriptableObject.CreateInstance<RuleDefinition>();
            AssetDatabase.CreateAsset(rule, assetPath);
        }

        rule.ruleTitle = "Documentos de matricula";
        rule.ruleBody = "Para matricula, exigir Carteira de Identidade, Historico Escolar e Comprovante de Matricula. O nome deve ser igual em todos os documentos. O RA deve bater entre Historico Escolar e Comprovante de Matricula.";
        rule.highlighted = true;

        EditorUtility.SetDirty(rule);
        return rule;
    }

    private static DayConfig LoadOrCreateDayConfig(string fileName)
    {
        var assetPath = Path.Combine(DaysFolder, fileName).Replace("\\", "/");
        var dayConfig = AssetDatabase.LoadAssetAtPath<DayConfig>(assetPath);

        if (dayConfig == null)
        {
            dayConfig = ScriptableObject.CreateInstance<DayConfig>();
            AssetDatabase.CreateAsset(dayConfig, assetPath);
        }

        return dayConfig;
    }

    private static EconomyConfig CreateOrUpdateDayOneEconomyConfig()
    {
        var assetPath = Path.Combine(EconomyFolder, "Economy_Day01_DebugFriendly.asset").Replace("\\", "/");
        var economyConfig = AssetDatabase.LoadAssetAtPath<EconomyConfig>(assetPath);

        if (economyConfig == null)
        {
            economyConfig = ScriptableObject.CreateInstance<EconomyConfig>();
            AssetDatabase.CreateAsset(economyConfig, assetPath);
        }

        economyConfig.initialMoney = 100;
        economyConfig.initialDebt = 0;
        economyConfig.payPerCorrectDecision = 10;
        economyConfig.penaltyPerMistake = 5;
        economyConfig.dailyExpenses = 0;
        economyConfig.warningLimit = 3;
        economyConfig.autoPayDebtFromMoney = false;

        EditorUtility.SetDirty(economyConfig);
        return economyConfig;
    }

    private static void EnsureFolder(string parentFolder, string childFolder)
    {
        if (!AssetDatabase.IsValidFolder(parentFolder))
        {
            Debug.LogError($"Pasta base nao encontrada: {parentFolder}");
            return;
        }

        var targetFolder = $"{parentFolder}/{childFolder}";
        if (!AssetDatabase.IsValidFolder(targetFolder))
        {
            AssetDatabase.CreateFolder(parentFolder, childFolder);
        }
    }
}
