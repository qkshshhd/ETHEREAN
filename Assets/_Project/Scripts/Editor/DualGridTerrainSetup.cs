using System;
using System.Collections.Generic;
using Etherean.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Etherean.EditorTools
{
    internal static class DualGridTerrainSetup
    {
        internal const string BrushPath = "Assets/_Project/Data/World/DualGrid/WhiteboxSolid.asset";
        private const string SourceSpritePath = "Assets/_Project/Art/Sprites/MovementSquare.png";
        private const string SamplePath = "Assets/_Project/Scenes/Tests/DualGridTerrainTest.unity";
        private const string PlayerPath = "Assets/_Project/Prefabs/Player/Player.prefab";
        private const float CellSize = .5f;

        [MenuItem("ETHEREAN/Level Design/Create Dual Grid Terrain")]
        private static void CreateInScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded || EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureBrush();
            var terrain = Create(scene, "Dual Grid Terrain");
            Undo.RegisterCreatedObjectUndo(terrain.gameObject, "Create Dual Grid Terrain");
            Selection.activeGameObject = terrain.gameObject;
        }

        [MenuItem("ETHEREAN/Level Design/Open Dual Grid Sample")]
        private static void OpenSample()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SamplePath) != null)
            {
                EditorSceneManager.OpenScene(SamplePath);
                return;
            }

            var brush = EnsureBrush();
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPath);
            if (playerPrefab == null) throw new InvalidOperationException("Player prefab is missing.");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var terrain = Create(scene, "Dual Grid Sample");
            var cells = new List<Vector3Int>();
            for (int x = -18; x <= 18; x++)
            {
                if (x == 3 || x == 4) continue;
                int top = x < -8 ? -1 : x < 3 ? 0 : x < 12 ? -1 : 0;
                for (int y = -4; y <= top; y++) cells.Add(new Vector3Int(x, y, 0));
            }
            for (int x = -5; x <= -2; x++) cells.Add(new Vector3Int(x, 6, 0));
            var tiles = new TileBase[cells.Count];
            Array.Fill(tiles, brush);
            terrain.Source.SetTiles(cells.ToArray(), tiles);
            DualGridTerrainBaker.Rebuild(terrain);

            var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            player.transform.position = new Vector3(-7.5f, 1.05f);
            var cameraObject = new GameObject("Sample Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0, 1.1f, -10);
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.625f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.12f, .16f, .23f);
            if (!EditorSceneManager.SaveScene(scene, SamplePath))
                throw new InvalidOperationException("Could not save the Dual Grid sample scene.");
            Selection.activeGameObject = terrain.gameObject;
        }

        private static DualGridTerrain Create(Scene scene, string name)
        {
            var root = new GameObject(name, typeof(Grid), typeof(DualGridTerrain));
            SceneManager.MoveGameObjectToScene(root, scene);
            root.GetComponent<Grid>().cellSize = new Vector3(CellSize, CellSize, 0);

            var source = CreateLayer(root.transform, "Terrain Source - Paint Here");
            var surface = CreateLayer(root.transform, "Terrain Surface - Generated");
            var body = source.gameObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;
            var composite = source.gameObject.AddComponent<CompositeCollider2D>();
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
            var collider = source.gameObject.AddComponent<TilemapCollider2D>();
            collider.compositeOperation = Collider2D.CompositeOperation.Merge;

            var terrain = root.GetComponent<DualGridTerrain>();
            var settings = new SerializedObject(terrain);
            settings.FindProperty("source").objectReferenceValue = source;
            settings.FindProperty("surface").objectReferenceValue = surface;
            settings.ApplyModifiedPropertiesWithoutUndo();
            DualGridTerrainBaker.Rebuild(terrain);
            EditorSceneManager.MarkSceneDirty(scene);
            return terrain;
        }

        private static Tilemap CreateLayer(Transform parent, string name)
        {
            var child = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
            child.transform.SetParent(parent, false);
            return child.GetComponent<Tilemap>();
        }

        private static Tile EnsureBrush()
        {
            var brush = AssetDatabase.LoadAssetAtPath<Tile>(BrushPath);
            if (brush != null) return brush;

            const string world = "Assets/_Project/Data/World";
            const string folder = world + "/DualGrid";
            const string square = folder + "/WhiteboxSolid.png";
            if (!AssetDatabase.IsValidFolder(world)) AssetDatabase.CreateFolder("Assets/_Project/Data", "World");
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder(world, "DualGrid");
            if (AssetImporter.GetAtPath(square) == null && !AssetDatabase.CopyAsset(SourceSpritePath, square))
                throw new InvalidOperationException("The existing MovementSquare sprite could not be copied.");

            var importer = (TextureImporter)AssetImporter.GetAtPath(square);
            importer.spritePixelsPerUnit = 32 / CellSize;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(square);
            if (sprite == null) throw new InvalidOperationException("Whitebox sprite import failed.");

            brush = ScriptableObject.CreateInstance<Tile>();
            brush.sprite = sprite;
            brush.colliderType = Tile.ColliderType.Grid;
            AssetDatabase.CreateAsset(brush, BrushPath);
            AssetDatabase.SaveAssets();
            return brush;
        }
    }
}
