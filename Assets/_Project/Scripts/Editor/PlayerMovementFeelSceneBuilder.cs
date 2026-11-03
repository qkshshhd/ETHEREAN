using System;
using System.IO;
using Etherean.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Etherean.Editor
{
    public static class PlayerMovementFeelSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Tests/PlayerMovementFeelTest.unity";

        [MenuItem("ETHEREAN/Player/Save Direct Movement Prefab")]
        public static void SaveDirectMovementPrefab()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
            var root = PrefabUtility.LoadPrefabContents(PlayerCharacterAnimationBuilder.PrefabPath);
            try { PrefabUtility.SaveAsPrefabAsset(root, PlayerCharacterAnimationBuilder.PrefabPath); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        [MenuItem("ETHEREAN/Player/Apply Balanced Jump Timing")]
        public static void ApplyTuning()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
            var root = PrefabUtility.LoadPrefabContents(PlayerCharacterAnimationBuilder.PrefabPath);
            try
            {
                var movement = new SerializedObject(root.GetComponent<PlayerMovement>());
                movement.FindProperty("riseGravity").floatValue = 32f;
                movement.FindProperty("fallGravity").floatValue = 52f;
                movement.FindProperty("maxFallSpeed").floatValue = 16f;
                movement.ApplyModifiedPropertiesWithoutUndo();
                Set(root.GetComponent<PlayerJump>(), "jumpSpeed", 8.986667f);
                Set(root.GetComponent<PlayerJump>(), "jumpCutMultiplier", 0.51080966f);
                Set(root.GetComponent<PlayerWallJump>(), "wallJumpVerticalSpeed", 8.086154f);
                PrefabUtility.SaveAsPrefabAsset(root, PlayerCharacterAnimationBuilder.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        [MenuItem("ETHEREAN/Tests/Create Movement Feel Scene")]
        public static void Create()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
            if (File.Exists(ScenePath)) return;
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                var cameraObject = new GameObject("Movement Camera"); SceneManager.MoveGameObjectToScene(cameraObject, scene);
                var camera = cameraObject.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 5.625f;
                camera.transform.position = new Vector3(0f, 2.2f, -10f); camera.backgroundColor = new Color(0.12f, 0.16f, 0.23f);
                camera.clearFlags = CameraClearFlags.SolidColor; cameraObject.tag = "MainCamera"; cameraObject.AddComponent<AudioListener>();
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerCharacterAnimationBuilder.PrefabPath);
                var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                var collider = player.GetComponent<CapsuleCollider2D>();
                player.transform.position = new Vector3(-9.1f, -collider.offset.y + collider.size.y * 0.5f + 0.01f, 0f);
                var abilities = new SerializedObject(player.GetComponent<PlayerTraversalAbilities>());
                abilities.FindProperty("wallTraversalUnlocked").boolValue = true;
                abilities.ApplyModifiedPropertiesWithoutUndo();
                Platform(scene, "Start Floor", -6.625f, -0.25f, 5.75f, 0.5f);
                Platform(scene, "Center Floor", 0f, -0.25f, 5f, 0.5f);
                Platform(scene, "Wall Floor", 7f, -0.25f, 5f, 0.5f);
                Platform(scene, "Step 0.35", -8.2f, 0.175f, 0.9f, 0.35f);
                Platform(scene, "Step 0.65", -6.6f, 0.325f, 0.9f, 0.65f);
                Platform(scene, "Step 0.95", -5f, 0.475f, 0.9f, 0.95f);
                Platform(scene, "Low Ceiling", 0f, 1.725f, 2.25f, 0.25f);
                Platform(scene, "Landing Step", 5.5f, 0.25f, 1.1f, 0.5f);
                Platform(scene, "Wall Left", 7.5f, 2.75f, 0.35f, 3.4f);
                Platform(scene, "Wall Right", 9f, 2.25f, 0.35f, 4.5f);
                Platform(scene, "Safety Floor", 0f, -2.25f, 20f, 0.5f);
                Platform(scene, "Safety Left", -9.8f, 0.5f, 0.25f, 5.5f);
                Platform(scene, "Safety Right", 9.8f, 0.5f, 0.25f, 5.5f);
                Label(scene, "HEIGHT: 0.35 / 0.65 / 0.95u", -6.6f, 2.2f);
                Label(scene, "GAP 1.25u", -3.125f, -0.6f);
                Label(scene, "GAP 2u", 3.5f, -0.6f);
                Label(scene, "LOW CEILING", 0f, 2.3f);
                Label(scene, "WALL JUMP", 8.25f, 4.9f);
                Label(scene, "ENTER BELOW", 7.5f, 0.4f);
                Label(scene, "A/D MOVE   SPACE JUMP / WALL JUMP", 0f, 6.4f);
                Label(scene, "Safety floor: restart Play to reset", 0f, -2.7f);
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void Platform(Scene scene, string name, float x, float y, float width, float height)
        {
            var item = new GameObject(name); SceneManager.MoveGameObjectToScene(item, scene);
            item.transform.position = new Vector3(x, y, 0f); item.transform.localScale = new Vector3(width, height, 1f);
            var renderer = item.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/MovementSquare.png");
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/MovementPlaceholder.mat");
            renderer.color = new Color(0.36f, 0.42f, 0.46f);
            item.AddComponent<BoxCollider2D>();
        }

        private static void Label(Scene scene, string text, float x, float y)
        {
            var item = new GameObject(text); SceneManager.MoveGameObjectToScene(item, scene);
            item.transform.position = new Vector3(x, y, -0.1f);
            var label = item.AddComponent<TextMesh>(); label.text = text; label.fontSize = 40; label.characterSize = 0.12f;
            label.anchor = TextAnchor.MiddleCenter; label.color = new Color(0.85f, 0.88f, 0.92f);
        }

        private static void Set(UnityEngine.Object target, string field, float value)
        {
            var settings = new SerializedObject(target); settings.FindProperty(field).floatValue = value;
            settings.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
