using System;
using Etherean.Enemies;
using Etherean.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Etherean.Editor
{
    public static class SpriteSortingSetup
    {
        public static void ConfigurePlayer(SpriteRenderer renderer) => Configure(renderer, true);
        public static void ConfigureEnemy(SpriteRenderer renderer) => Configure(renderer, false);

        private static void Configure(SpriteRenderer renderer, bool player)
        {
            int enemyLayer = EnsureLayer("Enemies", 1542101);
            int playerLayer = EnsureLayer("Player", 1542102);
            renderer.sortingLayerID = player ? playerLayer : enemyLayer;
            renderer.sortingOrder = 0;
        }

        private static int EnsureLayer(string name, int preferredId)
        {
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tags.FindProperty("m_SortingLayers");
            int id = 0;
            for (int i = 0; i < layers.arraySize; i++)
            {
                var layer = layers.GetArrayElementAtIndex(i);
                if (layer.FindPropertyRelative("name").stringValue == name)
                    id = layer.FindPropertyRelative("uniqueID").intValue;
            }
            if (id == 0)
            {
                bool occupied;
                do
                {
                    occupied = false;
                    for (int i = 0; i < layers.arraySize; i++)
                        occupied |= layers.GetArrayElementAtIndex(i).FindPropertyRelative("uniqueID").intValue == preferredId;
                    if (occupied) preferredId++;
                } while (occupied);
                id = preferredId;
                int index = layers.arraySize++;
                var added = layers.GetArrayElementAtIndex(index);
                added.FindPropertyRelative("name").stringValue = name;
                added.FindPropertyRelative("uniqueID").intValue = id;
                added.FindPropertyRelative("locked").boolValue = false;
                tags.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssetIfDirty(tags.targetObject);
            }
            return id;
        }

        [MenuItem("ETHEREAN/Rendering/Apply Character Sorting")]
        public static void Apply()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
            ApplyPrefab(BadgerCombatSceneBuilder.BadgerPath, false);
            ApplyPrefab(PlayerCharacterAnimationBuilder.PrefabPath, true);
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var player in root.GetComponentsInChildren<PlayerCharacterAnimator>(true))
                        ApplyInstance(player.transform, true, scene);
                    foreach (var enemy in root.GetComponentsInChildren<BadgerAnimator>(true))
                        ApplyInstance(enemy.transform, false, scene);
                }
            }
        }

        private static void ApplyPrefab(string path, bool player)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var renderer = root.transform.Find("CharacterVisual").GetComponent<SpriteRenderer>();
                if (player) ConfigurePlayer(renderer); else ConfigureEnemy(renderer);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void ApplyInstance(Transform actor, bool player, Scene scene)
        {
            var visual = actor.Find("CharacterVisual");
            if (visual == null || !visual.TryGetComponent<SpriteRenderer>(out var renderer)) return;
            string layer = player ? "Player" : "Enemies";
            if (renderer.sortingLayerName == layer) return;
            int order = renderer.sortingOrder;
            Undo.RecordObject(renderer, "Apply character sorting");
            if (player) ConfigurePlayer(renderer); else ConfigureEnemy(renderer);
            renderer.sortingOrder = order;
            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            EditorSceneManager.MarkSceneDirty(scene);
        }
    }
}
