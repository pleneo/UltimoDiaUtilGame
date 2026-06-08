using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DayStoryNoticePanel : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private GameObject introBubbleRoot;
    [SerializeField] private TMP_Text introText;
    [SerializeField] private GameObject newsBubbleRoot;
    [SerializeField] private TMP_Text newsText;
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text continueButtonText;

    private bool waitingForContinue;

    public void Configure(
        GameObject panelRoot,
        TMP_Text title,
        TMP_Text body,
        Button button,
        TMP_Text buttonText)
    {
        root = panelRoot;
        titleText = title;
        bodyText = body;
        continueButton = button;
        continueButtonText = buttonText;
    }

    public void ConfigureSeparatedBubbles(
        GameObject introRoot,
        TMP_Text intro,
        GameObject newsRoot,
        TMP_Text news)
    {
        introBubbleRoot = introRoot;
        introText = intro;
        newsBubbleRoot = newsRoot;
        newsText = news;
    }

    private void OnEnable()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(Continue);
        }
    }

    private void OnDisable()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(Continue);
        }
    }

    public IEnumerator ShowNotice(string title, string body, string buttonLabel)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            Hide();
            yield break;
        }

        Show();

        if (titleText != null)
        {
            titleText.text = title;
        }

        var splitNotice = SplitNoticeBody(body);
        if ((introText != null || newsText != null) &&
            (!string.IsNullOrWhiteSpace(splitNotice.intro) || !string.IsNullOrWhiteSpace(splitNotice.news)))
        {
            yield return ShowSeparatedNoticeSequence(title, splitNotice.intro, splitNotice.news, buttonLabel);
            Hide();
            yield break;
        }

        if (bodyText != null)
        {
            bodyText.gameObject.SetActive(true);
            bodyText.text = body;
        }

        if (continueButtonText != null)
        {
            continueButtonText.text = string.IsNullOrWhiteSpace(buttonLabel) ? "Continuar" : buttonLabel;
        }

        if (continueButton == null)
        {
            yield return new WaitForSeconds(3f);
            Hide();
            yield break;
        }

        waitingForContinue = true;
        while (waitingForContinue)
        {
            yield return null;
        }

        Hide();
    }

    public void Hide()
    {
        waitingForContinue = false;

        if (root != null)
        {
            root.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void Show()
    {
        if (root != null)
        {
            root.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    private void Continue()
    {
        waitingForContinue = false;
    }

    private void ApplySeparatedBody(string intro, string news)
    {
        if (bodyText != null)
        {
            bodyText.gameObject.SetActive(false);
        }

        var hasIntro = !string.IsNullOrWhiteSpace(intro);
        var hasNews = !string.IsNullOrWhiteSpace(news);

        if (introBubbleRoot != null)
        {
            introBubbleRoot.SetActive(hasIntro);
            if (hasIntro && !hasNews)
            {
                var introRect = introBubbleRoot.GetComponent<RectTransform>();
                if (introRect != null)
                {
                    introRect.anchoredPosition = new Vector2(0f, -148f);
                    introRect.sizeDelta = new Vector2(760f, 170f);
                }
            }
        }

        if (introText != null)
        {
            introText.text = intro;
        }

        if (newsBubbleRoot != null)
        {
            newsBubbleRoot.SetActive(hasNews);
            if (hasNews && !hasIntro)
            {
                var newsRect = newsBubbleRoot.GetComponent<RectTransform>();
                if (newsRect != null)
                {
                    newsRect.anchoredPosition = new Vector2(0f, -136f);
                    newsRect.sizeDelta = new Vector2(760f, 250f);
                }
            }
        }

        if (newsText != null)
        {
            newsText.text = news;
        }
    }

    private IEnumerator ShowSeparatedNoticeSequence(string title, string intro, string news, string finalButtonLabel)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }

        if (!string.IsNullOrWhiteSpace(intro))
        {
            ApplySeparatedBody(intro, string.Empty);
            SetButtonLabel(!string.IsNullOrWhiteSpace(news) ? "Continuar" : finalButtonLabel);
            yield return WaitForContinueOrDelay();
        }

        if (!string.IsNullOrWhiteSpace(news))
        {
            ApplySeparatedBody(string.Empty, news);
            SetButtonLabel(finalButtonLabel);
            yield return WaitForContinueOrDelay();
        }
    }

    private IEnumerator WaitForContinueOrDelay()
    {
        if (continueButton == null)
        {
            yield return new WaitForSeconds(3f);
            yield break;
        }

        waitingForContinue = true;
        while (waitingForContinue)
        {
            yield return null;
        }
    }

    private void SetButtonLabel(string buttonLabel)
    {
        if (continueButtonText != null)
        {
            continueButtonText.text = string.IsNullOrWhiteSpace(buttonLabel) ? "Continuar" : buttonLabel;
        }
    }

    private static (string intro, string news) SplitNoticeBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return (string.Empty, string.Empty);
        }

        const string marker = "Notícia do dia:";
        var markerIndex = body.IndexOf(marker, System.StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            return (body.Trim(), string.Empty);
        }

        var intro = body.Substring(0, markerIndex).Trim();
        var news = body.Substring(markerIndex).Trim();
        return (intro, news);
    }
}
