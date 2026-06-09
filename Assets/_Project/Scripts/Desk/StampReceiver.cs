using System;
using UnityEngine;

/// <summary>
/// Componente colocado nos documentos que podem receber um carimbo.
/// Quando o DraggableStamp é solto sobre este objeto, ele ativa
/// a imagem de marca correta (Aprovado ou Negado) e dispara um evento.
/// </summary>
public class StampReceiver : MonoBehaviour
{
    [Header("Marcas Visuais")]
    [Tooltip("GameObject filho com a imagem do carimbo APROVADO.")]
    [SerializeField] private GameObject marcaAprovado;

    [Tooltip("GameObject filho com a imagem do carimbo NEGADO.")]
    [SerializeField] private GameObject marcaNegado;

    [Header("Configuração")]
    [Tooltip("Se falso, o documento só pode ser carimbado uma vez.")]
    [SerializeField] private bool permiteRecarimbar = false;

    // -------------------------------------------------------------------------
    // Estado
    // -------------------------------------------------------------------------

    /// <summary>Tipo do último carimbo aplicado. Null se ainda não foi carimbado.</summary>
    public StampType? UltimoCarimbo { get; private set; }

    /// <summary>Disparado sempre que um carimbo é aplicado com sucesso.</summary>
    public event Action<StampType> CarimboAplicado;

    private CaseManager caseManager;

    // -------------------------------------------------------------------------
    // Unity Messages
    // -------------------------------------------------------------------------

    private void Awake()
    {
        caseManager = FindObjectOfType<CaseManager>();
        // Começa sem nenhuma marca visível
        LimparMarcas();
    }

    // -------------------------------------------------------------------------
    // API Pública
    // -------------------------------------------------------------------------

    /// <summary>
    /// Chamado pelo DraggableStamp quando o carimbo é solto sobre este documento.
    /// </summary>
    /// <param name="tipo">Aprovado ou Negado.</param>
    public void ReceberCarimbo(StampType tipo, Vector3 worldPosition)
    {
        if (!CanReceiveStamp())
        {
            return;
        }

        if (UltimoCarimbo.HasValue && !permiteRecarimbar)
        {
            // Documento já foi carimbado e recarimbar não é permitido
            return;
        }

        UltimoCarimbo = tipo;
        AtualizarVisuais(tipo, worldPosition);
        CarimboAplicado?.Invoke(tipo);
    }

    /// <summary>
    /// Remove qualquer marca visual e reseta o estado do documento.
    /// Útil ao trocar de caso no DayManager.
    /// </summary>
    public void LimparMarcas()
    {
        UltimoCarimbo = null;

        if (marcaAprovado != null) marcaAprovado.SetActive(false);
        if (marcaNegado  != null) marcaNegado.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Privado
    // -------------------------------------------------------------------------

    private bool CanReceiveStamp()
    {
        var draggableDocument = GetComponentInParent<DraggableDocument>();
        if (draggableDocument == null || draggableDocument.BoundRecord == null)
        {
            return false;
        }

        if (caseManager == null)
        {
            caseManager = FindObjectOfType<CaseManager>();
        }

        var currentCase = caseManager != null ? caseManager.CurrentCase : null;
        var allowedDocumentType = CaseDocumentRules.ResolveDecisionDocumentType(currentCase);
        if (allowedDocumentType == DocumentType.Unknown)
        {
            return false;
        }

        return draggableDocument.BoundRecord.GetDocumentType() == allowedDocumentType;
    }

    private void AtualizarVisuais(StampType tipo, Vector3 worldPosition)
    {
        bool aprovado = tipo == StampType.Aprovado;

        if (marcaAprovado != null) marcaAprovado.SetActive(aprovado);
        if (marcaNegado  != null) marcaNegado.SetActive(!aprovado);

        var activeMark = aprovado ? marcaAprovado : marcaNegado;
        if (activeMark == null)
        {
            return;
        }

        MoveMarkToWorldPosition(activeMark, worldPosition);
    }

    private void MoveMarkToWorldPosition(GameObject markObject, Vector3 worldPosition)
    {
        var documentRect = GetComponent<RectTransform>();
        var markRect = markObject.GetComponent<RectTransform>();
        if (documentRect == null || markRect == null)
        {
            return;
        }

        var canvas = GetComponentInParent<Canvas>();
        var eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        var screenPoint = RectTransformUtility.WorldToScreenPoint(eventCamera, worldPosition);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(documentRect, screenPoint, eventCamera, out var localPoint))
        {
            return;
        }

        var halfDocumentSize = documentRect.rect.size * 0.5f;
        var halfMarkSize = markRect.rect.size * 0.5f;

        localPoint.x = Mathf.Clamp(localPoint.x, -halfDocumentSize.x + halfMarkSize.x, halfDocumentSize.x - halfMarkSize.x);
        localPoint.y = Mathf.Clamp(localPoint.y, -halfDocumentSize.y + halfMarkSize.y, halfDocumentSize.y - halfMarkSize.y);

        markRect.anchoredPosition = localPoint;
    }
}
