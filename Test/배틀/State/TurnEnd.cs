using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using System;
using BattleCore;
public class TurnEnd : StateBase
{
    private Action<int, GameObject, BaseCombatant> _onIndexChangedHandler;
    public TurnEnd(BattleStateMachine _fsm, BattleContext _ctx) : base(_fsm, _ctx)
    {
        _onIndexChangedHandler = (i, unit, sf) => { }; //Debug.Log($"{ctx.index}번째 턴 종료"); };
    }



    public override async UniTask OnEnter(CancellationToken ct)
    {
        await base.OnEnter(ct);
        
        ctx.OnIndexChanged += _onIndexChangedHandler;
        ctx.IsInit = false;
        ctx.RBehaviour = "";

        ctx.CleanupUseUnitList();
        if (ctx.IsBattleOver())
        {
            await fsm.ChangeState(ctx.States.BattleEnd);
            return;
        }

        if(ctx.Roster.NotPlayerTurn() == false)
            await UniTask.WhenAll(TurnFlow.CheckEvent(ctx));

        TurnFlow.ApplyEndOfTurn(ctx);
        TurnFlow.AdvanceTurn(ctx);
        
        ctx.OnUIChanged?.Invoke();
        ctx.OnBuffUIChanged?.Invoke();
        ctx.OnTurnOrderChanged?.Invoke();

        await TurnFlow.RouteNextAsync(fsm, ctx);
    }


    public override async UniTaskVoid OnUpdate()
    {
        await UniTask.Yield();
    }


    public override UniTask OnExit()
    {
        ctx.OnIndexChanged -= _onIndexChangedHandler; 
        return base.OnExit();
    }
}
