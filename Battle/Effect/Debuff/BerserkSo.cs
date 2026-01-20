using System.Collections.Generic;
using UnityEngine;

// 광폭
public class BerserkSo : IEffectLogic
{
    public void OnRegister(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그적용(obj, "Debuff.Berserk");

    }

    public EffectResult OnUse(GameObject obj,EffectInstance inst)
    {
        return new EffectResult(2, 0.5f);
    }

    public void OnTurnEnd(GameObject obj, EffectInstance inst)
    {
        return;
    }
    
    public void OnRemove(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그제거(obj, "Debuff.Berserk");
    }
    
}