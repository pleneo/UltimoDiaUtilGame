using UnityEngine;

[CreateAssetMenu(menuName = "Ultimo Dia Util/Day/Rule Definition", fileName = "Rule_")]
public class RuleDefinition : ScriptableObject
{
    public string ruleTitle = "Regra";

    [TextArea(2, 5)]
    public string ruleBody;

    public bool highlighted;
}
