using System.Collections.Generic;
using UnityEngine;
using static BattleCore.BattleConstants;

namespace BattleCore
{
    /// <summary>
    /// Skill range sampling, target checks, tile preview, and skill execution.
    /// </summary>
    public class AttackSystem
    {
        private readonly GridService grid;
        private readonly UnitRoster roster;
        private readonly SelectionState sel;
        private readonly TileHighlighter hl;
        public skills skill;
        public int attackIndex = 0;

        public AttackSystem(GridService g, UnitRoster r, SelectionState s, TileHighlighter h)
        {
            grid = g; roster = r; sel = s; hl = h;
        }

        public bool CanCheckUseSkill(GameObject obj, string name)
            => CheckEnemyInSkillRange(obj, name) && CheckPointUseSkill(name);

        public List<Vector3> SkillRangeAtPosition(GameObject obj, string name)
        {
            Vector3 curPos = obj.transform.position;
            //var range = SkillManager.Instance.GetRangeByName(name);
            var range = skill.GetSkillRangeByName(name);
            var result = new List<Vector3>(range.Count);
            bool facingBack = Vector3.Dot(obj.transform.forward, Vector3.forward) < 0f;

            foreach (Vector2 vec in range)
            {
                Vector3 local = new Vector3(vec.x * grid.TileSpacing.x, 0f, vec.y * grid.TileSpacing.y);
                Vector3 world = facingBack ? new Vector3(-local.x, 0f, local.z) : new Vector3(+local.x, 0f, local.z);
                result.Add(curPos + new Vector3(world.x, 2f, world.z));
            }
            return result;
        }

        private bool CheckEnemyInSkillRange(GameObject obj, string name)
        {
            var points = SkillRangeAtPosition(obj, name);
            bool isPlayer = roster.Players.Contains(obj);
            int mask = EffectiveMask(MaskIgnoreRayCast);

            foreach (Vector3 pos in points)
            {
                if (Physics.Raycast(pos, Vector3.down, out var hit, 5f, 1 << LayerMask.NameToLayer("Ignore Raycast")))
                {
                    var hitGo = hit.collider.gameObject;
                    if (isPlayer && roster.Enemies.Contains(hitGo)) return true;
                    if (!isPlayer && roster.Players.Contains(hitGo)) return true;
                }
            }
            return false;
        }

        private bool CheckPointUseSkill(string name)
        {
            var bc = roster.CurrentBC;
            //if (bc != null) Debug.Log("Check1"); Debug.Log(SkillManager.Instance.GetCostByName(name));
            return bc != null && bc.현재_행동력 >= skill.GetCostByName(name);
        }

        public void SearchActionTile()
        {
            sel.EnterAttackSelection(CancelAttackMode);

            if (sel.maxAttackIndex == 0)
            {
                Debug.LogWarning("SelectAttackTile: 선택 가능한 스킬이 없습니다.");
                return;
            }

            sel.selectSkill = DataManager.Instance.CurrentPlayerData.characterStats.combatStats.equippedSkills[attackIndex];
            SelectAttackTile(roster.CurrentUnit, SkillRangeAtPosition(roster.CurrentUnit, sel.selectSkill));
        }

        public void SelectAttackTile(GameObject obj, List<Vector3> points)
        {
            int mask = EffectiveMask(MaskIgnoreRayCast);
            foreach (var p in points)
            {
                if (Physics.Raycast(p, Vector3.down, out var hit, 4f, 1 << LayerMask.NameToLayer("Field")))
                {
                    var go = hit.collider.gameObject;
                    if (go.CompareTag("Field") && !sel.ActionableTiles.Contains(go))
                        sel.ActionableTiles.Add(go);
                }
            }
        }

        public void AttackTile(string skillName)
        {
            //int mask = EffectiveMask(MaskIgnoreRayCast);
            bool isPlayer = roster.Players.Contains(roster.CurrentUnit);

            SelectAttackTile(roster.CurrentUnit, SkillRangeAtPosition(roster.CurrentUnit, skillName));
            
            foreach (var tile in sel.ActionableTiles)
            {
                Vector3 originPos = tile.transform.position + Vector3.up * 4f;
                if (Physics.Raycast(originPos, Vector3.down, out var hit, 4f, 1 << LayerMask.NameToLayer("Ignore Raycast"))) // 사실 저거 Ignore보다 지정해주는게 좋을거같은데
                {

                    var target = hit.collider.gameObject;
                    bool valid = isPlayer ? roster.Enemies.Contains(target) : roster.Players.Contains(target);
                    if (!valid) continue;

                    skill.스킬사용(roster.CurrentUnit, skillName, target, grid.WorldToGrid(roster.CurrentUnit.transform.position - target.transform.position));
                    Debug.Log($"가해자 : {roster.CurrentUnit.name} / 피해자 : {target.name}");
                }
            }

            int cost = skill.GetCostByName(skillName); // sel.selectSkill
            //roster.CurrentBC.행동력_변경(-cost);
            roster.CurrentBC.다음_행동력 = Mathf.Max(skill.GetNextActByName(skillName), roster.CurrentBC.다음_행동력); // sel.selectSkill
            roster.CurrentBC.다음_이동력  = Mathf.Max(skill.GetNextMovByName(skillName),  roster.CurrentBC.다음_이동력); // sel.selectSkill

            CancelAttackMode();
            if (sel.cancelStack.Count > 0) sel.cancelStack.Pop();
        }

        public void ShowAttackTile(string skillName)
        {
            hl.RemoveTileHighlight(sel.ActionableTiles);
            sel.ActionableTiles.Clear();

            var points = SkillRangeAtPosition(roster.CurrentUnit, skillName);
            int mask = EffectiveMask(MaskIgnoreRayCast);

            foreach (var p in points)
            {
                if (Physics.Raycast(p, Vector3.down, out var hit, 4f, 1 << LayerMask.NameToLayer("Field")))
                {
                    var go = hit.collider.gameObject;
                    if (go.CompareTag("Field") && !sel.ActionableTiles.Contains(go))
                        sel.ActionableTiles.Add(go);
                }
            }

            hl.Colorize(sel.ActionableTiles, TileAttack);
        }

        private void CancelAttackMode()
        {
            hl.RemoveAllTileHighlight();
            sel.ActionableTiles.Clear();
            sel.LeaveAttackSelection();
        }
    }
}