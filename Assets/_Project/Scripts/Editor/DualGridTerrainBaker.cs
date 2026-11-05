using System.Collections.Generic;
using Etherean.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Etherean.EditorTools
{
    [InitializeOnLoad]
    internal static class DualGridTerrainBaker
    {
        private static readonly Vector3Int East = Vector3Int.right;
        private static readonly Vector3Int North = Vector3Int.up;
        private static readonly Vector3Int NorthEast = East + North;
        private static readonly Dictionary<DualGridTerrain, HashSet<Vector3Int>> Pending = new();

        static DualGridTerrainBaker()
        {
            Tilemap.tilemapTileChanged += QueueChanges;
            Undo.undoRedoPerformed += RebuildLoadedTerrains;
            EditorSceneManager.sceneSaving += (_, _) => Flush();
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.ExitingEditMode) Flush();
            };
        }

        internal static bool IsConfigured(DualGridTerrain terrain) =>
            terrain != null && terrain.Source != null && terrain.Surface != null &&
            terrain.Source != terrain.Surface && terrain.Source.transform.parent == terrain.transform &&
            terrain.Surface.transform.parent == terrain.transform;

        internal static int GetMask(Tilemap source, Vector3Int cell) =>
            DualGridMask.FromCorners(source.HasTile(cell), source.HasTile(cell + East),
                                     source.HasTile(cell + North), source.HasTile(cell + NorthEast));

        private static void AddAffected(HashSet<Vector3Int> cells, Vector3Int sourceCell)
        {
            if (sourceCell.z != 0) return;
            cells.Add(sourceCell);
            cells.Add(sourceCell - East);
            cells.Add(sourceCell - North);
            cells.Add(sourceCell - NorthEast);
        }

        private static void QueueChanges(Tilemap map, Tilemap.SyncTile[] changes)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var terrain = map.GetComponentInParent<DualGridTerrain>();
            if (!IsConfigured(terrain) || terrain.Source != map || terrain.TileSet == null ||
                !terrain.gameObject.scene.IsValid() || !terrain.gameObject.scene.isLoaded) return;

            if (!Pending.TryGetValue(terrain, out var cells))
            {
                cells = new HashSet<Vector3Int>();
                Pending.Add(terrain, cells);
            }
            foreach (var change in changes) AddAffected(cells, change.position);
            EditorApplication.delayCall -= Flush;
            EditorApplication.delayCall += Flush;
        }

        private static void Flush()
        {
            if (!EditorApplication.isPlaying)
                foreach (var entry in Pending)
                    if (IsConfigured(entry.Key)) WriteCells(entry.Key, entry.Value);
            Pending.Clear();
        }

        private static void RebuildLoadedTerrains()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            foreach (var terrain in Object.FindObjectsByType<DualGridTerrain>(FindObjectsInactive.Include))
                if (IsConfigured(terrain) && terrain.TileSet != null) Rebuild(terrain);
        }

        internal static void Rebuild(DualGridTerrain terrain)
        {
            if (!IsConfigured(terrain)) return;
            var cells = new HashSet<Vector3Int>();
            foreach (var cell in OccupiedCells(terrain.Source)) AddAffected(cells, cell);
            foreach (var cell in OccupiedCells(terrain.Surface)) cells.Add(cell);
            WriteCells(terrain, cells);

            var grid = terrain.GetComponent<Grid>();
            var offset = (grid.CellToLocal(Vector3Int.right) + grid.CellToLocal(Vector3Int.up)) * .5f;
            if (terrain.Surface.transform.localPosition != offset)
            {
                terrain.Surface.transform.localPosition = offset;
                MarkDirty(terrain.Surface.transform);
            }

            var renderer = terrain.Source.GetComponent<TilemapRenderer>();
            bool showSource = terrain.TileSet == null || !terrain.TileSet.IsComplete;
            if (renderer != null && renderer.enabled != showSource)
            {
                renderer.enabled = showSource;
                MarkDirty(renderer);
            }
        }

        private static IEnumerable<Vector3Int> OccupiedCells(Tilemap map)
        {
            var bounds = map.cellBounds;
            if (bounds.size.x == 0 || bounds.size.y == 0 || bounds.size.z == 0) yield break;
            var end = bounds.max - Vector3Int.one;
            int count = map.GetTilesRangeCount(bounds.min, end);
            if (count == 0) yield break;
            var positions = new Vector3Int[count];
            var tiles = new TileBase[count];
            int read = map.GetTilesRangeNonAlloc(bounds.min, end, positions, tiles);
            for (int i = 0; i < read; i++)
                if (tiles[i] != null) yield return positions[i];
        }

        private static void WriteCells(DualGridTerrain terrain, IEnumerable<Vector3Int> cells)
        {
            var tileSet = terrain.TileSet;
            bool useSurface = tileSet != null && tileSet.IsComplete;
            var positions = new List<Vector3Int>();
            var tiles = new List<TileBase>();
            foreach (var cell in cells)
            {
                var expected = !useSurface || cell.z != 0 ? null : tileSet.GetTile(GetMask(terrain.Source, cell));
                if (terrain.Surface.GetTile(cell) == expected) continue;
                positions.Add(cell);
                tiles.Add(expected);
            }
            if (positions.Count == 0) return;
            terrain.Surface.SetTiles(positions.ToArray(), tiles.ToArray());
            MarkDirty(terrain.Surface);
        }

        private static void MarkDirty(Component component)
        {
            EditorUtility.SetDirty(component);
            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            var scene = component.gameObject.scene;
            if (scene.IsValid() && scene.isLoaded) EditorSceneManager.MarkSceneDirty(scene);
        }
    }

    internal static class DualGridMask
    {
        internal static int FromCorners(bool southWest, bool southEast, bool northWest, bool northEast) =>
            (southWest ? 1 : 0) | (southEast ? 2 : 0) | (northWest ? 4 : 0) | (northEast ? 8 : 0);
    }
}
