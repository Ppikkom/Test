using System.Collections.Generic;
using UnityEngine;

// 고양
public class EnhanceSo : IEffectLogic
{
    public void OnRegister(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그적용(obj, "Buff.Enhance");
    }

    public EffectResult OnUse(GameObject obj,EffectInstance inst)
    {
        return new EffectResult(1);
    }
    
    public void OnTurnEnd(GameObject obj, EffectInstance inst)
    {
        return;
    }
    
    // 문제점 1. 변수의 최소값 보정으로 인한 계산 오류가 있을 수 있음. => 최소 보정치 0 / - 다음_행동력을 적용한 후, +가 되었을 때.
    public void OnRemove(GameObject obj,EffectInstance inst)
    {
        var bc = obj.GetComponent<BaseCombatant>();
        bc.다음_행동력 += inst.Data.CurStack;
        BattleTagSystem.Instance.태그제거(obj, "Buff.Enhance");
    }

    
}