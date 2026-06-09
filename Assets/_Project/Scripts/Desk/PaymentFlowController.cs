using System.Collections;
using UnityEngine;

/// <summary>
/// Controla o fluxo de pagamento via maquininha.
/// A maquininha fica sempre presente na cena, mas só aparece e é usável
/// quando o caso atual exige pagamento (requiresPaymentProcessing = true).
/// Nos dias sem casos de pagamento (ex: Dia 1), ela simplesmente não é exibida.
/// </summary>
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

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

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

        // Garante que a maquininha comece oculta (nenhum caso ativo ainda).
        HideMachine();
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

    // -------------------------------------------------------------------------
    // API pública
    // -------------------------------------------------------------------------

    /// <summary>
    /// Retorna true se o envio de documentos deve ser bloqueado enquanto
    /// o pagamento do caso atual ainda não foi concluído.
    /// </summary>
    public bool ShouldBlockDocumentSubmission()
    {
        var currentCase = caseManager != null ? caseManager.CurrentCase : null;
        return CaseDocumentRules.RequiresPayment(currentCase) && !IsPaymentCompleted;
    }

    /// <summary>
    /// Chamado pela DocumentSubmissionZone quando a maquininha é arrastada
    /// para a área de entrega. Inicia o processamento do pagamento.
    /// </summary>
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

    // -------------------------------------------------------------------------
    // Lógica de visibilidade por caso
    // -------------------------------------------------------------------------

    private void HandleCaseLoaded(StudentCaseDefinition caseDefinition)
    {
        if (paymentRoutine != null)
        {
            StopCoroutine(paymentRoutine);
            paymentRoutine = null;
        }

        IsPaymentInProgress = false;
        IsPaymentCompleted = CaseDocumentRules.RequiresPayment(caseDefinition) && HasPaymentReceiptDocument();

        // Mostra a maquininha apenas quando o caso exige pagamento.
        if (CaseDocumentRules.RequiresPayment(caseDefinition))
        {
            ShowMachine();
        }
        else
        {
            HideMachine();
        }
    }

    private void HandleCaseResolved(CaseResolutionResult _)
    {
        IsPaymentInProgress = false;
        IsPaymentCompleted = false;
        HideMachine();
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

    // -------------------------------------------------------------------------
    // Controle de visibilidade da maquininha
    // -------------------------------------------------------------------------

    private void ShowMachine()
    {
        if (paymentMachine == null)
        {
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

    private void HideMachine()
    {
        if (paymentMachine == null)
        {
            return;
        }

        paymentMachine.SetVisible(false);
        paymentMachine.SetInteractionEnabled(false);
    }

    // -------------------------------------------------------------------------
    // Processamento de pagamento
    // -------------------------------------------------------------------------

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

    // -------------------------------------------------------------------------
    // Helpers de documento
    // -------------------------------------------------------------------------

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
