using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    private struct QueuedErrorFeedback
    {
        public string message;
        public Color color;

        public QueuedErrorFeedback(string message, Color color)
        {
            this.message = message;
            this.color = color;
        }
    }

    private struct QueuedStudentCase
    {
        public StudentCaseDefinition caseDefinition;
        public NpcDefinition npcDefinitionOverride;

        public QueuedStudentCase(StudentCaseDefinition caseDefinition, NpcDefinition npcDefinitionOverride)
        {
            this.caseDefinition = caseDefinition;
            this.npcDefinitionOverride = npcDefinitionOverride;
        }
    }

    [SerializeField] private CaseManager caseManager;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private bool waitForPlayerToAdvanceCase = true;

    [Header("NPC Queue Flow")]
    [SerializeField] private bool useNpcMovementFlow = true;
    [SerializeField] private NpcMovementController npcMovementController;
    [SerializeField, Min(0.1f)] private float npcEventTimeoutSeconds = 4f;
    [SerializeField, Min(0f)] private float postDecisionDelaySeconds = 0.1f;
    [SerializeField, Min(0f)] private float incorrectDecisionFeedbackDelaySeconds = 3f;
    [SerializeField, Min(0f)] private float incorrectDecisionFeedbackDisplaySeconds = 3f;
    [SerializeField] private bool enableQueueDebugLogs = true;

    [Header("NPC Random Visuals")]
    [SerializeField] private List<NpcDefinition> randomNpcDefinitions = new List<NpcDefinition>();
    [SerializeField] private bool avoidRepeatingRandomNpc = true;
    [Tooltip("Se houver apenas 1 caso no DayConfig, cria entradas extras desse mesmo caso para testar todos os NPCs aleatorios em sequencia.")]
    [SerializeField] private bool autoRepeatSingleCaseForRandomNpcs = true;

    [Header("Debug")]
    [Tooltip("Apenas para prototipo: repete o primeiro caso N vezes para testar varios NPCs em sequencia sem criar varios casos.")]
    [SerializeField, Min(0)] private int debugExtraRepeatedCases;

    public event Action<DaySummaryData> DayEnded;

    public DayConfig CurrentDayConfig { get; private set; }
    public float RemainingTimeSeconds { get; private set; }
    public int CurrentCaseIndex { get; private set; }
    public int ResolvedCasesCount { get; private set; }
    public int CorrectDecisions { get; private set; }
    public int IncorrectDecisions { get; private set; }
    public bool IsDayActive { get; private set; }
    public bool IsWaitingForNextCase { get; private set; }

    private readonly List<StudentCaseDefinition> currentCases = new List<StudentCaseDefinition>();
    private readonly Queue<QueuedStudentCase> pendingCaseQueue = new Queue<QueuedStudentCase>();
    private readonly Queue<QueuedErrorFeedback> pendingErrorFeedbackQueue = new Queue<QueuedErrorFeedback>();
    private readonly List<FixedDayCaseEntry> pendingFixedCaseEntries = new List<FixedDayCaseEntry>();

    private Coroutine dayFlowCoroutine;
    private bool npcArrivedCenter;
    private bool npcExitedDesk;
    private bool hasPendingCaseResolution;
    private bool warnedNpcMovementUnavailable;
    private NpcDefinition lastRandomNpcDefinition;
    private Coroutine incorrectDecisionFeedbackCoroutine;
    private System.Random generatedCaseRandom;
    private int generatedCaseCounter;

    private void Awake()
    {
        if (caseManager == null)
        {
            caseManager = FindObjectOfType<CaseManager>();
        }

        if (economyManager == null)
        {
            economyManager = FindObjectOfType<EconomyManager>();
        }

        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }

        if (npcMovementController == null)
        {
            npcMovementController = FindObjectOfType<NpcMovementController>();
        }
    }

    private void OnEnable()
    {
        if (caseManager != null)
        {
            caseManager.CaseResolved += HandleCaseResolved;
        }

        NpcMovementEvents.NpcArrivedCenter += HandleNpcArrivedCenter;
        NpcMovementEvents.NpcExited += HandleNpcExited;
    }

    private void OnDisable()
    {
        if (caseManager != null)
        {
            caseManager.CaseResolved -= HandleCaseResolved;
        }

        NpcMovementEvents.NpcArrivedCenter -= HandleNpcArrivedCenter;
        NpcMovementEvents.NpcExited -= HandleNpcExited;

        if (dayFlowCoroutine != null)
        {
            StopCoroutine(dayFlowCoroutine);
            dayFlowCoroutine = null;
        }

        if (incorrectDecisionFeedbackCoroutine != null)
        {
            StopCoroutine(incorrectDecisionFeedbackCoroutine);
            incorrectDecisionFeedbackCoroutine = null;
        }
    }

    private void Update()
    {
        if (!IsDayActive || CurrentDayConfig == null)
        {
            return;
        }

        RemainingTimeSeconds -= Time.deltaTime;
        if (RemainingTimeSeconds <= 0f)
        {
            RemainingTimeSeconds = 0f;
            EndDay(DayEndReason.TimeExpired);
            return;
        }

        PushHudUpdate();
    }

    public void BeginDay(DayConfig dayConfig)
    {
        if (dayConfig == null)
        {
            Debug.LogWarning("DayManager.BeginDay foi chamado sem DayConfig.");
            return;
        }

        if (dayFlowCoroutine != null)
        {
            StopCoroutine(dayFlowCoroutine);
            dayFlowCoroutine = null;
        }

        CurrentDayConfig = dayConfig;
        RemainingTimeSeconds = Mathf.Max(0f, dayConfig.workDurationSeconds);
        CurrentCaseIndex = 0;
        ResolvedCasesCount = 0;
        CorrectDecisions = 0;
        IncorrectDecisions = 0;
        IsDayActive = true;
        IsWaitingForNextCase = false;
        hasPendingCaseResolution = false;
        npcArrivedCenter = false;
        npcExitedDesk = false;
        warnedNpcMovementUnavailable = false;
        lastRandomNpcDefinition = null;

        currentCases.Clear();
        pendingCaseQueue.Clear();
        pendingErrorFeedbackQueue.Clear();
        pendingFixedCaseEntries.Clear();
        generatedCaseCounter = 0;
        generatedCaseRandom = null;

        if (dayConfig.useInfiniteGeneratedCases)
        {
            PrepareInfiniteCaseFlow(dayConfig);
        }
        else
        {
            PrepareFiniteCaseFlow(dayConfig);
        }

        LogDaySetupDiagnostics();

        if (economyManager != null)
        {
            economyManager.ApplyEconomyConfig(dayConfig.economyConfig);
        }

        if (uiManager != null)
        {
            uiManager.BindDay(dayConfig);
            uiManager.HideDaySummary();
            uiManager.ShowCurrentCase(null);
            uiManager.ClearCaseFeedback();
        }

        dayFlowCoroutine = StartCoroutine(dayConfig.useInfiniteGeneratedCases
            ? RunInfiniteDayCaseFlow()
            : RunDayCaseFlow());
        LogQueue(dayConfig.useInfiniteGeneratedCases
            ? $"Dia infinito iniciado. Casos fixos pendentes: {pendingFixedCaseEntries.Count}."
            : $"Dia iniciado com {pendingCaseQueue.Count} caso(s) na fila.");
        PushHudUpdate();
    }

    private void PrepareFiniteCaseFlow(DayConfig dayConfig)
    {
        if (dayConfig.enrollmentGenerationConfig != null)
        {
            currentCases.AddRange(EnrollmentCaseGenerator.GenerateCases(dayConfig.enrollmentGenerationConfig));
        }
        var enrollmentCaseCount = currentCases.Count;

        if (dayConfig.includeManualCases && dayConfig.cases != null)
        {
            for (var index = 0; index < dayConfig.cases.Count; index++)
            {
                var caseDefinition = dayConfig.cases[index];
                if (caseDefinition == null)
                {
                    continue;
                }

                currentCases.Add(caseDefinition);
                // Casos manuais já entram na fila aqui — não serão re-enfileirados depois
                pendingCaseQueue.Enqueue(new QueuedStudentCase(caseDefinition, null));
            }
        }

        AddDebugRepeatedCasesIfNeeded();
        AddAutomaticRandomNpcTestCasesIfNeeded();
        LogDaySetupDiagnostics();

        // FIX 1: tipo correto — QueuedStudentCase, não StudentCaseDefinition
        // FIX 2: só enfileira enrollment; manuais e debug já foram enfileirados
        for (var index = 0; index < enrollmentCaseCount; index++)
        {
            pendingCaseQueue.Enqueue(new QueuedStudentCase(currentCases[index], null));
        }
    }

    private void PrepareInfiniteCaseFlow(DayConfig dayConfig)
    {
        generatedCaseRandom = dayConfig.enrollmentGenerationConfig != null && !dayConfig.enrollmentGenerationConfig.useFreshRandomSeed
            ? new System.Random(dayConfig.enrollmentGenerationConfig.randomSeed)
            : new System.Random();

        if (dayConfig.fixedCases != null)
        {
            for (var index = 0; index < dayConfig.fixedCases.Count; index++)
            {
                var fixedCaseEntry = dayConfig.fixedCases[index];
                if (fixedCaseEntry == null || fixedCaseEntry.caseDefinition == null)
                {
                    continue;
                }

                pendingFixedCaseEntries.Add(new FixedDayCaseEntry
                {
                    caseDefinition = fixedCaseEntry.caseDefinition,
                    triggerResolvedCaseNumber = Mathf.Max(0, fixedCaseEntry.triggerResolvedCaseNumber)
                });
            }
        }

        pendingFixedCaseEntries.Sort((left, right) =>
        {
            var triggerCompare = left.triggerResolvedCaseNumber.CompareTo(right.triggerResolvedCaseNumber);
            if (triggerCompare != 0)
            {
                return triggerCompare;
            }

            var leftId = left.caseDefinition != null ? left.caseDefinition.caseId : string.Empty;
            var rightId = right.caseDefinition != null ? right.caseDefinition.caseId : string.Empty;
            return string.CompareOrdinal(leftId, rightId);
        });

        for (var index = 0; index < pendingFixedCaseEntries.Count; index++)
        {
            if (pendingFixedCaseEntries[index].caseDefinition != null)
            {
                currentCases.Add(pendingFixedCaseEntries[index].caseDefinition);
            }
        }
    }

    private void AddDebugRepeatedCasesIfNeeded()
    {
        if (debugExtraRepeatedCases <= 0 || currentCases.Count == 0)
        {
            return;
        }

        var repeatedCase = currentCases[0];
        for (var index = 0; index < debugExtraRepeatedCases; index++)
        {
            currentCases.Add(repeatedCase);
            pendingCaseQueue.Enqueue(new QueuedStudentCase(repeatedCase, null));
        }

        LogQueue(
            $"Modo debug adicionou {debugExtraRepeatedCases} repeticao(oes) do caso '{repeatedCase.caseId}'. " +
            "Use isso so para testar a fila visual."
        );
    }

    private void AddAutomaticRandomNpcTestCasesIfNeeded()
    {
        if (
            !autoRepeatSingleCaseForRandomNpcs ||
            debugExtraRepeatedCases > 0 ||
            currentCases.Count != 1 ||
            randomNpcDefinitions == null
        )
        {
            return;
        }

        var repeatedCase = currentCases[0];
        var addedCases = 0;
        for (var index = 0; index < randomNpcDefinitions.Count; index++)
        {
            var randomNpcDefinition = randomNpcDefinitions[index];
            if (randomNpcDefinition == null)
            {
                continue;
            }

            currentCases.Add(repeatedCase);
            pendingCaseQueue.Enqueue(new QueuedStudentCase(repeatedCase, randomNpcDefinition));
            addedCases++;
        }

        if (addedCases > 0)
        {
            LogQueue(
                $"Auto-repeat criou {addedCases} entrada(s) extra(s) para testar os NPCs aleatorios " +
                $"usando o caso '{repeatedCase.caseId}'."
            );
        }
    }

    private void LogDaySetupDiagnostics()
    {
        var randomNpcCount = CountValidRandomNpcDefinitions();
        Debug.Log(
            $"[QueueFlow] Setup do dia: casos={currentCases.Count}, fila={pendingCaseQueue.Count}, " +
            $"fixosPendentes={pendingFixedCaseEntries.Count}, infinito={CurrentDayConfig != null && CurrentDayConfig.useInfiniteGeneratedCases}, " +
            $"npcAleatorios={randomNpcCount}, usaMovimentoNpc={useNpcMovementFlow}, " +
            $"npcController={(npcMovementController != null ? npcMovementController.name : "nenhum")}.",
            this
        );

        if ((CurrentDayConfig == null || !CurrentDayConfig.useInfiniteGeneratedCases) && pendingCaseQueue.Count == 0)
        {
            Debug.LogWarning("DayManager nao tem casos na fila. Preencha DayConfig > Cases.", this);
        }

        if (useNpcMovementFlow && npcMovementController == null)
        {
            Debug.LogWarning("DayManager esta com Use Npc Movement Flow ligado, mas Npc Movement Controller nao foi encontrado/atribuido.", this);
        }
    }

    private int CountValidRandomNpcDefinitions()
    {
        if (randomNpcDefinitions == null)
        {
            return 0;
        }

        var validCount = 0;
        for (var index = 0; index < randomNpcDefinitions.Count; index++)
        {
            if (randomNpcDefinitions[index] != null)
            {
                validCount++;
            }
        }

        return validCount;
    }

    public void EndDay(DayEndReason reason)
    {
        if (!IsDayActive)
        {
            return;
        }

        IsDayActive = false;
        IsWaitingForNextCase = false;
        hasPendingCaseResolution = false;
        npcArrivedCenter = false;
        npcExitedDesk = false;

        if (dayFlowCoroutine != null)
        {
            StopCoroutine(dayFlowCoroutine);
            dayFlowCoroutine = null;
        }

        if (caseManager != null)
        {
            caseManager.ClearCurrentCase();
        }

        if (economyManager != null)
        {
            economyManager.FinalizeDay();
        }

        var summary = BuildSummary(reason);
        LogQueue($"Dia encerrado. Motivo: {reason}. Resolvidos: {ResolvedCasesCount}/{currentCases.Count}.");
        DayEnded?.Invoke(summary);
    }

    public void ContinueToNextCase()
    {
        if (!IsDayActive)
        {
            Debug.LogWarning("[DayManager] Nao foi possivel avancar para o proximo aluno porque o dia nao esta ativo.");
            return;
        }

        if (!waitForPlayerToAdvanceCase)
        {
            Debug.LogWarning("[DayManager] Avanco manual desativado nesta cena/day config.");
            return;
        }

        if (!IsWaitingForNextCase)
        {
            Debug.LogWarning("[DayManager] Nao foi possivel avancar para o proximo aluno porque o jogo nao esta aguardando avancar caso.");
            return;
        }

        IsWaitingForNextCase = false;
        LogQueue("Avancando para o proximo aluno.");
        PushHudUpdate();
    }

    private IEnumerator RunDayCaseFlow()
    {
        while (IsDayActive && pendingCaseQueue.Count > 0)
        {
            CurrentCaseIndex = Mathf.Clamp(ResolvedCasesCount, 0, currentCases.Count - 1);
            var nextQueueEntry = pendingCaseQueue.Dequeue();
            var nextCase = nextQueueEntry.caseDefinition;
            LogQueue($"NPC chamado para caso '{nextCase.caseId}'. Restantes na fila de casos: {pendingCaseQueue.Count}.");

            yield return StartCasePresentation(nextQueueEntry);
            if (!IsDayActive)
            {
                yield break;
            }

            hasPendingCaseResolution = true;
            while (IsDayActive && hasPendingCaseResolution)
            {
                yield return null;
            }

            if (!IsDayActive)
            {
                yield break;
            }

            if (postDecisionDelaySeconds > 0f)
            {
                yield return new WaitForSeconds(postDecisionDelaySeconds);
            }

            yield return EndCasePresentation();
            if (!IsDayActive)
            {
                yield break;
            }

            if (waitForPlayerToAdvanceCase && pendingCaseQueue.Count > 0)
            {
                IsWaitingForNextCase = true;
                while (IsDayActive && IsWaitingForNextCase)
                {
                    yield return null;
                }
            }
        }

        if (IsDayActive)
        {
            EndDay(DayEndReason.Completed);
        }
    }

    private IEnumerator RunInfiniteDayCaseFlow()
    {
        while (IsDayActive && RemainingTimeSeconds > 0f)
        {
            var nextQueueEntry = GetNextInfiniteQueueEntry();
            if (nextQueueEntry.caseDefinition == null)
            {
                EndDay(DayEndReason.Completed);
                yield break;
            }

            CurrentCaseIndex = ResolvedCasesCount;
            LogQueue($"NPC chamado para caso '{nextQueueEntry.caseDefinition.caseId}' (infinito).");

            yield return StartCasePresentation(nextQueueEntry);
            if (!IsDayActive)
            {
                yield break;
            }

            hasPendingCaseResolution = true;
            while (IsDayActive && hasPendingCaseResolution)
            {
                yield return null;
            }

            if (!IsDayActive)
            {
                yield break;
            }

            if (postDecisionDelaySeconds > 0f)
            {
                yield return new WaitForSeconds(postDecisionDelaySeconds);
            }

            yield return EndCasePresentation();
            if (!IsDayActive)
            {
                yield break;
            }
        }

        if (IsDayActive)
        {
            EndDay(DayEndReason.TimeExpired);
        }
    }

    private IEnumerator StartCasePresentation(QueuedStudentCase queueEntry)
    {
        var caseDefinition = queueEntry.caseDefinition;
        if (caseDefinition == null)
        {
            yield break;
        }

        LogQueue($"Caso ativo no balcao: '{caseDefinition.caseId}'.");

        if (ShouldUseNpcMovementFlow())
        {
            npcMovementController.ApplyNpcDefinition(ResolveNpcDefinitionForCase(queueEntry));
            npcArrivedCenter = false;
            NpcMovementEvents.RaiseNextNpc();
            yield return WaitForNpcSignal(() => npcArrivedCenter, "chegada do NPC ao balcao");

            if (!IsDayActive)
            {
                yield break;
            }
        }
        else if (useNpcMovementFlow && !warnedNpcMovementUnavailable)
        {
            warnedNpcMovementUnavailable = true;
            Debug.LogWarning("Fluxo de movimento do NPC nao rodou. Verifique se existe um NpcMovementController ativo na cena.", this);
        }

        if (caseManager != null)
        {
            caseManager.LoadCase(caseDefinition);
        }

        if (uiManager != null)
        {
            uiManager.ShowCurrentCase(caseDefinition);
        }

        PushHudUpdate();
    }

    private IEnumerator EndCasePresentation()
    {
        if (ShouldUseNpcMovementFlow())
        {
            npcExitedDesk = false;
            NpcMovementEvents.RaiseEndNpc();
            yield return WaitForNpcSignal(() => npcExitedDesk, "saida do NPC do balcao");
        }

        if (!IsDayActive)
        {
            yield break;
        }

        if (uiManager != null)
        {
            uiManager.ShowCurrentCase(null);
        }

        LogQueue("Balcao liberado para o proximo NPC.");
    }

    private IEnumerator WaitForNpcSignal(Func<bool> signal, string signalName)
    {
        if (signal == null)
        {
            yield break;
        }

        var elapsed = 0f;
        while (IsDayActive && !signal())
        {
            elapsed += Time.deltaTime;
            if (elapsed >= npcEventTimeoutSeconds)
            {
                Debug.LogWarning($"DayManager aguardou {npcEventTimeoutSeconds:0.0}s por {signalName}. Seguindo fluxo sem esse evento.");
                break;
            }

            yield return null;
        }
    }

    private bool ShouldUseNpcMovementFlow()
    {
        if (npcMovementController == null)
        {
            npcMovementController = FindObjectOfType<NpcMovementController>();
        }

        return useNpcMovementFlow && npcMovementController != null && npcMovementController.isActiveAndEnabled;
    }

    private NpcDefinition ResolveNpcDefinitionForCase(QueuedStudentCase queueEntry)
    {
        if (queueEntry.npcDefinitionOverride != null)
        {
            return queueEntry.npcDefinitionOverride;
        }

        var caseDefinition = queueEntry.caseDefinition;
        if (caseDefinition != null && caseDefinition.npcDefinition != null)
        {
            return caseDefinition.npcDefinition;
        }

        if (randomNpcDefinitions == null || randomNpcDefinitions.Count == 0)
        {
            return null;
        }

        var validDefinitions = new List<NpcDefinition>();
        for (var index = 0; index < randomNpcDefinitions.Count; index++)
        {
            if (randomNpcDefinitions[index] != null)
            {
                validDefinitions.Add(randomNpcDefinitions[index]);
            }
        }

        if (validDefinitions.Count == 0)
        {
            return null;
        }

        if (avoidRepeatingRandomNpc && validDefinitions.Count > 1 && lastRandomNpcDefinition != null)
        {
            validDefinitions.Remove(lastRandomNpcDefinition);
        }

        var selectedDefinition = validDefinitions[UnityEngine.Random.Range(0, validDefinitions.Count)];
        lastRandomNpcDefinition = selectedDefinition;
        return selectedDefinition;
    }

    private QueuedStudentCase GetNextInfiniteQueueEntry()
    {
        var fixedCase = TryConsumeFixedCaseForCurrentProgress();
        if (fixedCase != null)
        {
            return new QueuedStudentCase(fixedCase, null);
        }

        var generatedCase = EnrollmentCaseGenerator.GenerateSingleCase(
            CurrentDayConfig != null ? CurrentDayConfig.enrollmentGenerationConfig : null,
            generatedCaseRandom,
            ++generatedCaseCounter);

        if (generatedCase != null)
        {
            currentCases.Add(generatedCase);
        }

        return new QueuedStudentCase(generatedCase, null);
    }

    private StudentCaseDefinition TryConsumeFixedCaseForCurrentProgress()
    {
        if (pendingFixedCaseEntries.Count == 0)
        {
            return null;
        }

        if (pendingFixedCaseEntries[0].triggerResolvedCaseNumber > ResolvedCasesCount)
        {
            return null;
        }

        var fixedCaseDefinition = pendingFixedCaseEntries[0].caseDefinition;
        pendingFixedCaseEntries.RemoveAt(0);
        return fixedCaseDefinition;
    }

    private void HandleCaseResolved(CaseResolutionResult result)
    {
        if (!IsDayActive || result == null)
        {
            return;
        }

        if (result.isCorrectDecision)
        {
            CorrectDecisions++;
        }
        else
        {
            IncorrectDecisions++;
        }

        ResolvedCasesCount++;
        CurrentCaseIndex = currentCases.Count > 0
            ? Mathf.Min(ResolvedCasesCount, currentCases.Count - 1)
            : 0;

        IsWaitingForNextCase = waitForPlayerToAdvanceCase && pendingCaseQueue.Count > 0;

        LogQueue(
            $"Caso '{result.caseDefinition.caseId}' resolvido com {result.chosenDecision}. " +
            $"Correto: {result.isCorrectDecision}. Resolvidos: {ResolvedCasesCount}/{currentCases.Count}."
        );

        if (economyManager != null)
        {
            economyManager.ApplyCaseResolution(result);
            if (economyManager.IsGameOver)
            {
                EndDay(DayEndReason.GameOver);
                return;
            }
        }

        PushHudUpdate();

        if (result.isCorrectDecision)
        {
            if (uiManager != null)
            {
                uiManager.ClearCaseFeedback();
            }

            hasPendingCaseResolution = false;
            return;
        }

        EnqueueIncorrectDecisionFeedback(result);
        hasPendingCaseResolution = false;
    }

    private void EnqueueIncorrectDecisionFeedback(CaseResolutionResult result)
    {
        pendingErrorFeedbackQueue.Enqueue(new QueuedErrorFeedback(
            BuildIncorrectDecisionFeedbackMessage(result),
            new Color(0.92f, 0.34f, 0.28f, 1f)));

        if (incorrectDecisionFeedbackCoroutine == null)
        {
            incorrectDecisionFeedbackCoroutine = StartCoroutine(ProcessIncorrectDecisionFeedbackQueue());
        }
    }

    private IEnumerator ProcessIncorrectDecisionFeedbackQueue()
    {
        while (pendingErrorFeedbackQueue.Count > 0)
        {
            var feedback = pendingErrorFeedbackQueue.Dequeue();

            if (incorrectDecisionFeedbackDelaySeconds > 0f)
            {
                yield return new WaitForSeconds(incorrectDecisionFeedbackDelaySeconds);
            }

            if (uiManager != null)
            {
                uiManager.ShowCaseFeedback(feedback.message, feedback.color);
            }

            if (incorrectDecisionFeedbackDisplaySeconds > 0f)
            {
                yield return new WaitForSeconds(incorrectDecisionFeedbackDisplaySeconds);
            }

            if (uiManager != null)
            {
                uiManager.ClearCaseFeedback();
            }
        }

        incorrectDecisionFeedbackCoroutine = null;
    }

    private string BuildIncorrectDecisionFeedbackMessage(CaseResolutionResult result)
    {
        var penalty = economyManager != null ? economyManager.PenaltyPerMistake : 0;
        var warningDelta = Mathf.Max(0, result.warningDelta);
        var reason = BuildSpecificIncorrectReason(result);

        var message = $"Decisao incorreta. -{penalty}";
        if (warningDelta > 0)
        {
            message += $" | Advertencia +{warningDelta}";
        }

        message += $"\nMotivo: {reason}";
        return message;
    }

    private static string BuildSpecificIncorrectReason(CaseResolutionResult result)
    {
        if (result == null || result.validationResult == null)
        {
            return "Nao foi possivel identificar o problema do caso.";
        }

        if (result.chosenDecision == DecisionType.Reject && result.validationResult.recommendedDecision == DecisionType.Approve)
        {
            return "A documentacao estava em ordem. O caso deveria ter sido aprovado.";
        }

        if (result.chosenDecision == DecisionType.Approve && result.validationResult.issues != null && result.validationResult.issues.Count > 0)
        {
            var reasons = new List<string>();
            for (var index = 0; index < result.validationResult.issues.Count; index++)
            {
                var issue = result.validationResult.issues[index];
                if (issue == null || string.IsNullOrWhiteSpace(issue.message))
                {
                    continue;
                }

                if (!reasons.Contains(issue.message))
                {
                    reasons.Add(issue.message);
                }

                if (reasons.Count >= 2)
                {
                    break;
                }
            }

            if (reasons.Count > 0)
            {
                return string.Join(" | ", reasons);
            }
        }

        if (result.chosenDecision == DecisionType.Forward)
        {
            return result.validationResult.hasSupervisorReview
                ? "O caso foi encaminhado incorretamente."
                : "Esse caso nao exigia encaminhamento ao superior.";
        }

        return string.IsNullOrWhiteSpace(result.feedbackMessage)
            ? "Verifique os documentos do caso."
            : result.feedbackMessage;
    }

    private DaySummaryData BuildSummary(DayEndReason reason)
    {
        var summary = new DaySummaryData
        {
            dayNumber = CurrentDayConfig != null ? CurrentDayConfig.dayNumber : 1,
            dayLabel = CurrentDayConfig != null ? CurrentDayConfig.dayLabel : "Dia",
            totalCases = CurrentDayConfig != null && CurrentDayConfig.useInfiniteGeneratedCases
                ? ResolvedCasesCount
                : currentCases.Count,
            completedCases = CurrentDayConfig != null && CurrentDayConfig.useInfiniteGeneratedCases
                ? ResolvedCasesCount
                : Mathf.Clamp(ResolvedCasesCount, 0, currentCases.Count),
            correctDecisions = CorrectDecisions,
            incorrectDecisions = IncorrectDecisions,
            workDurationSeconds = CurrentDayConfig != null ? CurrentDayConfig.workDurationSeconds : 0f,
            remainingTimeSeconds = RemainingTimeSeconds,
            endReason = reason,
            economySnapshot = economyManager != null ? economyManager.GetSnapshot() : new EconomySnapshot()
        };

        summary.headline = reason == DayEndReason.Completed
            ? "Fim do dia"
            : reason == DayEndReason.TimeExpired
                ? "Tempo esgotado"
                : "Encerrado por erro";

        summary.details = reason == DayEndReason.Completed
            ? "O expediente foi encerrado sem interrupcoes."
            : reason == DayEndReason.TimeExpired
                ? "O horario acabou antes de concluir todos os casos."
                : "A sessao terminou por limite economico ou disciplinar.";

        if (CurrentDayConfig != null && !string.IsNullOrWhiteSpace(CurrentDayConfig.dayIntro))
        {
            summary.notes.Add(CurrentDayConfig.dayIntro);
        }

        return summary;
    }

    private void PushHudUpdate()
    {
        if (uiManager == null)
        {
            return;
        }

        var totalTime = CurrentDayConfig != null ? CurrentDayConfig.workDurationSeconds : 0f;
        var snapshot = economyManager != null ? economyManager.GetSnapshot() : new EconomySnapshot();
        uiManager.RefreshHUD(snapshot, RemainingTimeSeconds, totalTime);
    }

    private void LogQueue(string message)
    {
        if (!enableQueueDebugLogs)
        {
            return;
        }

        Debug.Log($"[QueueFlow] {message}", this);
    }
    // FIX 3: métodos ausentes que eram referenciados em OnEnable/OnDisable
    // sem eles o CS não compilava (método não definido)
    private void HandleNpcArrivedCenter()
    {
        npcArrivedCenter = true;
    }

    private void HandleNpcExited()
    {
        npcExitedDesk = true;
    }

}
