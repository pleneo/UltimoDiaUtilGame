using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
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

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private RectTransform rootCanvas;

    private Vector2 posicaoOrigem;
    private Vector2 offsetDrag;
    private bool estaArrastando;
    private Coroutine corotinaRetorno;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        posicaoOrigem = rectTransform.anchoredPosition;

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
        }

        estaArrastando = true;
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

        foreach (var receptor in receptores)
        {
            var rectReceptor = receptor.GetComponent<RectTransform>();
            if (rectReceptor == null)
            {
                continue;
            }

            if (RectTransformsOverlap(rectTransform, rectReceptor))
            {
                receptor.ReceberCarimbo(tipoCarimbo);
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
        while (Vector2.Distance(rectTransform.anchoredPosition, posicaoOrigem) > 0.5f)
        {
            rectTransform.anchoredPosition = Vector2.MoveTowards(
                rectTransform.anchoredPosition,
                posicaoOrigem,
                velocidadeRetorno * Time.deltaTime);

            yield return null;
        }

        rectTransform.anchoredPosition = posicaoOrigem;
        corotinaRetorno = null;
    }
}
