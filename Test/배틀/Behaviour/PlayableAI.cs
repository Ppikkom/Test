using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayableAI : GridAIBase
{
    protected override bool IsEnemyTeam => false;

    public override void FindNearestTarget()
    {
        if (ctx?.Roster.Enemies == null || ctx.Roster.Enemies.Count == 0) { target = null; return; }
        target = ctx.Roster.UseUnit
            .Where(u => u != gameObject && ctx.Roster.Enemies.Contains(u))
            .OrderBy(u => Vector3.Distance(transform.position, u.transform.position))
            .FirstOrDefault();
    }

    protected override void OnTurnEnd()
    {
        stat.턴종료 = true;
    }
}