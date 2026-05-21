using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class EfeitoLuzPiscando : MonoBehaviour
{
    private Image imagemFundo;

    [Header("Configurações da Luz")]
    [Tooltip("A cor normal da imagem (geralmente branco para 100% de luz).")]
    public Color luzForte = Color.white;
    
    [Tooltip("A cor de quando a luz falha (um cinza escuro).")]
    public Color luzFraca = new Color(0.4f, 0.4f, 0.4f, 1f); 

    [Tooltip("O quão rápido a luz fica piscando/tremendo.")]
    public float velocidade = 8f;

    [Tooltip("Chance de dar um 'apagão' mais longo (0 a 1).")]
    [Range(0f, 1f)]
    public float chanceDeApagao = 0.1f;

    private void Awake()
    {
        // Pega automaticamente o componente de imagem onde o script for colocado
        imagemFundo = GetComponent<Image>();
    }

    private void Update()
    {
        // O PerlinNoise gera um número aleatório suave entre 0 e 1 ao longo do tempo
        float ruido = Mathf.PerlinNoise(Time.time * velocidade, 0f);

        // Adiciona um pequeno "soluço" na luz de vez em quando para ficar mais caótico
        if (Random.value < chanceDeApagao * Time.deltaTime)
        {
            ruido = 0f; // Força a luz a ficar fraca num piscar rápido
        }

        // Mistura a cor fraca e a forte com base no valor do ruído
        imagemFundo.color = Color.Lerp(luzFraca, luzForte, ruido);
    }
}