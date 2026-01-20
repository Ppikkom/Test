using System.Collections.Generic;
using UnityEngine;

// 철벽
public class BattleIDefense : IEffectLogic
{

    public  void OnRegister(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그적용(obj, "Buff.IronDefense");
    }

    public EffectResult OnUse(GameObject obj,EffectInstance inst)
    {
        return new EffectResult(0.3f, 0.1f);
    }

    public void OnTurnEnd(GameObject obj, EffectInstance inst)
    {
        return;
    }
    
    public void OnRemove(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그제거(obj, "Buff.IronDefense");
    }

    
}