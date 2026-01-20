using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Linq;
using BattleCore;

public class BattleEndState : StateBase
{
    UnitRoster roster;

    public BattleEndState(BattleStateMachine _fsm, BattleContext _ctx) : base(_fsm, _ctx) { roster = _ctx.Roster; }

    public override async UniTask OnEnter(CancellationToken ct)
    {
        await base.OnEnter(ct);

        Debug.Log("전투 끝!");

        
        bool isWin = roster.UseUnit.Any(u => roster.Players.Contains(u));

        await BattleFlowManager.Instance.FinishBattleAsync(isWin);

        await OnExit();
    }

    public override async UniTaskVoid OnUpdate()
    {
        await UniTask.Yield();
    }

    public override UniTask OnExit()
    {
        return base.OnExit();
    }

}
