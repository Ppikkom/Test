using System.Collections.Generic;
using UnityEngine;

// 본능적 공포
public class BattleFear : IEffectLogic
{
    public void OnRegister(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그적용(obj, "Debuff.Fear");
    }
    public EffectResult OnUse(GameObject obj,EffectInstance inst)
    {
        return new EffectResult(0.8f);
    }

    public void OnTurnEnd(GameObject obj, EffectInstance inst)
    {
        return;
    }
    
    public void OnRemove(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그적용(obj, "Debuff.Fear");
    }
    
}