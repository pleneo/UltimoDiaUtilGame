using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Controla o Livro de Regras da mesa.
/// O jogador clica no ícone do livro → o painel expande.
/// Clicar no botão de fechar (ou no livro novamente) → o painel fecha.
/// O DayManager chama DefinirRegras() para atualizar o conteúdo a cada dia.
/// </summary>
public class RulebookController : MonoBehaviour
{
    [Header("Objetos da Cena")]
    [Tooltip("O ícone/botão do livro fechado que fica na mesa.")]
    [SerializeField] private GameObject iconeNaMesa;

    [Tooltip("O painel grande que aparece quando o livro é aberto.")]
    [SerializeField] private GameObject painelAberto;

    [Header("Conteúdo do Livro")]
    [Tooltip("Texto onde as regras do dia serão exibidas.")]
    [SerializeField] private TMP_Text textoRegras;

    [Header("Animação")]
    [Tooltip("Duração da animação de abrir/fechar em segundos.")]
    [SerializeField] private float duracaoAnimacao = 0.2f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip closeClip;
    [SerializeField] [Range(0f, 1f)] private float closeVolume = 1f;

    // -------------------------------------------------------------------------
    // Estado
    // -------------------------------------------------------------------------

    private bool estaAberto;
    private Coroutine corotinaAnimacao;

    // -------------------------------------------------------------------------
    // Unity Messages
    // -------------------------------------------------------------------------

    private void Awake()
    {
        ConfigureAudioSource();

        // Começa fechado
        painelAberto.SetActive(false);
        painelAberto.transform.localScale = Vector3.zero;
        estaAberto = false;
    }

    // -------------------------------------------------------------------------
    // API Pública
    // -------------------------------------------------------------------------

    /// <summary>
    /// Alterna entre aberto e fechado.
    /// Conecte este método ao OnClick do ícone do livro na mesa.
    /// </summary>
    public void AlternarLivro()
    {
        if (estaAberto)
            Fechar();
        else
            Abrir();
    }

    /// <summary>
    /// Fecha o painel explicitamente.
    /// Conecte este método ao botão de fechar dentro do painel.
    /// </summary>
    public void Fechar()
    {
        if (!estaAberto) return;
        estaAberto = false;
        PlayCloseSound();

        if (corotinaAnimacao != null) StopCoroutine(corotinaAnimacao);
        corotinaAnimacao = StartCoroutine(AnimarEscala(painelAberto, Vector3.zero, fecharAoFinal: true));
    }

    /// <summary>
    /// Chamado pelo DayManager no início de cada dia para definir
    /// as regras que aparecem no livro.
    /// </summary>
    /// <param name="regras">Lista de strings, uma por linha no livro.</param>
    public void DefinirRegras(List<string> regras)
    {
        if (textoRegras == null) return;

        textoRegras.text = regras != null && regras.Count > 0
            ? string.Join("\n\n", regras)
            : "Nenhuma regra definida para hoje.";
    }

    // -------------------------------------------------------------------------
    // Privado
    // -------------------------------------------------------------------------

    private void Abrir()
    {
        estaAberto = true;
        painelAberto.SetActive(true);

        if (corotinaAnimacao != null) StopCoroutine(corotinaAnimacao);
        corotinaAnimacao = StartCoroutine(AnimarEscala(painelAberto, Vector3.one, fecharAoFinal: false));
    }

    /// <summary>
    /// Anima a escala de um GameObject de onde está até o alvo.
    /// Usa uma curva suave (SmoothStep) para dar sensação de peso.
    /// </summary>
    private IEnumerator AnimarEscala(GameObject alvo, Vector3 escalaAlvo, bool fecharAoFinal)
    {
        Vector3 escalaInicial = alvo.transform.localScale;
        float tempo = 0f;

        while (tempo < duracaoAnimacao)
        {
            tempo += Time.deltaTime;

            // SmoothStep deixa a animação mais natural (acelera no início, desacelera no fim)
            float t = Mathf.SmoothStep(0f, 1f, tempo / duracaoAnimacao);
            alvo.transform.localScale = Vector3.LerpUnclamped(escalaInicial, escalaAlvo, t);

            yield return null;
        }

        alvo.transform.localScale = escalaAlvo;

        if (fecharAoFinal)
        {
            alvo.SetActive(false);
        }

        corotinaAnimacao = null;
    }

    private void ConfigureAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    private void PlayCloseSound()
    {
        if (audioSource == null || closeClip == null)
        {
            return;
        }

        audioSource.PlayOneShot(closeClip, closeVolume);
    }
}
