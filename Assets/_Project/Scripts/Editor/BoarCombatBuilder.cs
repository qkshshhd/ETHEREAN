using System;
using System.IO;
using Etherean.Combat;
using Etherean.Enemies;
using Etherean.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Etherean.Editor
{
    public static class BoarCombatBuilder
    {
        public const string PrefabPath = "Assets/_Project/Prefabs/Enemies/Boar.prefab";
        public const string DataPath = "Assets/_Project/Data/Enemies/BoarData.asset";
        public const string ControllerPath = "Assets/_Project/Art/Animations/Boar/Boar.controller";
        public const string ScenePath = "Assets/_Project/Scenes/Tests/PlayerBoarCombatTest.unity";
        public static readonly string[] AnimationStates = { "Idle", "Walk", "Alert", "Windup", "Charge", "Recovery", "Stun", "Stagger", "Death", "KnockdownLaunch", "KnockdownDown", "GetUp" };

        [MenuItem("ETHEREAN/Tests/Create Boar Combat Test")]
        public static void Build()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play first.");
            Directory.CreateDirectory(Path.GetDirectoryName(ControllerPath)); AssetDatabase.Refresh();
            var data = AssetDatabase.LoadAssetAtPath<BoarData>(DataPath);
            if (data == null) { data = ScriptableObject.CreateInstance<BoarData>(); AssetDatabase.CreateAsset(data, DataPath); }
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            var machine = controller.layers[0].stateMachine;
            foreach (var name in AnimationStates)
            {
                AnimatorState state = null;
                foreach (var child in machine.states) if (child.state.name == name) state = child.state;
                if (state == null) state = machine.AddState(name);
                if (name == "Idle") machine.defaultState = state;
            }
            EditorUtility.SetDirty(controller); AssetDatabase.SaveAssetIfDirty(controller);
            if (!File.Exists(PrefabPath)) CreatePrefab(data, controller);
            if (!File.Exists(ScenePath)) CreateScene();
        }

        private static void CreatePrefab(BoarData data, AnimatorController controller)
        {
            var root = new GameObject("Boar");
            try
            {
                root.layer = 9;
                var body = root.AddComponent<Rigidbody2D>(); body.gravityScale = 0; body.freezeRotation = true;
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                var collider = root.AddComponent<BoxCollider2D>(); collider.size = new Vector2(.9f, .9f);
                collider.sharedMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/_Project/Data/Player/PlayerMovement.physicsMaterial2D");
                var brain = root.AddComponent<BoarBrain>(); Set(brain, "data", data);
                var health = new SerializedObject(root.GetComponent<CombatHealth>()); health.FindProperty("maxHealth").intValue = 80; health.ApplyModifiedPropertiesWithoutUndo();
                var square = new GameObject("Square"); square.transform.SetParent(root.transform, false); square.layer = 9;
                square.transform.localScale = new Vector3(.9f, .9f, 1);
                var placeholder = square.AddComponent<SpriteRenderer>();
                placeholder.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/MovementSquare.png");
                placeholder.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/MovementPlaceholder.mat");
                SpriteSortingSetup.ConfigureEnemy(placeholder);
                var visual = new GameObject("CharacterVisual"); visual.transform.SetParent(root.transform, false); visual.layer = 9;
                visual.transform.localPosition = new Vector3(0, -.45f, 0);
                var renderer = visual.AddComponent<SpriteRenderer>(); renderer.enabled = false; renderer.sharedMaterial = placeholder.sharedMaterial;
                SpriteSortingSetup.ConfigureEnemy(renderer);
                var animator = visual.AddComponent<Animator>(); animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                var presenter = root.AddComponent<BoarAnimator>();
                Set(presenter, "brain", brain); Set(presenter, "motor", root.GetComponent<BoarMotor>());
                Set(presenter, "health", root.GetComponent<CombatHealth>()); Set(presenter, "reaction", root.GetComponent<CombatReaction>());
                Set(presenter, "characterAnimator", animator); Set(presenter, "characterRenderer", renderer);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        private static void CreateScene()
        {
            var previous = SceneManager.GetActiveScene(); var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                var cameraObject = new GameObject("Camera"); SceneManager.MoveGameObjectToScene(cameraObject, scene);
                var camera = cameraObject.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 4.5f;
                camera.transform.position = new Vector3(-1, 2, -10); cameraObject.tag = "MainCamera"; cameraObject.AddComponent<AudioListener>();
                Terrain(scene, "Ground", new Vector2(-2, -.5f), new Vector2(16, 1));
                Terrain(scene, "Charge Stun Wall", new Vector2(-7.5f, 1.5f), new Vector2(.5f, 3));
                var player = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PlayerCharacterAnimationBuilder.PrefabPath), scene);
                player.transform.position = new Vector3(-3.5f, .91f, 0);
                var unlock = new SerializedObject(player.GetComponent<PlayerAttack>()); unlock.FindProperty("weaponUnlocked").boolValue = true; unlock.ApplyModifiedPropertiesWithoutUndo();
                var enemy = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), scene);
                enemy.transform.position = new Vector3(0, .46f, 0); Set(enemy.GetComponent<BoarBrain>(), "target", player.GetComponent<CombatHealth>());
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            finally { if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous); EditorSceneManager.CloseScene(scene, true); }
        }
        private static void Terrain(Scene scene, string name, Vector2 position, Vector2 size)
        {
            var root = new GameObject(name); SceneManager.MoveGameObjectToScene(root, scene);
            root.transform.position = position; root.transform.localScale = new Vector3(size.x, size.y, 1);
            var renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/MovementSquare.png");
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/MovementPlaceholder.mat"); root.AddComponent<BoxCollider2D>();
        }
        private static void Set(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            var settings = new SerializedObject(target); settings.FindProperty(field).objectReferenceValue = value; settings.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
