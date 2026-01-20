using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TagDefinition")]
public class BattleTagDefinition : ScriptableObject
{
    public string tagName;
    public BattleTagDefinition parent;
    public List<BattleTagDefinition> children;

    public string FullPath
    {
        get
        {
            return parent == null ? tagName : parent.FullPath + "." + tagName;
        }
    }
}