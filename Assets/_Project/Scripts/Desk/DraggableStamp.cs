using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(AudioSource))]
public class DraggableStamp : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Tipo do Carimbo")]
    [SerializeField] private StampType tipoCarimbo = StampType.Aprovado;

    [Header("Retorno a Origem")]
    [Tooltip("Velocidade com que o carimbo volta ao lugar original (unidades/segundo).")]
    [SerializeField] private float velocidadeRetorno = 800f;

    [Header("Feedback Visual")]
    [Tooltip("Escala do carimbo enquanto esta sendo arrastado.")]
    [SerializeField] private float escalaArrastando = 1.1f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip returnClip;
    [SerializeField] [Range(0f, 1f)] private float returnVolume = 1f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private RectTransform rootCanvas;
    private RectTransform originalParent;

    private Vector2 posicaoOrigem;
    private Vector2 posicaoOrigemNoCanvas;
    private Vector2 offsetDrag;
    private bool estaArrastando;
    private Coroutine corotinaRetorno;
    private int originalSiblingIndex;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        ConfigureAudioSource();
        originalParent = rectTransform.parent as RectTransform;
        originalSiblingIndex = rectTransform.GetSiblingIndex();
        SaveOriginalLayout();

        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            rootCanvas = canvas.GetComponent<RectTransform>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (rootCanvas == null)
        {
            return;
        }

        if (corotinaRetorno != null)
        {
            StopCoroutine(corotinaRetorno);
            corotinaRetorno = null;
            RestoreOriginalParent();
            rectTransform.anchoredPosition = posicaoOrigem;
        }

        estaArrastando = true;
        SaveOriginalLayout();

        if (rootCanvas != null && rectTransform.parent != rootCanvas)
        {
            rectTransform.SetParent(rootCanvas, true);
        }

        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
        transform.localScale = Vector3.one * escalaArrastando;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas, eventData.position, eventData.pressEventCamera, out var ponteiro);
        offsetDrag = rectTransform.anchoredPosition - ponteiro;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!estaArrastando || rootCanvas == null)
        {
            return;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas, eventData.position, eventData.pressEventCamera, out var ponteiro))
        {
            rectTransform.anchoredPosition = ponteiro + offsetDrag;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        estaArrastando = false;
        transform.localScale = Vector3.one;

        TentarCarimbar();

        canvasGroup.blocksRaycasts = true;
        corotinaRetorno = StartCoroutine(RetornarAOrigem());
    }

    private void TentarCarimbar()
    {
        var receptores = FindObjectsByType<StampReceiver>(FindObjectsSortMode.None);
        var stampWorldPosition = rectTransform.position;

        foreach (var receptor in receptores)
        {
            var rectReceptor = receptor.GetComponent<RectTransform>();
            if (rectReceptor == null)
            {
                continue;
            }

            if (RectTransformsOverlap(rectTransform, rectReceptor))
            {
                receptor.ReceberCarimbo(tipoCarimbo, stampWorldPosition);
                return;
            }
        }
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

    private IEnumerator RetornarAOrigem()
    {
        while (Vector2.Distance(rectTransform.anchoredPosition, posicaoOrigemNoCanvas) > 0.5f)
        {
            rectTransform.anchoredPosition = Vector2.MoveTowards(
                rectTransform.anchoredPosition,
                posicaoOrigemNoCanvas,
                velocidadeRetorno * Time.deltaTime);

            yield return null;
        }

        RestoreOriginalParent();
        rectTransform.anchoredPosition = posicaoOrigem;
        PlayReturnSound();
        corotinaRetorno = null;
    }

    private void ConfigureAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    private void PlayReturnSound()
    {
        if (audioSource == null || returnClip == null)
        {
            return;
        }

        audioSource.PlayOneShot(returnClip, returnVolume);
    }

    private void RestoreOriginalParent()
    {
        if (originalParent == null)
        {
            return;
        }

        rectTransform.SetParent(originalParent, true);
        rectTransform.SetSiblingIndex(Mathf.Clamp(originalSiblingIndex, 0, originalParent.childCount - 1));
    }

    private void SaveOriginalLayout()
    {
        originalParent = rectTransform.parent as RectTransform;
        originalSiblingIndex = rectTransform.GetSiblingIndex();
        posicaoOrigem = rectTransform.anchoredPosition;

        if (rootCanvas != null && originalParent != null)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvas,
                screenPoint,
                null,
                out posicaoOrigemNoCanvas);
        }
        else
        {
            posicaoOrigemNoCanvas = posicaoOrigem;
        }
    }
}
