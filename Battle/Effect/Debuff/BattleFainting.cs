using System.Collections.Generic;
using UnityEngine;

// 기절
public class BattleFainting : IEffectLogic
{
    // 턴 처음 시작할 때, 태그 확인하고 있으면 바로 TurnEnd
    // 따로 변수 만들어야 할 수도 있음.
    public void OnRegister(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그적용(obj, "Debuff.Fainting");
    }

    public  EffectResult OnUse(GameObject obj,EffectInstance inst)
    {
        return new EffectResult();
    }

    public void OnTurnEnd(GameObject obj, EffectInstance inst)
    {
        return;
    }
    
    public void OnRemove(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그적용(obj, "Debuff.Fainting");
    }
    
}