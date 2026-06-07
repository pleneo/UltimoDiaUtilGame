using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class DeskSceneLayout : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private RectTransform worldBackground;
    [SerializeField] private RectTransform deskScene;
    [SerializeField] private RectTransform deskVisual;
    [SerializeField] private RectTransform deskPanel;
    [SerializeField] private RectTransform sidePanel;

    [Header("Edit Mode")]
    [SerializeField] private bool applyOnValidate = true;

    [Header("World Background")]
    [SerializeField] private Color worldBackgroundColor = new(0.22f, 0.28f, 0.33f, 1f);

    [Header("Desk Scene")]
    [SerializeField] private Vector2 deskSceneAnchorMin = new(0.03f, 0f);
    [SerializeField] private Vector2 deskSceneAnchorMax = new(0.75f, 1f);

    [Header("Desk Visual")]
    [SerializeField] private Vector2 deskVisualAnchorMin = new(0f, 0f);
    [SerializeField] private Vector2 deskVisualAnchorMax = new(1f, 0.32f);
    [SerializeField] private Color deskVisualColor = new(0.37f, 0.23f, 0.15f, 1f);

    [Header("Desk Panel")]
    [SerializeField] private Vector2 deskPanelAnchorMin = new(0.08f, 0.18f);
    [SerializeField] private Vector2 deskPanelAnchorMax = new(0.92f, 0.78f);
    [SerializeField] private Color deskPanelColor = new(0.92f, 0.88f, 0.77f, 0.45f);

    [Header("Side Panel")]
    [SerializeField] private Vector2 sidePanelAnchorMin = new(0.78f, 0.08f);
    [SerializeField] private Vector2 sidePanelAnchorMax = new(0.97f, 0.92f);

    // private void OnValidate()
    // {
    //     if (!applyOnValidate)
    //     {
    //         return;
    //     }
    //
    //     ApplyLayout();
    // }
    //
    // [ContextMenu("Apply Desk Layout")]
    // public void ApplyLayout()
    // {
    //     ApplyStretch(worldBackground, Vector2.zero, Vector2.one);
    //     ApplyStretch(deskScene, deskSceneAnchorMin, deskSceneAnchorMax);
    //     ApplyStretch(deskVisual, deskVisualAnchorMin, deskVisualAnchorMax);
    //     ApplyStretch(deskPanel, deskPanelAnchorMin, deskPanelAnchorMax);
    //     ApplyStretch(sidePanel, sidePanelAnchorMin, sidePanelAnchorMax);
    //
    //     ApplyImageColor(worldBackground, worldBackgroundColor);
    //     ApplyImageColor(deskVisual, deskVisualColor);
    //     ApplyImageColor(deskPanel, deskPanelColor);
    //
    //     OrderCanvasChildren();
    // }
    //
    // private static void ApplyStretch(RectTransform target, Vector2 anchorMin, Vector2 anchorMax)
    // {
    //     if (target == null)
    //     {
    //         return;
    //     }
    //
    //     target.anchorMin = anchorMin;
    //     target.anchorMax = anchorMax;
    //     target.offsetMin = Vector2.zero;
    //     target.offsetMax = Vector2.zero;
    //     target.localScale = Vector3.one;
    //     target.localRotation = Quaternion.identity;
    // }
    //
    // private static void ApplyImageColor(Component target, Color color)
    // {
    //     if (target == null)
    //     {
    //         return;
    //     }
    //
    //     var image = target.GetComponent<Image>();
    //     if (image != null)
    //     {
    //         image.color = color;
    //     }
    // }
    //
    // private void OrderCanvasChildren()
    // {
    //     if (worldBackground != null)
    //     {
    //         worldBackground.SetSiblingIndex(0);
    //     }
    //
    //     if (deskScene != null)
    //     {
    //         deskScene.SetSiblingIndex(1);
    //     }
    //
    //     if (sidePanel != null)
    //     {
    //         sidePanel.SetSiblingIndex(2);
    //     }
    //
    //     if (deskVisual != null)
    //     {
    //         deskVisual.SetSiblingIndex(0);
    //     }
    //
    //     if (deskPanel != null)
    //     {
    //         deskPanel.SetSiblingIndex(1);
    //     }
    // }
}
