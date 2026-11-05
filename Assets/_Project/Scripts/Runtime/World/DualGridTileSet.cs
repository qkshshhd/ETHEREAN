using UnityEngine;
using UnityEngine.Tilemaps;

namespace Etherean.World
{
    [CreateAssetMenu(menuName = "ETHEREAN/Dual Grid Tile Set")]
    public sealed class DualGridTileSet : ScriptableObject
    {
        [SerializeField] private TileBase[] tilesByMask = new TileBase[16];

        public TileBase GetTile(int mask) =>
            mask > 0 && mask < 16 && mask < tilesByMask.Length ? tilesByMask[mask] : null;

        public bool IsComplete
        {
            get
            {
                for (int mask = 1; mask < 16; mask++)
                    if (GetTile(mask) == null) return false;
                return true;
            }
        }
    }
}
