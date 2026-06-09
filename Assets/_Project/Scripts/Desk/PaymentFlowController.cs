using System.Collections;
using UnityEngine;

public class PaymentFlowController : MonoBehaviour
{
    [SerializeField, Min(1)] private int firstDayWithPaymentMachine = 2;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CaseManager caseManager;
    [SerializeField] private DocumentManager documentManager;
    [SerializeField] private DayManager dayManager;
    [SerializeField] private DraggablePaymentMachine paymentMachine;
    [SerializeField] private DocumentDefinition fallbackPaymentReceiptDefinition;
    [SerializeField, Min(0.1f)] private float machineProcessingDelaySeconds = 1.2f;

    public bool IsPaymentInProgress { get; private set; }
    public bool IsPaymentCompleted { get; private set; }

    private Coroutine paymentRoutine;
    private int lastAppliedDayNumber = -1;

    private void Awake()
    {
        if (caseManager == null)
        {
            caseManager = FindObjectOfType<CaseManager>();
        }

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (documentManager == null)
        {
            documentManager = FindObjectOfType<DocumentManager>();
        }

        if (dayManager == null)
        {
            dayManager = FindObjectOfType<DayManager>();
        }

        if (paymentMachine == null)
        {
            paymentMachine = FindObjectOfType<DraggablePaymentMachine>(true);
        }

        RefreshPaymentMachineVisibility();
    }

    private void OnEnable()
    {
        if (caseManager != null)
        {
            caseManager.CaseLoaded += HandleCaseLoaded;
            caseManager.CaseResolved += HandleCaseResolved;
        }

        if (documentManager != null)
        {
            documentManager.DocumentsChanged += RefreshPaymentStateFromDocuments;
        }

        if (gameManager != null)
        {
            lastAppliedDayNumber = gameManager.CurrentDayIndex;
        }

        SyncCurrentState();
    }

    private void OnDisable()
    {
        if (caseManager != null)
        {
            caseManager.CaseLoaded -= HandleCaseLoaded;
            caseManager.CaseResolved -= HandleCaseResolved;
        }

        if (documentManager != null)
        {
            documentManager.DocumentsChanged -= RefreshPaymentStateFromDocuments;
        }

        if (gameManager != null)
        {
            lastAppliedDayNumber = -1;
        }

        if (paymentRoutine != null)
        {
            StopCoroutine(paymentRoutine);
            paymentRoutine = null;
        }
    }

    public bool ShouldBlockDocumentSubmission()
    {
        var currentCase = caseManager != null ? caseManager.CurrentCase : null;
        return CaseDocumentRules.RequiresPayment(currentCase) && !IsPaymentCompleted;
    }

    public bool TryStartPayment(DraggablePaymentMachine deliveredMachine)
    {
        if (deliveredMachine == null || !CanProcessCurrentCasePayment())
        {
            deliveredMachine?.ReturnToOriginAnimated();
            return false;
        }

        if (paymentRoutine != null)
        {
            StopCoroutine(paymentRoutine);
        }

        paymentRoutine = StartCoroutine(ProcessPaymentRoutine(deliveredMachine));
        return true;
    }

    private bool CanProcessCurrentCasePayment()
    {
        var currentCase = caseManager != null ? caseManager.CurrentCase : null;
        return currentCase != null &&
               CaseDocumentRules.RequiresPayment(currentCase) &&
               !IsPaymentInProgress &&
               !IsPaymentCompleted &&
               documentManager != null;
    }

    private IEnumerator ProcessPaymentRoutine(DraggablePaymentMachine deliveredMachine)
    {
        IsPaymentInProgress = true;
        deliveredMachine.SetInteractionEnabled(false);
        deliveredMachine.SetVisible(false);

        if (machineProcessingDelaySeconds > 0f)
        {
            yield return new WaitForSeconds(machineProcessingDelaySeconds);
        }

        var receiptRecord = BuildReceiptForCurrentCase();
        if (receiptRecord != null && documentManager != null && !HasPaymentReceiptDocument())
        {
            documentManager.AddDocument(receiptRecord, true);
        }

        IsPaymentCompleted = HasPaymentReceiptDocument();
        IsPaymentInProgress = false;

        deliveredMachine.ReturnToOriginImmediate();
        deliveredMachine.SetVisible(true);
        deliveredMachine.SetInteractionEnabled(true);
        paymentRoutine = null;
    }

    private void HandleCaseLoaded(StudentCaseDefinition caseDefinition)
    {
        if (paymentRoutine != null)
        {
            StopCoroutine(paymentRoutine);
            paymentRoutine = null;
        }

        IsPaymentInProgress = false;
        IsPaymentCompleted = CaseDocumentRules.RequiresPayment(caseDefinition) && HasPaymentReceiptDocument();
            RefreshPaymentMachineVisibility();
    }

    private void HandleCaseResolved(CaseResolutionResult _)
    {
        IsPaymentInProgress = false;
        IsPaymentCompleted = false;
        RefreshPaymentMachineVisibility();
    }

    private void RefreshPaymentStateFromDocuments()
    {
        var currentCase = caseManager != null ? caseManager.CurrentCase : null;
        if (!CaseDocumentRules.RequiresPayment(currentCase))
        {
            IsPaymentCompleted = false;
            return;
        }

        IsPaymentCompleted = HasPaymentReceiptDocument();
    }

    private void RefreshPaymentMachineVisibility()
    {
        if (paymentMachine == null)
        {
            return;
        }

        paymentMachine.ReturnToOriginImmediate();

        if (!IsPaymentMachineAllowed())
        {
            paymentMachine.SetVisible(false);
            paymentMachine.SetInteractionEnabled(false);
            paymentMachine.gameObject.SetActive(false);
            return;
        }

        if (!paymentMachine.gameObject.activeSelf)
        {
            paymentMachine.gameObject.SetActive(true);
        }

        paymentMachine.SetVisible(true);
        paymentMachine.SetInteractionEnabled(true);
    }

    private void SyncCurrentState()
    {
        SyncPaymentMachineByDay();
        RefreshPaymentStateFromDocuments();
        RefreshPaymentMachineVisibility();
    }

    private void Update()
    {
        SyncPaymentMachineByDay();
    }

    private void SyncPaymentMachineByDay()
    {
        var currentDayIndex = gameManager != null ? gameManager.CurrentDayIndex : -1;

        if (currentDayIndex == lastAppliedDayNumber)
        {
            return;
        }

        lastAppliedDayNumber = currentDayIndex;
        ApplyPaymentMachineState();
    }

    private void ApplyPaymentMachineState()
    {
        if (paymentMachine == null)
        {
            return;
        }

        if (!IsPaymentMachineAllowed())
        {
            paymentMachine.ReturnToOriginImmediate();
            paymentMachine.SetVisible(false);
            paymentMachine.SetInteractionEnabled(false);
            paymentMachine.gameObject.SetActive(false);
            return;
        }

        if (!paymentMachine.gameObject.activeSelf)
        {
            paymentMachine.gameObject.SetActive(true);
        }

        paymentMachine.ReturnToOriginImmediate();
        paymentMachine.SetVisible(true);
        paymentMachine.SetInteractionEnabled(true);
    }

    private bool IsPaymentMachineAllowed()
    {
        return gameManager != null && (gameManager.CurrentDayIndex + 1) >= firstDayWithPaymentMachine;
    }

    private bool HasPaymentReceiptDocument()
    {
        if (documentManager == null || documentManager.CurrentDocuments == null)
        {
            return false;
        }

        for (var index = 0; index < documentManager.CurrentDocuments.Count; index++)
        {
            var document = documentManager.CurrentDocuments[index];
            if (document != null && document.GetDocumentType() == DocumentType.PaymentReceipt)
            {
                return true;
            }
        }

        return false;
    }

    private DocumentRecord BuildReceiptForCurrentCase()
    {
        var currentCase = caseManager != null ? caseManager.CurrentCase : null;
        if (currentCase == null)
        {
            return null;
        }

        if (currentCase.paymentReceiptTemplate != null)
        {
            return currentCase.paymentReceiptTemplate.Clone();
        }

        var identityDocument = FindDocument(DocumentType.IdentityCard);
        var fallbackRecord = new DocumentRecord
        {
            definition = fallbackPaymentReceiptDefinition,
            hasValidStamp = true,
            issueDateIso = currentCase.referenceDateIso,
            notes = "Via devolvida pela maquininha."
        };

        fallbackRecord.fields.Add(new DocumentField
        {
            key = "nome",
            value = currentCase.applicantName
        });

        if (identityDocument != null && identityDocument.TryGetFieldValue("cpf", out var cpf))
        {
            fallbackRecord.fields.Add(new DocumentField
            {
                key = "cpf",
                value = cpf
            });
        }

        fallbackRecord.fields.Add(new DocumentField
        {
            key = "valor",
            value = currentCase.expectedPaymentAmount.ToString()
        });

        fallbackRecord.fields.Add(new DocumentField
        {
            key = "status",
            value = "AUTORIZADO"
        });

        return fallbackRecord;
    }

    private DocumentRecord FindDocument(DocumentType documentType)
    {
        if (documentManager == null || documentManager.CurrentDocuments == null)
        {
            return null;
        }

        for (var index = 0; index < documentManager.CurrentDocuments.Count; index++)
        {
            var document = documentManager.CurrentDocuments[index];
            if (document != null && document.GetDocumentType() == documentType)
            {
                return document;
            }
        }

        return null;
    }
}
