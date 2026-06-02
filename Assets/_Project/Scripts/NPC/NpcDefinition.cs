using UnityEngine;

[CreateAssetMenu(menuName = "Ultimo Dia Util/NPC/NPC Definition", fileName = "NPC_")]
public class NpcDefinition : ScriptableObject
{
    public string npcName = "Estudante";

    [Header("Sprites")]
    public Sprite sideSprite;
    public Sprite frontSprite;
}
