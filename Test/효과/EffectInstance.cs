using UnityEngine;


public class EffectInstance
{
    public EffectData Data;
    public IEffectLogic Logic;
    
    public EffectInstance(EffectData data, IEffectLogic logic)
    {
        Data = data;
        Logic = logic;
    }
}