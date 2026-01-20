using UnityEngine;


namespace BattleCore
{
    public static class BattleConstants
    {
    public const int BLOCKED = 0;
    public const int FREE = 1;
    public const int OCCUPIED = 2;


    // Layers
    public static readonly int MaskField = LayerMask.GetMask("Field");
    public static readonly int MaskIgnoreRayCast = LayerMask.GetMask("Ignore Raycast"); // authoring spelling kept


    public static int EffectiveMask(int m) => m == 0 ? Physics.DefaultRaycastLayers : m;


    // Tile colors
    public static readonly Color TileHidden = Color.black;
    public static readonly Color TileMove = Color.red;
    public static readonly Color TileMoveCur = Color.yellow;
    public static readonly Color TileAttack = Color.yellow;
    }
}