using System.Linq;
using BattleCore;
using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "BreakIn", menuName = "Battle_System/Battle_Event/BreakIn", order = 1)]
public class BattleBreakInEvent : BaseBattleEvent
{
    [SerializeField] private int _turn;
    [SerializeField] private string _objName;
    [SerializeField] private Vector2Int _pos;
    public int Turn => _turn;
    public string objName => _objName;
    public Vector2Int Pos => _pos;


    public override async UniTask OccurEvent()
    {
        // 오브젝트 소환
        var ctx = FindAnyObjectByType<BattleBootstrapper>()?.GetCtx();

        // 플레이어 혹은 에너미 넣기
        ctx.Roster.UnitDB.TryGetValue(_objName, out GameObject obj);
        ctx.Roster.UseUnit.Add(obj);

        // 좌표 설정
        obj.transform.position = ctx.GridToWorld(_pos);

        // 애니메이션 재생

        // 대화가 있다면 대화 재생

        await UniTask.Yield();
    }
    
     public override bool CheckCondition(int turn)
    {
        throw new System.NotImplementedException();
    }
}