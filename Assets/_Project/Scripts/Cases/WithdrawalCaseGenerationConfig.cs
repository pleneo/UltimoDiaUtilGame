using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tipos de caso de trancamento que o gerador pode produzir.
/// </summary>
public enum WithdrawalGeneratedCaseType
{
    /// <summary>Pagamento aprovado. Decisão correta: Aprovar.</summary>
    AuthorizedPayment,

    /// <summary>Pagamento negado. Decisão correta: Rejeitar.</summary>
    UnauthorizedPayment,

    /// <summary>Formulário de trancamento faltando (sem maquininha). Decisão correta: Rejeitar.</summary>
    MissingWithdrawalForm
}

[Serializable]
public class WithdrawalMinimumCaseRule
{
    public WithdrawalGeneratedCaseType caseType = WithdrawalGeneratedCaseType.AuthorizedPayment;
    [Min(0)] public int minimumCount = 1;
}

[Serializable]
public class WithdrawalCaseTypeWeight
{
    public WithdrawalGeneratedCaseType caseType = WithdrawalGeneratedCaseType.AuthorizedPayment;
    [Min(0)] public int weight = 10;
}

/// <summary>
/// Configuração para geração dinâmica de casos de trancamento de disciplina.
/// Análogo ao EnrollmentCaseGenerationConfig, mas para ClassWithdrawal com pagamento via maquininha.
/// Crie via: Tools > Ultimo Dia Util > Withdrawal Generation Config.
/// </summary>
[CreateAssetMenu(menuName = "Ultimo Dia Util/Cases/Withdrawal Generation Config", fileName = "WithdrawalGen_")]
public class WithdrawalCaseGenerationConfig : ScriptableObject
{
    [Header("Geração")]
    [Min(1)] public int totalGeneratedCases = 4;
    public bool shuffleGeneratedCases = true;
    public bool useFreshRandomSeed = false;
    public int randomSeed = 99999;

    [Header("Documentos necessários")]
    [Tooltip("DocumentDefinition para o Formulário de Trancamento.")]
    public DocumentDefinition withdrawalFormDefinition;
    [Tooltip("DocumentDefinition para a Carteira de Identidade.")]
    public DocumentDefinition identityCardDefinition;
    [Tooltip("DocumentDefinition para a Via da Maquininha (PaymentReceipt).")]
    public DocumentDefinition paymentReceiptDefinition;

    [Header("Regras de pagamento")]
    public string referenceDateIso = "2026-05-21";
    [Min(1)] public int paymentAmount = 150;

    [Header("Mix garantido de casos")]
    [Tooltip("Garante que ao menos N casos de cada tipo estejam presentes antes do preenchimento aleatório.")]
    public List<WithdrawalMinimumCaseRule> minimumCases = new List<WithdrawalMinimumCaseRule>();

    [Header("Pesos para casos aleatórios")]
    public List<WithdrawalCaseTypeWeight> caseTypeWeights = new List<WithdrawalCaseTypeWeight>();

    [Header("Dados dos estudantes gerados")]
    public List<string> firstNames = new List<string>();
    public List<string> lastNames = new List<string>();
    public List<string> courses = new List<string>();

    [Header("NPCs por perfil")]
    public bool useProfiledNpcGeneration = true;
    public List<string> maleFirstNames = new List<string>();
    public List<string> femaleFirstNames = new List<string>();
    public List<NpcDefinition> maleNpcDefinitions = new List<NpcDefinition>();
    public List<NpcDefinition> femaleNpcDefinitions = new List<NpcDefinition>();
}
