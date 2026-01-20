using System.Linq;
using System.Threading.Tasks;
using BattleCore;
using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Escape", menuName = "Battle_System/Battle_Event/Escape", order = 2)]
public class BattleEscapeEvent : BaseBattleEvent
{
    [SerializeField] private int _turn;
    [SerializeField] private string _objName;
    [SerializeField] private int _hpCondition;
    public int Turn => _turn;
    public string objName => _objName;
    public int HPCondition => _hpCondition;


    public override async UniTask OccurEvent()
    {
        // 조건 검사

        // 대화가 있다면 대화 재생

        // 애니메이션 재생

        // 오브젝트 제거
        await Task.Yield();
    }
    
     public override bool CheckCondition(int turn)
    {
        throw new System.NotImplementedException();
    }
}