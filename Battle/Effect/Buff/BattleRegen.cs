using System.Collections.Generic;
using UnityEngine;

// 재생
public class BattleRegen : IEffectLogic
{
    public  void OnRegister(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그적용(obj, "Buff.Regeneration");
    }

    public EffectResult OnUse(GameObject obj,EffectInstance inst)
    {
        return new EffectResult(0.05f);
    }

    public void OnTurnEnd(GameObject obj, EffectInstance inst)
    {
        var bc = obj.GetComponent<BaseCombatant>();
        bc.현재체력 += (int)(bc.최대체력 * 0.05);
    }
    
    public void OnRemove(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그제거(obj, "Buff.Regeneration");
    }
    
}