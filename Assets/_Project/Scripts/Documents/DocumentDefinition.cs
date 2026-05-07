using UnityEngine;

[CreateAssetMenu(menuName = "Ultimo Dia Util/Documents/Document Definition", fileName = "Document_")]
public class DocumentDefinition : ScriptableObject
{
    public DocumentType documentType = DocumentType.Unknown;
    public string displayName = "Documento";
    public Sprite icon;

    [TextArea(2, 5)]
    public string description;
}
