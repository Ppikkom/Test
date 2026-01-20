using System.Collections.Generic;
using UnityEngine;
using static BattleCore.BattleConstants;

namespace BattleCore
{
    /// <summary>
    /// Tile highlight show/hide helpers.
    /// </summary>
    public class TileHighlighter
    {
        public GameObject FieldRoot;

        public void RemoveTileHighlight(List<GameObject> tiles)
        {
            foreach (var v in tiles)
            {
                if (v == null) continue;
                var mr = v.GetComponent<MeshRenderer>();
                var r = v.GetComponent<Renderer>();
                if (mr) mr.enabled = false;
                if (r) r.material.color = TileHidden;
            }
        }

        public void RemoveAllTileHighlight()
        {
            if (FieldRoot == null) return;
            var renderers = FieldRoot.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                var mr = r as MeshRenderer;
                if (mr) mr.enabled = false;
                r.material.color = TileHidden;
            }
        }

        public void Colorize(List<GameObject> tiles, Color c)
        {
            foreach (var v in tiles)
            {
                var r = v.GetComponent<Renderer>();
                var mr = v.GetComponent<MeshRenderer>();
                if (mr) mr.enabled = true;
                if (r)  r.material.color = c;
            }
        }
    }
}