using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using BattleCore;
using System;

public class PlayerStart : StateBase
{
    UnitRoster roster;
    public PlayerStart(BattleStateMachine _fsm, BattleContext _ctx) : base(_fsm, _ctx) { roster = ctx.Roster; }

    public override async UniTask OnEnter(CancellationToken ct)
    {
        await base.OnEnter(ct);
        
        // 적이나 아군이 전부 비활성화 되었다면 Battle End로 넘어가게.
        if (ctx.IsInit == false)
        {
            ctx.IsInit = true;

            if (ctx.index < 0 || ctx.index >= roster.UseUnit.Count) { await fsm.ChangeState(ctx.States.TurnEnd); return; } // PlayerTurnEnd -> TurnEnd

            var unit = roster.UseUnit[ctx.index];

            if (unit == null || !unit.activeSelf)
            {
                await fsm.ChangeState(ctx.States.TurnEnd); // PlayerTurnEnd -> TurnEnd
                return;
            }

            var pc = unit.GetComponent<BaseCombatant>();
            pc.PointChange();
        }

        ctx.InitTrigger();
        
        ctx.OnUIChanged?.Invoke();
        ctx.OnBuffUIChanged?.Invoke();
        ctx.OnTurnCountChanged?.Invoke();
        ctx.OnTurnOrderChanged?.Invoke();
        ctx.OnButtonInteractiveChanged?.Invoke();
        ctx.OnUltiGaugeChanged?.Invoke();
        ctx.OnTeamPanelChanged?.Invoke();
        
        var skills = DataManager.Instance.CurrentPlayerData.characterStats.combatStats.equippedSkills;
        ctx.Selection.maxAttackIndex = (skills != null) ? skills.Count : 0;

    }


    public override async UniTaskVoid OnUpdate()
    {
        //Debug.Log($"1. {ctx.Selection.attackTrigger} / 2. {ctx.Selection.attackTrigger2}");
        // --- 공격 흐름 ---
        if (!ctx.Selection.attackTrigger && !ctx.Selection.attackTrigger2 && ctx.BC != null && ctx.BC.현재_행동력 > 0)
        {
            ctx.SearchActionTile();
        }
        else if (ctx.Selection.attackTrigger2) //  && ctx.Selection.ActionableTiles.Count > 0
        {
            var skills = DataManager.Instance.CurrentPlayerData.characterStats.combatStats.equippedSkills;
            if (skills != null && ctx.Selection.attackIndex >= 0 && ctx.Selection.attackIndex < skills.Count)
            {
                Debug.Log($"Index : {ctx.Selection.attackIndex} ");
                ctx.RBehaviour = "Attack"; // 나중에 스킬 이름으로 해서 애니메이션 할당하면 될듯?
                await fsm.ChangeState(ctx.States.TurnAction);
                
                return;
            }
        }

        if (ctx.Selection.rotateTrigger)
        {
            ctx.RotateObject();
            ctx.RBehaviour = "Rotate";
            await fsm.ChangeState(ctx.States.TurnAction);
            return;
        }
        
        // --- 이동 흐름 ---
        if (!ctx.Selection.moveTrigger && !ctx.Selection.moveTrigger2 && ctx.BC != null && ctx.BC.현재_이동력 > 0)
        {
            // ctx.SearchMoveTile();
            ctx.SelectMoveTile();
        }
        else if (ctx.Selection.moveTrigger2 && ctx.Selection.ActionableTiles.Count > 0)
        {
            ctx.MovePos = ctx.MoveTile();
            ctx.RBehaviour = "Move";
            await fsm.ChangeState(ctx.States.TurnAction);
            return;
        }


        // --- 전투 종료 체크 ---
        if (ctx.IsBattleOver())
        {
            await fsm.ChangeState(ctx.States.BattleEnd);
            return;
        }

        // --- 턴 종료 버튼 ---
        if (ctx.Selection.turnEndTrigger)
        {
            ctx.Selection.turnEndTrigger = false;
            await fsm.ChangeState(ctx.States.TurnEnd); // 여기 바꿈.
            return;
        }

        await UniTask.Yield();
    }


    public override UniTask OnExit()
    {

        return base.OnExit();
    }

    void 버튼_비활성화()
    {
        var bc = ctx.BC;
        if (bc == null) return;

        if (bc.현재_행동력 == 0) ctx.Selection.attackTrigger = false;
        if (bc.현재_이동력  == 0) ctx.Selection.moveTrigger   = false;
        if (bc.투지게이지   < 100) ctx.Selection.ultimateTrigger = false;
    }
}
