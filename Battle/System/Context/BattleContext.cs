using System;
using UnityEngine;

namespace BattleCore
{
    /// <summary>
    /// Central façade that wires services together and exposes the original API surface.
    /// Keep this as a POCO (non-MonoBehaviour) to match your original usage.
    /// </summary>
    public class BattleContext
    {
        // Subsystems
        public readonly GridService Grid = new();
        public readonly UnitRoster Roster = new();
        public readonly SelectionState Selection = new();
        public readonly TileHighlighter Highlighter = new();
        public readonly MovementSystem Movement;
        public readonly AttackSystem Attack;

        // Misc / external ties
        public StateRef States; // left as-is
        public int totalTurn = 1;
        public int lastTurn = -1, lastUseUnitCount = -1;
        public int playerCountCached = 0, enemyCountCached = 0;

        // Rework Test Var
        public bool IsInit = false;
        public string RBehaviour;
        public Vector2Int MovePos;
        
        public Action OnUIChanged;
        public Action OnBuffUIChanged;
        public Action OnTurnCountChanged;
        public Action OnTurnOrderChanged;
        public Action OnButtonInteractiveChanged;
        public Action OnUltiGaugeChanged;
        public Action OnTeamPanelChanged;

        public BaseBattleEvent[] BattleEvents;
        // public string BattleName; => Grid.MapID

        public BattleContext()
        {
            Movement = new MovementSystem(Grid, Roster, Selection, Highlighter);
            Attack = new AttackSystem(Grid, Roster, Selection, Highlighter);
        }

        // === Events passthrough (compat) ===
        public event Action<int, GameObject, BaseCombatant> OnIndexChanged
        {
            add { Roster.OnIndexChanged += value; }
            remove { Roster.OnIndexChanged -= value; }
        }

        public void InitSetEvent()
        {
            BattleEvents = GameObject.Find("EventContainer").GetComponent<BattleEventContainer>().FindEventsByName(Grid.MapID);
        }

        public void InitAttackSkill(skills s) => Attack.skill = s;

        // === Properties (compat) ===
        public int index
        {
            get => Roster.Index;
            set => Roster.Index = value;
        }

        public GameObject curUnit
        {
            get => Roster.CurrentUnit;
            set { /* read-only in original semantics; kept for API compatibility */ }
        }

        public BaseCombatant BC
        {
            get => Roster.CurrentBC;
            set { /* read-only hookup; keep for API compatibility */ }
        }

        public BaseCombatant PC
        {
            get
            {
                var go = Roster.UseUnit.Find(u => u != null && u.TryGetComponent<PlayerCombatant>(out _));
                return go != null ? go.GetComponent<PlayerCombatant>() : null;
            }
        }

        // === Bridged calls for external code (compat) ===
        public bool IsBattleOver() => Roster.IsBattleOver();
        public void SetListSort() => Roster.SortByAgilityDesc();
        public void CleanupUseUnitList() => Roster.Cleanup();
        public void RefreshCurrentUnit() => Roster.RefreshCurrent();
        public bool NotPlayerTurn() => Roster.NotPlayerTurn();

        public Vector2Int WorldToGrid(Vector3 p) => Grid.WorldToGrid(p);
        public Vector3 GridToWorld(Vector2Int g, float y = 1f) => Grid.GridToWorld(g, y); // 0이었음

        public void InitTileFind() => Grid.InitTileFind(Roster.UseUnit);
        public void 타일길찾기변경(Vector3 oldPos, Vector3 newPos) => Grid.UpdateOccupancy(oldPos, newPos);

        public void InitTrigger() => Selection.InitTriggers();

        // Simple actions (compat)
        public void 공격활성화() => Selection.공격활성화(Roster.CurrentBC, Attack.skill.GetCostByName(Selection.selectSkill));
        public void 이동활성화() => Selection.이동활성화(Roster.CurrentBC);
        public void 이동회전활성화()
        {
            Selection.이동회전활성화(Roster.CurrentBC);
            // cancel highlight and state if we rotated as a move
            //Movement.GetType().GetMethod("MoveObject"); // no-op placeholder to silence analyzer
        }
        public void 방어활성화() => Selection.방어활성화(Roster.CurrentBC);
        public void 턴종료활성화() => Selection.턴종료활성화();

        // Move flow (compat)
        public void SearchMoveTile() => Movement.SearchMoveTile();
        public void SelectMoveTile() => Movement.SelectMoveTile();
        public Vector2Int MoveTile() => Movement.MoveTile();
        public void MoveObject(GameObject obj, Vector2Int grid) => Movement.MoveObject(obj, grid);
        public void MoveObejct(GameObject obj, Vector2 vec) => Movement.MoveObejct(obj, vec);
        public void RotateObject() => Movement.RotateObject();

        // Attack flow (compat)
        public void SearchActionTile() => Attack.SearchActionTile();
        public void SelectAttackTile(GameObject o, System.Collections.Generic.List<Vector3> pts)
                                                     => Attack.SelectAttackTile(o, pts);
        public void AttackTile(string skillName) => Attack.AttackTile(skillName);
        public void ShowAttackTile(string skillName) => Attack.ShowAttackTile(skillName);
        public void ChangeAttackIndex(int idx) => Attack.attackIndex = idx;

        // Highlighter passthroughs
        public void RemoveAllTileHighlight() => Highlighter.RemoveAllTileHighlight();

        // Refactoring...
        public bool IsDiffCount() => playerCountCached != Roster.Players.Count || enemyCountCached != Roster.Enemies.Count;
        public bool IsDiffTurn() => lastTurn != totalTurn || lastUseUnitCount != Roster.UseUnit.Count;
        public void UpdateTurnCount() { lastTurn = totalTurn; lastUseUnitCount = Roster.UseUnit.Count; }
        public void UpdateEntityCount() { playerCountCached = Roster.Players.Count; enemyCountCached = Roster.Enemies.Count; }
        public System.Collections.Generic.List<BaseCombatant> GetAllBC()
        {
            System.Collections.Generic.List<BaseCombatant> bc = new();
            foreach (var v in Roster.Players) bc.Add(v.GetComponent<BaseCombatant>());
            foreach (var v in Roster.Enemies) bc.Add(v.GetComponent<BaseCombatant>());
            return bc;
        }
        public PlayerCombatant GetPlayerCombatant()
        {
            var go = Roster.UseUnit.Find(u => u != null && u.TryGetComponent<PlayerCombatant>(out _));
            return go != null ? go.GetComponent<PlayerCombatant>() : null;
        }

        public bool HasUnit() => Roster.UseUnit != null && Roster.UseUnit.Count > 0;
        public bool CanPlayerUseUltimate() => !NotPlayerTurn() && PC.투지게이지 >= 120 && totalTurn >= 3;
        public bool IsMoveState() => !Selection.moveTrigger; // 평소엔 true 상태, false이면 타일이 활성화 됨.
    }
}