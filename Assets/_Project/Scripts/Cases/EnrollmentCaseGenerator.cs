using System;
using System.Collections.Generic;
using UnityEngine;

public static class EnrollmentCaseGenerator
{
    public static List<StudentCaseDefinition> GenerateCases(EnrollmentCaseGenerationConfig config)
    {
        var generatedCases = new List<StudentCaseDefinition>();
        if (config == null)
        {
            return generatedCases;
        }

        var random = config.useFreshRandomSeed
            ? new System.Random()
            : new System.Random(config.randomSeed);

        AddMinimumCases(config, random, generatedCases);
        AddWeightedCases(config, random, generatedCases);

        if (config.shuffleGeneratedCases)
        {
            Shuffle(generatedCases, random);
        }

        return generatedCases;
    }

    public static StudentCaseDefinition GenerateSingleCase(
        EnrollmentCaseGenerationConfig config,
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
        return CreateEnrollmentCase(caseType, config, random, generatedIndex);
    }

    private static void AddMinimumCases(
        EnrollmentCaseGenerationConfig config,
        System.Random random,
        List<StudentCaseDefinition> generatedCases)
    {
        if (config.minimumCases == null)
        {
            return;
        }

        for (var index = 0; index < config.minimumCases.Count; index++)
        {
            var minimumCase = config.minimumCases[index];
            if (minimumCase == null)
            {
                continue;
            }

            var count = Mathf.Max(0, minimumCase.minimumCount);
            for (var caseIndex = 0; caseIndex < count && generatedCases.Count < config.totalGeneratedCases; caseIndex++)
            {
                generatedCases.Add(CreateEnrollmentCase(minimumCase.caseType, config, random, generatedCases.Count + 1));
            }
        }
    }

    private static void AddWeightedCases(
        EnrollmentCaseGenerationConfig config,
        System.Random random,
        List<StudentCaseDefinition> generatedCases)
    {
        while (generatedCases.Count < config.totalGeneratedCases)
        {
            var caseType = PickWeightedCaseType(config, random);
            generatedCases.Add(CreateEnrollmentCase(caseType, config, random, generatedCases.Count + 1));
        }
    }

    private static EnrollmentGeneratedCaseType PickWeightedCaseType(EnrollmentCaseGenerationConfig config, System.Random random)
    {
        if (config.caseTypeWeights == null || config.caseTypeWeights.Count == 0)
        {
            return EnrollmentGeneratedCaseType.Valid;
        }

        var totalWeight = 0;
        for (var index = 0; index < config.caseTypeWeights.Count; index++)
        {
            var weight = config.caseTypeWeights[index];
            if (weight == null)
            {
                continue;
            }

            totalWeight += Mathf.Max(0, weight.weight);
        }

        if (totalWeight <= 0)
        {
            return EnrollmentGeneratedCaseType.Valid;
        }

        var roll = random.Next(0, totalWeight);
        var current = 0;
        for (var index = 0; index < config.caseTypeWeights.Count; index++)
        {
            var weight = config.caseTypeWeights[index];
            if (weight == null || weight.weight <= 0)
            {
                continue;
            }

            current += weight.weight;
            if (roll < current)
            {
                return weight.caseType;
            }
        }

        return EnrollmentGeneratedCaseType.Valid;
    }

    private static StudentCaseDefinition CreateEnrollmentCase(
        EnrollmentGeneratedCaseType caseType,
        EnrollmentCaseGenerationConfig config,
        System.Random random,
        int generatedIndex)
    {
        var visualProfile = PickVisualProfile(config, random);
        var studentName = PickFullName(config, random, visualProfile);
        var course = PickValue(config.courses, random, "Ciencia da Computacao");
        var ra = random.Next(config.firstRaNumber, config.lastRaNumber + 1).ToString();
        var wrongName = CreateWrongName(studentName, config, random);
        var wrongRa = CreateWrongRa(ra, config, random);

        var studentCase = ScriptableObject.CreateInstance<StudentCaseDefinition>();
        studentCase.caseId = $"generated_enrollment_{generatedIndex:00}_{caseType}";
        studentCase.caseTitle = BuildCaseTitle(caseType);
        studentCase.applicantName = studentName;
        studentCase.requestType = RequestType.Enrollment;
        studentCase.npcDialogue = BuildDialogue(caseType);
        studentCase.caseSummary = BuildSummary(caseType);
        studentCase.referenceDateIso = string.IsNullOrWhiteSpace(config.referenceDateIso)
            ? "2026-05-21"
            : config.referenceDateIso;
        studentCase.requiresSupervisorReview = false;
        studentCase.hasDecisionOverride = false;
        studentCase.overriddenCorrectDecision = DecisionType.Approve;
        studentCase.overrideReason = string.Empty;
        studentCase.mistakeIsCritical = true;
        studentCase.npcDefinition = PickNpcDefinition(config, random, visualProfile);

        AddEnrollmentRequirements(studentCase, caseType);
        AddEnrollmentComparisonRules(studentCase);
        AddDocuments(studentCase, config, caseType, studentName, wrongName, ra, wrongRa, course);

        return studentCase;
    }

    private static void AddEnrollmentRequirements(StudentCaseDefinition studentCase, EnrollmentGeneratedCaseType caseType)
    {
        studentCase.requiredDocuments.Clear();
        studentCase.requiredDocuments.Add(CreateRequirement(DocumentType.IdentityCard, "nome"));
        studentCase.requiredDocuments.Add(CreateRequirement(DocumentType.SchoolTranscript, "nome", "ra"));

        var enrollmentProofRequirement = caseType == EnrollmentGeneratedCaseType.MissingRequiredField
            ? CreateRequirement(DocumentType.EnrollmentProof, "nome", "ra", "curso")
            : CreateRequirement(DocumentType.EnrollmentProof, "nome", "ra");

        studentCase.requiredDocuments.Add(enrollmentProofRequirement);

        if (caseType == EnrollmentGeneratedCaseType.ExpiredDocument)
        {
            studentCase.requiredDocuments[0].requiresNonExpiredDate = true;
        }
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

    private static void AddDocuments(
        StudentCaseDefinition studentCase,
        EnrollmentCaseGenerationConfig config,
        EnrollmentGeneratedCaseType caseType,
        string studentName,
        string wrongName,
        string ra,
        string wrongRa,
        string course)
    {
        studentCase.documents.Clear();

        if (caseType != EnrollmentGeneratedCaseType.MissingDocument)
        {
            var identityCard = CreateIdentityCard(config.identityCardDefinition, studentName);
            if (caseType == EnrollmentGeneratedCaseType.ExpiredDocument)
            {
                identityCard.expiryDateIso = "2025-01-01";
            }

            studentCase.documents.Add(identityCard);
        }

        studentCase.documents.Add(CreateSchoolTranscript(config.schoolTranscriptDefinition, studentName, ra, course));

        var proofName = caseType == EnrollmentGeneratedCaseType.NameMismatch ? wrongName : studentName;
        var proofRa = caseType == EnrollmentGeneratedCaseType.RaMismatch ? wrongRa : ra;
        var proof = CreateEnrollmentProof(config.enrollmentProofDefinition, proofName, proofRa, course);

        if (caseType == EnrollmentGeneratedCaseType.MissingRequiredField)
        {
            RemoveField(proof, "curso");
        }

        studentCase.documents.Add(proof);
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
        record.expiryDateIso = "2028-12-31";
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

    private static void RemoveField(DocumentRecord document, string key)
    {
        if (document.fields == null)
        {
            return;
        }

        document.fields.RemoveAll(field =>
            field != null && string.Equals(field.key, key, StringComparison.OrdinalIgnoreCase));
    }

    private static string PickFullName(EnrollmentCaseGenerationConfig config, System.Random random, NpcVisualProfile visualProfile)
    {
        var firstName = PickFirstNameByProfile(config, random, visualProfile);
        var lastName = PickValue(config.lastNames, random, "Silva");
        return $"{firstName} {lastName}";
    }

    private static string PickFirstNameByProfile(EnrollmentCaseGenerationConfig config, System.Random random, NpcVisualProfile visualProfile)
    {
        if (config != null && config.useProfiledNpcGeneration)
        {
            var profiledNames = visualProfile == NpcVisualProfile.Feminino
                ? config.femaleFirstNames
                : config.maleFirstNames;

            var profiledFallback = visualProfile == NpcVisualProfile.Feminino ? "Ana" : "Joao";
            var profiledName = PickValue(profiledNames, random, string.Empty);
            if (!string.IsNullOrWhiteSpace(profiledName))
            {
                return profiledName;
            }

            var fallbackName = PickValue(config.firstNames, random, profiledFallback);
            if (!string.IsNullOrWhiteSpace(fallbackName))
            {
                return fallbackName;
            }

            return profiledFallback;
        }

        return PickValue(config != null ? config.firstNames : null, random, "Aluno");
    }

    private static string PickValue(IReadOnlyList<string> values, System.Random random, string fallback)
    {
        if (values == null || values.Count == 0)
        {
            return fallback;
        }

        var attempts = values.Count;
        for (var attempt = 0; attempt < attempts; attempt++)
        {
            var value = values[random.Next(0, values.Count)];
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return fallback;
    }

    private static string CreateWrongName(string studentName, EnrollmentCaseGenerationConfig config, System.Random random)
    {
        var firstSpace = studentName.IndexOf(' ');
        if (firstSpace > 0)
        {
            var firstName = studentName.Substring(0, firstSpace);
            var currentLastName = studentName.Substring(firstSpace + 1);
            var replacementLastName = PickDifferentValue(config.lastNames, random, currentLastName, "Souza");
            return $"{firstName} {replacementLastName}";
        }

        return $"{studentName} Filho";
    }

    private static string PickDifferentValue(IReadOnlyList<string> values, System.Random random, string currentValue, string fallback)
    {
        if (values != null && values.Count > 0)
        {
            var attempts = values.Count * 2;
            for (var attempt = 0; attempt < attempts; attempt++)
            {
                var value = values[random.Next(0, values.Count)];
                if (!string.IsNullOrWhiteSpace(value) &&
                    !string.Equals(value.Trim(), currentValue, StringComparison.OrdinalIgnoreCase))
                {
                    return value.Trim();
                }
            }
        }

        return string.Equals(fallback, currentValue, StringComparison.OrdinalIgnoreCase)
            ? $"{fallback} Filho"
            : fallback;
    }

    private static string CreateWrongRa(string ra, EnrollmentCaseGenerationConfig config, System.Random random)
    {
        var wrongRa = ra;
        var attempts = 5;
        while (wrongRa == ra && attempts > 0)
        {
            wrongRa = random.Next(config.firstRaNumber, config.lastRaNumber + 1).ToString();
            attempts--;
        }

        return wrongRa == ra ? $"{ra}9" : wrongRa;
    }

    private static NpcVisualProfile PickVisualProfile(EnrollmentCaseGenerationConfig config, System.Random random)
    {
        if (config == null || !config.useProfiledNpcGeneration)
        {
            return NpcVisualProfile.Masculino;
        }

        var hasMale = HasAnyValue(config.maleFirstNames) || HasAnyNpc(config.maleNpcDefinitions);
        var hasFemale = HasAnyValue(config.femaleFirstNames) || HasAnyNpc(config.femaleNpcDefinitions);

        if (hasMale && hasFemale)
        {
            return random.Next(0, 2) == 0 ? NpcVisualProfile.Masculino : NpcVisualProfile.Feminino;
        }

        if (hasFemale)
        {
            return NpcVisualProfile.Feminino;
        }

        return NpcVisualProfile.Masculino;
    }

    private static NpcDefinition PickNpcDefinition(EnrollmentCaseGenerationConfig config, System.Random random, NpcVisualProfile visualProfile)
    {
        if (config == null || !config.useProfiledNpcGeneration)
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

        var validPool = new List<NpcDefinition>();
        for (var index = 0; index < pool.Count; index++)
        {
            if (pool[index] != null)
            {
                validPool.Add(pool[index]);
            }
        }

        if (validPool.Count == 0)
        {
            return null;
        }

        return validPool[random.Next(0, validPool.Count)];
    }

    private static bool HasAnyValue(IReadOnlyList<string> values)
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

    private static bool HasAnyNpc(IReadOnlyList<NpcDefinition> values)
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

    private static string BuildCaseTitle(EnrollmentGeneratedCaseType caseType)
    {
        return caseType switch
        {
            EnrollmentGeneratedCaseType.Valid => "Matricula gerada regular",
            EnrollmentGeneratedCaseType.MissingDocument => "Matricula gerada com documento faltando",
            EnrollmentGeneratedCaseType.NameMismatch => "Matricula gerada com nome divergente",
            EnrollmentGeneratedCaseType.RaMismatch => "Matricula gerada com RA divergente",
            EnrollmentGeneratedCaseType.MissingRequiredField => "Matricula gerada com campo faltando",
            EnrollmentGeneratedCaseType.ExpiredDocument => "Matricula gerada com documento vencido",
            _ => "Matricula gerada"
        };
    }

    private static string BuildDialogue(EnrollmentGeneratedCaseType caseType)
    {
        return caseType switch
        {
            EnrollmentGeneratedCaseType.Valid => "Trouxe tudo que pediram para a matricula.",
            EnrollmentGeneratedCaseType.MissingDocument => "Acho que esta tudo ai, nao tive tempo de conferir.",
            EnrollmentGeneratedCaseType.NameMismatch => "Os documentos estao certos, so muda uma coisinha no nome.",
            EnrollmentGeneratedCaseType.RaMismatch => "Meu RA esta nos documentos, pode conferir.",
            EnrollmentGeneratedCaseType.MissingRequiredField => "O comprovante saiu meio incompleto, mas deve servir.",
            EnrollmentGeneratedCaseType.ExpiredDocument => "Esse documento e antigo, mas ainda da para usar, ne?",
            _ => "Quero fazer minha matricula."
        };
    }

    private static string BuildSummary(EnrollmentGeneratedCaseType caseType)
    {
        return caseType switch
        {
            EnrollmentGeneratedCaseType.Valid => "Caso gerado valido para matricula.",
            EnrollmentGeneratedCaseType.MissingDocument => "Caso gerado com documento obrigatorio faltando.",
            EnrollmentGeneratedCaseType.NameMismatch => "Caso gerado com divergencia de nome.",
            EnrollmentGeneratedCaseType.RaMismatch => "Caso gerado com divergencia de RA.",
            EnrollmentGeneratedCaseType.MissingRequiredField => "Caso gerado com campo obrigatorio ausente.",
            EnrollmentGeneratedCaseType.ExpiredDocument => "Caso gerado com documento vencido.",
            _ => "Caso gerado de matricula."
        };
    }

    private static void Shuffle<T>(IList<T> values, System.Random random)
    {
        for (var index = values.Count - 1; index > 0; index--)
        {
            var swapIndex = random.Next(0, index + 1);
            (values[index], values[swapIndex]) = (values[swapIndex], values[index]);
        }
    }
}
