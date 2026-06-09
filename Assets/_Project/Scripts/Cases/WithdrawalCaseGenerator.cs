using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gerador estático de casos de trancamento de disciplina (ClassWithdrawal).
/// Análogo ao EnrollmentCaseGenerator, mas para o fluxo de pagamento via maquininha.
///
/// Tipos gerados:
///   AuthorizedPayment   → pagamento aprovado → decisão correta: Aprovar
///   UnauthorizedPayment → pagamento negado   → decisão correta: Rejeitar
///   MissingWithdrawalForm → formulário faltando (sem maquininha) → Rejeitar
/// </summary>
public static class WithdrawalCaseGenerator
{
    // -------------------------------------------------------------------------
    // API pública
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gera uma lista de casos de trancamento conforme a configuração.
    /// Retorna lista vazia se config for null.
    /// </summary>
    public static List<StudentCaseDefinition> GenerateCases(WithdrawalCaseGenerationConfig config)
    {
        var cases = new List<StudentCaseDefinition>();
        if (config == null)
        {
            return cases;
        }

        var random = config.useFreshRandomSeed
            ? new System.Random()
            : new System.Random(config.randomSeed);

        AddMinimumCases(config, random, cases);
        AddWeightedCases(config, random, cases);

        if (config.shuffleGeneratedCases)
        {
            Shuffle(cases, random);
        }

        return cases;
    }

    /// <summary>
    /// Gera um único caso de trancamento para uso em fluxo infinito.
    /// </summary>
    public static StudentCaseDefinition GenerateSingleCase(
        WithdrawalCaseGenerationConfig config,
        System.Random random,
        int generatedIndex)
    {
        if (config == null)
        {
            return null;
        }

        random ??= config.useFreshRandomSeed
            ? new System.Random()
            : new System.Random(config.randomSeed);

        var caseType = PickWeightedCaseType(config, random);
        return CreateWithdrawalCase(caseType, config, random, generatedIndex);
    }

    // -------------------------------------------------------------------------
    // Geração por quantidade
    // -------------------------------------------------------------------------

    private static void AddMinimumCases(
        WithdrawalCaseGenerationConfig config,
        System.Random random,
        List<StudentCaseDefinition> cases)
    {
        if (config.minimumCases == null)
        {
            return;
        }

        for (var index = 0; index < config.minimumCases.Count; index++)
        {
            var rule = config.minimumCases[index];
            if (rule == null)
            {
                continue;
            }

            var count = Mathf.Max(0, rule.minimumCount);
            for (var i = 0; i < count && cases.Count < config.totalGeneratedCases; i++)
            {
                cases.Add(CreateWithdrawalCase(rule.caseType, config, random, cases.Count + 1));
            }
        }
    }

    private static void AddWeightedCases(
        WithdrawalCaseGenerationConfig config,
        System.Random random,
        List<StudentCaseDefinition> cases)
    {
        while (cases.Count < config.totalGeneratedCases)
        {
            var caseType = PickWeightedCaseType(config, random);
            cases.Add(CreateWithdrawalCase(caseType, config, random, cases.Count + 1));
        }
    }

    // -------------------------------------------------------------------------
    // Criação de um caso individual
    // -------------------------------------------------------------------------

    private static StudentCaseDefinition CreateWithdrawalCase(
        WithdrawalGeneratedCaseType caseType,
        WithdrawalCaseGenerationConfig config,
        System.Random random,
        int generatedIndex)
    {
        var visualProfile = PickVisualProfile(config, random);
        var studentName = PickFullName(config, random, visualProfile);
        var course = PickValue(config.courses, random, "Ciencia da Computacao");
        var refDate = string.IsNullOrWhiteSpace(config.referenceDateIso)
            ? "2026-05-21"
            : config.referenceDateIso;

        var studentCase = ScriptableObject.CreateInstance<StudentCaseDefinition>();
        studentCase.caseId = $"generated_withdrawal_{generatedIndex:00}_{caseType}";
        studentCase.caseTitle = BuildCaseTitle(caseType);
        studentCase.applicantName = studentName;
        studentCase.requestType = RequestType.ClassWithdrawal;
        studentCase.referenceDateIso = refDate;
        studentCase.requiresSupervisorReview = false;
        studentCase.hasDecisionOverride = false;
        studentCase.mistakeIsCritical = true;
        studentCase.npcDefinition = PickNpcDefinition(config, random, visualProfile);

        // Decisão e fluxo de pagamento
        ApplyWithdrawalCaseSettings(studentCase, caseType, config, studentName, course, refDate);

        return studentCase;
    }

    private static void ApplyWithdrawalCaseSettings(
        StudentCaseDefinition studentCase,
        WithdrawalGeneratedCaseType caseType,
        WithdrawalCaseGenerationConfig config,
        string studentName,
        string course,
        string refDate)
    {
        studentCase.decisionDocumentType = DocumentType.WithdrawalForm;

        switch (caseType)
        {
            case WithdrawalGeneratedCaseType.AuthorizedPayment:
                ApplyAuthorizedPaymentCase(studentCase, config, studentName, course, refDate);
                break;

            case WithdrawalGeneratedCaseType.UnauthorizedPayment:
                ApplyUnauthorizedPaymentCase(studentCase, config, studentName, course, refDate);
                break;

            case WithdrawalGeneratedCaseType.MissingWithdrawalForm:
                ApplyMissingFormCase(studentCase, config, studentName, refDate);
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Casos específicos
    // -------------------------------------------------------------------------

    /// <summary>Pagamento aprovado → deve APROVAR.</summary>
    private static void ApplyAuthorizedPaymentCase(
        StudentCaseDefinition studentCase,
        WithdrawalCaseGenerationConfig config,
        string studentName,
        string course,
        string refDate)
    {
        studentCase.requiresPaymentProcessing = true;
        studentCase.expectedPaymentAmount = config.paymentAmount;
        studentCase.caseSummary = "Trancamento com pagamento autorizado. Deve ser aprovado.";

        // Template da via: status autorizado
        studentCase.paymentReceiptTemplate = BuildPaymentReceiptRecord(
            config, studentName, config.paymentAmount, "autorizado", refDate);

        // Documentos iniciais (sem a via — ela é gerada pela maquininha)
        AddWithdrawalDocuments(studentCase, config, studentName, course, refDate, includeMissingForm: false);

        // Regra de validação: status deve ser "autorizado"
        studentCase.expectedFieldValueRules.Add(new DocumentExpectedFieldValueRule
        {
            documentType = DocumentType.PaymentReceipt,
            fieldKey = "status",
            expectedValue = "autorizado"
        });
    }

    /// <summary>Pagamento negado → deve REJEITAR.</summary>
    private static void ApplyUnauthorizedPaymentCase(
        StudentCaseDefinition studentCase,
        WithdrawalCaseGenerationConfig config,
        string studentName,
        string course,
        string refDate)
    {
        studentCase.requiresPaymentProcessing = true;
        studentCase.expectedPaymentAmount = config.paymentAmount;
        studentCase.caseSummary = "Trancamento com pagamento negado. Deve ser rejeitado.";

        // Template da via: status nao autorizado
        studentCase.paymentReceiptTemplate = BuildPaymentReceiptRecord(
            config, studentName, config.paymentAmount, "nao autorizado", refDate);

        // O override garante que a decisão correta é Rejeitar
        studentCase.hasDecisionOverride = true;
        studentCase.overriddenCorrectDecision = DecisionType.Reject;
        studentCase.overrideReason = "Pagamento nao autorizado.";

        AddWithdrawalDocuments(studentCase, config, studentName, course, refDate, includeMissingForm: false);

        studentCase.expectedFieldValueRules.Add(new DocumentExpectedFieldValueRule
        {
            documentType = DocumentType.PaymentReceipt,
            fieldKey = "status",
            expectedValue = "autorizado"
        });
    }

    /// <summary>Formulário de trancamento faltando → deve REJEITAR (sem maquininha).</summary>
    private static void ApplyMissingFormCase(
        StudentCaseDefinition studentCase,
        WithdrawalCaseGenerationConfig config,
        string studentName,
        string refDate)
    {
        studentCase.requiresPaymentProcessing = false;
        studentCase.caseSummary = "Formulário de trancamento ausente. Deve ser rejeitado.";

        // Exige o formulário, mas não o fornece → missing document
        studentCase.requiredDocuments.Add(new DocumentRequirement
        {
            documentType = DocumentType.WithdrawalForm,
            required = true,
            requiredFieldKeys = new System.Collections.Generic.List<string> { "nome" }
        });
        studentCase.requiredDocuments.Add(new DocumentRequirement
        {
            documentType = DocumentType.IdentityCard,
            required = true,
            requiredFieldKeys = new System.Collections.Generic.List<string> { "nome" }
        });

        // Apenas carteira de identidade — sem formulário
        var identityCard = new DocumentRecord
        {
            definition = config.identityCardDefinition,
            hasValidStamp = true,
            issueDateIso = refDate,
            expiryDateIso = "2028-12-31"
        };
        identityCard.fields.Add(new DocumentField { key = "nome", value = studentName });
        studentCase.documents.Add(identityCard);
    }

    // -------------------------------------------------------------------------
    // Helpers de documento
    // -------------------------------------------------------------------------

    private static void AddWithdrawalDocuments(
        StudentCaseDefinition studentCase,
        WithdrawalCaseGenerationConfig config,
        string studentName,
        string course,
        string refDate,
        bool includeMissingForm)
    {
        // Documento de identidade
        var identityCard = new DocumentRecord
        {
            definition = config.identityCardDefinition,
            hasValidStamp = true,
            issueDateIso = refDate,
            expiryDateIso = "2028-12-31"
        };
        identityCard.fields.Add(new DocumentField { key = "nome", value = studentName });
        studentCase.documents.Add(identityCard);

        // Formulário de trancamento
        if (!includeMissingForm)
        {
            var withdrawalForm = new DocumentRecord
            {
                definition = config.withdrawalFormDefinition,
                hasValidStamp = true,
                issueDateIso = refDate
            };
            withdrawalForm.fields.Add(new DocumentField { key = "nome", value = studentName });
            withdrawalForm.fields.Add(new DocumentField { key = "curso", value = course });
            studentCase.documents.Add(withdrawalForm);
        }

        // Exigências
        studentCase.requiredDocuments.Add(new DocumentRequirement
        {
            documentType = DocumentType.IdentityCard,
            required = true,
            requiredFieldKeys = new System.Collections.Generic.List<string> { "nome" }
        });
        studentCase.requiredDocuments.Add(new DocumentRequirement
        {
            documentType = DocumentType.WithdrawalForm,
            required = true,
            requiredFieldKeys = new System.Collections.Generic.List<string> { "nome" }
        });
        studentCase.requiredDocuments.Add(new DocumentRequirement
        {
            documentType = DocumentType.PaymentReceipt,
            required = true,
            requiredFieldKeys = new System.Collections.Generic.List<string> { "status" }
        });
    }

    private static DocumentRecord BuildPaymentReceiptRecord(
        WithdrawalCaseGenerationConfig config,
        string studentName,
        int amount,
        string status,
        string refDate)
    {
        var receipt = new DocumentRecord
        {
            definition = config.paymentReceiptDefinition,
            hasValidStamp = true,
            issueDateIso = refDate,
            notes = "Via devolvida pela maquininha."
        };
        receipt.fields.Add(new DocumentField { key = "nome", value = studentName });
        receipt.fields.Add(new DocumentField { key = "valor", value = amount.ToString() });
        receipt.fields.Add(new DocumentField { key = "status", value = status });
        return receipt;
    }

    // -------------------------------------------------------------------------
    // Seleção ponderada de tipo de caso
    // -------------------------------------------------------------------------

    private static WithdrawalGeneratedCaseType PickWeightedCaseType(
        WithdrawalCaseGenerationConfig config,
        System.Random random)
    {
        if (config.caseTypeWeights == null || config.caseTypeWeights.Count == 0)
        {
            return WithdrawalGeneratedCaseType.AuthorizedPayment;
        }

        var totalWeight = 0;
        for (var index = 0; index < config.caseTypeWeights.Count; index++)
        {
            var entry = config.caseTypeWeights[index];
            if (entry != null)
            {
                totalWeight += Mathf.Max(0, entry.weight);
            }
        }

        if (totalWeight <= 0)
        {
            return WithdrawalGeneratedCaseType.AuthorizedPayment;
        }

        var roll = random.Next(0, totalWeight);
        var current = 0;
        for (var index = 0; index < config.caseTypeWeights.Count; index++)
        {
            var entry = config.caseTypeWeights[index];
            if (entry == null || entry.weight <= 0)
            {
                continue;
            }

            current += entry.weight;
            if (roll < current)
            {
                return entry.caseType;
            }
        }

        return WithdrawalGeneratedCaseType.AuthorizedPayment;
    }

    // -------------------------------------------------------------------------
    // Nomes e NPCs
    // -------------------------------------------------------------------------

    private static string PickFullName(
        WithdrawalCaseGenerationConfig config,
        System.Random random,
        NpcVisualProfile visualProfile)
    {
        var firstName = PickFirstNameByProfile(config, random, visualProfile);
        var lastName = PickValue(config.lastNames, random, "Santos");
        return $"{firstName} {lastName}";
    }

    private static string PickFirstNameByProfile(
        WithdrawalCaseGenerationConfig config,
        System.Random random,
        NpcVisualProfile visualProfile)
    {
        if (config.useProfiledNpcGeneration)
        {
            var profiledNames = visualProfile == NpcVisualProfile.Feminino
                ? config.femaleFirstNames
                : config.maleFirstNames;

            var profiledFallback = visualProfile == NpcVisualProfile.Feminino ? "Ana" : "Carlos";
            var picked = PickValue(profiledNames, random, string.Empty);
            if (!string.IsNullOrWhiteSpace(picked))
            {
                return picked;
            }
        }

        return PickValue(config.firstNames, random, "Estudante");
    }

    private static NpcDefinition PickNpcDefinition(
        WithdrawalCaseGenerationConfig config,
        System.Random random,
        NpcVisualProfile visualProfile)
    {
        if (!config.useProfiledNpcGeneration)
        {
            return null;
        }

        var pool = visualProfile == NpcVisualProfile.Feminino
            ? config.femaleNpcDefinitions
            : config.maleNpcDefinitions;

        if (pool == null || pool.Count == 0)
        {
            return null;
        }

        var validPool = new System.Collections.Generic.List<NpcDefinition>();
        for (var index = 0; index < pool.Count; index++)
        {
            if (pool[index] != null)
            {
                validPool.Add(pool[index]);
            }
        }

        return validPool.Count == 0 ? null : validPool[random.Next(0, validPool.Count)];
    }

    private static NpcVisualProfile PickVisualProfile(
        WithdrawalCaseGenerationConfig config,
        System.Random random)
    {
        if (!config.useProfiledNpcGeneration)
        {
            return NpcVisualProfile.Masculino;
        }

        var hasMale = HasAnyNpc(config.maleNpcDefinitions) || HasAnyString(config.maleFirstNames);
        var hasFemale = HasAnyNpc(config.femaleNpcDefinitions) || HasAnyString(config.femaleFirstNames);

        if (hasMale && hasFemale)
        {
            return random.Next(0, 2) == 0 ? NpcVisualProfile.Masculino : NpcVisualProfile.Feminino;
        }

        return hasFemale ? NpcVisualProfile.Feminino : NpcVisualProfile.Masculino;
    }

    // -------------------------------------------------------------------------
    // Strings e textos
    // -------------------------------------------------------------------------

    private static string BuildCaseTitle(WithdrawalGeneratedCaseType caseType)
    {
        return caseType switch
        {
            WithdrawalGeneratedCaseType.AuthorizedPayment => "Trancamento - pagamento autorizado",
            WithdrawalGeneratedCaseType.UnauthorizedPayment => "Trancamento - pagamento negado",
            WithdrawalGeneratedCaseType.MissingWithdrawalForm => "Trancamento - formulário faltando",
            _ => "Trancamento gerado"
        };
    }

    // -------------------------------------------------------------------------
    // Utilitários
    // -------------------------------------------------------------------------

    private static string PickValue(System.Collections.Generic.IReadOnlyList<string> values, System.Random random, string fallback)
    {
        if (values == null || values.Count == 0)
        {
            return fallback;
        }

        for (var attempt = 0; attempt < values.Count; attempt++)
        {
            var value = values[random.Next(0, values.Count)];
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return fallback;
    }

    private static bool HasAnyNpc(System.Collections.Generic.IReadOnlyList<NpcDefinition> values)
    {
        if (values == null)
        {
            return false;
        }

        for (var index = 0; index < values.Count; index++)
        {
            if (values[index] != null)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasAnyString(System.Collections.Generic.IReadOnlyList<string> values)
    {
        if (values == null)
        {
            return false;
        }

        for (var index = 0; index < values.Count; index++)
        {
            if (!string.IsNullOrWhiteSpace(values[index]))
            {
                return true;
            }
        }

        return false;
    }

    private static void Shuffle<T>(System.Collections.Generic.IList<T> values, System.Random random)
    {
        for (var index = values.Count - 1; index > 0; index--)
        {
            var swapIndex = random.Next(0, index + 1);
            (values[index], values[swapIndex]) = (values[swapIndex], values[index]);
        }
    }
}
