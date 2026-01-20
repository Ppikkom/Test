using System.Collections.Generic;
using UnityEngine;

// 집중
public class FocusSo : IEffectLogic
{
    public void OnRegister(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그적용(obj, "Buff.Focus");
    }

    public EffectResult OnUse(GameObject obj,EffectInstance inst)
    {
        return new EffectResult(0.3f); // 이게 합연산인지 곱연산인지?
    }

    public void OnTurnEnd(GameObject obj, EffectInstance inst)
    {
        return;
    }
    
    public void OnRemove(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그제거(obj, "Buff.Focus");
    }

    
}