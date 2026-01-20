using UnityEngine;
using Cysharp.Threading.Tasks;

public enum BattleEventType { BreakIn, Escape, Undying, Picture, Story }

public abstract class BaseBattleEvent : ScriptableObject
{
    [SerializeField] private BattleEventType _event;
    [SerializeField] private string _battleName;

    public BattleEventType Event => _event;
    public string BattleName => _battleName;

    public abstract UniTask OccurEvent();
    public abstract bool CheckCondition(int turn);
}
