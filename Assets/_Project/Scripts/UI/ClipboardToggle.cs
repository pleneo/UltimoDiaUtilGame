using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class ClipboardToggle : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Vector2 collapsedSize = new Vector2(180f, 240f);
    [SerializeField] private Vector2 expandedSize = new Vector2(300f, 360f);
    [SerializeField] private bool startExpanded;

    public bool IsExpanded { get; private set; }

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        IsExpanded = startExpanded;
        ApplyState();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        Toggle();
    }

    public void Toggle()
    {
        IsExpanded = !IsExpanded;
        ApplyState();
    }

    private void ApplyState()
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.sizeDelta = IsExpanded ? expandedSize : collapsedSize;
    }
}
