using Etherean.World;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Etherean.EditorTools
{
    internal static class DualGridCellPreview
    {
        private static readonly Color SourceOutline = new(.15f, .85f, .9f, 1);
        private static readonly Color SurfaceOutline = new(1f, .73f, .2f, 1);
        private static readonly Rect Panel = new(14, 32, 312, 176);
        private static DualGridTerrain _terrain;
        private static TileBase _brush;
        private static Vector3Int _cell;
        private static bool _hasCell;
        private static bool _pinned;

        internal static bool IsActive(DualGridTerrain terrain) => _terrain != null && _terrain == terrain;

        internal static void Toggle(DualGridTerrain terrain)
        {
            bool active = IsActive(terrain);
            Stop();
            if (active || !DualGridTerrainBaker.IsConfigured(terrain) || EditorApplication.isPlayingOrWillChangePlaymode) return;
            DualGridSlopeTool.CancelDrawing();
            ToolManager.RestorePreviousPersistentTool();
            _terrain = terrain;
            _brush = AssetDatabase.LoadAssetAtPath<TileBase>(DualGridTerrainSetup.BrushPath);
            SceneView.duringSceneGui += Draw;
            Selection.selectionChanged += Stop;
            EditorApplication.playModeStateChanged += PlayModeChanged;
            SceneView.lastActiveSceneView?.Focus();
            SceneView.RepaintAll();
        }

        internal static void Stop()
        {
            SceneView.duringSceneGui -= Draw;
            Selection.selectionChanged -= Stop;
            EditorApplication.playModeStateChanged -= PlayModeChanged;
            _terrain = null;
            _brush = null;
            _hasCell = false;
            _pinned = false;
            SceneView.RepaintAll();
        }

        private static void PlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode) Stop();
        }

        private static void Draw(SceneView view)
        {
            if (!DualGridTerrainBaker.IsConfigured(_terrain))
            {
                Stop();
                return;
            }
            var e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                e.Use();
                Stop();
                return;
            }

            bool overPanel = Panel.Contains(e.mousePosition);
            int control = GUIUtility.GetControlID(FocusType.Passive);
            if (e.type == EventType.Layout && !e.alt && !overPanel) HandleUtility.AddDefaultControl(control);

            if (!_pinned && !e.alt && !overPanel)
            {
                var surface = _terrain.Surface;
                var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (new Plane(surface.transform.forward, surface.transform.position).Raycast(ray, out float distance))
                {
                    _cell = surface.WorldToCell(ray.GetPoint(distance));
                    _cell.z = 0;
                    _hasCell = true;
                    if (e.type == EventType.MouseMove) view.Repaint();
                }
            }

            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt && !overPanel && _hasCell &&
                HandleUtility.nearestControl == control)
            {
                _pinned = true;
                e.Use();
                view.Repaint();
            }

            if (_hasCell && e.type == EventType.Repaint)
            {
                DrawCell(_terrain.Source, _cell, SourceOutline);
                DrawCell(_terrain.Source, _cell + Vector3Int.right, SourceOutline);
                DrawCell(_terrain.Source, _cell + Vector3Int.up, SourceOutline);
                DrawCell(_terrain.Source, _cell + Vector3Int.up + Vector3Int.right, SourceOutline);
                DrawCell(_terrain.Surface, _cell, SurfaceOutline);
            }

            Handles.BeginGUI();
            GUI.Box(Panel, GUIContent.none, EditorStyles.helpBox);
            GUI.Label(Row(7), "Dual Grid Preview", EditorStyles.boldLabel);
            GUI.Label(Row(27), "Cyan: 4 source cells   Yellow: 1 output cell");
            if (_hasCell)
            {
                int mask = DualGridTerrainBaker.GetMask(_terrain.Source, _cell);
                var tile = _terrain.TileSet != null ? _terrain.TileSet.GetTile(mask) : null;
                GUI.Label(Row(47), $"Mask {mask:00}   |   " + (mask == 0 ? "Empty" : tile != null ? tile.name : "Art unassigned"));
                using (new EditorGUI.DisabledScope(!_pinned))
                {
                    DrawCorner(_cell + Vector3Int.up, Button(68, 0));
                    DrawCorner(_cell + Vector3Int.up + Vector3Int.right, Button(68, 1));
                    DrawCorner(_cell, Button(93, 0));
                    DrawCorner(_cell + Vector3Int.right, Button(93, 1));
                }
            }
            GUI.Label(Row(119), _pinned ? "Click a corner above to change it." : "Hover over the terrain; click to pin a cell.");
            if (_pinned && GUI.Button(Button(144, 0), "Unpin")) { _pinned = false; view.Repaint(); }
            bool close = GUI.Button(Button(144, 1), "Close (Esc)");
            Handles.EndGUI();
            if (close) Stop();
        }

        private static Rect Row(int y) => new(Panel.x + 8, Panel.y + y, Panel.width - 16, 20);

        private static Rect Button(int y, int column) => new(Panel.x + 8 + 150 * column, Panel.y + y, 146, 22);

        private static void DrawCorner(Vector3Int cell, Rect button)
        {
            var source = _terrain.Source;
            bool solid = source.HasTile(cell);
            using (new EditorGUI.DisabledScope(!solid && _brush == null))
            {
                if (!GUI.Button(button, solid ? "Ground" : "Air")) return;
                Undo.RegisterCompleteObjectUndo(source, "Edit Dual Grid Corner");
                source.SetTile(cell, solid ? null : _brush);
                PrefabUtility.RecordPrefabInstancePropertyModifications(source);
                EditorUtility.SetDirty(source);
                EditorSceneManager.MarkSceneDirty(source.gameObject.scene);
                SceneView.RepaintAll();
            }
        }

        private static void DrawCell(Tilemap map, Vector3Int cell, Color color)
        {
            Handles.DrawSolidRectangleWithOutline(new[]
            {
                map.CellToWorld(cell), map.CellToWorld(cell + Vector3Int.up),
                map.CellToWorld(cell + Vector3Int.up + Vector3Int.right), map.CellToWorld(cell + Vector3Int.right)
            }, new Color(color.r, color.g, color.b, .06f), color);
        }
    }
}
