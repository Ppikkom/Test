using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using BattleCore;
using System;

public class NpcStart : StateBase
{
    public NpcStart(BattleStateMachine _fsm, BattleContext _ctx) : base(_fsm, _ctx) { }

    private GameObject _aiObject;
    private BaseCombatant _stat;
    private INode _node;
    private SelectionState _ss;

    public override async UniTask OnEnter(CancellationToken ct)
    {
        await base.OnEnter(ct);
        if (ctx.IsInit == false)
        {
            ctx.IsInit = true;
            _aiObject = ctx.curUnit;
            _stat = ctx.BC;
            if (_aiObject == null || !_aiObject.activeSelf) { await fsm.ChangeState(ctx.States.TurnEnd); return; } // AITurnEnd -> TurnEnd; 

            _stat.PointChange();
        }
        // Effect 턴 시작 시 발동되는 효과가 있으면 여기다 넣기.
        ctx.OnBuffUIChanged?.Invoke();
        ctx.OnUIChanged?.Invoke();
        ctx.OnTurnCountChanged?.Invoke();
        ctx.OnTurnOrderChanged?.Invoke();
        ctx.OnButtonInteractiveChanged?.Invoke();
        ctx.OnUltiGaugeChanged?.Invoke();
        ctx.OnTeamPanelChanged?.Invoke();
        
        await UniTask.Delay(250, cancellationToken: localCts.Token);
        var ai = _aiObject.GetComponent<GridAIBase>();
        try
        {
            ai.FindNearestTarget();
            ai.ReloadRootNode();
            _node = ai.GetNode();

            await UniTask.WaitUntil(
            () => ai.GetNode() != null,
            cancellationToken: localCts.Token);

            _node.Evaluate();
            await UniTask.Delay(200, cancellationToken: localCts.Token);

            ctx.CleanupUseUnitList();
        }
        catch (OperationCanceledException) { }

        if (!IsCanceled && _stat.턴종료 == false)
        {
            //if(ctx.RBehaviour == null) Debug.Log("Null");
            //else Debug.Log(ctx.RBehaviour);
            
            await fsm.ChangeState(ctx.States.TurnAction);
            return;
        }

        if (!IsCanceled && _stat.턴종료 == true)
        {
            //_ss.behavioirText.text = "";
            await fsm.ChangeState(ctx.States.TurnEnd);
            return;
        }

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
