using UnityEngine;

// 야성개방
public class BattleWildOpen : IEffectLogic
{
    // 매 행동 종료될때마다 검사해서 해당 효과 부여하면 될 듯?
    public void OnRegister(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그적용(obj, "Buff.WildOpen");
    }

    public EffectResult OnUse(GameObject obj,EffectInstance inst)
    {
        return new EffectResult(1.2f);
    }

    public void OnTurnEnd(GameObject obj, EffectInstance inst)
    {
        return;
    }
    
    public void OnRemove(GameObject obj,EffectInstance inst)
    {
        BattleTagSystem.Instance.태그제거(obj, "Buff.WildOpen");
    }

    
}