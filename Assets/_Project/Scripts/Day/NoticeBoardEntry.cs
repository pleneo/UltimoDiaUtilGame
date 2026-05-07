using UnityEngine;

[CreateAssetMenu(menuName = "Ultimo Dia Util/Day/Notice Board Entry", fileName = "Notice_")]
public class NoticeBoardEntry : ScriptableObject
{
    public string entryTitle = "Aviso";

    [TextArea(2, 5)]
    public string entryBody;

    public bool requiresForward;
}
