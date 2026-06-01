using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class DocumentSubmissionZone : MonoBehaviour
{
    [SerializeField] private DocumentManager documentManager;
    [SerializeField] private CaseManager caseManager;
    [SerializeField] private RectTransform submissionArea;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private string missingStampMessage = "Carimbe o documento antes de entregar.";

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

        var stampReceiver = document.GetComponentInChildren<StampReceiver>();
        if (stampReceiver == null || !stampReceiver.UltimoCarimbo.HasValue)
        {
            ShowFeedback(missingStampMessage);
            document.ReturnToLastValidPosition();
            return;
        }

        ClearFeedback();

        var decision = stampReceiver.UltimoCarimbo.Value == StampType.Aprovado
            ? DecisionType.Approve
            : DecisionType.Reject;

        if (caseManager != null)
        {
            caseManager.SubmitDecision(decision);
        }
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
