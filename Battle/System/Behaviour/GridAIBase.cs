using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class GridAIBase : AIBase
{
    protected abstract bool IsEnemyTeam { get; }

    // 계획(Plan) 구조체: 한 번의 공격을 위한 이동/회전/스킬 시퀀스
    protected struct Plan
    {
        public bool hasPlan;
        public Vector2Int moveGrid;
        public List<Vector2Int> path;
        public bool needRotate;
        public string skill;
        public GameObject planTarget;
        public int expectedDamage;
    }

    // 내부 상태
    protected GameObject curUnit;
    protected Plan plan;

    #region Behavior Tree
    protected override INode BuildBehaviorTree()
    {
        // 최초 타겟/스킬 풀 갱신
        curUnit = ctx.curUnit;
        FindNearestTarget();
        RefreshUsableSkills();

        var root = new SelectorNode();
        
        // 1) 공격(필요 시 이동/회전 포함 플래닝)
        var attackSeq = new SequenceNode();
        attackSeq.Add(new ActionNode(UsePlannedSkill));
        root.Add(attackSeq);

        // 2) 회전만 필요할 때(인접 가로 방향에서 타겟을 등지고 있으면)
        var rotateSeq = new SequenceNode();
        rotateSeq.Add(new ActionNode(() => ShouldRotateToFaceAdjacentTarget() ? INode.STATE.SUCCESS : INode.STATE.FAIL));
        rotateSeq.Add(new ActionNode(UseRotateInPlace));
        root.Add(rotateSeq);

        // 3) 이동 (타겟을 향해 한 칸 이동)
        var moveSeq = new SequenceNode();
        moveSeq.Add(new ActionNode(() => stat.이동력_유무() ? INode.STATE.SUCCESS : INode.STATE.FAIL));
        moveSeq.Add(new ActionNode(UseChaseMove));
        root.Add(moveSeq);

        // 4) 방어
        var defSeq = new SequenceNode();
        defSeq.Add(new ActionNode(() => stat.행동력_유무() ? INode.STATE.SUCCESS : INode.STATE.FAIL));
        defSeq.Add(new ActionNode(UseDefend));
        root.Add(defSeq);

        // 5) 턴 종료 (팀별로 처리 다름)
        var endSeq = new SequenceNode();
        endSeq.Add(new ActionNode(UseTurnEnd));
        root.Add(endSeq);

        return root;
    }
    #endregion

    #region Actions
    protected INode.STATE UseRotateInPlace()
    {
        if (target == null) return INode.STATE.FAIL;

        var my = ctx.WorldToGrid(transform.position);
        var tg = ctx.WorldToGrid(target.transform.position);
        if (my.y != tg.y || Mathf.Abs(my.x - tg.x) != 1) return INode.STATE.FAIL;

        float desiredYaw = (tg.x > my.x) ? 0f : 180f;

        stat.이동력_변경(-1);
        transform.rotation = Quaternion.Euler(0f, desiredYaw, 0f);
        ShowActionText("회전!");
        return INode.STATE.SUCCESS;
    }

    protected bool ShouldRotateToFaceAdjacentTarget()
    {
        if (target == null || !stat.행동력_유무()) return false;
        var my = ctx.WorldToGrid(transform.position);
        var tg = ctx.WorldToGrid(target.transform.position);
        if (my.y != tg.y || Mathf.Abs(my.x - tg.x) != 1) return false; // 가로 한 칸 인접
        float desiredYaw = (tg.x > my.x) ? 0f : 180f;
        return Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, desiredYaw)) > 1f;
    }

    protected INode.STATE UseTurnEnd()
    {
        OnTurnEnd();
        return INode.STATE.SUCCESS;
    }

    protected virtual void OnTurnEnd()
    {
        // 파생 클래스에서 구현
    }

    protected INode.STATE UseDefend()
    {
        BattleEffectSystem.Instance.효과등록(gameObject, "Defence");
        stat.행동력_변경(-1);
        stat.다음_행동력 = Mathf.Max(stat.다음_행동력, 1);
        ShowActionText("방어!");
        ctx.RBehaviour = "Move"; // 나중에 애니메이션 생기면 수정
        return INode.STATE.SUCCESS;
    }

    protected INode.STATE UseBuff()
    {
        // 테스트용 철벽 버프
        BattleEffectSystem.Instance.효과등록(gameObject, "IronDefense");
        stat.행동력_변경(-1);
        ShowActionText("버프!");
        ctx.RBehaviour = "버프";
        return INode.STATE.SUCCESS;
    }

    protected INode.STATE UseChaseMove()
    {
        if (target == null) return INode.STATE.FAIL;
        var my = ctx.WorldToGrid(transform.position);
        var tg = ctx.WorldToGrid(target.transform.position);

        var path = GridPathfinder.FindPath_Enemy(my, tg, ctx.Grid.FieldMask);
        if (path == null || path.Count < 2) return INode.STATE.FAIL;
        var next = path[1];
        if (IsOccupiedByAlly(next)) return INode.STATE.FAIL;
        if (next == tg) return INode.STATE.FAIL; // 타겟 위로 진입 금지

        // 이동 방향으로 회전(좌우만 존재)
        var delta = next - my;
        if (delta.x != 0 && delta.y == 0)
            transform.rotation = Quaternion.Euler(0f, delta.x > 0 ? 0f : 180f, 0f);

        ctx.타일길찾기변경(transform.position, new Vector3(next.x * ctx.Grid.TileSpacing.x, 0, next.y * ctx.Grid.TileSpacing.y));
        //ctx.MoveObejct(gameObject, next); // 애니메이션으로 줄거면 여기 바꿔야할듯?
        
        stat.이동력_변경(-1);
        ShowActionText("이동!");

        ctx.MovePos = next;
        ctx.RBehaviour = "Move";

        return INode.STATE.SUCCESS;
    }

    protected INode.STATE UsePlannedSkill()
    {
        // 타겟/스킬 갱신 & 계획 수립
        if (plan.hasPlan == false || plan.planTarget != target)
        {
            FindNearestTarget();
            RefreshUsableSkills();
            plan = BuildBestPlan();
        }

        if (!plan.hasPlan) return INode.STATE.FAIL;

        var myGrid = ctx.WorldToGrid(transform.position);

        // 1) 목적지까지 한 칸 이동 (남은 이동력 체크)
        if (myGrid != plan.moveGrid)
        {
            if (stat.현재_이동력 < 1) { plan.hasPlan = false; return INode.STATE.FAIL; }
            if (plan.path == null || plan.path.Count < 2) { plan.hasPlan = false; return INode.STATE.FAIL; }

            var next = plan.path[1];
            ctx.타일길찾기변경(transform.position, new Vector3(next.x * ctx.Grid.TileSpacing.x, 0, next.y * ctx.Grid.TileSpacing.y));
            //ctx.MoveObejct(gameObject, next);
            stat.이동력_변경(-1);
            ShowActionText("이동!");

            ctx.MovePos = next;
            ctx.RBehaviour = "Move";

            plan.path.RemoveAt(0);
            return INode.STATE.SUCCESS; // 한 칸씩 이동
        }
        // 2) 필요 시 제자리 회전 (이동력 >= 1)
        if (plan.needRotate && stat.현재_이동력 > 0)
        {
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y + 180f, 0f);
            stat.이동력_변경(-1);
            ShowActionText("회전!");
            plan.needRotate = false;
            return INode.STATE.SUCCESS;
        }
        // 3) 공격 시전 (행동력 체크)
        int cost = ctx.Attack.skill.GetCostByName(plan.skill);
        if (stat.현재_행동력 < cost) { plan.hasPlan = false; return INode.STATE.FAIL; }
        
        //ctx.Attack.skill.스킬사용(gameObject, plan.skill, target, ctx.WorldToGrid(transform.position - target.transform.position));
        //stat.행동력_변경(-cost);

        ShowActionText("공격!");
        ctx.RBehaviour = plan.skill; // 애니메이션 재생
        //Debug.Log(plan.skill);

        // 타겟 사망 처리 => 스킬 사용에 있음.
        //var tStat = target.GetComponent<BaseCombatant>();
        //if (tStat != null && tStat.현재체력 <= 0) target.SetActive(false);

        // nextPoint/nextMove 반영
        var sk = ctx.Attack.skill.GetSkillByName(plan.skill);
        var self = curUnit.GetComponent<BaseCombatant>();
        self.다음_행동력 = Mathf.Max(sk.다음_행동력, self.다음_행동력);
        self.다음_이동력 = Mathf.Max(sk.다음_이동력, self.다음_이동력);

        plan.hasPlan = false; // 일회성 계획 완료
        return INode.STATE.SUCCESS;
    }
    #endregion

    #region Planning
    protected Plan BuildBestPlan()
    {
        var p = new Plan
        {
            hasPlan = false,
            planTarget = target,
            moveGrid = ctx.WorldToGrid(transform.position),
            path = null,
            needRotate = false,
            skill = null,
            expectedDamage = int.MinValue
        };
        
        if (target == null) return p;
        // 1) 현재 위치에서의 최적 스킬
        TryImprovePlanAtGrid(ref p, ctx.WorldToGrid(transform.position), simulateRotate: false);

        // 2) 타겟 사방 4방향 후보 위치들 탐색 (이동 + 선택적 회전)
        Vector2Int my = ctx.WorldToGrid(transform.position);
        Vector2Int tg = ctx.WorldToGrid(target.transform.position);
        var dirs = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        
        var originPos = transform.position;
        var originRot = transform.rotation;
        foreach (var d in dirs)
        {
            var nextGrid = tg + d;
            if (!IsGridInside(nextGrid)) continue;
            if (my == nextGrid) continue;
            if (!IsWalkable(nextGrid)) continue;

            var path = GridPathfinder.FindPath(my, nextGrid, ctx.Grid.FieldMask);
            if (path == null || path.Count < 2) continue;
            var reqMoves = path.Count - 1;
            if (stat.현재_이동력 < reqMoves) continue;

            // 가상 이동 후 스킬 가능성 평가 (프로젝트의 CanCheckUseSkill가 transform 기반이므로 임시 위치/회전)
            var nextPos = ctx.Grid.SpawnOrigin + new Vector3(nextGrid.x * ctx.Grid.TileSpacing.x, originPos.y, nextGrid.y * ctx.Grid.TileSpacing.y);

            // 2-A) 회전 없이
            transform.position = nextPos;
            transform.rotation = originRot;
            RefreshUsableSkills();
            TryImprovePlanAtGrid(ref p, nextGrid, simulateRotate: false, forcedPath: path);

            // 2-B) 회전 포함(이동력 1 더 필요)
            if (stat.현재_이동력 >= reqMoves + 1)
            {
                transform.rotation = Quaternion.Euler(0f, originRot.eulerAngles.y + 180f, 0f);
                RefreshUsableSkills();
                TryImprovePlanAtGrid(ref p, nextGrid, simulateRotate: true, forcedPath: path);
            }

            // 원상 복구
            transform.position = originPos;
            transform.rotation = originRot;
        }
        
        return p;
    }

    protected void TryImprovePlanAtGrid(ref Plan p, Vector2Int grid, bool simulateRotate, List<Vector2Int> forcedPath = null)
    {
        if (usableSkills.Count == 0) return;

        int localBest = int.MinValue;
        string localSkill = null;

        BaseCombatant curBC = gameObject.GetComponent<BaseCombatant>();
        BaseCombatant targetBC = target.GetComponent<BaseCombatant>();

        foreach (var s in usableSkills)
        {
            int dmg = ctx.Attack.skill.공격력계산(curBC, targetBC, grid - ctx.WorldToGrid(target.transform.position), s);
            if (dmg > localBest)
            {
                localBest = dmg;
                localSkill = s;
            }
        }
        if (localSkill == null) return;

        if (localBest <= p.expectedDamage) return;

        p.hasPlan = true;
        p.moveGrid = grid;
        p.needRotate = simulateRotate;
        p.skill = localSkill;
        p.expectedDamage = localBest;
        p.path = forcedPath ?? GridPathfinder.FindPath(ctx.WorldToGrid(transform.position), grid, ctx.Grid.FieldMask);
    }
    #endregion

    #region Helpers
    protected bool IsGridInside(Vector2Int g)
    {
        int h = ctx.Grid.FieldMask.GetLength(0); // y
        int w = ctx.Grid.FieldMask.GetLength(1); // x
        return g.x >= 0 && g.y >= 0 && g.x < w && g.y < h;
    }

    protected bool IsWalkable(Vector2Int g)
    {
        // FieldMask[y, x] 규약 고정 (프로젝트 전반과 일치)
        int tile = ctx.Grid.FieldMask[g.y, g.x];
        // 0: 비어있음(통행 불가), 2: 막힘 으로 가정 → 필요시 프로젝트 값에 맞게 조정
        if (tile == 0 || tile == 2) return false;
        return true;
    }

    protected bool IsOccupiedByAlly(Vector2Int grid)
    {
        var list = IsEnemyTeam ? ctx.Roster.Enemies : ctx.Roster.Players;
        return list != null && list.Any(p => ctx.WorldToGrid(p.transform.position) == grid);
    }
    #endregion
}