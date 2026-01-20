using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using BattleCore;

public class BattleStartState : StateBase
{
    private List<InitPosRecord> _records;
    private System.Action<int, GameObject, BaseCombatant> _onIndexChangedHandler;
    UnitRoster roster; GridService grid;
    

    class InitPosRecord
    {
        public string map;
        public int playerCnt;
        public int enemyCnt;
        public List<Vector2> position;
    }


    public BattleStartState(BattleStateMachine _fsm, BattleContext _ctx) : base(_fsm, _ctx)
    {
        _onIndexChangedHandler = (i, unit, bc) => { };
        roster = _ctx.Roster;
        grid = _ctx.Grid;
    }

    public override async UniTask OnEnter(CancellationToken ct)
    {
        await base.OnEnter(ct);

        if (_records == null) LoadCSVRecords();

        BuildField();
        SetupTeams(BattleFlowManager.Instance._pendingRequest.Value.playerKeys, BattleFlowManager.Instance._pendingRequest.Value.enemyKeys);
        ApplyCSVPositions();

        ctx.index = 0;
        foreach (GameObject obj in roster.UseUnit)
            obj.GetComponent<BaseCombatant>().BattleStart();


        ctx.SetListSort();
        ctx.InitTileFind();
        ctx.RefreshCurrentUnit();

        ctx.OnBuffUIChanged?.Invoke();
        await UniTask.Yield();

        await (ctx.curUnit != null && ctx.curUnit.CompareTag("Player")
            ? fsm.ChangeState(ctx.States.PlayerStart) // PlayerTurnStart
            : fsm.ChangeState(ctx.States.NpcStart)); // AITurnStart
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

    void BuildField()
    {
        for (int i = 0; i < grid.FieldMask.GetLength(0); i++)
        {
            for (int j = 0; j < grid.FieldMask.GetLength(1); j++)
            {
                if (grid.FieldMask[i, j] == 0) continue;
                Vector3 origin = grid.SpawnOrigin + new Vector3(grid.TileSpacing.x * j, 0, grid.TileSpacing.y * i);

                int blockMask = ~LayerMask.GetMask("Field");

                if (Physics.BoxCast(origin + Vector3.up * 5f, new Vector3(grid.TileSpacing.x * 0.5f, 0.75f, Mathf.Abs(grid.TileSpacing.y * 0.5f)), Vector3.down, out var _hit, Quaternion.identity, Mathf.Abs(grid.TileSpacing.y), blockMask))
                {
                    grid.FieldMask[i, j] = 0;
                    continue;
                }

                Object.Instantiate(grid.TilePrefab, origin, Quaternion.identity, grid.FieldRoot.transform);
            }
        }
    }
    void SetupTeams(string[] p, string[] e)
    {

        foreach (var key in p)
        {
            if (roster.UnitDB.TryGetValue(key, out var unit))
            {
                var clone = Object.Instantiate(unit, Vector3.zero, Quaternion.identity);
                roster.Players.Add(clone);
            } 
        }

        foreach (var key in e)
        {
            if (roster.UnitDB.TryGetValue(key, out var unit))
            {
                var clone = Object.Instantiate(unit, Vector3.zero, Quaternion.Euler(0f, 180f, 0f));
                roster.Enemies.Add(clone);
            }
        }

        roster.UseUnit.AddRange(roster.Players);
        roster.UseUnit.AddRange(roster.Enemies);
    }

    void LoadCSVRecords()
    {
        _records = new List<InitPosRecord>();
        var text = grid.InitPositionCsv?.text;
        if (string.IsNullOrEmpty(text)) return;

        var lines = text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < lines.Length; i++)
        {
            var cols = lines[i].Split(',');

            var _map = cols[0].Trim();

            int _pCnt = int.Parse(cols[1]);
            int _eCnt = int.Parse(cols[2]);

            var _posList = new List<Vector2>();
            for (int j = 3; j < cols.Length; j++)
            {
                var clean = cols[j].Trim().Trim('"', '(', ')', ' ');
                var xy = clean.Split();
                int col = int.Parse(xy[0]);
                int row = int.Parse(xy[1]);
                _posList.Add(new Vector2(col, row));
            }

            _records.Add(new InitPosRecord
            {
                map = _map,
                playerCnt = _pCnt,
                enemyCnt = _eCnt,
                position = _posList
            });
            
        }
    }

    void ApplyCSVPositions()
    {
        /*
        var req = BattleFlowManager.Instance._pendingRequest;

        var rec = _records.FirstOrDefault(r =>
            r.map == req.Value.fieldId &&
            r.playerCnt == req.Value.playerKeys.Length &&
            r.enemyCnt  == req.Value.enemyKeys.Length
        );
        */
        
        var rec = _records.FirstOrDefault(r =>
            r.map == grid.MapID &&
            r.playerCnt == roster.Players.Count &&
            r.enemyCnt  == roster.Enemies.Count
        );
        
        if (rec == null)
        {
            Debug.LogWarning("요청에 맞는 CSV 데이터가 없습니다.");
            return;
        }

        for (int i = 0; i < roster.Players.Count; i++)
            ctx.MoveObejct(roster.Players[i], rec.position[i]);

        for (int j = 0; j < roster.Enemies.Count; j++)
            ctx.MoveObejct(roster.Enemies[j],
                rec.position[roster.Players.Count + j]);
    }
}
