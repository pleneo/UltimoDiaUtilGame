using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class IntroTerminalController : MonoBehaviour
{
    [Header("Componentes de UI")]
    [SerializeField] private TMP_Text textoTerminal;
    [SerializeField] private GameObject logoDoJogo;
    [SerializeField] private AudioSource musicAudioSource;

    [Header("Configurações do Terminal")]
    [SerializeField] private float velocidadeDigitacao = 0.05f;
    [SerializeField] private Vector2 intervaloEntreLinhas = new Vector2(0.2f, 0.5f);
    [SerializeField] private float tempoAntesDeLimparTerminal = 0.6f;
    [SerializeField] private float tempoEsperaFinal = 2.5f;
    [SerializeField] private float tempoFadeOut = 1.8f;

    private CanvasGroup canvasGroup;
    private List<string> linhasDoTerminal;
    private Coroutine introCoroutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (logoDoJogo != null) logoDoJogo.SetActive(false);
        if (textoTerminal != null) textoTerminal.text = "";
        if (musicAudioSource != null)
        {
            musicAudioSource.playOnAwake = false;
            musicAudioSource.Stop();
        }

        // Prepara as frases simulando o sistema da Reitoria iniciando
        linhasDoTerminal = new List<string>()
        {
            "Iniciando terminal reitoria_sys v4.01...",
            "Conectando ao Banco de Dados Central...",
            "Verificando Status Financeiro do Estudante...",
            "Alerta: Dividas de Mensalidade Detectadas!",
            "Bloqueio de Formatura: Ativo.",
            "Carregando Rotinas de Trabalho Diario...",
            "Sistema Pronto. Acessando 'ULTIMOS DIAS UTEIS'..."
        };
    }

    private void Start()
    {
        if (textoTerminal == null)
        {
            Debug.LogWarning("IntroTerminalController está sem referência para textoTerminal. A intro será ignorada.", this);
            FinalizarIntroInstantaneamente();
            return;
        }

        introCoroutine = StartCoroutine(ExecutarIntroTerminal());
    }

    private IEnumerator ExecutarIntroTerminal()
    {
        // Efeito de digitação linha a linha
        string textoAcumulado = string.Empty;
        foreach (string linha in linhasDoTerminal)
        {
            string linhaCompleta = $"> {linha}\n";

            for (int i = 0; i < linhaCompleta.Length; i++)
            {
                textoTerminal.text = textoAcumulado + linhaCompleta.Substring(0, i + 1);
                yield return new WaitForSecondsRealtime(velocidadeDigitacao);
            }

            textoAcumulado += linhaCompleta;
            yield return new WaitForSecondsRealtime(Random.Range(intervaloEntreLinhas.x, intervaloEntreLinhas.y));
        }

        // Espera um pouco com o terminal cheio, dá uma piscada e limpa
        yield return new WaitForSecondsRealtime(tempoAntesDeLimparTerminal);
        textoTerminal.text = "";
        
        // Ativa o Logo do Jogo de forma impactante
        if (logoDoJogo != null)
        {
            logoDoJogo.SetActive(true);
        }

        if (musicAudioSource != null && !musicAudioSource.isPlaying)
        {
            musicAudioSource.Play();
        }

        // Deixa o logo visível pelo tempo programado
        yield return new WaitForSecondsRealtime(tempoEsperaFinal);

        // Fade Out da tela preta inteira para revelar o menu de trás
        float tempoPassado = 0f;
        while (tempoPassado < tempoFadeOut)
        {
            tempoPassado += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, tempoPassado / tempoFadeOut);
            yield return null;
        }

        FinalizarIntroInstantaneamente();
    }

    private void FinalizarIntroInstantaneamente()
    {
        if (logoDoJogo != null)
        {
            logoDoJogo.SetActive(true);
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (introCoroutine != null)
        {
            StopCoroutine(introCoroutine);
            introCoroutine = null;
        }
    }
}
