using System.Collections.Generic;
using UnityEngine;

// 분노
public class AngerSo : IEffectLogic
{
    // 등록될 때
    public void OnRegister(GameObject obj ,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그적용(obj, "Buff.Anger");
    }

    // 배틀 중, 사용(필요)될 때 발동.
    public EffectResult OnUse(GameObject obj,EffectInstance inst)
    {
        return new EffectResult(2);
    }

    // 배틀 중, 턴이 종료될 때 발동.
    public void OnTurnEnd(GameObject obj, EffectInstance inst)
    {
        return;
    }
    
    // 제거될 때
    public void OnRemove(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그제거(obj, "Buff.Anger");
    }
}