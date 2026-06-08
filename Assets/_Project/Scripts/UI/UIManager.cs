using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private HUDController hudController;
    [SerializeField] private RulebookPanel rulebookPanel;
    [SerializeField] private NoticeBoardPanel noticeBoardPanel;
    [SerializeField] private DaySummaryPanel daySummaryPanel;
    [SerializeField] private TMP_Text caseFeedbackText;
    [SerializeField] private GameObject caseFeedbackRoot;
    [SerializeField] private Image caseFeedbackBackground;
    [SerializeField] private Image caseFeedbackAccent;
    [SerializeField] private TMP_Text caseFeedbackTitleText;

    private void Awake()
    {
        EnsureRuntimeUi();
    }

    private void EnsureRuntimeUi()
    {
        if (hudController == null)
        {
            hudController = FindObjectOfType<HUDController>(true);
        }

        if (daySummaryPanel == null)
        {
            daySummaryPanel = FindObjectOfType<DaySummaryPanel>(true);
        }

        var canvas = FindObjectOfType<Canvas>(true);
        if (canvas == null)
        {
            return;
        }

        if (hudController == null)
        {
            hudController = CreateFallbackHud(canvas.transform);
        }

        if (daySummaryPanel == null)
        {
            daySummaryPanel = CreateFallbackDaySummary(canvas.transform);
        }

        if (caseFeedbackText == null)
        {
            CreateFallbackCaseFeedbackCard(canvas.transform);
        }
        else if (caseFeedbackRoot == null)
        {
            WrapAssignedCaseFeedbackText();
        }

        ClearCaseFeedback();
    }

    public void BindDay(DayConfig dayConfig)
    {
        if (rulebookPanel != null)
        {
            rulebookPanel.SetRules(dayConfig != null ? dayConfig.rulebookEntries : null);
        }

        if (noticeBoardPanel != null)
        {
            noticeBoardPanel.SetEntries(dayConfig != null ? dayConfig.noticeBoardEntries : null);
        }
    }

    public void ShowCurrentCase(StudentCaseDefinition caseDefinition)
    {
        if (hudController != null)
        {
            hudController.SetCaseInfo(caseDefinition);
        }
    }

    public void RefreshHUD(EconomySnapshot snapshot, float remainingTimeSeconds, float totalTimeSeconds)
    {
        if (hudController != null)
        {
            hudController.Refresh(snapshot, remainingTimeSeconds, totalTimeSeconds);
        }
    }

    public void ShowDaySummary(DaySummaryData summary)
    {
        if (daySummaryPanel != null)
        {
            daySummaryPanel.Show(summary);
        }
    }

    public void HideDaySummary()
    {
        if (daySummaryPanel != null)
        {
            daySummaryPanel.Hide();
        }
    }

    public void ShowCaseFeedback(string message, Color color)
    {
        ShowCaseFeedback(string.Empty, message, color);
    }

    public void ShowCaseFeedback(string title, string message, Color color)
    {
        if (caseFeedbackText == null)
        {
            return;
        }

        if (caseFeedbackRoot != null)
        {
            caseFeedbackRoot.SetActive(!string.IsNullOrWhiteSpace(message) || !string.IsNullOrWhiteSpace(title));
        }

        if (caseFeedbackBackground != null)
        {
            caseFeedbackBackground.color = new Color(0.04f, 0.045f, 0.055f, 0.94f);
        }

        if (caseFeedbackAccent != null)
        {
            caseFeedbackAccent.color = color;
        }

        if (caseFeedbackTitleText != null)
        {
            caseFeedbackTitleText.text = title;
            caseFeedbackTitleText.color = color;
            caseFeedbackTitleText.gameObject.SetActive(!string.IsNullOrWhiteSpace(title));
        }

        caseFeedbackText.text = message;
        caseFeedbackText.color = new Color(0.98f, 0.95f, 0.84f, 1f);
        caseFeedbackText.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
    }

    public void ClearCaseFeedback()
    {
        if (caseFeedbackText == null)
        {
            return;
        }

        caseFeedbackText.text = string.Empty;
        caseFeedbackText.gameObject.SetActive(false);

        if (caseFeedbackTitleText != null)
        {
            caseFeedbackTitleText.text = string.Empty;
            caseFeedbackTitleText.gameObject.SetActive(false);
        }

        if (caseFeedbackRoot != null)
        {
            caseFeedbackRoot.SetActive(false);
        }
    }

    private HUDController CreateFallbackHud(Transform parent)
    {
        var root = CreatePanelRoot("RuntimeHUD", parent);
        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0f, 1f);
        rootRect.anchorMax = new Vector2(1f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.anchoredPosition = new Vector2(0f, -16f);
        rootRect.sizeDelta = new Vector2(-32f, 140f);

        var image = root.AddComponent<Image>();
        image.color = new Color(0.08f, 0.11f, 0.14f, 0.82f);

        var controller = root.AddComponent<HUDController>();
        var currentCase = CreateText("CurrentCaseText", root.transform, new Vector2(16f, -14f), new Vector2(560f, 56f), 24, TextAlignmentOptions.TopLeft);
        var money = CreateText("MoneyText", root.transform, new Vector2(16f, -76f), new Vector2(260f, 32f), 22, TextAlignmentOptions.TopLeft);
        var debt = CreateText("DebtText", root.transform, new Vector2(292f, -76f), new Vector2(260f, 32f), 22, TextAlignmentOptions.TopLeft);
        var warnings = CreateText("WarningsText", root.transform, new Vector2(568f, -76f), new Vector2(320f, 32f), 22, TextAlignmentOptions.TopLeft);
        var timer = CreateText("TimerText", root.transform, new Vector2(-16f, -24f), new Vector2(260f, 32f), 22, TextAlignmentOptions.TopRight, true);

        controller.Configure(currentCase, money, debt, warnings, timer);
        return controller;
    }

    private DaySummaryPanel CreateFallbackDaySummary(Transform parent)
    {
        var root = CreatePanelRoot("RuntimeDaySummaryPanel", parent);
        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(760f, 520f);
        rootRect.anchoredPosition = Vector2.zero;

        var image = root.AddComponent<Image>();
        image.color = new Color(0.05f, 0.06f, 0.08f, 0.94f);

        var title = CreateText("TitleText", root.transform, new Vector2(0f, -24f), new Vector2(680f, 48f), 34, TextAlignmentOptions.Center);
        title.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        title.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        title.rectTransform.pivot = new Vector2(0.5f, 1f);

        var body = CreateText("BodyText", root.transform, new Vector2(0f, -92f), new Vector2(680f, 330f), 22, TextAlignmentOptions.TopLeft);
        body.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        body.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        body.rectTransform.pivot = new Vector2(0.5f, 1f);
        body.textWrappingMode = TextWrappingModes.Normal;

        var buttonObject = new GameObject("ContinueButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(root.transform, false);
        var buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = new Vector2(0f, 28f);
        buttonRect.sizeDelta = new Vector2(260f, 56f);

        var buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.18f, 0.48f, 0.34f, 1f);

        var buttonText = CreateText("ContinueButtonText", buttonObject.transform, Vector2.zero, new Vector2(240f, 46f), 24, TextAlignmentOptions.Center);
        buttonText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        buttonText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        buttonText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        buttonText.raycastTarget = false;

        var panel = root.AddComponent<DaySummaryPanel>();
        panel.Configure(root, title, body, buttonObject.GetComponent<Button>(), buttonText);
        root.SetActive(false);
        return panel;
    }

    private void CreateFallbackCaseFeedbackCard(Transform parent)
    {
        var root = new GameObject("RuntimeCaseFeedbackCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        root.transform.SetParent(parent, false);
        caseFeedbackRoot = root;

        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = new Vector2(0f, 105f);
        rootRect.sizeDelta = new Vector2(720f, 170f);

        caseFeedbackBackground = root.GetComponent<Image>();
        caseFeedbackBackground.color = new Color(0.04f, 0.045f, 0.055f, 0.94f);

        var accentObject = new GameObject("AccentBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        accentObject.transform.SetParent(root.transform, false);
        var accentRect = accentObject.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 0f);
        accentRect.anchorMax = new Vector2(0f, 1f);
        accentRect.pivot = new Vector2(0f, 0.5f);
        accentRect.sizeDelta = new Vector2(10f, 0f);
        accentRect.anchoredPosition = Vector2.zero;
        caseFeedbackAccent = accentObject.GetComponent<Image>();

        caseFeedbackTitleText = CreateText(
            "TitleText",
            root.transform,
            new Vector2(34f, -22f),
            new Vector2(652f, 38f),
            25,
            TextAlignmentOptions.Left);
        caseFeedbackTitleText.rectTransform.anchorMin = new Vector2(0f, 1f);
        caseFeedbackTitleText.rectTransform.anchorMax = new Vector2(0f, 1f);
        caseFeedbackTitleText.rectTransform.pivot = new Vector2(0f, 1f);
        caseFeedbackTitleText.fontStyle = FontStyles.Bold;

        caseFeedbackText = CreateText(
            "BodyText",
            root.transform,
            new Vector2(34f, -66f),
            new Vector2(652f, 84f),
            22,
            TextAlignmentOptions.TopLeft);
        caseFeedbackText.rectTransform.anchorMin = new Vector2(0f, 1f);
        caseFeedbackText.rectTransform.anchorMax = new Vector2(0f, 1f);
        caseFeedbackText.rectTransform.pivot = new Vector2(0f, 1f);
        caseFeedbackText.textWrappingMode = TextWrappingModes.Normal;
        caseFeedbackText.gameObject.SetActive(false);
        root.SetActive(false);
    }

    private void WrapAssignedCaseFeedbackText()
    {
        if (caseFeedbackText == null || caseFeedbackText.transform.parent == null)
        {
            return;
        }

        var originalParent = caseFeedbackText.transform.parent;
        var originalSiblingIndex = caseFeedbackText.transform.GetSiblingIndex();

        var root = new GameObject("RuntimeCaseFeedbackCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        root.transform.SetParent(originalParent, false);
        root.transform.SetSiblingIndex(originalSiblingIndex);
        caseFeedbackRoot = root;

        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = new Vector2(0f, 105f);
        rootRect.sizeDelta = new Vector2(720f, 170f);

        caseFeedbackBackground = root.GetComponent<Image>();
        caseFeedbackBackground.color = new Color(0.04f, 0.045f, 0.055f, 0.94f);

        var accentObject = new GameObject("AccentBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        accentObject.transform.SetParent(root.transform, false);
        var accentRect = accentObject.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 0f);
        accentRect.anchorMax = new Vector2(0f, 1f);
        accentRect.pivot = new Vector2(0f, 0.5f);
        accentRect.sizeDelta = new Vector2(10f, 0f);
        accentRect.anchoredPosition = Vector2.zero;
        caseFeedbackAccent = accentObject.GetComponent<Image>();

        caseFeedbackTitleText = CreateText(
            "TitleText",
            root.transform,
            new Vector2(34f, -22f),
            new Vector2(652f, 38f),
            25,
            TextAlignmentOptions.Left);
        caseFeedbackTitleText.rectTransform.anchorMin = new Vector2(0f, 1f);
        caseFeedbackTitleText.rectTransform.anchorMax = new Vector2(0f, 1f);
        caseFeedbackTitleText.rectTransform.pivot = new Vector2(0f, 1f);
        caseFeedbackTitleText.fontStyle = FontStyles.Bold;

        caseFeedbackText.transform.SetParent(root.transform, false);
        caseFeedbackText.rectTransform.anchorMin = new Vector2(0f, 1f);
        caseFeedbackText.rectTransform.anchorMax = new Vector2(0f, 1f);
        caseFeedbackText.rectTransform.pivot = new Vector2(0f, 1f);
        caseFeedbackText.rectTransform.anchoredPosition = new Vector2(34f, -66f);
        caseFeedbackText.rectTransform.sizeDelta = new Vector2(652f, 84f);
        caseFeedbackText.fontSize = 22f;
        caseFeedbackText.alignment = TextAlignmentOptions.TopLeft;
        caseFeedbackText.fontStyle = FontStyles.Normal;
        caseFeedbackText.textWrappingMode = TextWrappingModes.Normal;
        root.SetActive(false);
    }

    private static GameObject CreatePanelRoot(string name, Transform parent)
    {
        var panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        panel.transform.SetParent(parent, false);
        return panel;
    }

    private static TMP_Text CreateText(
        string name,
        Transform parent,
        Vector2 anchoredPosition,
        Vector2 size,
        float fontSize,
        TextAlignmentOptions alignment,
        bool alignRight = false)
    {
        var textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        var rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = alignRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
        rect.anchorMax = alignRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
        rect.pivot = alignRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        var text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = new Color(0.96f, 0.95f, 0.88f, 1f);
        text.text = string.Empty;
        text.raycastTarget = false;
        return text;
    }
}
