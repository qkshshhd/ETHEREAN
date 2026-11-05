using System;
using System.IO;
using Etherean.Combat;
using Etherean.Enemies;
using Etherean.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Etherean.Editor
{
    public static class BadgerCombatSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Tests/PlayerBadgerCombatTest.unity";
        public const string BadgerPath = "Assets/_Project/Prefabs/Enemies/Badger.prefab";
        public const string DataPath = "Assets/_Project/Data/Enemies/BadgerData.asset";
        [MenuItem("ETHEREAN/Tests/Create Badger Combat Scene")]
        public static void Create()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
            if (File.Exists(ScenePath)) return;
            var player = PrefabUtility.LoadPrefabContents(PlayerCharacterAnimationBuilder.PrefabPath);
            try
            {
                if (player.GetComponent<CombatDamageReceiver>() == null) player.AddComponent<CombatDamageReceiver>();
                player.layer = 8;
                var hitbox = new SerializedObject(player.GetComponent<PlayerAttackHitbox>());
                hitbox.FindProperty("targetLayers").intValue |= 1 << 9;
                hitbox.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(player, PlayerCharacterAnimationBuilder.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(player); }
            Directory.CreateDirectory("Assets/_Project/Prefabs/Enemies"); Directory.CreateDirectory("Assets/_Project/Data/Enemies"); AssetDatabase.Refresh();
            var data = AssetDatabase.LoadAssetAtPath<BadgerData>(DataPath);
            if (data == null) { data = ScriptableObject.CreateInstance<BadgerData>(); AssetDatabase.CreateAsset(data, DataPath); }
            var enemy = new GameObject("Badger");
            try
            {
                enemy.layer = 9;
                var body = enemy.AddComponent<Rigidbody2D>(); body.gravityScale = 0; body.freezeRotation = true; body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                var collider = enemy.AddComponent<BoxCollider2D>(); collider.size = new Vector2(0.7f, 0.7f);
                collider.sharedMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/_Project/Data/Player/PlayerMovement.physicsMaterial2D");
                var visual = new GameObject("Square"); visual.transform.SetParent(enemy.transform, false); visual.transform.localScale = new Vector3(0.7f,0.7f,1);
                var renderer = visual.AddComponent<SpriteRenderer>(); renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/MovementSquare.png");
                renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/MovementPlaceholder.mat");
                var brain = enemy.AddComponent<BadgerBrain>();
                Set(brain,"data",data);
                PrefabUtility.SaveAsPrefabAsset(enemy,BadgerPath);
            }
            finally { UnityEngine.Object.DestroyImmediate(enemy); }
            var previous = SceneManager.GetActiveScene(); var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            try
            {
                var cameraObject = new GameObject("Camera"); SceneManager.MoveGameObjectToScene(cameraObject,scene); var camera = cameraObject.AddComponent<Camera>();
                camera.orthographic=true;camera.orthographicSize=5.625f;camera.transform.position=new Vector3(0,2,-10);cameraObject.tag="MainCamera";cameraObject.AddComponent<AudioListener>();
                var floor=new GameObject("Ground");SceneManager.MoveGameObjectToScene(floor,scene);floor.transform.position=new Vector3(0,-0.5f,0);floor.transform.localScale=new Vector3(16,1,1);
                var groundRenderer=floor.AddComponent<SpriteRenderer>();groundRenderer.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/MovementSquare.png");groundRenderer.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/MovementPlaceholder.mat");floor.AddComponent<BoxCollider2D>();
                var actor=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PlayerCharacterAnimationBuilder.PrefabPath),scene);actor.transform.position=new Vector3(-3,0.91f,0);
                var unlock=new SerializedObject(actor.GetComponent<PlayerAttack>());unlock.FindProperty("weaponUnlocked").boolValue=true;unlock.ApplyModifiedPropertiesWithoutUndo();
                var badger=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BadgerPath),scene);badger.transform.position=new Vector3(1,0.36f,0);
                Set(badger.GetComponent<BadgerBrain>(),"target",actor.GetComponent<CombatHealth>());
                EditorSceneManager.SaveScene(scene,ScenePath);
            }
            finally { if(previous.IsValid()&&previous.isLoaded)SceneManager.SetActiveScene(previous);EditorSceneManager.CloseScene(scene,true); }
        }
        private static void Set(UnityEngine.Object target,string field,UnityEngine.Object value)
        {
            var settings=new SerializedObject(target);settings.FindProperty(field).objectReferenceValue=value;settings.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
