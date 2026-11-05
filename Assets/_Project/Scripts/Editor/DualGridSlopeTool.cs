using Etherean.World;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Etherean.EditorTools
{
    internal static class DualGridSlopeTool
    {
        private static readonly Color PreviewColor = new(.1f, .85f, .75f, .95f);
        private static DualGridTerrain _terrain;
        private static bool _dragging;
        private static Vector2Int _start;
        private static int _dragControl;

        internal static bool CanDraw(DualGridTerrain terrain) =>
            DualGridTerrainBaker.IsConfigured(terrain) &&
            terrain.Source.TryGetComponent<CompositeCollider2D>(out _) &&
            terrain.Source.TryGetComponent<TilemapCollider2D>(out var tiles) &&
            tiles.compositeOperation == Collider2D.CompositeOperation.Merge && tiles.compositeOrder < 10 &&
            terrain.Source.TryGetComponent<Rigidbody2D>(out var body) && body.bodyType == RigidbodyType2D.Static;

        internal static void StartDrawing(DualGridTerrain terrain)
        {
            StopDrawing();
            if (!CanDraw(terrain) || EditorApplication.isPlayingOrWillChangePlaymode) return;
            DualGridCellPreview.Stop();
            ToolManager.RestorePreviousPersistentTool();
            _terrain = terrain;
            SceneView.duringSceneGui += OnSceneGUI;
            Selection.selectionChanged += StopDrawing;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            SceneView.lastActiveSceneView?.Focus();
            SceneView.RepaintAll();
        }

        internal static void CancelDrawing() => StopDrawing();

        private static void StopDrawing()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            Selection.selectionChanged -= StopDrawing;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            if (_dragControl != 0 && GUIUtility.hotControl == _dragControl) GUIUtility.hotControl = 0;
            _dragControl = 0;
            _dragging = false;
            _terrain = null;
            SceneView.RepaintAll();
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode) StopDrawing();
        }

        private static void OnSceneGUI(SceneView view)
        {
            if (!CanDraw(_terrain))
            {
                StopDrawing();
                return;
            }

            var e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                e.Use();
                StopDrawing();
                return;
            }

            int control = GUIUtility.GetControlID(FocusType.Passive);
            if (e.type == EventType.Layout && !e.alt) HandleUtility.AddDefaultControl(control);
            var source = _terrain.Source;
            var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            var plane = new Plane(source.transform.forward, source.transform.position);
            if (!plane.Raycast(ray, out float distance)) return;
            var cell = (Vector2Int)source.WorldToCell(ray.GetPoint(distance));

            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt && HandleUtility.nearestControl == control)
            {
                _dragControl = control;
                GUIUtility.hotControl = control;
                _start = cell;
                _dragging = true;
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0 && _dragging && GUIUtility.hotControl == _dragControl)
            {
                var cells = BoundsOf(_start, cell);
                var terrain = _terrain;
                bool risesRight = RisesRight(_start, cell);
                e.Use();
                StopDrawing();
                if (!CanPlace(terrain, cells, null))
                {
                    view.ShowNotification(new GUIContent("Slope regions may touch but cannot overlap."));
                    return;
                }
                Create(terrain, cells, risesRight);
                return;
            }
            else if (e.type == EventType.MouseDrag && _dragging)
            {
                e.Use();
                view.Repaint();
            }
            else if (e.type == EventType.MouseMove) view.Repaint();

            if (e.type != EventType.Repaint) return;
            var bounds = BoundsOf(_dragging ? _start : cell, cell);
            bool rise = !_dragging || RisesRight(_start, cell);
            DrawBounds(_terrain, bounds, rise);
            Handles.BeginGUI();
            GUI.Label(new Rect(12, 28, 540, 25), "Drag across the cells to smooth. Esc cancels.", EditorStyles.helpBox);
            Handles.EndGUI();
        }

        internal static RectInt BoundsOf(Vector2Int start, Vector2Int end)
        {
            var min = Vector2Int.Min(start, end);
            var max = Vector2Int.Max(start, end);
            return new RectInt(min, max - min + Vector2Int.one);
        }

        private static bool RisesRight(Vector2Int start, Vector2Int end) =>
            end.x == start.x || end.y == start.y || ((end.x > start.x) == (end.y > start.y));

        internal static bool CanPlace(DualGridTerrain terrain, RectInt cells, DualGridSlopeRegion ignore)
        {
            if (!CanDraw(terrain) || cells.width <= 0 || cells.height <= 0) return false;
            foreach (var other in terrain.Source.GetComponentsInChildren<DualGridSlopeRegion>(true))
                if (other != ignore && other.Cells.Overlaps(cells)) return false;
            return true;
        }

        private static void Create(DualGridTerrain terrain, RectInt cells, bool risesRight)
        {
            var obj = new GameObject("Slope Region");
            obj.transform.SetParent(terrain.Source.transform, false);
            obj.layer = terrain.Source.gameObject.layer;
            var region = obj.AddComponent<DualGridSlopeRegion>();
            var cut = obj.AddComponent<BoxCollider2D>();
            var fill = obj.AddComponent<PolygonCollider2D>();
            var data = new SerializedObject(region);
            data.FindProperty("cells").rectIntValue = cells;
            data.FindProperty("risesRight").boolValue = risesRight;
            data.FindProperty("cut").objectReferenceValue = cut;
            data.FindProperty("fill").objectReferenceValue = fill;
            data.ApplyModifiedPropertiesWithoutUndo();
            Bake(terrain, region);
            Undo.RegisterCreatedObjectUndo(obj, "Draw Slope Region");
            Selection.activeGameObject = obj;
        }

        internal static void Bake(DualGridTerrain terrain, DualGridSlopeRegion region)
        {
            var cells = region.Cells;
            var source = terrain.Source;
            Vector2 min = source.CellToLocal(new Vector3Int(cells.xMin, cells.yMin, 0));
            Vector2 max = source.CellToLocal(new Vector3Int(cells.xMax, cells.yMax, 0));
            var cut = region.Cut;
            cut.offset = (min + max) * .5f;
            cut.size = max - min;
            cut.compositeOrder = 10;
            cut.compositeOperation = Collider2D.CompositeOperation.Difference;

            var fill = region.Fill;
            fill.offset = Vector2.zero;
            fill.pathCount = 1;
            fill.SetPath(0, Triangle(min, max, region.RisesRight));
            fill.compositeOrder = 20;
            fill.compositeOperation = Collider2D.CompositeOperation.Merge;

            PrefabUtility.RecordPrefabInstancePropertyModifications(cut);
            PrefabUtility.RecordPrefabInstancePropertyModifications(fill);
            EditorUtility.SetDirty(cut);
            EditorUtility.SetDirty(fill);
            EditorSceneManager.MarkSceneDirty(region.gameObject.scene);
            SceneView.RepaintAll();
        }

        internal static Vector2[] Triangle(Vector2 min, Vector2 max, bool risesRight) =>
            new[] { min, new Vector2(max.x, min.y), risesRight ? max : new Vector2(min.x, max.y) };

        internal static void DrawBounds(DualGridTerrain terrain, RectInt cells, bool risesRight)
        {
            var source = terrain.Source;
            Vector3 Corner(int x, int y) => source.CellToWorld(new Vector3Int(x, y, 0));
            var bottomLeft = Corner(cells.xMin, cells.yMin);
            var topLeft = Corner(cells.xMin, cells.yMax);
            var topRight = Corner(cells.xMax, cells.yMax);
            var bottomRight = Corner(cells.xMax, cells.yMin);
            Handles.DrawSolidRectangleWithOutline(new[] { bottomLeft, topLeft, topRight, bottomRight },
                new Color(.1f, .85f, .75f, .08f), PreviewColor);
            using (new Handles.DrawingScope(PreviewColor))
                Handles.DrawAAPolyLine(3, risesRight ? bottomLeft : topLeft, risesRight ? topRight : bottomRight);
        }
    }

    [CustomEditor(typeof(DualGridSlopeRegion))]
    internal sealed class DualGridSlopeRegionInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var region = (DualGridSlopeRegion)target;
            var terrain = region.GetComponentInParent<DualGridTerrain>();
            if (!DualGridSlopeTool.CanDraw(terrain) || region.transform.parent != terrain.Source.transform ||
                region.Cut == null || region.Fill == null)
            {
                EditorGUILayout.HelpBox("Create regions using Draw Slope Region on the terrain root.", MessageType.Warning);
                return;
            }

            EditorGUILayout.HelpBox("Edit the area using Cells, not Transform. Keep both ends level with the adjacent floor.", MessageType.Info);
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                var cells = EditorGUILayout.RectIntField("Cells", region.Cells);
                bool risesRight = EditorGUILayout.Toggle("Rises Right", region.RisesRight);
                if (cells == region.Cells && risesRight == region.RisesRight) return;
                if (!DualGridSlopeTool.CanPlace(terrain, cells, region))
                {
                    Debug.LogWarning("Slope area must be non-empty and must not overlap another slope.", region);
                    return;
                }

                Undo.RecordObjects(new Object[] { region, region.Cut, region.Fill }, "Edit Slope Region");
                serializedObject.Update();
                serializedObject.FindProperty("cells").rectIntValue = cells;
                serializedObject.FindProperty("risesRight").boolValue = risesRight;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                DualGridSlopeTool.Bake(terrain, region);
            }
        }

        private void OnSceneGUI()
        {
            var region = (DualGridSlopeRegion)target;
            var terrain = region.GetComponentInParent<DualGridTerrain>();
            if (DualGridSlopeTool.CanDraw(terrain) && region.transform.parent == terrain.Source.transform)
                DualGridSlopeTool.DrawBounds(terrain, region.Cells, region.RisesRight);
        }
    }
}
