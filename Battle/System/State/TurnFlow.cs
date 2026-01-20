using Cysharp.Threading.Tasks;
using BattleCore;

public static class TurnFlow
{
    public static void ApplyEndOfTurn(BattleContext ctx)
    {
        var bc = ctx.BC;
        if (bc == null) return;

        // 이거 왜 넣은건지 까먹ㅇㅆ음.
        if (bc.다음_이동력 == 0) bc.다음_이동력 = 1;
        if (bc.다음_행동력 == 0) bc.다음_행동력 = 1;

        // 여기 고치면 될듯?
        if (ctx.curUnit != null)
            EffectManager.Instance.효과턴종료(ctx.curUnit);
    }

    public static void AdvanceTurn(BattleContext ctx)
    {
        var roster = ctx.Roster;
        if (roster.UseUnit == null || roster.UseUnit.Count == 0)
        {
            ctx.index = 0;
            return;
        }

        if (ctx.index == roster.UseUnit.Count - 1)
        {
            ctx.totalTurn++;
            ctx.SetListSort();
            ctx.InitTileFind();
            
            ctx.index = 0;

            ctx.OnTurnCountChanged?.Invoke();
        }
        else
        {
            //Debug.Log($"Cur unitIndex : {ctx.index}");
            ctx.index += 1;
        }
    }

    public static async UniTask CheckEvent(BattleContext ctx)
    {
        foreach (var v in ctx.BattleEvents)
        {
            if (v.CheckCondition(ctx.totalTurn) == true) // 다른 조건 추가할 수도 있음
            {
                await UniTask.WhenAll(v.OccurEvent());
            }
        }
        await UniTask.Yield();
    }

    public static UniTask RouteNextAsync(BattleStateMachine fsm, BattleContext ctx)
    {
        return (ctx.curUnit != null && ctx.curUnit.CompareTag("Player"))
            ? fsm.ChangeState(ctx.States.PlayerStart)
            : fsm.ChangeState(ctx.States.NpcStart);
    }
}