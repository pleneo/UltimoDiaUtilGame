using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class DocumentSubmissionZone : MonoBehaviour
{
    [SerializeField] private DocumentManager documentManager;
    [SerializeField] private CaseManager caseManager;
    [SerializeField] private PaymentFlowController paymentFlowController;
    [SerializeField] private RectTransform submissionArea;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private string missingStampMessage = "Carimbe o documento principal antes de entregar.";
    [SerializeField] private string pendingDocumentsMessage = "Entregue os demais documentos.";
    [SerializeField] private string pendingPaymentMessage = "Entregue a maquininha ao aluno antes de devolver os documentos.";

    private readonly HashSet<DraggableDocument> submittedDocuments = new HashSet<DraggableDocument>();
    private DecisionType? queuedDecision;

    private void Awake()
    {
        if (submissionArea == null)
        {
            submissionArea = GetComponent<RectTransform>();
        }

        if (documentManager == null)
        {
            documentManager = FindObjectOfType<DocumentManager>();
        }

        if (caseManager == null)
        {
            caseManager = FindObjectOfType<CaseManager>();
        }

        if (paymentFlowController == null)
        {
            paymentFlowController = FindObjectOfType<PaymentFlowController>();
        }
    }

    public bool TryReceivePaymentMachine(DraggablePaymentMachine machine)
    {
        if (machine == null || submissionArea == null || paymentFlowController == null)
        {
            return false;
        }

        if (!RectTransformsOverlap(machine.RectTransform, submissionArea))
        {
            return false;
        }

        return paymentFlowController.TryStartPayment(machine);
    }

    private void OnEnable()
    {
        if (documentManager != null)
        {
            documentManager.DocumentsChanged += RefreshDocumentSubscriptions;
            RefreshDocumentSubscriptions();
        }
    }

    private void OnDisable()
    {
        if (documentManager != null)
        {
            documentManager.DocumentsChanged -= RefreshDocumentSubscriptions;
        }

        UnsubscribeFromCurrentDocuments();
    }

    private void RefreshDocumentSubscriptions()
    {
        UnsubscribeFromCurrentDocuments();
        ResetSubmissionState();

        if (documentManager == null || documentManager.SpawnedViews == null)
        {
            return;
        }

        for (var index = 0; index < documentManager.SpawnedViews.Count; index++)
        {
            var document = documentManager.SpawnedViews[index];
            if (document != null)
            {
                document.DragEnded += HandleDocumentDragEnded;
            }
        }
    }

    private void UnsubscribeFromCurrentDocuments()
    {
        if (documentManager == null || documentManager.SpawnedViews == null)
        {
            return;
        }

        for (var index = 0; index < documentManager.SpawnedViews.Count; index++)
        {
            var document = documentManager.SpawnedViews[index];
            if (document != null)
            {
                document.DragEnded -= HandleDocumentDragEnded;
            }
        }
    }

    private void HandleDocumentDragEnded(DraggableDocument document, PointerEventData eventData)
    {
        if (document == null || submissionArea == null)
        {
            return;
        }

        if (!IsDocumentInsideSubmissionArea(document, eventData))
        {
            return;
        }

        if (submittedDocuments.Contains(document))
        {
            return;
        }

        if (paymentFlowController != null && paymentFlowController.ShouldBlockDocumentSubmission())
        {
            ShowFeedback(pendingPaymentMessage);
            document.ReturnToLastValidPosition();
            return;
        }

        if (!TryGetStampedDecision(out var stampedDecision))
        {
            ShowFeedback(BuildMissingStampMessage());
            document.ReturnToLastValidPosition();
            return;
        }

        if (IsDecisionDocument(document))
        {
            queuedDecision = stampedDecision;
        }

        RegisterSubmittedDocument(document);

        if (!HaveAllDocumentsBeenSubmitted())
        {
            ShowFeedback(pendingDocumentsMessage);
            return;
        }

        if (!queuedDecision.HasValue)
        {
            ShowFeedback(BuildMissingStampMessage());
            return;
        }

        ClearFeedback();

        if (caseManager != null)
        {
            caseManager.SubmitDecision(queuedDecision.Value);
        }
    }

    private bool TryGetStampedDecision(out DecisionType decision)
    {
        decision = default;
        var decisionDocumentType = GetDecisionDocumentType();
        if (decisionDocumentType == DocumentType.Unknown)
        {
            return false;
        }

        if (documentManager == null || documentManager.SpawnedViews == null)
        {
            return false;
        }

        for (var index = 0; index < documentManager.SpawnedViews.Count; index++)
        {
            var document = documentManager.SpawnedViews[index];
            if (!IsDecisionDocument(document))
            {
                continue;
            }

            var stampReceiver = document != null ? document.GetComponentInChildren<StampReceiver>() : null;
            if (stampReceiver == null || !stampReceiver.UltimoCarimbo.HasValue)
            {
                return false;
            }

            decision = stampReceiver.UltimoCarimbo.Value == StampType.Aprovado
                ? DecisionType.Approve
                : DecisionType.Reject;
            return true;
        }

        return false;
    }

    private void ResetSubmissionState()
    {
        submittedDocuments.Clear();
        queuedDecision = null;
    }

    private bool HaveAllDocumentsBeenSubmitted()
    {
        if (documentManager == null || documentManager.SpawnedViews == null)
        {
            return false;
        }

        return submittedDocuments.Count >= documentManager.SpawnedViews.Count;
    }

    private void RegisterSubmittedDocument(DraggableDocument document)
    {
        submittedDocuments.Add(document);
        LockDocumentInSubmissionArea(document, submittedDocuments.Count - 1);
    }

    private void LockDocumentInSubmissionArea(DraggableDocument document, int submissionIndex)
    {
        if (document == null || submissionArea == null)
        {
            return;
        }

        document.SetSelectedVisual(false);
        document.enabled = false;

        var canvasGroup = document.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0f;
        }
        else
        {
            document.gameObject.SetActive(false);
        }
    }

    private bool IsDecisionDocument(DraggableDocument document)
    {
        if (document == null || document.BoundRecord == null)
        {
            return false;
        }

        var currentCase = caseManager != null ? caseManager.CurrentCase : null;
        var decisionDocumentType = CaseDocumentRules.ResolveDecisionDocumentType(currentCase);
        return decisionDocumentType != DocumentType.Unknown &&
               document.BoundRecord.GetDocumentType() == decisionDocumentType;
    }

    private DocumentType GetDecisionDocumentType()
    {
        var currentCase = caseManager != null ? caseManager.CurrentCase : null;
        return CaseDocumentRules.ResolveDecisionDocumentType(currentCase);
    }

    private bool IsDocumentInsideSubmissionArea(DraggableDocument document, PointerEventData eventData)
    {
        var camera = eventData != null ? eventData.pressEventCamera : null;
        var screenPoint = RectTransformUtility.WorldToScreenPoint(camera, document.RectTransform.position);
        return RectTransformUtility.RectangleContainsScreenPoint(submissionArea, screenPoint, camera);
    }

    private static bool RectTransformsOverlap(RectTransform first, RectTransform second)
    {
        if (first == null || second == null)
        {
            return false;
        }

        var firstCorners = new Vector3[4];
        var secondCorners = new Vector3[4];
        first.GetWorldCorners(firstCorners);
        second.GetWorldCorners(secondCorners);

        var firstMin = firstCorners[0];
        var firstMax = firstCorners[2];
        var secondMin = secondCorners[0];
        var secondMax = secondCorners[2];

        return firstMin.x <= secondMax.x &&
               firstMax.x >= secondMin.x &&
               firstMin.y <= secondMax.y &&
               firstMax.y >= secondMin.y;
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }

    private string BuildMissingStampMessage()
    {
        var decisionDocumentType = GetDecisionDocumentType();
        var decisionLabel = GetDocumentTypeLabel(decisionDocumentType);
        return decisionDocumentType == DocumentType.Unknown
            ? missingStampMessage
            : $"Carimbe {decisionLabel.ToLowerInvariant()} antes de entregar.";
    }

    private static string GetDocumentTypeLabel(DocumentType documentType)
    {
        return documentType switch
        {
            DocumentType.EnrollmentProof => "Comprovante de Matricula",
            DocumentType.WithdrawalForm => "Formulario de Trancamento",
            DocumentType.TuitionReceipt => "Comprovante de Mensalidade",
            _ => "o documento principal"
        };
    }

    private void ClearFeedback()
    {
        if (feedbackText != null)
        {
            feedbackText.text = string.Empty;
        }
    }
}
