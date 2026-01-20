using UnityEngine;

public interface IEffectLogic
{
    void OnRegister(GameObject obj, EffectInstance inst);
    EffectResult OnUse(GameObject obj, EffectInstance inst);
    void OnTurnEnd(GameObject obj, EffectInstance inst);
    void OnRemove(GameObject obj, EffectInstance inst);
}