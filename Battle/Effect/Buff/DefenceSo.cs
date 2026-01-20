using System.Collections.Generic;
using UnityEngine;

// 방어
public class DefenceSo : IEffectLogic
{
    public void OnRegister(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그적용(obj, "Buff.Defence");    
    }

    public EffectResult OnUse(GameObject obj,EffectInstance inst)
    {
        return new EffectResult(1.2f);
    }
    
    public void OnTurnEnd(GameObject obj, EffectInstance inst)
    {
        return;
    }

    public void OnRemove(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그제거(obj, "Buff.Defence");
    }
}