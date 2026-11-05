using System;
using System.IO;
using Etherean.Combat;
using Etherean.Enemies;
using Etherean.Persistence;
using Etherean.Player;
using Etherean.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Etherean.Editor
{
    public static class CheckpointBuilder
    {
        public const string CheckpointPath = "Assets/_Project/Prefabs/World/Checkpoint.prefab";
        public const string SpawnPath = "Assets/_Project/Prefabs/Enemies/EnemySpawnPoint.prefab";
        public const string SessionPath = "Assets/_Project/Prefabs/World/CheckpointSession.prefab";
        public const string ScenePath = "Assets/_Project/Scenes/Tests/PlayerCheckpointTest.unity";

        [MenuItem("ETHEREAN/Tests/Create Checkpoint Save Test")]
        public static void Build()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play first.");
            Directory.CreateDirectory(Path.GetDirectoryName(CheckpointPath)); AssetDatabase.Refresh();
            if (!File.Exists(CheckpointPath))
            {
                var root = new GameObject("Checkpoint");
                try
                {
                    var checkpoint = root.AddComponent<Checkpoint>();
                    var collider = root.GetComponent<BoxCollider2D>(); collider.isTrigger = true;
                    collider.size = new Vector2(1.2f, 1.2f); collider.offset = new Vector2(0, .55f);
                    var spawn = new GameObject("SpawnPoint"); spawn.transform.SetParent(root.transform, false); spawn.transform.localPosition = new Vector3(0, .91f, 0);
                    var visual = new GameObject("Visual"); visual.transform.SetParent(root.transform, false);
                    Set(checkpoint, "spawnPoint", spawn.transform);
                    ConfigureChair(root);
                    PrefabUtility.SaveAsPrefabAsset(root, CheckpointPath);
                }
                finally { UnityEngine.Object.DestroyImmediate(root); }
            }
            else UpdatePrefab(CheckpointPath, ConfigureChair);
            if (!File.Exists(SpawnPath))
            {
                var root = new GameObject("EnemySpawnPoint");
                try { root.AddComponent<EnemySpawnPoint>(); PrefabUtility.SaveAsPrefabAsset(root, SpawnPath); }
                finally { UnityEngine.Object.DestroyImmediate(root); }
            }
            var interact = AssetDatabase.LoadAssetAtPath<InputActionReference>("Assets/_Project/Data/Player/PlayerInteractAction.asset");
            if (interact == null)
            {
                var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
                interact = InputActionReference.Create(actions.FindAction("Player/Interact", true));
                AssetDatabase.CreateAsset(interact, "Assets/_Project/Data/Player/PlayerInteractAction.asset");
            }
            bool ConfigureSession(GameObject root)
            {
                var session = root.GetComponent<CheckpointSession>();
                if (new SerializedObject(session).FindProperty("interactActionReference").objectReferenceValue != null) return false;
                Set(session, "interactActionReference", interact);
                return true;
            }
            if (!File.Exists(SessionPath))
            {
                var root = new GameObject("CheckpointSession");
                try { root.AddComponent<CheckpointSession>(); ConfigureSession(root); PrefabUtility.SaveAsPrefabAsset(root, SessionPath); }
                finally { UnityEngine.Object.DestroyImmediate(root); }
            }
            else UpdatePrefab(SessionPath, ConfigureSession);
            var player = PrefabUtility.LoadPrefabContents(PlayerCharacterAnimationBuilder.PrefabPath);
            try
            {
                bool changed = false;
                if (player.GetComponent<PlayerSaveBinding>() == null) { player.AddComponent<PlayerSaveBinding>(); changed = true; }
                if (player.GetComponent<PlayerRespawn>() == null) { player.AddComponent<PlayerRespawn>(); changed = true; }
                if (changed) PrefabUtility.SaveAsPrefabAsset(player, PlayerCharacterAnimationBuilder.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(player); }
            if (!File.Exists(ScenePath)) CreateScene();
        }

        private static bool ConfigureChair(GameObject root)
        {
            var checkpoint = root.GetComponent<Checkpoint>();
            if (new SerializedObject(checkpoint).FindProperty("seatPoint").objectReferenceValue != null) return false;
            var seat = root.transform.Find("SeatPoint");
            if (seat == null)
            {
                seat = new GameObject("SeatPoint").transform;
                seat.SetParent(root.transform, false);
                seat.position = checkpoint.SpawnPosition;
            }
            Set(checkpoint, "seatPoint", seat);
            return true;
        }

        private static void UpdatePrefab(string path, Func<GameObject, bool> update)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try { if (update(root)) PrefabUtility.SaveAsPrefabAsset(root, path); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void CreateScene()
        {
            if (!AssetDatabase.CopyAsset(HealingItemBuilder.ScenePath, ScenePath)) throw new Exception("Healing test scene must exist.");
            var previous = SceneManager.GetActiveScene(); var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                var inventory = Find<PlayerInventory>(scene); var badger = Find<BadgerBrain>(scene); var boar = Find<BoarBrain>(scene);
                CreateSpawn(scene, "BadgerSpawn", badger.transform.position, BadgerCombatSceneBuilder.BadgerPath);
                CreateSpawn(scene, "BoarSpawn", boar.transform.position, BoarCombatBuilder.PrefabPath);
                UnityEngine.Object.DestroyImmediate(badger.gameObject); UnityEngine.Object.DestroyImmediate(boar.gameObject);
                var first = CreateCheckpoint(scene, "checkpoint-a", new Vector3(-4.5f, 0, 0));
                CreateCheckpoint(scene, "checkpoint-b", new Vector3(-7, 0, 0));
                foreach (var root in scene.GetRootGameObjects())
                foreach (var pickup in root.GetComponentsInChildren<HealingPickup>(true))
                {
                    if (pickup.IsMonsterDrop) continue;
                    var persistence = pickup.gameObject.AddComponent<PersistentPickup>();
                    var settings = new SerializedObject(persistence); settings.FindProperty("persistentId").stringValue = Guid.NewGuid().ToString("N"); settings.ApplyModifiedPropertiesWithoutUndo();
                }
                var sessionObject = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(SessionPath), scene);
                var session = sessionObject.GetComponent<CheckpointSession>(); Set(session, "player", inventory); Set(session, "defaultCheckpoint", first);
                var profile = new SerializedObject(session); profile.FindProperty("editorSaveFileName").stringValue = "etherean-checkpoint-test.json"; profile.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.SaveScene(scene);
            }
            finally { if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous); EditorSceneManager.CloseScene(scene, true); }
        }
        private static Checkpoint CreateCheckpoint(Scene scene, string id, Vector3 position)
        {
            var root = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(CheckpointPath), scene);
            root.name = id; root.transform.position = position;
            var checkpoint = root.GetComponent<Checkpoint>(); var settings = new SerializedObject(checkpoint);
            settings.FindProperty("checkpointId").stringValue = id; settings.ApplyModifiedPropertiesWithoutUndo(); return checkpoint;
        }
        private static void CreateSpawn(Scene scene, string name, Vector3 position, string prefab)
        {
            var root = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(SpawnPath), scene);
            root.name = name; root.transform.position = position;
            Set(root.GetComponent<EnemySpawnPoint>(), "enemyPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(prefab));
        }
        private static T Find<T>(Scene scene) where T : Component
        {
            foreach (var root in scene.GetRootGameObjects()) { var value = root.GetComponentInChildren<T>(true); if (value != null) return value; }
            throw new InvalidOperationException(typeof(T).Name + " not found.");
        }
        private static void Set(UnityEngine.Object target, string field, UnityEngine.Object value)
        { var settings = new SerializedObject(target); settings.FindProperty(field).objectReferenceValue = value; settings.ApplyModifiedPropertiesWithoutUndo(); }
    }
}
