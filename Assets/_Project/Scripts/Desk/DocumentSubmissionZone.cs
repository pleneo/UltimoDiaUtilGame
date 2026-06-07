using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class DocumentSubmissionZone : MonoBehaviour
{
    private const DocumentType DecisionDocumentType = DocumentType.EnrollmentProof;

    [SerializeField] private DocumentManager documentManager;
    [SerializeField] private CaseManager caseManager;
    [SerializeField] private RectTransform submissionArea;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private string missingStampMessage = "Carimbe o comprovante de matricula antes de entregar.";
    [SerializeField] private string pendingDocumentsMessage = "Entregue os demais documentos.";

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

        if (!TryGetStampedDecision(out var stampedDecision))
        {
            ShowFeedback(missingStampMessage);
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
            ShowFeedback(missingStampMessage);
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

    private static bool IsDecisionDocument(DraggableDocument document)
    {
        if (document == null || document.BoundRecord == null)
        {
            return false;
        }

        return document.BoundRecord.GetDocumentType() == DecisionDocumentType;
    }

    private bool IsDocumentInsideSubmissionArea(DraggableDocument document, PointerEventData eventData)
    {
        var camera = eventData != null ? eventData.pressEventCamera : null;
        var screenPoint = RectTransformUtility.WorldToScreenPoint(camera, document.RectTransform.position);
        return RectTransformUtility.RectangleContainsScreenPoint(submissionArea, screenPoint, camera);
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }

    private void ClearFeedback()
    {
        if (feedbackText != null)
        {
            feedbackText.text = string.Empty;
        }
    }
}
