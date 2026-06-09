using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// Controla o menu principal do jogo.
/// Gerencia a navegação entre os painéis (Menu, Como Jogar, Configurações)
/// e o carregamento da cena de gameplay.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Transicao de Inicio")]
    [SerializeField] private CanvasGroup transitionOverlay;
    [SerializeField] private RectTransform stampTransform;
    [SerializeField] private CanvasGroup stampCanvasGroup;
    [SerializeField] private TMP_Text dayTitleText;
    [SerializeField] private string tituloDoDia = "DIA 1 - MATRICULAS";
    [SerializeField] private AudioClip somCarimbo;
    [SerializeField] [Range(0f, 1f)] private float volumeCarimbo = 1f;
    [SerializeField] private float delayAntesDoCarimbo = 0.12f;
    [SerializeField] private float duracaoEntradaCarimbo = 0.16f;
    [SerializeField] private float duracaoSaidaCarimbo = 0.3f;
    [SerializeField] private float duracaoFadeParaPreto = 0.45f;
    [SerializeField] private float delayAntesDoTitulo = 0.18f;
    [SerializeField] private float duracaoFadeTitulo = 0.45f;
    [SerializeField] private float tempoVisivelTitulo = 1.2f;
    [SerializeField] private float duracaoSaidaTitulo = 0.45f;
    [SerializeField] private bool desabilitarBotoesDuranteTransicao = true;

    // -------------------------------------------------------------------------
    // Referências aos painéis — conecte no Inspector
    // -------------------------------------------------------------------------

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource menuMusicAudioSource;
    [SerializeField] private AudioClip somClique;
    [SerializeField] [Range(0f, 1f)] private float volumeClique = 1f;
    [SerializeField] private float duracaoFadeMusicaMenu = 1.1f;
    
    [Header("Painéis")]
    [SerializeField] private GameObject painelMenu;
    [SerializeField] private GameObject painelComoJogar;
    [SerializeField] private GameObject painelConfiguracoes;

    [Header("Configurações de Cena")]
    [SerializeField] private string nomeCenaJogo = "Game";

    [Header("Volume")]
    [SerializeField] private Slider sliderVolume;

    // Chave usada para salvar o volume no PlayerPrefs
    private const string ChaveVolume = "VolumeGeral";
    private bool iniciandoJogo;
    private float volumeOriginalMusicaMenu = 1f;

    // -------------------------------------------------------------------------
    // Unity Messages
    // -------------------------------------------------------------------------

    private void Awake()
    {
        // Garante que apenas o painel principal começa visível
        MostrarPainel(painelMenu);
        PrepararTransicaoInicial();
        ResolveMenuMusicAudioSource();

        if (menuMusicAudioSource != null)
        {
            volumeOriginalMusicaMenu = menuMusicAudioSource.volume;
        }
    }

    private void Start()
    {
        // Carrega o volume salvo anteriormente (padrão: 1.0 = 100%)
        float volumeSalvo = PlayerPrefs.GetFloat(ChaveVolume, 1f);
        AudioListener.volume = volumeSalvo;

        // Atualiza o slider para refletir o volume carregado
        if (sliderVolume != null)
        {
            sliderVolume.value = volumeSalvo;
        }
    }

    // -------------------------------------------------------------------------
    // Botões do Painel Principal
    // -------------------------------------------------------------------------

    /// <summary>
    /// Chamado pelo botão "Iniciar".
    /// Carrega a cena principal do jogo.
    /// </summary>
    public void IniciarJogo()
    {
        if (iniciandoJogo)
        {
            return;
        }

        iniciandoJogo = true;
        TocarClique();
        StartCoroutine(IniciarJogoComTransicao());
    }
    
    private IEnumerator IniciarJogoComTransicao()
    {
        if (desabilitarBotoesDuranteTransicao)
        {
            SetPainelInterativo(painelMenu, false);
            SetPainelInterativo(painelComoJogar, false);
            SetPainelInterativo(painelConfiguracoes, false);
        }

        if (menuMusicAudioSource != null)
        {
            StartCoroutine(FadeOutMusicaMenu(CalculateMenuMusicFadeDuration()));
        }

        yield return new WaitForSecondsRealtime(delayAntesDoCarimbo);

        if (stampTransform != null)
        {
            stampTransform.gameObject.SetActive(true);
            stampTransform.localScale = Vector3.one * 1.35f;
        }

        if (stampCanvasGroup != null)
        {
            stampCanvasGroup.alpha = 0f;
        }

        if (somCarimbo != null && audioSource != null)
        {
            audioSource.PlayOneShot(somCarimbo, volumeCarimbo);
        }

        float tempo = 0f;
        while (tempo < duracaoEntradaCarimbo)
        {
            tempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tempo / duracaoEntradaCarimbo);

            if (stampTransform != null)
            {
                stampTransform.localScale = Vector3.LerpUnclamped(Vector3.one * 1.35f, Vector3.one, Mathf.SmoothStep(0f, 1f, t));
            }

            if (stampCanvasGroup != null)
            {
                stampCanvasGroup.alpha = t;
            }

            yield return null;
        }

        if (stampTransform != null)
        {
            stampTransform.localScale = Vector3.one;
        }

        if (stampCanvasGroup != null)
        {
            stampCanvasGroup.alpha = 1f;
        }

        tempo = 0f;
        while (tempo < duracaoFadeParaPreto)
        {
            tempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tempo / duracaoFadeParaPreto);

            if (transitionOverlay != null)
            {
                transitionOverlay.alpha = Mathf.Lerp(0f, 1f, t);
            }

            yield return null;
        }

        if (transitionOverlay != null)
        {
            transitionOverlay.alpha = 1f;
        }

        yield return new WaitForSecondsRealtime(delayAntesDoTitulo);

        tempo = 0f;
        while (tempo < duracaoSaidaCarimbo)
        {
            tempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tempo / duracaoSaidaCarimbo);

            if (stampCanvasGroup != null)
            {
                stampCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            }

            yield return null;
        }

        if (stampCanvasGroup != null)
        {
            stampCanvasGroup.alpha = 0f;
        }

        if (stampTransform != null)
        {
            stampTransform.gameObject.SetActive(false);
        }

        if (dayTitleText != null)
        {
            dayTitleText.gameObject.SetActive(true);
            dayTitleText.text = tituloDoDia;
            var titleColor = dayTitleText.color;
            titleColor.a = 0f;
            dayTitleText.color = titleColor;

            tempo = 0f;
            while (tempo < duracaoFadeTitulo)
            {
                tempo += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(tempo / duracaoFadeTitulo);
                titleColor.a = t;
                dayTitleText.color = titleColor;
                yield return null;
            }

            titleColor.a = 1f;
            dayTitleText.color = titleColor;
            yield return new WaitForSecondsRealtime(tempoVisivelTitulo);

            tempo = 0f;
            while (tempo < duracaoSaidaTitulo)
            {
                tempo += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(tempo / duracaoSaidaTitulo);
                titleColor.a = Mathf.Lerp(1f, 0f, t);
                dayTitleText.color = titleColor;
                yield return null;
            }

            titleColor.a = 0f;
            dayTitleText.color = titleColor;
            dayTitleText.gameObject.SetActive(false);
        }

        SceneManager.LoadScene(nomeCenaJogo);
    }

    /// <summary>
    /// Chamado pelo botão "Como Jogar".
    /// Mostra o painel de instruções.
    /// </summary>
    public void AbrirComoJogar()
    {
        TocarClique();
        MostrarPainel(painelComoJogar);
    }

    /// <summary>
    /// Chamado pelo botão "Configurações".
    /// Mostra o painel de configurações.
    /// </summary>
    public void AbrirConfiguracoes()
    {
        TocarClique();
        MostrarPainel(painelConfiguracoes);
    }

    // -------------------------------------------------------------------------
    // Botão Voltar (usado nos dois sub-painéis)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Chamado pelo botão "Voltar" em qualquer sub-painel.
    /// Retorna ao painel principal.
    /// </summary>
    public void Voltar()
    {
        TocarClique();
        MostrarPainel(painelMenu);
    }

    // -------------------------------------------------------------------------
    // Slider de Volume
    // -------------------------------------------------------------------------

    /// <summary>
    /// Chamado automaticamente quando o slider de volume é movido.
    /// Conecte o evento OnValueChanged do Slider a este método no Inspector.
    /// </summary>
    /// <param name="novoVolume">Valor entre 0 (mudo) e 1 (máximo).</param>
    public void AlterarVolume(float novoVolume)
    {
        AudioListener.volume = novoVolume;

        // Salva o valor para que persista entre sessões do jogo
        PlayerPrefs.SetFloat(ChaveVolume, novoVolume);
        PlayerPrefs.Save();
    }
    
    private void TocarClique()
    {
        if (audioSource != null && somClique != null)
        {
            audioSource.PlayOneShot(somClique, volumeClique);
        }
    }

    private void PrepararTransicaoInicial()
    {
        if (transitionOverlay != null)
        {
            transitionOverlay.alpha = 0f;
            transitionOverlay.blocksRaycasts = false;
            transitionOverlay.interactable = false;
        }

        if (stampTransform != null)
        {
            stampTransform.gameObject.SetActive(false);
            stampTransform.localScale = Vector3.one;
        }

        if (stampCanvasGroup != null)
        {
            stampCanvasGroup.alpha = 0f;
        }

        if (dayTitleText != null)
        {
            dayTitleText.gameObject.SetActive(false);
        }
    }

    private IEnumerator FadeOutMusicaMenu(float duration)
    {
        if (menuMusicAudioSource == null)
        {
            yield break;
        }

        float volumeInicial = menuMusicAudioSource.volume;
        float tempo = 0f;

        while (tempo < duration && menuMusicAudioSource != null)
        {
            tempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(duration <= 0f ? 1f : tempo / duration);
            menuMusicAudioSource.volume = Mathf.Lerp(volumeInicial, 0f, t);
            yield return null;
        }

        if (menuMusicAudioSource != null)
        {
            menuMusicAudioSource.volume = 0f;
            menuMusicAudioSource.Stop();
            menuMusicAudioSource.volume = volumeOriginalMusicaMenu;
        }
    }

    private float CalculateMenuMusicFadeDuration()
    {
        float totalTransitionDuration =
            delayAntesDoCarimbo +
            duracaoEntradaCarimbo +
            duracaoFadeParaPreto +
            delayAntesDoTitulo +
            duracaoSaidaCarimbo +
            duracaoFadeTitulo +
            tempoVisivelTitulo +
            duracaoSaidaTitulo;

        return Mathf.Max(duracaoFadeMusicaMenu, totalTransitionDuration);
    }

    private void ResolveMenuMusicAudioSource()
    {
        if (menuMusicAudioSource != null)
        {
            return;
        }

        var namedObject = GameObject.Find("MusicaDeFundo");
        if (namedObject != null)
        {
            menuMusicAudioSource = namedObject.GetComponent<AudioSource>();
        }

        if (menuMusicAudioSource != null)
        {
            return;
        }

        var allSources = FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var source in allSources)
        {
            if (source != null && source != audioSource)
            {
                menuMusicAudioSource = source;
                return;
            }
        }
    }

    private static void SetPainelInterativo(GameObject painel, bool interativo)
    {
        if (painel == null)
        {
            return;
        }

        var selectableItems = painel.GetComponentsInChildren<Selectable>(true);
        foreach (var item in selectableItems)
        {
            item.interactable = interativo;
        }
    }

    // -------------------------------------------------------------------------
    // Método auxiliar privado
    // -------------------------------------------------------------------------

    /// <summary>
    /// Ativa apenas o painel informado e desativa todos os outros.
    /// </summary>
    private void MostrarPainel(GameObject painelAlvo)
    {
        painelMenu.SetActive(painelAlvo == painelMenu);
        painelComoJogar.SetActive(painelAlvo == painelComoJogar);
        painelConfiguracoes.SetActive(painelAlvo == painelConfiguracoes);
    }
}
