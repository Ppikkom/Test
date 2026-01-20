using UnityEngine;
public struct EffectResult
{
    public float value1;
    public float value2;

    public EffectResult(float v1 = 0, float v2 = 0)
    {
        value1 = v1;
        value2 = v2;
    }
}

[CreateAssetMenu(fileName = "effect", menuName = "Game Data/EffectSO")]
public class BaseEffectSo : ScriptableObject
{
    public EffectType effectType;
    public string id;
    
    public bool isContinue;
    public int defaultDuration;
    public int maxDuration;
    public bool isStack;
    public int maxStack;
    public Sprite[] buffIcons;
}
