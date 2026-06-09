using System.Collections;
using UnityEngine;

public class PaymentFlowController : MonoBehaviour
{
    [SerializeField] private CaseManager caseManager;
    [SerializeField] private DocumentManager documentManager;
    [SerializeField] private DraggablePaymentMachine paymentMachine;
    [SerializeField] private DocumentDefinition fallbackPaymentReceiptDefinition;
    [SerializeField, Min(0.1f)] private float machineProcessingDelaySeconds = 1.2f;

    public bool IsPaymentInProgress { get; private set; }
    public bool IsPaymentCompleted { get; private set; }

    private Coroutine paymentRoutine;

    private void Awake()
    {
        if (caseManager == null)
        {
            caseManager = FindObjectOfType<CaseManager>();
        }

        if (documentManager == null)
        {
            documentManager = FindObjectOfType<DocumentManager>();
        }

        if (paymentMachine == null)
        {
            paymentMachine = FindObjectOfType<DraggablePaymentMachine>(true);
        }

        RefreshPaymentMachineVisibility(null);
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
    }

    private void Update()
    {
        if (IsPaymentInProgress)
        {
            return;
        }

        var currentCase = caseManager != null ? caseManager.CurrentCase : null;
        RefreshPaymentMachineVisibility(currentCase);
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
        deliveredMachine.SetVisible(CaseDocumentRules.RequiresPayment(caseManager != null ? caseManager.CurrentCase : null));
        deliveredMachine.SetInteractionEnabled(CaseDocumentRules.RequiresPayment(caseManager != null ? caseManager.CurrentCase : null));
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
        RefreshPaymentMachineVisibility(caseDefinition);
    }

    private void HandleCaseResolved(CaseResolutionResult _)
    {
        IsPaymentInProgress = false;
        IsPaymentCompleted = false;
        RefreshPaymentMachineVisibility(null);
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

    private void RefreshPaymentMachineVisibility(StudentCaseDefinition caseDefinition)
    {
        if (paymentMachine == null)
        {
            return;
        }

        var requiresPayment = CaseDocumentRules.RequiresPayment(caseDefinition);
        paymentMachine.ReturnToOriginImmediate();
        paymentMachine.SetVisible(requiresPayment);
        paymentMachine.SetInteractionEnabled(requiresPayment);
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
            value = "autorizado"
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
