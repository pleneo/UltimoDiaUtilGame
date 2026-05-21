using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor customizado para UIThemeApplier.
/// Adiciona o botão "Aplicar Tema Agora" no Inspector,
/// permitindo ver o resultado sem precisar dar Play.
/// </summary>
[CustomEditor(typeof(UIThemeApplier))]
public class UIThemeApplierEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Desenha os campos padrão do Inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(8);

        var applier = (UIThemeApplier)target;

        GUI.backgroundColor = new Color(0.3f, 0.7f, 0.4f);
        if (GUILayout.Button("🎨  Aplicar Tema Agora", GUILayout.Height(36)))
        {
            applier.ApplyTheme();
            EditorUtility.SetDirty(applier.gameObject);
            Debug.Log("[UIThemeApplier] Tema aplicado via Editor.");
        }

        GUI.backgroundColor = Color.white;

        EditorGUILayout.HelpBox(
            "Adicione UIThemeTarget em cada elemento de UI e defina o 'Role' correspondente.\n" +
            "O botão acima aplica o tema no Editor sem precisar de Play.",
            MessageType.Info
        );
    }
}