using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class DraggableDocument : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private float selectedScale = 1.03f;

    private static DraggableDocument currentSelected;

    public event Action<DraggableDocument> Selected;

    public DocumentRecord BoundRecord { get; private set; }
    public bool IsDragging { get; private set; }

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private RectTransform parentRectTransform;
    private Vector2 dragOffset;
    private Vector3 baseScale = Vector3.one;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        parentRectTransform = ResolveDragRoot();
        baseScale = transform.localScale;
    }

    public void Bind(DocumentRecord record)
    {
        BoundRecord = record;

        if (titleText != null)
        {
            titleText.text = record != null ? record.GetDisplayName() : "Documento";
        }

        if (bodyText != null)
        {
            bodyText.text = record != null ? record.BuildSummary() : string.Empty;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentRectTransform = ResolveDragRoot();

        if (parentRectTransform == null)
        {
            return;
        }

        transform.SetAsLastSibling();
        Select();
        IsDragging = true;
        canvasGroup.blocksRaycasts = false;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, eventData.position, eventData.pressEventCamera, out var localPointerPosition);
        dragOffset = rectTransform.anchoredPosition - localPointerPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsDragging || parentRectTransform == null)
        {
            return;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, eventData.position, eventData.pressEventCamera, out var localPointerPosition))
        {
            rectTransform.anchoredPosition = localPointerPosition + dragOffset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDragging = false;
        canvasGroup.blocksRaycasts = true;
        SetSelectedVisual(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        transform.SetAsLastSibling();
        Select();
        Selected?.Invoke(this);
    }

    public void Select()
    {
        if (currentSelected != null && currentSelected != this)
        {
            currentSelected.SetSelectedVisual(false);
        }

        currentSelected = this;
        SetSelectedVisual(true);
    }

    public void SetSelectedVisual(bool isSelected)
    {
        transform.localScale = isSelected ? baseScale * selectedScale : baseScale;
    }

    private void OnDisable()
    {
        if (currentSelected == this)
        {
            currentSelected = null;
        }
    }

    private RectTransform ResolveDragRoot()
    {
        var current = transform.parent;
        while (current != null)
        {
            if (current is RectTransform rect)
            {
                return rect;
            }

            current = current.parent;
        }

        return null;
    }
}
