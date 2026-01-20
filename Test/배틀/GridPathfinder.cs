using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grid-based A* pathfinder.
/// fieldMask semantics: 0 = blocked, 1 = free, 2 = occupied (unit)
/// </summary>
public static class GridPathfinder
{
    private const int BLOCKED = 0;
    private const int FREE = 1;
    private const int OCCUPIED = 2;

    private class Node
    {
        public Vector2Int Pos;
        public Node Parent;
        public int G; // cost from start
        public int H; // manhattan to goal
        public int F => G + H;
    }

    private static readonly Vector2Int[] Neighbors =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    /// <summary>
    /// Generic A*.
    /// If allowOccupiedGoal is true, you may step onto a tile with value 2 only if it is the goal.
    /// </summary>
    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, int[,] fieldMask, bool allowOccupiedGoal = false)
    {
        int H = fieldMask.GetLength(0); // rows (y)
        int W = fieldMask.GetLength(1); // cols (x)

        if (!InBounds(start, W, H) || !InBounds(goal, W, H))
            return null;

        // Early-out
        if (start == goal)
            return new List<Vector2Int> { start };

        var open = new List<Node>(64);
        var openMap = new Dictionary<Vector2Int, Node>(); // pos -> node (fast updates)
        var closed = new HashSet<Vector2Int>();

        Node startNode = new Node { Pos = start, G = 0, H = Heuristic(start, goal) };
        open.Add(startNode);
        openMap[start] = startNode;

        while (open.Count > 0)
        {
            // pick node with min F (stable, no full sort each loop)
            int bestIdx = 0; Node current = open[0];
            for (int i = 1; i < open.Count; i++)
            {
                var n = open[i];
                if (n.F < current.F || (n.F == current.F && n.H < current.H))
                { bestIdx = i; current = n; }
            }
            open.RemoveAt(bestIdx);
            openMap.Remove(current.Pos);
            closed.Add(current.Pos);

            if (current.Pos == goal)
                return Reconstruct(current);

            foreach (var d in Neighbors)
            {
                var np = current.Pos + d;

                if (!InBounds(np, W, H)) continue;

                int cell = fieldMask[np.y, np.x];
                if (cell == BLOCKED) continue;
                if (cell == OCCUPIED && !(allowOccupiedGoal && np == goal)) continue;
                if (closed.Contains(np)) continue;

                int tentativeG = current.G + 1;
                if (openMap.TryGetValue(np, out var existing))
                {
                    // better path
                    if (tentativeG < existing.G)
                    {
                        existing.G = tentativeG;
                        existing.Parent = current;
                    }
                }
                else
                {
                    var node = new Node
                    {
                        Pos = np,
                        Parent = current,
                        G = tentativeG,
                        H = Heuristic(np, goal)
                    };
                    open.Add(node);
                    openMap[np] = node;
                }
            }
        }

        return null; // no path
    }

    /// <summary>
    /// Enemy path: allows stepping onto goal even if occupied.
    /// </summary>
    public static List<Vector2Int> FindPath_Enemy(Vector2Int start, Vector2Int goal, int[,] fieldMask)
        => FindPath(start, goal, fieldMask, allowOccupiedGoal: true);

    private static bool InBounds(Vector2Int p, int W, int H)
        => p.x >= 0 && p.y >= 0 && p.x < W && p.y < H;

    private static int Heuristic(Vector2Int a, Vector2Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    private static List<Vector2Int> Reconstruct(Node end)
    {
        var path = new List<Vector2Int>();
        var cur = end;
        while (cur != null)
        {
            path.Add(cur.Pos);
            cur = cur.Parent;
        }
        path.Reverse();
        return path;
    }
}
