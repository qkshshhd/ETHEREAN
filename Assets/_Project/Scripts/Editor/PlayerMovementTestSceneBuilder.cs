using System;
using System.IO;
using Etherean.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Etherean.Editor
{
    public static class PlayerMovementTestSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Tests/PlayerMovementTest.unity";
        private const string PrefabPath = "Assets/_Project/Prefabs/Player/Player.prefab";
        private const string SpritePath = "Assets/_Project/Art/Sprites/MovementSquare.png";
        private const string MaterialPath = "Assets/_Project/Art/Materials/MovementPlaceholder.mat";
        private const string PhysicsMaterialPath = "Assets/_Project/Data/Player/PlayerMovement.physicsMaterial2D";
        private const string ActionReferencePath = "Assets/_Project/Data/Player/PlayerMoveAction.asset";
        private const string JumpActionReferencePath = "Assets/_Project/Data/Player/PlayerJumpAction.asset";

        [MenuItem("ETHEREAN/Tests/Create Horizontal Movement Scene")]
        public static void Create()
        {
            Create(false);
        }

        internal static void RebuildForValidation()
        {
            Create(true);
        }

        private static void Create(bool rebuild)
        {
            string[] paths = { ScenePath, PrefabPath, SpritePath, MaterialPath, PhysicsMaterialPath, ActionReferencePath, JumpActionReferencePath };
            foreach (string path in paths)
            {
                if (!rebuild && File.Exists(path))
                    throw new InvalidOperationException($"Asset already exists; open the existing movement test scene instead: {path}");
            }

            if (rebuild && UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Save the current scene before rebuilding the movement test.");

            if (!rebuild && !Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            if (actions == null)
                throw new InvalidOperationException("The existing Input Actions asset was not found.");

            InputAction move = actions.FindAction("Player/Move", true);
            InputAction jump = actions.FindAction("Player/Jump", true);
            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null)
                throw new InvalidOperationException("URP 2D unlit sprite shader was not found.");

            Sprite sprite = CreateSquareSprite();
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader) { name = "MovementPlaceholder" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            var physicsMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(PhysicsMaterialPath);
            if (physicsMaterial == null)
            {
                physicsMaterial = new PhysicsMaterial2D("PlayerMovement") { friction = 0f, bounciness = 0f };
                AssetDatabase.CreateAsset(physicsMaterial, PhysicsMaterialPath);
            }
            InputActionReference actionReference = AssetDatabase.LoadAssetAtPath<InputActionReference>(ActionReferencePath);
            if (actionReference == null)
            {
                actionReference = InputActionReference.Create(move);
                AssetDatabase.CreateAsset(actionReference, ActionReferencePath);
            }
            actionReference.Set(move);
            EditorUtility.SetDirty(actionReference);
            InputActionReference jumpReference = AssetDatabase.LoadAssetAtPath<InputActionReference>(JumpActionReferencePath);
            if (jumpReference == null)
            {
                jumpReference = InputActionReference.Create(jump);
                AssetDatabase.CreateAsset(jumpReference, JumpActionReferencePath);
            }
            jumpReference.Set(jump);
            EditorUtility.SetDirty(jumpReference);
            AssetDatabase.SaveAssets();
            if (!EditorUtility.IsPersistent(actionReference) || actionReference.action == null)
                throw new InvalidOperationException("The Move action reference was not saved correctly.");

            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 1f, -10f);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.05f, 0.08f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;

            var player = new GameObject("Player");
            player.SetActive(false);
            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 1f;
            body.linearDamping = 0f;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            var collider = player.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(0.8f, 1.8f);
            collider.sharedMaterial = physicsMaterial;
            var reader = player.AddComponent<NewInputMovementReader>();
            var readerSettings = new SerializedObject(reader);
            readerSettings.FindProperty("moveActionReference").objectReferenceValue = actionReference;
            readerSettings.FindProperty("jumpActionReference").objectReferenceValue = jumpReference;
            readerSettings.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(reader);
            if (new SerializedObject(reader).FindProperty("moveActionReference").objectReferenceValue == null)
                throw new InvalidOperationException("The Move action reference was not assigned to the reader.");
            var groundSensor = player.AddComponent<PlayerGroundSensor>();
            var movement = player.AddComponent<PlayerMovement>();
            var movementSettings = new SerializedObject(movement);
            movementSettings.FindProperty("movementInputSource").objectReferenceValue = reader;
            movementSettings.FindProperty("groundSensor").objectReferenceValue = groundSensor;
            movementSettings.ApplyModifiedPropertiesWithoutUndo();
            GameObject visual = CreateRectangle("Visual", Vector2.zero, new Vector2(0.8f, 1.8f),
                new Color(0.35f, 0.9f, 0.85f), sprite, material);
            visual.transform.SetParent(player.transform, false);
            player.SetActive(true);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(player, PrefabPath);
            if (prefab == null || new SerializedObject(prefab.GetComponent<NewInputMovementReader>())
                    .FindProperty("moveActionReference").objectReferenceValue == null)
                throw new InvalidOperationException("The saved player prefab lost its Move action reference.");
            UnityEngine.Object.DestroyImmediate(player);
            var playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            if (!PrefabUtility.IsPartOfPrefabInstance(playerInstance))
                throw new InvalidOperationException("The test scene player is not linked to its prefab.");

            CreateRectangle("Ground", new Vector2(0f, -1.5f), new Vector2(20f, 1f),
                new Color(0.22f, 0.28f, 0.35f), sprite, material).AddComponent<BoxCollider2D>();
            CreateRectangle("Left Wall", new Vector2(-8f, 0.5f), new Vector2(0.5f, 3f),
                new Color(0.36f, 0.43f, 0.5f), sprite, material).AddComponent<BoxCollider2D>();
            CreateRectangle("Right Wall", new Vector2(8f, 0.5f), new Vector2(0.5f, 3f),
                new Color(0.36f, 0.43f, 0.5f), sprite, material).AddComponent<BoxCollider2D>();
            CreateRectangle("Jump Platform", new Vector2(-3f, 1f), new Vector2(3f, 0.4f),
                new Color(0.35f, 0.45f, 0.3f), sprite, material).AddComponent<BoxCollider2D>();
            CreateRectangle("Low Ceiling", new Vector2(3f, 2.5f), new Vector2(3f, 0.4f),
                new Color(0.45f, 0.35f, 0.3f), sprite, material).AddComponent<BoxCollider2D>();

            var ruler = new GameObject("One Unit Markers");
            for (int x = -7; x <= 7; x++)
            {
                GameObject marker = CreateRectangle($"X {x}", new Vector2(x, -1.2f), new Vector2(0.025f, 0.25f),
                    new Color(0.65f, 0.73f, 0.8f), sprite, material);
                marker.GetComponent<SpriteRenderer>().sortingOrder = 1;
                marker.transform.SetParent(ruler.transform, true);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new IOException("Failed to save the movement test scene.");
            AssetDatabase.SaveAssets();
            Debug.Log($"Horizontal movement scene created: {ScenePath}");
        }

        private static Sprite CreateSquareSprite()
        {
            var texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            var pixels = new Color32[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(255, 255, 255, 255);
            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(SpritePath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(SpritePath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(SpritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32f;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        }

        private static GameObject CreateRectangle(string name, Vector2 position, Vector2 size,
            Color color, Sprite sprite, Material material)
        {
            var rectangle = new GameObject(name, typeof(SpriteRenderer));
            rectangle.transform.position = position;
            rectangle.transform.localScale = new Vector3(size.x, size.y, 1f);
            SpriteRenderer renderer = rectangle.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sharedMaterial = material;
            return rectangle;
        }
    }
}
