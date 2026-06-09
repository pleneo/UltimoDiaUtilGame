using System.IO;
using UnityEditor;
using UnityEngine;

public static class WithdrawalPaymentDayAssetCreator
{
    private const string DocumentsFolder = "Assets/_Project/ScriptableObjects/Documents";
    private const string CasesFolder = "Assets/_Project/ScriptableObjects/Cases/Day02";
    private const string DaysFolder = "Assets/_Project/ScriptableObjects/Days";
    private const string RulesFolder = "Assets/_Project/ScriptableObjects/Rules";
    private const string EconomyFolder = "Assets/_Project/ScriptableObjects/Economy";

    [MenuItem("Tools/Ultimo Dia Util/Day 2/Create Withdrawal Payment Documents")]
    public static void CreateWithdrawalPaymentDocuments()
    {
        EnsureFolder("Assets/_Project/ScriptableObjects", "Documents");

        CreateOrUpdateDocument(
            "Document_WithdrawalForm.asset",
            DocumentType.WithdrawalForm,
            "Formulario de Trancamento",
            "Formulario do pedido de trancamento de disciplina.");

        CreateOrUpdateDocument(
            "Document_PaymentReceipt.asset",
            DocumentType.PaymentReceipt,
            "Via da Maquininha",
            "Comprovante emitido pela maquininha de pagamento.");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Ultimo Dia Util/Day 2/Create Withdrawal Payment Cases")]
    public static void CreateWithdrawalPaymentCases()
    {
        EnrollmentDayAssetCreator.CreateEnrollmentDocuments();
        CreateWithdrawalPaymentDocuments();
        EnsureFolder("Assets/_Project/ScriptableObjects/Cases", "Day02");

        var identityCard = LoadDocument("Document_IdentityCard.asset");
        var enrollmentProof = LoadDocument("Document_EnrollmentProof.asset");
        var withdrawalForm = LoadDocument("Document_WithdrawalForm.asset");
        var paymentReceipt = LoadDocument("Document_PaymentReceipt.asset");

        if (identityCard == null || enrollmentProof == null || withdrawalForm == null || paymentReceipt == null)
        {
            Debug.LogError("Nao foi possivel criar os casos do Dia 2 porque faltam DocumentDefinitions.");
            return;
        }

        CreateOrUpdateAuthorizedWithdrawalCase(identityCard, enrollmentProof, withdrawalForm, paymentReceipt);
        CreateOrUpdateUnauthorizedWithdrawalCase(identityCard, enrollmentProof, withdrawalForm, paymentReceipt);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Ultimo Dia Util/Day 2/Create Withdrawal Payment Day Config")]
    public static void CreateWithdrawalPaymentDayConfig()
    {
        CreateWithdrawalPaymentCases();
        EnsureFolder("Assets/_Project/ScriptableObjects", "Days");
        EnsureFolder("Assets/_Project/ScriptableObjects", "Rules");

        var validCase = LoadCase("Case_Day02_Withdrawal_Authorized.asset");
        var invalidCase = LoadCase("Case_Day02_Withdrawal_Unauthorized.asset");
        if (validCase == null || invalidCase == null)
        {
            Debug.LogError("Nao foi possivel criar o DayConfig do Dia 2 porque faltam casos.");
            return;
        }

        var economyConfig = AssetDatabase.LoadAssetAtPath<EconomyConfig>(Path.Combine(EconomyFolder, "Economy_Day02.asset").Replace("\\", "/"));
        var rule = CreateOrUpdateWithdrawalPaymentRule();
        var dayConfig = LoadOrCreateDayConfig("Day_02_WithdrawalPayments.asset");

        dayConfig.dayNumber = 2;
        dayConfig.dayLabel = "Dia 2";
        dayConfig.dayIntro = "Hoje, pedidos de trancamento exigem taxa paga na maquininha.";
        dayConfig.workDurationSeconds = 330f;
        dayConfig.economyConfig = economyConfig;
        dayConfig.useInfiniteGeneratedCases = false;
        dayConfig.enrollmentGenerationConfig = null;
        dayConfig.maxCasesForDay = 5;
        dayConfig.shuffleCaseQueue = true;
        dayConfig.includeManualCases = true;

        dayConfig.availableRequestTypes.Clear();
        dayConfig.availableRequestTypes.Add(RequestType.ClassWithdrawal);

        dayConfig.rulebookEntries.Clear();
        dayConfig.rulebookEntries.Add(rule);

        dayConfig.noticeBoardEntries.Clear();
        dayConfig.cases.Clear();
        dayConfig.cases.Add(validCase);
        dayConfig.cases.Add(invalidCase);

        EditorUtility.SetDirty(dayConfig);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CreateOrUpdateAuthorizedWithdrawalCase(
        DocumentDefinition identityCard,
        DocumentDefinition enrollmentProof,
        DocumentDefinition withdrawalForm,
        DocumentDefinition paymentReceipt)
    {
        var studentCase = LoadOrCreateCase("Case_Day02_Withdrawal_Authorized.asset");
        ConfigureBaseWithdrawalCase(
            studentCase,
            "day02_withdrawal_authorized",
            "Trancamento com pagamento autorizado",
            "Marina Costa",
            "Quero trancar a disciplina de Cálculo II.",
            "O pedido de trancamento está em ordem e a maquininha autorizou o pagamento.",
            1);

        studentCase.paymentReceiptTemplate = CreatePaymentReceipt(paymentReceipt, "Marina Costa", "123.456.789-00", 50, "autorizado");

        studentCase.documents.Clear();
        studentCase.documents.Add(CreateIdentityCard(identityCard, "Marina Costa", "123.456.789-00"));
        studentCase.documents.Add(CreateEnrollmentProof(enrollmentProof, "Marina Costa", "2026017", "Ciencia da Computacao"));
        studentCase.documents.Add(CreateWithdrawalForm(withdrawalForm, "Marina Costa", "2026017", "Ciencia da Computacao"));

        EditorUtility.SetDirty(studentCase);
    }

    private static void CreateOrUpdateUnauthorizedWithdrawalCase(
        DocumentDefinition identityCard,
        DocumentDefinition enrollmentProof,
        DocumentDefinition withdrawalForm,
        DocumentDefinition paymentReceipt)
    {
        var studentCase = LoadOrCreateCase("Case_Day02_Withdrawal_Unauthorized.asset");
        ConfigureBaseWithdrawalCase(
            studentCase,
            "day02_withdrawal_unauthorized",
            "Trancamento com pagamento nao autorizado",
            "Marina Costa",
            "Preciso trancar a disciplina hoje.",
            "Os documentos estão corretos, mas o pagamento voltou nao autorizado.",
            1);

        studentCase.paymentReceiptTemplate = CreatePaymentReceipt(paymentReceipt, "Marina Costa", "123.456.789-00", 50, "nao autorizado");

        studentCase.documents.Clear();
        studentCase.documents.Add(CreateIdentityCard(identityCard, "Marina Costa", "123.456.789-00"));
        studentCase.documents.Add(CreateEnrollmentProof(enrollmentProof, "Marina Costa", "2026017", "Ciencia da Computacao"));
        studentCase.documents.Add(CreateWithdrawalForm(withdrawalForm, "Marina Costa", "2026017", "Ciencia da Computacao"));

        EditorUtility.SetDirty(studentCase);
    }

    private static void ConfigureBaseWithdrawalCase(
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
        studentCase.requestType = RequestType.ClassWithdrawal;
        studentCase.npcDialogue = npcDialogue;
        CaseDialogueMockLibrary.ApplyMockDialogue(studentCase, mockDialogueIndex);
        studentCase.caseSummary = caseSummary;
        studentCase.referenceDateIso = "2026-05-22";
        studentCase.requiresSupervisorReview = false;
        studentCase.hasDecisionOverride = false;
        studentCase.overriddenCorrectDecision = DecisionType.Approve;
        studentCase.overrideReason = string.Empty;
        studentCase.mistakeIsCritical = true;
        studentCase.decisionDocumentType = DocumentType.WithdrawalForm;
        studentCase.requiresPaymentProcessing = true;
        studentCase.expectedPaymentAmount = 50;

        studentCase.requiredDocuments.Clear();
        studentCase.requiredDocuments.Add(CreateRequirement(DocumentType.IdentityCard, "nome", "cpf"));
        studentCase.requiredDocuments.Add(CreateRequirement(DocumentType.EnrollmentProof, "nome", "ra"));
        studentCase.requiredDocuments.Add(CreateRequirement(DocumentType.WithdrawalForm, "nome", "ra", "curso"));
        studentCase.requiredDocuments.Add(CreateRequirement(DocumentType.PaymentReceipt, "nome", "cpf", "valor", "status"));

        studentCase.comparisonRules.Clear();
        studentCase.comparisonRules.Add(CreateComparison(
            DocumentType.IdentityCard,
            "nome",
            DocumentType.WithdrawalForm,
            "nome",
            "O nome da Carteira de Identidade deve bater com o Formulario de Trancamento."));
        studentCase.comparisonRules.Add(CreateComparison(
            DocumentType.IdentityCard,
            "cpf",
            DocumentType.PaymentReceipt,
            "cpf",
            "O CPF da Carteira de Identidade deve bater com a Via da Maquininha."));
        studentCase.comparisonRules.Add(CreateComparison(
            DocumentType.IdentityCard,
            "nome",
            DocumentType.PaymentReceipt,
            "nome",
            "O nome da Carteira de Identidade deve bater com a Via da Maquininha."));

        studentCase.expectedFieldValueRules.Clear();
        studentCase.expectedFieldValueRules.Add(new DocumentExpectedFieldValueRule
        {
            documentType = DocumentType.PaymentReceipt,
            fieldKey = "status",
            expectedValue = "autorizado",
            description = "Pagamento nao autorizado na maquininha."
        });
    }

    private static RuleDefinition CreateOrUpdateWithdrawalPaymentRule()
    {
        var assetPath = Path.Combine(RulesFolder, "Rule_Day02_WithdrawalPayment.asset").Replace("\\", "/");
        var rule = AssetDatabase.LoadAssetAtPath<RuleDefinition>(assetPath);
        if (rule == null)
        {
            rule = ScriptableObject.CreateInstance<RuleDefinition>();
            AssetDatabase.CreateAsset(rule, assetPath);
        }

        rule.ruleTitle = "Trancamento com taxa";
        rule.ruleBody = "Pedidos de trancamento exigem Carteira de Identidade, Comprovante de Matricula, Formulario de Trancamento e pagamento na maquininha. A via deve retornar com status autorizado.";
        rule.highlighted = true;
        EditorUtility.SetDirty(rule);
        return rule;
    }

    private static void CreateOrUpdateDocument(string fileName, DocumentType documentType, string displayName, string description)
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

    private static DocumentRequirement CreateRequirement(DocumentType documentType, params string[] requiredFieldKeys)
    {
        var requirement = new DocumentRequirement
        {
            documentType = documentType,
            required = true
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

    private static DocumentRecord CreateIdentityCard(DocumentDefinition definition, string name, string cpf)
    {
        var record = CreateDocumentRecord(definition);
        record.fields.Add(CreateField("nome", name));
        record.fields.Add(CreateField("cpf", cpf));
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

    private static DocumentRecord CreateWithdrawalForm(DocumentDefinition definition, string name, string ra, string course)
    {
        var record = CreateDocumentRecord(definition);
        record.fields.Add(CreateField("nome", name));
        record.fields.Add(CreateField("ra", ra));
        record.fields.Add(CreateField("curso", course));
        record.notes = "Pedido de trancamento de disciplina.";
        return record;
    }

    private static DocumentRecord CreatePaymentReceipt(DocumentDefinition definition, string name, string cpf, int value, string status)
    {
        var record = CreateDocumentRecord(definition);
        record.fields.Add(CreateField("nome", name));
        record.fields.Add(CreateField("cpf", cpf));
        record.fields.Add(CreateField("valor", value.ToString()));
        record.fields.Add(CreateField("status", status));
        record.notes = "Via devolvida pela maquininha.";
        return record;
    }

    private static DocumentRecord CreateDocumentRecord(DocumentDefinition definition)
    {
        return new DocumentRecord
        {
            definition = definition,
            hasValidStamp = true,
            issueDateIso = "2026-05-22",
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

    private static void EnsureFolder(string parentFolder, string childFolder)
    {
        var combinedPath = Path.Combine(parentFolder, childFolder).Replace("\\", "/");
        if (!AssetDatabase.IsValidFolder(combinedPath))
        {
            AssetDatabase.CreateFolder(parentFolder, childFolder);
        }
    }
}
