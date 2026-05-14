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

    // -------------------------------------------------------------------------
    // Unity Messages
    // -------------------------------------------------------------------------

    private void Awake()
    {
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
    public void ReceberCarimbo(StampType tipo)
    {
        if (UltimoCarimbo.HasValue && !permiteRecarimbar)
        {
            // Documento já foi carimbado e recarimbar não é permitido
            return;
        }

        UltimoCarimbo = tipo;
        AtualizarVisuais(tipo);
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

    private void AtualizarVisuais(StampType tipo)
    {
        bool aprovado = tipo == StampType.Aprovado;

        if (marcaAprovado != null) marcaAprovado.SetActive(aprovado);
        if (marcaNegado  != null) marcaNegado.SetActive(!aprovado);
    }
}