using System.IO;
using UnityEditor;
using UnityEngine;

public static class EnrollmentDayAssetCreator
{
    private const string DocumentsFolder = "Assets/_Project/ScriptableObjects/Documents";
    private const string CasesFolder = "Assets/_Project/ScriptableObjects/Cases/Day01";
    private const string GenerationFolder = "Assets/_Project/ScriptableObjects/Cases/Generation";
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
        EnsureFolder("Assets/_Project/ScriptableObjects/Cases", "Generation");

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
        dayConfig.useInfiniteGeneratedCases = false;
        dayConfig.enrollmentGenerationConfig = null;
        dayConfig.maxCasesForDay = 5;
        dayConfig.shuffleCaseQueue = true;
        dayConfig.includeManualCases = true;

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

    [MenuItem("Tools/Ultimo Dia Util/Create 5 Day Enrollment Prototype")]
    public static void CreateFiveDayEnrollmentPrototype()
    {
        CreateEnrollmentCases();
        EnsureFolder("Assets/_Project/ScriptableObjects", "Days");
        EnsureFolder("Assets/_Project/ScriptableObjects", "Rules");
        EnsureFolder("Assets/_Project/ScriptableObjects", "Economy");
        EnsureFolder("Assets/_Project/ScriptableObjects/Cases", "Generation");

        var enrollmentRule = CreateOrUpdateEnrollmentRule();

        for (var dayNumber = 1; dayNumber <= 5; dayNumber++)
        {
            var economyConfig = CreateOrUpdateEconomyConfigForDay(dayNumber);
            var generationConfig = CreateOrUpdateGenerationConfigForDay(dayNumber);
            var dayConfig = LoadOrCreateDayConfig($"Day_{dayNumber:00}_EnrollmentQueue.asset");

            dayConfig.dayNumber = dayNumber;
            dayConfig.dayLabel = $"Dia {dayNumber}";
            dayConfig.dayIntro = BuildDayIntro(dayNumber);
            dayConfig.workDurationSeconds = 240f + (dayNumber * 30f);
            dayConfig.economyConfig = economyConfig;
            dayConfig.useInfiniteGeneratedCases = false;
            dayConfig.enrollmentGenerationConfig = generationConfig;
            dayConfig.maxCasesForDay = generationConfig.totalGeneratedCases;
            dayConfig.shuffleCaseQueue = true;
            dayConfig.useFixedQueueSeed = false;
            dayConfig.queueRandomSeed = 1000 + dayNumber;
            dayConfig.includeManualCases = false;

            dayConfig.availableRequestTypes.Clear();
            dayConfig.availableRequestTypes.Add(RequestType.Enrollment);

            dayConfig.rulebookEntries.Clear();
            dayConfig.rulebookEntries.Add(enrollmentRule);

            dayConfig.noticeBoardEntries.Clear();
            dayConfig.cases.Clear();

            EditorUtility.SetDirty(dayConfig);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Protótipo de 5 dias criado em Assets/_Project/ScriptableObjects/Days. Arraste os 5 DayConfig para GameManager > Day Sequence.");
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
            "Todos os documentos obrigatorios foram entregues. Nome e RA batem entre os documentos.",
            0);

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
            "O historico escolar esta faltando, entao a matricula deve ser indeferida.",
            1);

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
            "O comprovante de matricula usa um nome diferente, entao a matricula deve ser indeferida.",
            2);

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
        string caseSummary,
        int mockDialogueIndex)
    {
        studentCase.caseId = caseId;
        studentCase.caseTitle = caseTitle;
        studentCase.applicantName = applicantName;
        studentCase.requestType = RequestType.Enrollment;
        studentCase.npcDialogue = npcDialogue;
        CaseDialogueMockLibrary.ApplyMockDialogue(studentCase, mockDialogueIndex);
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

    private static EconomyConfig CreateOrUpdateEconomyConfigForDay(int dayNumber)
    {
        var assetPath = Path.Combine(EconomyFolder, $"Economy_Day{dayNumber:00}.asset").Replace("\\", "/");
        var economyConfig = AssetDatabase.LoadAssetAtPath<EconomyConfig>(assetPath);

        if (economyConfig == null)
        {
            economyConfig = ScriptableObject.CreateInstance<EconomyConfig>();
            AssetDatabase.CreateAsset(economyConfig, assetPath);
        }

        economyConfig.initialMoney = dayNumber == 1 ? 120 : 0;
        economyConfig.initialDebt = dayNumber == 1 ? 300 : 0;
        economyConfig.payPerCorrectDecision = 12 + (dayNumber * 2);
        economyConfig.penaltyPerMistake = 5 + dayNumber;
        economyConfig.dailyExpenses = 4 + (dayNumber * 2);
        economyConfig.warningLimit = 4;
        economyConfig.autoPayDebtFromMoney = true;

        EditorUtility.SetDirty(economyConfig);
        return economyConfig;
    }

    private static EnrollmentCaseGenerationConfig CreateOrUpdateDayOneGenerationConfig()
    {
        var assetPath = Path.Combine(GenerationFolder, "EnrollmentGen_Day01.asset").Replace("\\", "/");
        var generationConfig = AssetDatabase.LoadAssetAtPath<EnrollmentCaseGenerationConfig>(assetPath);

        if (generationConfig == null)
        {
            generationConfig = ScriptableObject.CreateInstance<EnrollmentCaseGenerationConfig>();
            AssetDatabase.CreateAsset(generationConfig, assetPath);
        }

        generationConfig.totalGeneratedCases = 6;
        generationConfig.shuffleGeneratedCases = true;
        generationConfig.useFreshRandomSeed = false;
        generationConfig.randomSeed = 12345;
        generationConfig.identityCardDefinition = LoadDocument("Document_IdentityCard.asset");
        generationConfig.schoolTranscriptDefinition = LoadDocument("Document_SchoolTranscript.asset");
        generationConfig.enrollmentProofDefinition = LoadDocument("Document_EnrollmentProof.asset");
        generationConfig.referenceDateIso = "2026-05-21";
        generationConfig.firstRaNumber = 2026001;
        generationConfig.lastRaNumber = 2026999;

        generationConfig.minimumCases.Clear();
        generationConfig.minimumCases.Add(CreateMinimumCaseRule(EnrollmentGeneratedCaseType.Valid, 1));
        generationConfig.minimumCases.Add(CreateMinimumCaseRule(EnrollmentGeneratedCaseType.MissingDocument, 1));
        generationConfig.minimumCases.Add(CreateMinimumCaseRule(EnrollmentGeneratedCaseType.NameMismatch, 1));

        generationConfig.caseTypeWeights.Clear();
        generationConfig.caseTypeWeights.Add(CreateCaseTypeWeight(EnrollmentGeneratedCaseType.Valid, 70));
        generationConfig.caseTypeWeights.Add(CreateCaseTypeWeight(EnrollmentGeneratedCaseType.MissingDocument, 20));
        generationConfig.caseTypeWeights.Add(CreateCaseTypeWeight(EnrollmentGeneratedCaseType.NameMismatch, 10));
        generationConfig.caseTypeWeights.Add(CreateCaseTypeWeight(EnrollmentGeneratedCaseType.RaMismatch, 0));
        generationConfig.caseTypeWeights.Add(CreateCaseTypeWeight(EnrollmentGeneratedCaseType.MissingRequiredField, 0));
        generationConfig.caseTypeWeights.Add(CreateCaseTypeWeight(EnrollmentGeneratedCaseType.ExpiredDocument, 0));

        generationConfig.firstNames.Clear();
        generationConfig.firstNames.AddRange(new[]
        {
            "Ana", "Bruno", "Camila", "Diego", "Eduarda", "Felipe", "Giovana", "Henrique",
            "Isabela", "Joao", "Larissa", "Marcos", "Natalia", "Pedro", "Rafaela", "Thiago"
        });

        generationConfig.lastNames.Clear();
        generationConfig.lastNames.AddRange(new[]
        {
            "Silva", "Souza", "Oliveira", "Lima", "Costa", "Santos", "Pereira", "Almeida",
            "Ferreira", "Gomes", "Rocha", "Barbosa", "Moura", "Araujo", "Nunes", "Cardoso"
        });

        generationConfig.courses.Clear();
        generationConfig.courses.AddRange(new[]
        {
            "Ciencia da Computacao", "Direito", "Administracao", "Psicologia",
            "Engenharia Civil", "Medicina", "Arquitetura", "Design"
        });

        EditorUtility.SetDirty(generationConfig);
        return generationConfig;
    }

    private static EnrollmentCaseGenerationConfig CreateOrUpdateGenerationConfigForDay(int dayNumber)
    {
        var assetPath = Path.Combine(GenerationFolder, $"EnrollmentGen_Day{dayNumber:00}.asset").Replace("\\", "/");
        var generationConfig = AssetDatabase.LoadAssetAtPath<EnrollmentCaseGenerationConfig>(assetPath);

        if (generationConfig == null)
        {
            generationConfig = ScriptableObject.CreateInstance<EnrollmentCaseGenerationConfig>();
            AssetDatabase.CreateAsset(generationConfig, assetPath);
        }

        generationConfig.totalGeneratedCases = 4 + dayNumber;
        generationConfig.shuffleGeneratedCases = true;
        generationConfig.useFreshRandomSeed = true;
        generationConfig.randomSeed = 12000 + dayNumber;
        generationConfig.identityCardDefinition = LoadDocument("Document_IdentityCard.asset");
        generationConfig.schoolTranscriptDefinition = LoadDocument("Document_SchoolTranscript.asset");
        generationConfig.enrollmentProofDefinition = LoadDocument("Document_EnrollmentProof.asset");
        generationConfig.referenceDateIso = "2026-05-21";
        generationConfig.firstRaNumber = 2026001;
        generationConfig.lastRaNumber = 2026999;

        generationConfig.minimumCases.Clear();
        generationConfig.minimumCases.Add(CreateMinimumCaseRule(EnrollmentGeneratedCaseType.Valid, 1));
        generationConfig.minimumCases.Add(CreateMinimumCaseRule(EnrollmentGeneratedCaseType.MissingDocument, 1));
        generationConfig.minimumCases.Add(CreateMinimumCaseRule(EnrollmentGeneratedCaseType.NameMismatch, 1));

        if (dayNumber >= 3)
        {
            generationConfig.minimumCases.Add(CreateMinimumCaseRule(EnrollmentGeneratedCaseType.RaMismatch, 1));
        }

        if (dayNumber >= 4)
        {
            generationConfig.minimumCases.Add(CreateMinimumCaseRule(EnrollmentGeneratedCaseType.MissingRequiredField, 1));
        }

        if (dayNumber >= 5)
        {
            generationConfig.minimumCases.Add(CreateMinimumCaseRule(EnrollmentGeneratedCaseType.ExpiredDocument, 1));
        }

        generationConfig.caseTypeWeights.Clear();
        generationConfig.caseTypeWeights.Add(CreateCaseTypeWeight(EnrollmentGeneratedCaseType.Valid, Mathf.Max(25, 70 - (dayNumber * 8))));
        generationConfig.caseTypeWeights.Add(CreateCaseTypeWeight(EnrollmentGeneratedCaseType.MissingDocument, 18 + dayNumber));
        generationConfig.caseTypeWeights.Add(CreateCaseTypeWeight(EnrollmentGeneratedCaseType.NameMismatch, 12 + dayNumber));
        generationConfig.caseTypeWeights.Add(CreateCaseTypeWeight(EnrollmentGeneratedCaseType.RaMismatch, dayNumber >= 3 ? 12 : 0));
        generationConfig.caseTypeWeights.Add(CreateCaseTypeWeight(EnrollmentGeneratedCaseType.MissingRequiredField, dayNumber >= 4 ? 10 : 0));
        generationConfig.caseTypeWeights.Add(CreateCaseTypeWeight(EnrollmentGeneratedCaseType.ExpiredDocument, dayNumber >= 5 ? 8 : 0));

        generationConfig.firstNames.Clear();
        generationConfig.firstNames.AddRange(new[]
        {
            "Ana", "Bruno", "Camila", "Diego", "Eduarda", "Felipe", "Giovana", "Henrique",
            "Isabela", "Joao", "Larissa", "Marcos", "Natalia", "Pedro", "Rafaela", "Thiago"
        });

        generationConfig.lastNames.Clear();
        generationConfig.lastNames.AddRange(new[]
        {
            "Silva", "Souza", "Oliveira", "Lima", "Costa", "Santos", "Pereira", "Almeida",
            "Ferreira", "Gomes", "Rocha", "Barbosa", "Moura", "Araujo", "Nunes", "Cardoso"
        });

        generationConfig.courses.Clear();
        generationConfig.courses.AddRange(new[]
        {
            "Ciencia da Computacao", "Direito", "Administracao", "Psicologia",
            "Engenharia Civil", "Medicina", "Arquitetura", "Design"
        });

        EditorUtility.SetDirty(generationConfig);
        return generationConfig;
    }

    private static string BuildDayIntro(int dayNumber)
    {
        switch (dayNumber)
        {
            case 1:
                return "Primeiro dia de atendimento. Confira documentos de matricula com calma.";
            case 2:
                return "A fila aumentou. Ainda sao matriculas, mas a ordem dos alunos muda a cada expediente.";
            case 3:
                return "Agora divergencias de RA aparecem com mais frequencia. Compare os numeros com atencao.";
            case 4:
                return "Alguns comprovantes chegam incompletos. Campos obrigatorios tambem reprovam o pedido.";
            default:
                return "Ultimo dia do prototipo. Documentos vencidos entram na mistura e a pressao aumenta.";
        }
    }

    private static EnrollmentMinimumCaseRule CreateMinimumCaseRule(EnrollmentGeneratedCaseType caseType, int minimumCount)
    {
        return new EnrollmentMinimumCaseRule
        {
            caseType = caseType,
            minimumCount = minimumCount
        };
    }

    private static EnrollmentCaseTypeWeight CreateCaseTypeWeight(EnrollmentGeneratedCaseType caseType, int weight)
    {
        return new EnrollmentCaseTypeWeight
        {
            caseType = caseType,
            weight = weight
        };
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
