using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Aplica o UITheme nos componentes de UI de uma cena.
/// Coloque este script em um GameObject raiz da cena e defina o tema no Inspector.
/// Funciona tanto em runtime (Start) quanto no Editor (botão "Aplicar Tema").
/// </summary>
public class UIThemeApplier : MonoBehaviour
{
    [Header("Tema")]
    [SerializeField] private UITheme theme;

    [Header("Configuração")]
    [Tooltip("Se verdadeiro, reaplica o tema sempre que a cena iniciar.")]
    [SerializeField] private bool applyOnStart = true;

    [Tooltip("Se verdadeiro, loga no Console quais elementos foram alterados.")]
    [SerializeField] private bool debugLog = false;

    // -------------------------------------------------------------------------
    // Unity Messages
    // -------------------------------------------------------------------------

    private void Start()
    {
        if (applyOnStart && theme != null)
        {
            ApplyTheme();
        }
    }

    // -------------------------------------------------------------------------
    // API Pública
    // -------------------------------------------------------------------------

    /// <summary>
    /// Aplica o tema em todos os elementos marcados com UIThemeTarget na cena.
    /// Pode ser chamado pelo editor via botão ou em runtime.
    /// </summary>
    public void ApplyTheme()
    {
        if (theme == null)
        {
            Debug.LogWarning("[UIThemeApplier] Nenhum UITheme atribuído.");
            return;
        }

        // Aplica fonte em TODOS os TMP_Text da cena (sem precisar de UIThemeTarget)
        ApplyFontToAll();

        var targets = FindObjectsByType<UIThemeTarget>(FindObjectsSortMode.None);
        int count = 0;

        foreach (var target in targets)
        {
            ApplyToTarget(target);
            count++;
        }

        if (debugLog)
        {
            Debug.Log($"[UIThemeApplier] Tema aplicado em {count} elemento(s).");
        }
    }

    /// <summary>
    /// Aplica fontPrimary em todos os TMP_Text da cena que não tenham UIThemeTarget.
    /// Garante que nenhum texto fique com a fonte padrão esquecida.
    /// </summary>
    private void ApplyFontToAll()
    {
        if (theme.fontPrimary == null) return;

        var todosOsTextos = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
        int count = 0;

        foreach (var txt in todosOsTextos)
        {
            var themeTarget = txt.GetComponent<UIThemeTarget>();
            if (themeTarget != null && themeTarget.ignoreThemeFont)
            {
                continue;
            }

            // Se tem UIThemeTarget com role de Heading e fontHeading definida, usa ela
            bool isHeading = themeTarget != null && themeTarget.role == UIColorRole.TextHeading;

            if (isHeading && theme.fontHeading != null)
                txt.font = theme.fontHeading;
            else
                txt.font = theme.fontPrimary;

            count++;
        }

        Log($"Fonte aplicada em {count} TMP_Text(s).");
    }

    // -------------------------------------------------------------------------
    // Aplicação por elemento
    // -------------------------------------------------------------------------

    private void ApplyToTarget(UIThemeTarget target)
    {
        if (target == null) return;

        Color cor = ResolveColor(target.role);

        // Aplica em Image
        var image = target.GetComponent<Image>();
        if (image != null)
        {
            image.color = cor;
            Log($"Image '{target.name}' → {target.role}");
        }

        // Aplica em TextMeshProUGUI
        var tmp = target.GetComponent<TMP_Text>();
        if (tmp != null)
        {
            tmp.color = cor;
            if (target.overrideFontSize)
            {
                tmp.fontSize = ResolveFontSize(target.fontRole);
            }

            if (!target.ignoreThemeFont)
            {
                // Aplica a fonte correta: heading usa fontHeading (se definida), resto usa fontPrimary
                var fonte = (target.role == UIColorRole.TextHeading && theme.fontHeading != null)
                    ? theme.fontHeading
                    : theme.fontPrimary;

                if (fonte != null)
                {
                    tmp.font = fonte;
                }
            }

            Log($"TMP '{target.name}' → {target.role}");
        }

        // Aplica em Slider (barra de fill)
        if (target.role == UIColorRole.HUDTimerBar)
        {
            var slider = target.GetComponent<Slider>();
            if (slider != null && slider.fillRect != null)
            {
                var fill = slider.fillRect.GetComponent<Image>();
                if (fill != null)
                {
                    float t = slider.normalizedValue;
                    fill.color = theme.GetTimerColor(t);
                }
            }
        }
    }

    /// <summary>
    /// Resolve qual cor do tema corresponde a um UIColorRole.
    /// Equivalente a usar uma variável CSS: var(--cor-primaria)
    /// </summary>
    private Color ResolveColor(UIColorRole role)
    {
        return role switch
        {
            UIColorRole.BackgroundPrimary     => theme.backgroundPrimary,
            UIColorRole.BackgroundDesk        => theme.backgroundDesk,
            UIColorRole.BackgroundPanel       => theme.backgroundPanel,
            UIColorRole.BackgroundPanelBorder => theme.backgroundPanelBorder,

            UIColorRole.Institutional         => theme.institutional,
            UIColorRole.InstitutionalLight    => theme.institutionalLight,
            UIColorRole.InstitutionalBlue     => theme.institutionalBlue,

            UIColorRole.FeedbackCorrect       => theme.feedbackCorrect,
            UIColorRole.FeedbackError         => theme.feedbackError,
            UIColorRole.FeedbackWarning       => theme.feedbackWarning,
            UIColorRole.FeedbackInfo          => theme.feedbackInfo,

            UIColorRole.TextPrimary           => theme.textPrimary,
            UIColorRole.TextSecondary         => theme.textSecondary,
            UIColorRole.TextDisabled          => theme.textDisabled,
            UIColorRole.TextOnDark            => theme.textOnDark,
            UIColorRole.TextHeading           => theme.textHeading,

            UIColorRole.ButtonNormal          => theme.buttonNormal,
            UIColorRole.ButtonHover           => theme.buttonHover,
            UIColorRole.ButtonPressed         => theme.buttonPressed,
            UIColorRole.ButtonDisabled        => theme.buttonDisabled,

            UIColorRole.HUDTimerBar           => theme.hudTimerNormal,
            UIColorRole.HUDMoney              => theme.hudMoney,
            UIColorRole.HUDDebt               => theme.hudDebt,

            UIColorRole.DocumentPaper         => theme.documentPaper,
            UIColorRole.DocumentSuspicious    => theme.documentSuspicious,
            UIColorRole.DocumentSelected      => theme.documentSelected,
            UIColorRole.DocumentText          => theme.documentText,

            _ => Color.white
        };
    }

    private float ResolveFontSize(UIFontRole role)
    {
        return role switch
        {
            UIFontRole.Heading    => theme.fontSizeHeading,
            UIFontRole.Subheading => theme.fontSizeSubheading,
            UIFontRole.Body       => theme.fontSizeBody,
            UIFontRole.Small      => theme.fontSizeSmall,
            _                     => theme.fontSizeBody
        };
    }

    private void Log(string msg)
    {
        if (debugLog) Debug.Log($"[UIThemeApplier] {msg}");
    }
}
