using UnityEngine;

/// <summary>
/// Marcador colocado em cada elemento de UI que deve receber o tema.
/// Equivalente a uma classe CSS: class="bg-panel text-heading"
/// O UIThemeApplier lê este componente e aplica a cor/fonte correta.
/// </summary>
public class UIThemeTarget : MonoBehaviour
{
    [Tooltip("Qual variável de cor do tema este elemento representa.")]
    public UIColorRole role = UIColorRole.TextPrimary;

    [Tooltip("Se verdadeiro, sobrescreve o tamanho da fonte pelo tema.")]
    public bool overrideFontSize = false;

    [Tooltip("Qual tamanho de fonte usar (somente se overrideFontSize = true).")]
    public UIFontRole fontRole = UIFontRole.Body;
}

// =============================================================================
// ENUMS — equivalente aos tokens de design (variáveis CSS)
// =============================================================================

/// <summary>
/// Todos os papéis de cor disponíveis no tema.
/// Adicione novos tokens aqui conforme o projeto crescer.
/// </summary>
public enum UIColorRole
{
    // Fundos
    BackgroundPrimary,
    BackgroundDesk,
    BackgroundPanel,
    BackgroundPanelBorder,

    // Identidade institucional
    Institutional,
    InstitutionalLight,
    InstitutionalBlue,

    // Feedback de gameplay
    FeedbackCorrect,
    FeedbackError,
    FeedbackWarning,
    FeedbackInfo,

    // Texto
    TextPrimary,
    TextSecondary,
    TextDisabled,
    TextOnDark,
    TextHeading,

    // Botões
    ButtonNormal,
    ButtonHover,
    ButtonPressed,
    ButtonDisabled,

    // HUD
    HUDTimerBar,
    HUDMoney,
    HUDDebt,

    // Documentos
    DocumentPaper,
    DocumentSuspicious,
    DocumentSelected,
    DocumentText,
}

/// <summary>
/// Papéis tipográficos disponíveis no tema.
/// </summary>
public enum UIFontRole
{
    Heading,
    Subheading,
    Body,
    Small
}