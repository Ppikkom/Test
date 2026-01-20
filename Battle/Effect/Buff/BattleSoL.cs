using System.Collections.Generic;
using UnityEngine;

// 광속
public class BattleSoL : IEffectLogic
{
    public void OnRegister(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그적용(obj, "Buff.SoL");
    }

    public EffectResult OnUse(GameObject obj,EffectInstance inst)
    {
        return new EffectResult(1);
    }

    public void OnTurnEnd(GameObject obj, EffectInstance inst)
    {
        return;
    }
    
    public void OnRemove(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그제거(obj, "Buff.SoL");
        var bc = obj.GetComponent<BaseCombatant>();
        bc.다음_이동력 += inst.Data.CurStack;
    }


    
}