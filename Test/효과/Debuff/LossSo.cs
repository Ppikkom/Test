using System.Collections.Generic;
using UnityEngine;

// 상실
public class LossSo : IEffectLogic
{
    public void OnRegister(GameObject obj ,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그적용(obj, "Debuff.Loss");
    }
    public EffectResult OnUse(GameObject obj,EffectInstance inst)
    {
        return new EffectResult();
    }

    public void OnTurnEnd(GameObject obj, EffectInstance inst)
    {
        var bc = obj.GetComponent<BaseCombatant>();
        bc.투지게이지 -= 5; // 최대 투지게이지가 따로 있으면 그에 맞게 수정
    }
    
    public void OnRemove(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그적용(obj, "Debuff.Loss");
    }
    
}