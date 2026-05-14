using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Permite que o jogador arraste o carimbo pela tela.
/// Ao soltar, verifica se caiu sobre um documento (StampReceiver).
/// Se sim, aplica o carimbo. Depois, volta automaticamente à posição original.
/// </summary>
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class DraggableStamp : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Tipo do Carimbo")]
    [SerializeField] private StampType tipoCarimbo = StampType.Aprovado;

    [Header("Retorno à Origem")]
    [Tooltip("Velocidade com que o carimbo volta ao lugar original (unidades/segundo).")]
    [SerializeField] private float velocidadeRetorno = 800f;

    [Header("Feedback Visual")]
    [Tooltip("Escala do carimbo enquanto está sendo arrastado.")]
    [SerializeField] private float escalaArrastando = 1.1f;

    // -------------------------------------------------------------------------
    // Estado interno
    // -------------------------------------------------------------------------

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private RectTransform rootCanvas;

    private Vector2 posicaoOrigem;
    private Vector2 offsetDrag;
    private bool estaArrastando;
    private Coroutine corotinaRetorno;

    // -------------------------------------------------------------------------
    // Unity Messages
    // -------------------------------------------------------------------------

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // Guarda a posição inicial para poder retornar depois
        posicaoOrigem = rectTransform.anchoredPosition;

        // Sobe até o Canvas raiz para converter posições corretamente
        rootCanvas = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }

    // -------------------------------------------------------------------------
    // Drag Handlers (interfaces de evento da UI do Unity)
    // -------------------------------------------------------------------------

    public void OnPointerDown(PointerEventData eventData)
    {
        // Se estava animando o retorno, cancela e começa a arrastar de onde está
        if (corotinaRetorno != null)
        {
            StopCoroutine(corotinaRetorno);
            corotinaRetorno = null;
        }

        estaArrastando = true;

        // Fica na frente de tudo durante o arraste
        transform.SetAsLastSibling();

        // Deixa raycasts passarem pelo carimbo para detectar documentos abaixo
        canvasGroup.blocksRaycasts = false;

        // Aumenta levemente o tamanho para dar sensação de "pegar"
        transform.localScale = Vector3.one * escalaArrastando;

        // Calcula o offset para o carimbo não "saltar" ao ser clicado
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas, eventData.position, eventData.pressEventCamera, out var ponteiro);
        offsetDrag = rectTransform.anchoredPosition - ponteiro;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!estaArrastando) return;

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

        // Tenta carimbar um documento que esteja embaixo do cursor
        TentarCarimbar();

        // Volta ao lugar original com animação
        canvasGroup.blocksRaycasts = true;
        corotinaRetorno = StartCoroutine(RetornarAOrigem());
    }

    // -------------------------------------------------------------------------
    // Lógica de Carimbo
    // -------------------------------------------------------------------------

    /// <summary>
    /// Faz um raycast na posição em que o carimbo foi solto.
    /// Se encontrar um StampReceiver, aplica o carimbo nele.
    /// </summary>
    private void TentarCarimbar()
    {
        // Converte a posição atual do carimbo (centro do RectTransform) para screen space
        Vector2 posicaoTela = RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);

        // Busca todos os StampReceivers ativos na cena e verifica sobreposição
        var receptores = FindObjectsByType<StampReceiver>(FindObjectsSortMode.None);

        foreach (var receptor in receptores)
        {
            var rectReceptor = receptor.GetComponent<RectTransform>();
            if (rectReceptor == null) continue;

            // Verifica se o centro do carimbo está dentro do retângulo do receptor
            if (RectTransformUtility.RectangleContainsScreenPoint(rectReceptor, posicaoTela, null))
            {
                receptor.ReceberCarimbo(tipoCarimbo);
                return;
            }
        }

    }

    // -------------------------------------------------------------------------
    // Animação de Retorno
    // -------------------------------------------------------------------------

    /// <summary>
    /// Move o carimbo de volta à posição original usando MoveTowards
    /// para uma velocidade constante e previsível.
    /// </summary>
    private IEnumerator RetornarAOrigem()
    {
        while (Vector2.Distance(rectTransform.anchoredPosition, posicaoOrigem) > 0.5f)
        {
            rectTransform.anchoredPosition = Vector2.MoveTowards(
                rectTransform.anchoredPosition,
                posicaoOrigem,
                velocidadeRetorno * Time.deltaTime
            );

            yield return null;
        }

        // Garante posição exata ao final
        rectTransform.anchoredPosition = posicaoOrigem;
        corotinaRetorno = null;
    }
}