using System.Collections.Generic;
using UnityEngine;

// 본능고조
public class BattleIH : IEffectLogic
{

    public void OnRegister(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그적용(obj, "Buff.InstinctHeightened");     
    }

    public  EffectResult OnUse(GameObject obj,EffectInstance inst)
    {
        return new EffectResult(1.1f);
    }

    public void OnTurnEnd(GameObject obj, EffectInstance inst)
    {
        return;
    }
    
    public void OnRemove(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그제거(obj, "Buff.InstinctHeightened");          
    }
    
    
}