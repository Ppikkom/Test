using System.Collections.Generic;
using UnityEngine;

// 무기력
public class BattleLethargy : IEffectLogic
{
    public void OnRegister(GameObject obj,EffectInstance inst)
    { // 중첩 필요함.
        BattleTagSystem.Instance.태그적용(obj, "Debuff.Lethargy");
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
        BattleTagSystem.Instance.태그제거(obj, "Debuff.Lethargy");
        var bc = obj.GetComponent<BaseCombatant>();
        bc.다음_행동력 -= inst.Data.CurStack;
    }
    
}