using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class DraggablePaymentMachine : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private float returnSpeed = 900f;
    [SerializeField] private float dragScale = 1.08f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private RectTransform rootCanvas;
    private Coroutine returnCoroutine;
    private Vector2 homePosition;
    private Vector2 dragOffset;
    private bool isDragging;
    private bool interactionEnabled = true;

    public RectTransform RectTransform => rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            rootCanvas = canvas.GetComponent<RectTransform>();
        }

        homePosition = rectTransform.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!interactionEnabled || rootCanvas == null)
        {
            return;
        }

        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        isDragging = true;
        transform.SetAsLastSibling();
        transform.localScale = Vector3.one * dragScale;
        canvasGroup.blocksRaycasts = false;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas,
            eventData.position,
            eventData.pressEventCamera,
            out var pointerPosition);

        dragOffset = rectTransform.anchoredPosition - pointerPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!interactionEnabled || !isDragging || rootCanvas == null)
        {
            return;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvas,
                eventData.position,
                eventData.pressEventCamera,
                out var pointerPosition))
        {
            rectTransform.anchoredPosition = pointerPosition + dragOffset;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!interactionEnabled)
        {
            return;
        }

        isDragging = false;
        transform.localScale = Vector3.one;
        canvasGroup.blocksRaycasts = true;

        if (TryDropOnStudent())
        {
            return;
        }

        ReturnToOriginAnimated();
    }

    public void SetInteractionEnabled(bool isEnabled)
    {
        interactionEnabled = isEnabled;
        isDragging = false;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = isEnabled && canvasGroup.alpha > 0f;
        }
    }

    public void SetVisible(bool isVisible)
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = isVisible ? 1f : 0f;
        canvasGroup.blocksRaycasts = isVisible && interactionEnabled;
    }

    public void ReturnToOriginImmediate()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        rectTransform.anchoredPosition = homePosition;
        transform.localScale = Vector3.one;
    }

    public void ReturnToOriginAnimated()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
        }

        returnCoroutine = StartCoroutine(ReturnRoutine());
    }

    private bool TryDropOnStudent()
    {
        var zones = FindObjectsByType<DocumentSubmissionZone>(FindObjectsSortMode.None);
        for (var index = 0; index < zones.Length; index++)
        {
            var zone = zones[index];
            if (zone != null && zone.TryReceivePaymentMachine(this))
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator ReturnRoutine()
    {
        while (Vector2.Distance(rectTransform.anchoredPosition, homePosition) > 0.5f)
        {
            rectTransform.anchoredPosition = Vector2.MoveTowards(
                rectTransform.anchoredPosition,
                homePosition,
                returnSpeed * Time.deltaTime);

            yield return null;
        }

        rectTransform.anchoredPosition = homePosition;
        returnCoroutine = null;
    }
}
