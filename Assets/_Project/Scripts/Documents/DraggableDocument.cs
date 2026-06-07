using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class DraggableDocument : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private DocumentView documentView;
    [SerializeField] private float selectedScale = 1.03f;

    private static DraggableDocument currentSelected;

    public event Action<DraggableDocument> Selected;
    public event Action<DraggableDocument, PointerEventData> DragEnded;

    public DocumentRecord BoundRecord { get; private set; }
    public bool IsDragging { get; private set; }
    public RectTransform RectTransform => rectTransform;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private RectTransform parentRectTransform;
    private Vector2 dragOffset;
    private Vector2 lastValidAnchoredPosition;
    private Vector3 baseScale = Vector3.one;
    private bool interactionEnabled = true;

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

        ResolveDocumentView();
    }

    public void Bind(DocumentRecord record)
    {
        EnsureTextFields();
        BoundRecord = record;

        if (ResolveDocumentView() != null)
        {
            documentView.Bind(record);
            return;
        }

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
        if (!interactionEnabled)
        {
            return;
        }

        parentRectTransform = ResolveDragRoot();

        if (parentRectTransform == null)
        {
            return;
        }

        transform.SetAsLastSibling();
        Select();
        IsDragging = true;
        lastValidAnchoredPosition = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, eventData.position, eventData.pressEventCamera, out var localPointerPosition);
        dragOffset = rectTransform.anchoredPosition - localPointerPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!interactionEnabled || !IsDragging || parentRectTransform == null)
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
        if (!interactionEnabled)
        {
            return;
        }

        IsDragging = false;
        canvasGroup.blocksRaycasts = true;
        SetSelectedVisual(false);
        DragEnded?.Invoke(this, eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!interactionEnabled)
        {
            return;
        }

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

    public void ReturnToLastValidPosition()
    {
        rectTransform.anchoredPosition = lastValidAnchoredPosition;
    }

    public void SetInteractionEnabled(bool isEnabled)
    {
        interactionEnabled = isEnabled;
        IsDragging = false;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = isEnabled;
        }
    }

    public void SetVisualAlpha(float alpha)
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = Mathf.Clamp01(alpha);
    }

    private void OnDisable()
    {
        if (currentSelected == this)
        {
            currentSelected = null;
        }
    }

    private DocumentView ResolveDocumentView()
    {
        if (documentView == null)
        {
            documentView = GetComponent<DocumentView>();
        }

        if (documentView == null)
        {
            documentView = GetComponentInChildren<DocumentView>(true);
        }

        return documentView;
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

    private void EnsureTextFields()
    {
        if (titleText == null)
        {
            titleText = CreateTextChild("Title", new Vector2(0f, 120f), new Vector2(180f, 34f), 20f, TextAlignmentOptions.Center);
            titleText.fontStyle = FontStyles.Bold;
        }

        if (bodyText == null)
        {
            bodyText = CreateTextChild("Body", new Vector2(0f, -28f), new Vector2(180f, 210f), 13f, TextAlignmentOptions.TopLeft);
        }
    }

    private TMP_Text CreateTextChild(string childName, Vector2 anchoredPosition, Vector2 size, float fontSize, TextAlignmentOptions alignment)
    {
        var textObject = new GameObject(childName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(transform, false);

        var textRect = textObject.GetComponent<RectTransform>();
        textRect.sizeDelta = size;
        textRect.anchoredPosition = anchoredPosition;

        var text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.color = new Color(0.12f, 0.1f, 0.08f, 1f);
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Ellipsis;
        return text;
    }
}
