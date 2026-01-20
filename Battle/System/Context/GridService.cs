using System.Collections.Generic;
using UnityEngine;
using static BattleCore.BattleConstants;

namespace BattleCore
{
    /// <summary>
    /// Grid / field utilities, coordinates, and occupancy map.
    /// </summary>
    public class GridService
    {
        public GameObject TilePrefab;
        public Vector3 SpawnOrigin;
        public Vector2 TileSpacing;
        public int[,] FieldMask; // [y, x]
        public GameObject FieldRoot;
        public string MapID;
        public TextAsset InitPositionCsv;

        public Vector2Int WorldToGrid(Vector3 p)
        {
            int gx = Mathf.RoundToInt((p.x - SpawnOrigin.x) / TileSpacing.x);
            int gy = Mathf.RoundToInt((p.z - SpawnOrigin.z) / TileSpacing.y);
            return new Vector2Int(gx, gy);
        }

        public Vector3 GridToWorld(Vector2Int g, float y = 0f)
            => SpawnOrigin + new Vector3(TileSpacing.x * g.x, y, TileSpacing.y * g.y);

        public bool InBounds(Vector2Int g)
        {
            int H = FieldMask.GetLength(0);
            int W = FieldMask.GetLength(1);
            return g.x >= 0 && g.y >= 0 && g.x < W && g.y < H;
        }

        public void InitTileFind(List<GameObject> units)
        {
            int H = FieldMask.GetLength(0);
            int W = FieldMask.GetLength(1);

            for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
                FieldMask[y, x] = (FieldMask[y, x] == BLOCKED) ? BLOCKED : FREE;

            foreach (var v in units)
            {
                if (v == null) continue;
                Vector2Int vec = WorldToGrid(v.transform.position);
                if (InBounds(vec)) FieldMask[vec.y, vec.x] = OCCUPIED;
            }
        }

        public void UpdateOccupancy(Vector3 oldPos, Vector3 newPos)
        {
            Vector2Int oldVec = WorldToGrid(oldPos);
            if (InBounds(oldVec)) FieldMask[oldVec.y, oldVec.x] = FREE;

            Vector2Int newVec = WorldToGrid(newPos);
            if (InBounds(newVec)) FieldMask[newVec.y, newVec.x] = OCCUPIED;
        }
    }
}