using UnityEngine;

/// <summary>
/// Tema visual global do jogo — equivalente ao :root { } do CSS.
/// Altere os valores aqui para refletir em toda a UI automaticamente.
/// Crie via: Assets → Create → Ultimo Dia Util → UI → Theme
/// </summary>
[CreateAssetMenu(menuName = "Ultimo Dia Util/UI/Theme", fileName = "UITheme")]
public class UITheme : ScriptableObject
{
    // -------------------------------------------------------------------------
    // CORES DE FUNDO
    // -------------------------------------------------------------------------
    [Header("─── Fundos ───────────────────────────")]

    [Tooltip("Fundo principal da cena (ambiente sombrio da reitoria)")]
    public Color backgroundPrimary   = new Color(0.10f, 0.12f, 0.15f); // #1A1F26

    [Tooltip("Fundo da mesa de trabalho")]
    public Color backgroundDesk      = new Color(0.22f, 0.16f, 0.10f); // #382918 – madeira escura

    [Tooltip("Fundo de painéis e modais (livro, resumo do dia)")]
    public Color backgroundPanel     = new Color(0.13f, 0.16f, 0.20f); // #212933

    [Tooltip("Borda/separador de painéis")]
    public Color backgroundPanelBorder = new Color(0.20f, 0.25f, 0.30f); // #334050

    // -------------------------------------------------------------------------
    // CORES INSTITUCIONAIS
    // -------------------------------------------------------------------------
    [Header("─── Identidade Institucional ─────────")]

    [Tooltip("Verde institucional da reitoria — cor dominante de elementos fixos")]
    public Color institutional       = new Color(0.10f, 0.30f, 0.20f); // #1A4D33

    [Tooltip("Versão mais clara do verde institucional para hover/highlight")]
    public Color institutionalLight  = new Color(0.15f, 0.42f, 0.28f); // #266B47

    [Tooltip("Azul escuro institucional — títulos, cabeçalhos")]
    public Color institutionalBlue   = new Color(0.08f, 0.15f, 0.28f); // #142647

    // -------------------------------------------------------------------------
    // CORES DE FEEDBACK (gameplay)
    // -------------------------------------------------------------------------
    [Header("─── Feedback de Gameplay ─────────────")]

    [Tooltip("Verde de acerto — deferido, aprovado, correto")]
    public Color feedbackCorrect     = new Color(0.18f, 0.72f, 0.38f); // #2EB861

    [Tooltip("Vermelho de erro — indeferido, negado, incorreto")]
    public Color feedbackError       = new Color(0.85f, 0.20f, 0.18f); // #D9332E

    [Tooltip("Amarelo de atenção — encaminhar ao superior, alerta")]
    public Color feedbackWarning     = new Color(0.95f, 0.75f, 0.10f); // #F2BF1A

    [Tooltip("Azul de informação — neutro, dica")]
    public Color feedbackInfo        = new Color(0.22f, 0.55f, 0.85f); // #388CD9

    // -------------------------------------------------------------------------
    // TEXTO
    // -------------------------------------------------------------------------
    [Header("─── Texto ────────────────────────────")]

    [Tooltip("Texto principal — corpo, labels")]
    public Color textPrimary         = new Color(0.88f, 0.85f, 0.78f); // #E0D9C7 – bege envelhecido

    [Tooltip("Texto secundário — subtítulos, metadados")]
    public Color textSecondary       = new Color(0.58f, 0.58f, 0.55f); // #94948C

    [Tooltip("Texto desabilitado / placeholder")]
    public Color textDisabled        = new Color(0.35f, 0.35f, 0.33f); // #595954

    [Tooltip("Texto sobre fundos escuros de destaque (ex: botões cheios)")]
    public Color textOnDark          = new Color(0.95f, 0.93f, 0.88f); // #F2EDE0

    [Tooltip("Texto de título / cabeçalho (maior peso visual)")]
    public Color textHeading         = new Color(0.95f, 0.90f, 0.75f); // #F2E6BF – dourado desgastado

    // -------------------------------------------------------------------------
    // INTERATIVOS
    // -------------------------------------------------------------------------
    [Header("─── Botões e Interativos ─────────────")]

    [Tooltip("Cor padrão de botões normais")]
    public Color buttonNormal        = new Color(0.18f, 0.22f, 0.28f); // #2E3847

    [Tooltip("Cor de botão com hover (mouse sobre)")]
    public Color buttonHover         = new Color(0.25f, 0.30f, 0.38f); // #404D61

    [Tooltip("Cor de botão pressionado")]
    public Color buttonPressed       = new Color(0.12f, 0.15f, 0.20f); // #1F2633

    [Tooltip("Cor de botão desabilitado")]
    public Color buttonDisabled      = new Color(0.15f, 0.18f, 0.22f); // #262D38

    // -------------------------------------------------------------------------
    // HUD
    // -------------------------------------------------------------------------
    [Header("─── HUD ──────────────────────────────")]

    [Tooltip("Barra de tempo — cor normal")]
    public Color hudTimerNormal      = new Color(0.18f, 0.72f, 0.38f); // igual feedbackCorrect

    [Tooltip("Barra de tempo — urgência (últimos 30%)")]
    public Color hudTimerUrgent      = new Color(0.95f, 0.75f, 0.10f); // igual feedbackWarning

    [Tooltip("Barra de tempo — crítico (últimos 10%)")]
    public Color hudTimerCritical    = new Color(0.85f, 0.20f, 0.18f); // igual feedbackError

    [Tooltip("Cor do contador de dinheiro")]
    public Color hudMoney            = new Color(0.95f, 0.75f, 0.10f); // dourado

    [Tooltip("Cor do contador de dívida")]
    public Color hudDebt             = new Color(0.85f, 0.20f, 0.18f); // vermelho

    // -------------------------------------------------------------------------
    // DOCUMENTOS
    // -------------------------------------------------------------------------
    [Header("─── Documentos ───────────────────────")]

    [Tooltip("Fundo do papel (documento legítimo)")]
    public Color documentPaper       = new Color(0.92f, 0.89f, 0.80f); // #EBE3CC – papel envelhecido

    [Tooltip("Fundo do papel suspeito / falso")]
    public Color documentSuspicious  = new Color(0.92f, 0.85f, 0.75f); // levemente diferente

    [Tooltip("Highlight de documento selecionado")]
    public Color documentSelected    = new Color(0.22f, 0.55f, 0.85f, 0.35f); // azul semitransparente

    [Tooltip("Texto dentro dos documentos")]
    public Color documentText        = new Color(0.10f, 0.10f, 0.12f); // #1A1A1F – quase preto

    // -------------------------------------------------------------------------
    // TAMANHOS E ESPAÇAMENTO
    // -------------------------------------------------------------------------
    [Header("─── Tipografia e Espaçamento ─────────")]

    [Tooltip("Tamanho de fonte: título grande")]
    public float fontSizeHeading     = 28f;

    [Tooltip("Tamanho de fonte: subtítulo / label")]
    public float fontSizeSubheading  = 20f;

    [Tooltip("Tamanho de fonte: corpo de texto")]
    public float fontSizeBody        = 16f;

    [Tooltip("Tamanho de fonte: texto pequeno / metadado")]
    public float fontSizeSmall       = 12f;

    [Tooltip("Raio de arredondamento padrão de painéis (px)")]
    public float borderRadiusPanel   = 4f;

    [Tooltip("Espaçamento interno padrão de painéis (px)")]
    public float paddingPanel        = 12f;

    // -------------------------------------------------------------------------
    // MÉTODOS UTILITÁRIOS
    // -------------------------------------------------------------------------

    /// <summary>
    /// Retorna a cor da barra de tempo com base no percentual restante (0–1).
    /// </summary>
    public Color GetTimerColor(float normalizedTime)
    {
        if (normalizedTime <= 0.10f) return hudTimerCritical;
        if (normalizedTime <= 0.30f) return hudTimerUrgent;
        return hudTimerNormal;
    }

    /// <summary>
    /// Retorna a cor de feedback baseada no resultado de uma decisão.
    /// </summary>
    public Color GetDecisionColor(bool isCorrect)
    {
        return isCorrect ? feedbackCorrect : feedbackError;
    }
}