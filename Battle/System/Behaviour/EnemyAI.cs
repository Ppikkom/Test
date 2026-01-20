using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : GridAIBase
{
    protected override bool IsEnemyTeam => true;

    public override void FindNearestTarget()
    {
        if (ctx?.Roster.Players == null || ctx.Roster.Players.Count == 0) { target = null; return; }
        target = ctx.Roster.UseUnit
            .Where(u => u != gameObject && ctx.Roster.Players.Contains(u))
            .OrderBy(u => Vector3.Distance(transform.position, u.transform.position))
            .FirstOrDefault();
    }

    protected override void OnTurnEnd()
    {
        // 적 AI는 전투 시스템에 턴 종료 신호를 보냄
        //if (ctx?.BC != null) ctx.BC.턴종료 = true;
        stat.턴종료 = true;
    }
}