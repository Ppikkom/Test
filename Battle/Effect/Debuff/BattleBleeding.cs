using System.Collections.Generic;
using UnityEngine;

// 출혈
public class BattleBleeding : IEffectLogic
{
    public void OnRegister(GameObject obj,EffectInstance inst)
    { 
        if (BattleTagSystem.Instance.태그확인(obj, "Debuff.Bleeding") == false)
        {
            BattleTagSystem.Instance.태그적용(obj, "Debuff.Bleeding");
            return;
        }

        
    }

    public EffectResult OnUse(GameObject obj,EffectInstance inst)
    {
        return new EffectResult(0.05f);
    }
    
    public void OnTurnEnd(GameObject obj, EffectInstance inst)
    {
        // 매 턴마다 체력의 최대 체력의 5%을 잃음.
        var bc = obj.GetComponent<BaseCombatant>(); // 차라리 BaseCombatant를 인자로 하는 게 좋을 수도 있겟다.
        bc.현재체력 -= (int)(bc.최대체력 * 0.05f);
    }

    public  void OnRemove(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그제거(obj, "Debuff.Bleeding");
    }
    
}