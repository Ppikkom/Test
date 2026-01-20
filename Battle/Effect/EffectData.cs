

using UnityEngine;

public enum EffectType { None, Anger, Defence, Enhance, Focus, InstinctHeightened, IronDefense, Regen, SoL, WildOpen, 
    Berserk, Bleeding, Lethargy, Blindness, Burst, Fainting, Fear, Loss, Stagger }

public class EffectData
{
    public string Id;
    public EffectType Type;
    
    public bool IsContinue;
    public int CurDuration;
    private int _defaultDuration;
    public int MaxDuration;

    public bool IsStack;
    public int CurStack;
    public int MaxStack;

    public Sprite[] BuffIcons;

    public EffectData(BaseEffectSo so)
    {
        Id = so.id;
        Type = so.effectType;
        
        IsContinue = so.isContinue;
        CurDuration = so.defaultDuration;
        _defaultDuration = so.defaultDuration;
        MaxDuration = so.maxDuration;

        IsStack = so.isStack;
        CurStack = 1;
        MaxStack = so.maxStack;

        BuffIcons = so.buffIcons;
    }

    public bool TickTurn()
    {
        if (!IsContinue) return true;

        CurDuration--;
        return CurDuration <= 0;
    }

    public void AddStack()
    {
        if (IsStack && CurStack < MaxStack)
        {
            CurStack++;
            CurDuration = _defaultDuration;
        }
    }
}