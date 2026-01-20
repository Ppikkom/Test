using System.Collections.Generic;
using UnityEngine;

// 경직
public class BattleStagger : IEffectLogic
{

    public void OnRegister(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그적용(obj, "Debuff.Stagger");
    }

    public EffectResult OnUse(GameObject obj,EffectInstance inst)
    {
        return new EffectResult();
    }

    public void OnTurnEnd(GameObject obj, EffectInstance inst)
    {
        return;
    }
    
    public void OnRemove(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그제거(obj, "Debuff.Stagger");
        var bc = obj.GetComponent<BaseCombatant>();
        bc.다음_이동력 -= (inst.Data.CurStack * 2);
    }
    
}