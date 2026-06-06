using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Adiciona o efeito de setas de seleção no menu principal,
/// inspirado no Hollow Knight. As setas « e » aparecem nos lados
/// do botão atualmente selecionado (hover ou teclado).
///
/// Como usar:
/// 1. Adicione este script em um GameObject vazio chamado "MenuSelector"
/// 2. Atribua os botões do menu na lista "botoesDoMenu"
/// 3. Crie dois TMP_Text filhos chamados "SetaEsquerda" e "SetaDireita"
///    e atribua nos campos correspondentes
/// </summary>
public class MenuSelector : MonoBehaviour
{
    [Header("Botões do Menu (na ordem de cima pra baixo)")]
    [SerializeField] private List<Button> botoesDoMenu = new();

    [Header("Setas Visuais")]
    [Tooltip("TMP_Text que exibe a seta da esquerda (ex: »)")]
    [SerializeField] private TMP_Text setaEsquerda;

    [Tooltip("TMP_Text que exibe a seta da direita (ex: «)")]
    [SerializeField] private TMP_Text setaDireita;

    [Header("Textos das Setas")]
    [SerializeField] private string textoSetaEsquerda = "»";
    [SerializeField] private string textoSetaDireita  = "«";

    [Header("Animação")]
    [Tooltip("Distância horizontal que as setas se movem ao aparecer (px)")]
    [SerializeField] private float distanciaEntrada = 8f;

    [Tooltip("Velocidade da animação de entrada/saída")]
    [SerializeField] private float velocidadeAnimacao = 8f;

    [Tooltip("Velocidade do efeito de flutuação das setas")]
    [SerializeField] private float velocidadeFlutuacao = 2f;

    [Tooltip("Amplitude do efeito de flutuação das setas (px)")]
    [SerializeField] private float amplitudeFlutuacao = 4f;

    // -------------------------------------------------------------------------
    // Estado
    // -------------------------------------------------------------------------

    private int indiceSelecionado = -1;
    private int indiceAnterior    = -1;

    private RectTransform rtSetaEsquerda;
    private RectTransform rtSetaDireita;

    private Vector2 posAlvoEsquerda;
    private Vector2 posAlvoDireita;

    private Coroutine corotinaFade;

    private CanvasGroup cgEsquerda;
    private CanvasGroup cgDireita;

    // -------------------------------------------------------------------------
    // Unity Messages
    // -------------------------------------------------------------------------

    private void Awake()
    {
        rtSetaEsquerda = setaEsquerda.GetComponent<RectTransform>();
        rtSetaDireita  = setaDireita.GetComponent<RectTransform>();

        cgEsquerda = ObterOuAdicionarCanvasGroup(setaEsquerda.gameObject);
        cgDireita  = ObterOuAdicionarCanvasGroup(setaDireita.gameObject);

        cgEsquerda.alpha = 0f;
        cgDireita.alpha  = 0f;

        setaEsquerda.text = textoSetaEsquerda;
        setaDireita.text  = textoSetaDireita;

        RegistrarEventosDeBotoes();
    }

    private void Update()
    {
        NavegacaoTeclado();
        AtualizarPosicaoSetas();
    }

    // -------------------------------------------------------------------------
    // Registro de eventos nos botões
    // -------------------------------------------------------------------------

    private void RegistrarEventosDeBotoes()
    {
        for (int i = 0; i < botoesDoMenu.Count; i++)
        {
            int indice = i;
            var gatilho = ObterOuAdicionarEventTrigger(botoesDoMenu[i].gameObject);

            AdicionarEntrada(gatilho, EventTriggerType.PointerEnter,
                _ => SelecionarBotao(indice));

            AdicionarEntrada(gatilho, EventTriggerType.PointerExit,
                _ => DeselecionarSeForEste(indice));
        }
    }

    // -------------------------------------------------------------------------
    // Seleção
    // -------------------------------------------------------------------------

    public void SelecionarBotao(int indice)
    {
        if (indice == indiceSelecionado) return;

        indiceAnterior    = indiceSelecionado;
        indiceSelecionado = indice;

        AtualizarAlvoSetas();
        MostrarSetas();
    }

    public void DeselecionarSeForEste(int indice)
    {
        if (indiceSelecionado != indice) return;

        indiceSelecionado = -1;
        EsconderSetas();
    }

    // -------------------------------------------------------------------------
    // Navegação por teclado — usa novo Input System (não UnityEngine.Input legado)
    // -------------------------------------------------------------------------

    private void NavegacaoTeclado()
    {
        if (botoesDoMenu.Count == 0) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        bool desceu = keyboard.downArrowKey.wasPressedThisFrame
                   || keyboard.sKey.wasPressedThisFrame;

        bool subiu  = keyboard.upArrowKey.wasPressedThisFrame
                   || keyboard.wKey.wasPressedThisFrame;

        bool confirmou = keyboard.enterKey.wasPressedThisFrame
                      || keyboard.numpadEnterKey.wasPressedThisFrame
                      || keyboard.spaceKey.wasPressedThisFrame;

        if (desceu)
        {
            int proximo = (indiceSelecionado + 1) % botoesDoMenu.Count;
            SelecionarBotao(proximo);
            botoesDoMenu[proximo].Select();
        }
        else if (subiu)
        {
            int anterior = (indiceSelecionado - 1 + botoesDoMenu.Count) % botoesDoMenu.Count;
            SelecionarBotao(anterior);
            botoesDoMenu[anterior].Select();
        }

        if (confirmou && indiceSelecionado >= 0 && indiceSelecionado < botoesDoMenu.Count)
        {
            botoesDoMenu[indiceSelecionado].onClick.Invoke();
        }
    }

    // -------------------------------------------------------------------------
    // Posicionamento das setas
    // -------------------------------------------------------------------------

    private void AtualizarAlvoSetas()
    {
        if (indiceSelecionado < 0 || indiceSelecionado >= botoesDoMenu.Count) return;

        var rtBotao = botoesDoMenu[indiceSelecionado].GetComponent<RectTransform>();

        Vector2 centroMundo  = rtBotao.position;
        float   larguraBotao = rtBotao.rect.width * rtBotao.lossyScale.x;

        var painelPai = rtSetaEsquerda.parent as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            painelPai,
            RectTransformUtility.WorldToScreenPoint(null, centroMundo),
            null,
            out Vector2 centroPainel
        );

        float offsetX = (larguraBotao * 0.5f) + amplitudeFlutuacao + 16f;

        posAlvoEsquerda = new Vector2(centroPainel.x - offsetX, centroPainel.y);
        posAlvoDireita  = new Vector2(centroPainel.x + offsetX, centroPainel.y);

        if (indiceAnterior != indiceSelecionado)
        {
            rtSetaEsquerda.anchoredPosition = posAlvoEsquerda + Vector2.left  * distanciaEntrada;
            rtSetaDireita.anchoredPosition  = posAlvoDireita  + Vector2.right * distanciaEntrada;
        }
    }

    private void AtualizarPosicaoSetas()
    {
        if (indiceSelecionado < 0) return;

        float flutuacao = Mathf.Sin(Time.time * velocidadeFlutuacao) * amplitudeFlutuacao;

        Vector2 alvoE = posAlvoEsquerda + Vector2.left  * flutuacao;
        Vector2 alvoD = posAlvoDireita  + Vector2.right * flutuacao;

        rtSetaEsquerda.anchoredPosition = Vector2.Lerp(
            rtSetaEsquerda.anchoredPosition, alvoE, Time.deltaTime * velocidadeAnimacao);

        rtSetaDireita.anchoredPosition = Vector2.Lerp(
            rtSetaDireita.anchoredPosition, alvoD, Time.deltaTime * velocidadeAnimacao);
    }

    // -------------------------------------------------------------------------
    // Fade das setas
    // -------------------------------------------------------------------------

    private void MostrarSetas()
    {
        if (corotinaFade != null) StopCoroutine(corotinaFade);
        corotinaFade = StartCoroutine(FadeSetas(1f));
    }

    private void EsconderSetas()
    {
        if (corotinaFade != null) StopCoroutine(corotinaFade);
        corotinaFade = StartCoroutine(FadeSetas(0f));
    }

    private IEnumerator FadeSetas(float alvo)
    {
        while (!Mathf.Approximately(cgEsquerda.alpha, alvo))
        {
            float novoAlpha = Mathf.MoveTowards(cgEsquerda.alpha, alvo, Time.deltaTime * velocidadeAnimacao);
            cgEsquerda.alpha = novoAlpha;
            cgDireita.alpha  = novoAlpha;
            yield return null;
        }

        cgEsquerda.alpha = alvo;
        cgDireita.alpha  = alvo;
    }

    // -------------------------------------------------------------------------
    // Utilitários
    // -------------------------------------------------------------------------

    private static CanvasGroup ObterOuAdicionarCanvasGroup(GameObject go)
    {
        var cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }

    private static EventTrigger ObterOuAdicionarEventTrigger(GameObject go)
    {
        var et = go.GetComponent<EventTrigger>();
        if (et == null) et = go.AddComponent<EventTrigger>();
        return et;
    }

    private static void AdicionarEntrada(EventTrigger trigger, EventTriggerType tipo,
        System.Action<BaseEventData> callback)
    {
        var entrada = new EventTrigger.Entry { eventID = tipo };
        entrada.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(callback));
        trigger.triggers.Add(entrada);
    }
}