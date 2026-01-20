using System.Collections.Generic;
using UnityEngine;
using static BattleCore.BattleConstants;

namespace BattleCore
{
    /// <summary>
    /// Move search/select/execute flow. Owns raycasts for neighbor tiles.
    /// </summary>
    public class MovementSystem
    {
        private readonly GridService grid;
        private readonly UnitRoster roster;
        private readonly SelectionState sel;
        private readonly TileHighlighter hl;

        //private int lastIndex = -1;

        public MovementSystem(GridService g, UnitRoster r, SelectionState s, TileHighlighter h)
        {
            grid = g; roster = r; sel = s; hl = h;
        }

        public void SearchMoveTile()
        {
            hl.RemoveTileHighlight(sel.ActionableTiles);
            sel.ActionableTiles.Clear();

            var cur = roster.CurrentUnit;
            sel.SelectedUnit = cur;
            if (!CheckUnderTile(cur, out var underTile))
            {
                Debug.Log("아래에 타일이 없습니다.");
                return;
            }

            Vector3[] offsets =
            {
                new Vector3(grid.TileSpacing.x, 0, 0),
                new Vector3(0, 0, -grid.TileSpacing.y),
                new Vector3(-grid.TileSpacing.x, 0, 0),
                new Vector3(0, 0, grid.TileSpacing.y),
            };

            foreach (var off in offsets)
            {
                Vector3 origin = underTile.transform.position + off + Vector3.up * 1f;
                if (Physics.Raycast(origin, Vector3.down, out var hit, 2f))
                {
                    // block if a unit is already standing there
                    Vector3 probe = cur.transform.position + off + Vector3.up * 4f;
                    if (Physics.Raycast(probe, Vector3.down, out var hit2, 5f, LayerMask.NameToLayer("Ignore RayCast")) &&
                        roster.UseUnit.Contains(hit2.collider.gameObject))
                    {
                        continue;
                    }

                    var go = hit.collider.gameObject;
                    if (go.CompareTag("Field") && !sel.ActionableTiles.Contains(go))
                        sel.ActionableTiles.Add(go);
                }
            }

            foreach (var v in sel.ActionableTiles)
            {
                var r = v.GetComponent<Renderer>();
                var mr = v.GetComponent<MeshRenderer>();
                if (r)  r.material.color = TileMove;
                if (mr) mr.enabled = true;
            }
        }

        public void SelectMoveTile()
        {
            SearchMoveTile();            
            sel.EnterMoveSelection(sel.ActionableTiles, CancelMoveMode);

            if (sel.maxMoveIndex == 0)
            {
                if (sel.arrowTest) sel.arrowTest.SetActive(false);
                Debug.LogWarning("SelectMoveTile: 선택 가능한 타일이 없습니다.");
                return;
            }

            for (int i = 0; i < sel.maxMoveIndex; i++)
            {
                var r = sel.ActionableTiles[i].GetComponent<Renderer>();
                if (r) r.material.color = (sel.moveIndex == i) ? TileMoveCur : TileMove;
            }

            if (sel.arrowTest)
                sel.arrowTest.transform.position = sel.ActionableTiles[sel.moveIndex].transform.position + new Vector3(0, 3f, 0);
        }

        public Vector2Int MoveTile() // void -> Vector2Int
        {
            if (sel.arrowTest) sel.arrowTest.SetActive(false);

            Vector3 dst = sel.ActionableTiles[sel.moveIndex].transform.position;
            grid.UpdateOccupancy(roster.CurrentUnit.transform.position, dst);

            Vector2Int delta = grid.WorldToGrid(dst) - grid.WorldToGrid(roster.CurrentUnit.transform.position);

            if (delta.x != 0 && delta.y == 0)
            {
                float desiredYaw = (delta.x > 0) ? 0f : 180f;
                roster.CurrentUnit.transform.rotation = Quaternion.Euler(0f, desiredYaw, 0f);
            }

            //MoveObject(roster.CurrentUnit, grid.WorldToGrid(dst));
            roster.CurrentBC.이동력_변경(-1);
            CancelMoveMode();
            if (sel.cancelStack.Count > 0) sel.cancelStack.Pop();

            return grid.WorldToGrid(dst);
        }

        public void RotateObject()
        {
            if (sel.arrowTest) sel.arrowTest.SetActive(false);
            
            float desiredYaw = roster.CurrentBC.transform.eulerAngles.y > 90f ? 0f : 180f;
            roster.CurrentBC.transform.rotation = Quaternion.Euler(0f, desiredYaw, 0f);
            roster.CurrentBC.이동력_변경(-1);
            CancelMoveMode();
            if (sel.cancelStack.Count > 0) sel.cancelStack.Pop(); // 일단 넣어봄/
        }
        

        private void CancelMoveMode()
        {
            if (sel.arrowTest) sel.arrowTest.SetActive(false);
            hl.RemoveTileHighlight(sel.ActionableTiles);
            sel.ActionableTiles.Clear();
            sel.LeaveMoveSelection();
        }

        public void MoveObject(GameObject obj, Vector2Int gridPos)
        {
            obj.transform.position = grid.GridToWorld(gridPos, obj.transform.localScale.y);
        }

        // Backward-compat (original typo API)
        public void MoveObejct(GameObject obj, Vector2 vec)
            => MoveObject(obj, new Vector2Int(Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.y)));

        private bool CheckUnderTile(GameObject obj, out GameObject tile)
        {
            tile = null;
            if (obj == null) return false;

            Vector3 startPos = obj.transform.position - new Vector3(0, obj.transform.position.y / 2f, 0);
            if (Physics.Raycast(startPos, Vector3.down, out var hit, 1.5f) && hit.collider.gameObject.CompareTag("Field"))
            {
                tile = hit.collider.gameObject;
                return true;
            }
            return false;
        }
    }
}