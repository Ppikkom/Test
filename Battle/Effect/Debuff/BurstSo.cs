using System.Collections.Generic;
using UnityEngine;

// 파열
public class BurstSo : IEffectLogic
{
    // 전투가 끝나면 따로 검사해서 3스택이면 효과 작동
    public  void OnRegister(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그적용(obj, "Debuff.Burst");

    }

    public EffectResult OnUse(GameObject obj,EffectInstance inst)
    {
        return new EffectResult(0.1f);
    }

    public void OnTurnEnd(GameObject obj, EffectInstance inst)
    {
        inst.Data.CurDuration += 1; // 파열의 시간을 무한으로 만들기 위함.
    }
    
    public void OnRemove(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그제거(obj, "Debuff.Burst");
    }
}

