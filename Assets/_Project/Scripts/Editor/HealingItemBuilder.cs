using System;
using System.IO;
using Etherean.Combat;
using Etherean.Enemies;
using Etherean.Player;
using Etherean.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
namespace Etherean.Editor
{
    public static class HealingItemBuilder
    {
        public const string DataPath = "Assets/_Project/Data/Player/HealingItem.asset";
        public const string ActionPath = "Assets/_Project/Data/Player/PlayerHealingAction.asset";
        public const string PickupPath = "Assets/_Project/Prefabs/Items/HealingPickup.prefab";
        public const string ScenePath = "Assets/_Project/Scenes/Tests/PlayerHealingTest.unity";
        public const string BadgerDropPath = "Assets/_Project/Data/Enemies/BadgerHealingDrop.asset";
        public const string BoarDropPath = "Assets/_Project/Data/Enemies/BoarHealingDrop.asset";
        [MenuItem("ETHEREAN/Tests/Create Healing Item Test")]
        public static void Build()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play first.");
            Directory.CreateDirectory(Path.GetDirectoryName(PickupPath)); AssetDatabase.Refresh();
            var data = AssetDatabase.LoadAssetAtPath<HealingItemData>(DataPath);
            if (data == null) { data = ScriptableObject.CreateInstance<HealingItemData>(); AssetDatabase.CreateAsset(data, DataPath); }
            var action = CreateInput();
            if (!File.Exists(PickupPath)) CreatePickup();
            ConnectPlayer(data, action);
            ConnectDrop(BadgerCombatSceneBuilder.BadgerPath, CreateDrop(BadgerDropPath, .35f));
            ConnectDrop(BoarCombatBuilder.PrefabPath, CreateDrop(BoarDropPath, .5f));
            if (!File.Exists(ScenePath)) CreateScene();
        }
        private static InputActionReference CreateInput()
        {
            const string path = "Assets/InputSystem_Actions.inputactions";
            var copy = InputActionAsset.FromJson(File.ReadAllText(path));
            try
            {
                if (copy.FindAction("Player/Heal") == null)
                {
                    var heal = copy.FindActionMap("Player", true).AddAction("Heal", InputActionType.Button);
                    heal.AddBinding("<Keyboard>/r", groups: "Keyboard&Mouse");
                    heal.AddBinding("<Gamepad>/dpad/up", groups: "Gamepad");
                    File.WriteAllText(path, copy.ToJson()); AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(copy); }
            var live = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
            var reference = AssetDatabase.LoadAssetAtPath<InputActionReference>(ActionPath);
            if (reference == null)
            {
                reference = InputActionReference.Create(live.FindAction("Player/Heal", true)); AssetDatabase.CreateAsset(reference, ActionPath);
            }
            return reference;
        }
        private static void ConnectPlayer(HealingItemData data, InputActionReference action)
        {
            var player = PrefabUtility.LoadPrefabContents(PlayerCharacterAnimationBuilder.PrefabPath);
            try
            {
                var healing = player.GetComponent<PlayerHealing>(); if (healing == null) healing = player.AddComponent<PlayerHealing>();
                Set(healing, "data", data); Set(player.GetComponent<PlayerInventory>(), "data", data);
                Set(player.GetComponent<NewInputHealingReader>(), "healActionReference", action);
                PrefabUtility.SaveAsPrefabAsset(player, PlayerCharacterAnimationBuilder.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(player); }
        }
        private static void CreatePickup()
        {
            var item = new GameObject("HealingPickup");
            try
            {
                var collider = item.AddComponent<BoxCollider2D>(); collider.isTrigger = true; collider.size = new Vector2(.3f, .3f);
                item.AddComponent<HealingPickup>();
                var visual = new GameObject("Square"); visual.transform.SetParent(item.transform, false); visual.transform.localScale = new Vector3(.25f, .25f, 1);
                var renderer = visual.AddComponent<SpriteRenderer>(); renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/MovementSquare.png");
                renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/MovementPlaceholder.mat"); renderer.sortingOrder = 1;
                PrefabUtility.SaveAsPrefabAsset(item, PickupPath);
            }
            finally { UnityEngine.Object.DestroyImmediate(item); }
        }
        private static HealingDropData CreateDrop(string path, float chance)
        {
            var data = AssetDatabase.LoadAssetAtPath<HealingDropData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<HealingDropData>(); AssetDatabase.CreateAsset(data, path);
                var settings = new SerializedObject(data);
                settings.FindProperty("dropChance").floatValue = chance;
                settings.FindProperty("pickupPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(PickupPath).GetComponent<HealingPickup>();
                settings.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.SaveAssetIfDirty(data);
            }
            return data;
        }
        private static void ConnectDrop(string path, HealingDropData data)
        {
            var enemy = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var drop = enemy.GetComponent<EnemyHealingDrop>(); if (drop == null) drop = enemy.AddComponent<EnemyHealingDrop>();
                Set(drop, "data", data); PrefabUtility.SaveAsPrefabAsset(enemy, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(enemy); }
        }
        private static void CreateScene()
        {
            var previous = SceneManager.GetActiveScene(); var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                var cameraObject = new GameObject("Camera"); SceneManager.MoveGameObjectToScene(cameraObject, scene);
                var camera = cameraObject.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 4.5f;
                camera.transform.position = new Vector3(0, 2, -10); cameraObject.tag = "MainCamera"; cameraObject.AddComponent<AudioListener>();
                var ground = new GameObject("Ground"); SceneManager.MoveGameObjectToScene(ground, scene);
                ground.transform.position = new Vector3(0, -.5f, 0); ground.transform.localScale = new Vector3(22, 1, 1);
                var renderer = ground.AddComponent<SpriteRenderer>(); renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/MovementSquare.png");
                renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/MovementPlaceholder.mat"); ground.AddComponent<BoxCollider2D>();
                var player = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PlayerCharacterAnimationBuilder.PrefabPath), scene);
                player.transform.position = new Vector3(-4.5f, .91f, 0);
                var unlock = new SerializedObject(player.GetComponent<PlayerAttack>()); unlock.FindProperty("weaponUnlocked").boolValue = true; unlock.ApplyModifiedPropertiesWithoutUndo();
                for (int i = 0; i < 4; i++)
                {
                    var item = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PickupPath), scene);
                    item.transform.position = new Vector3(-3.5f + i * .7f, .2f, 0);
                }
                var badger = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BadgerCombatSceneBuilder.BadgerPath), scene);
                badger.transform.position = new Vector3(1.5f, .36f, 0); Set(badger.GetComponent<BadgerBrain>(), "target", player.GetComponent<CombatHealth>());
                var boar = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BoarCombatBuilder.PrefabPath), scene);
                boar.transform.position = new Vector3(6.5f, .46f, 0); Set(boar.GetComponent<BoarBrain>(), "target", player.GetComponent<CombatHealth>());
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            finally { if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous); EditorSceneManager.CloseScene(scene, true); }
        }
        private static void Set(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            var settings = new SerializedObject(target); settings.FindProperty(field).objectReferenceValue = value; settings.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
