using UnityEngine;
using UnityEngine.Tilemaps;

namespace Etherean.World
{
    [RequireComponent(typeof(Grid)), DisallowMultipleComponent]
    public sealed class DualGridTerrain : MonoBehaviour
    {
        [SerializeField] private Tilemap source;
        [SerializeField] private Tilemap surface;
        [SerializeField] private DualGridTileSet tileSet;

        public Tilemap Source => source;
        public Tilemap Surface => surface;
        public DualGridTileSet TileSet => tileSet;
    }
}
