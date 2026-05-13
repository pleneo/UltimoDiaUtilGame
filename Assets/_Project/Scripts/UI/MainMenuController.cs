using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controla o menu principal do jogo.
/// Gerencia a navegação entre os painéis (Menu, Como Jogar, Configurações)
/// e o carregamento da cena de gameplay.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Referências aos painéis — conecte no Inspector
    // -------------------------------------------------------------------------

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

    // -------------------------------------------------------------------------
    // Unity Messages
    // -------------------------------------------------------------------------

    private void Awake()
    {
        // Garante que apenas o painel principal começa visível
        MostrarPainel(painelMenu);
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
        SceneManager.LoadScene(nomeCenaJogo);
    }

    /// <summary>
    /// Chamado pelo botão "Como Jogar".
    /// Mostra o painel de instruções.
    /// </summary>
    public void AbrirComoJogar()
    {
        MostrarPainel(painelComoJogar);
    }

    /// <summary>
    /// Chamado pelo botão "Configurações".
    /// Mostra o painel de configurações.
    /// </summary>
    public void AbrirConfiguracoes()
    {
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