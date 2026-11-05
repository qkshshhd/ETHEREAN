using Etherean.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Etherean.EditorTools
{
    [CustomEditor(typeof(DualGridTerrain))]
    internal sealed class DualGridTerrainInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var terrain = (DualGridTerrain)target;
            if (!DualGridTerrainBaker.IsConfigured(terrain))
            {
                EditorGUILayout.HelpBox("Create the terrain using ETHEREAN > Level Design > Create Dual Grid Terrain.", MessageType.Warning);
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Paint Target", terrain.Source, typeof(Tilemap), true);
                EditorGUILayout.ObjectField("Generated Surface", terrain.Surface, typeof(Tilemap), true);
            }
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tileSet"));
            bool changed = serializedObject.ApplyModifiedProperties();

            if (terrain.TileSet == null || !terrain.TileSet.IsComplete)
                EditorGUILayout.HelpBox("Whitebox view is active. Assign all 15 surface tiles to switch to dual-grid art.", MessageType.Info);

            if (changed || GUILayout.Button("Rebuild Surface")) DualGridTerrainBaker.Rebuild(terrain);
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
                if (GUILayout.Button(DualGridCellPreview.IsActive(terrain) ? "Close Dual Grid Preview" : "Preview Dual Grid"))
                    DualGridCellPreview.Toggle(terrain);
            using (new EditorGUI.DisabledScope(!DualGridSlopeTool.CanDraw(terrain) || EditorApplication.isPlayingOrWillChangePlaymode))
                if (GUILayout.Button("Draw Slope Region")) DualGridSlopeTool.StartDrawing(terrain);
            if (GUILayout.Button("Select Paint Target")) Selection.activeGameObject = terrain.Source.gameObject;
            if (GUILayout.Button("Locate Whitebox Brush"))
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Tile>(DualGridTerrainSetup.BrushPath));
        }
    }

    [CustomEditor(typeof(DualGridTileSet))]
    internal sealed class DualGridTileSetInspector : UnityEditor.Editor
    {
        private static readonly GUIContent[] Labels = BuildLabels();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("Corners shown as upper row / lower row. A filled circle is solid. Mask 0 is empty.", MessageType.Info);
            var tiles = serializedObject.FindProperty("tilesByMask");
            if (tiles.arraySize != 16) tiles.arraySize = 16;
            for (int mask = 1; mask < 16; mask++)
                EditorGUILayout.PropertyField(tiles.GetArrayElementAtIndex(mask), Labels[mask]);
            if (serializedObject.ApplyModifiedProperties())
                foreach (var terrain in Object.FindObjectsByType<DualGridTerrain>(FindObjectsInactive.Include))
                    if (terrain.TileSet == target) DualGridTerrainBaker.Rebuild(terrain);
        }

        private static GUIContent[] BuildLabels()
        {
            var labels = new GUIContent[16];
            for (int mask = 1; mask < 16; mask++)
                labels[mask] = new GUIContent($"{mask:00}  {Dot(mask, 4)}{Dot(mask, 8)} / {Dot(mask, 1)}{Dot(mask, 2)}");
            return labels;
        }

        private static char Dot(int mask, int bit) => (mask & bit) != 0 ? '●' : '○';
    }
}
