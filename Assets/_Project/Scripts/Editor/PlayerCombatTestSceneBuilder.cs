using System;
using System.IO;
using Etherean.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Etherean.Editor
{
    public static class PlayerCombatTestSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Tests/PlayerTimedComboTest.unity";
        public const string GroundDataPath = "Assets/_Project/Data/Player/GroundAttack.asset";
        public const string AirDataPath = "Assets/_Project/Data/Player/AirAttack.asset";
        private const string PlayerPath = "Assets/_Project/Prefabs/Player/Player.prefab";
        private const string SourceScene = "Assets/_Project/Scenes/Tests/PlayerGreatswordComboTest.unity";
        private const string OldMotions = "Assets/_Project/Data/Player/GreatswordMotions";

        [MenuItem("ETHEREAN/Tests/Convert To Timed Character Attacks")]
        public static void Create()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
            if (File.Exists(ScenePath)) throw new InvalidOperationException("Timed attack scene already exists; open it instead.");
            for (int i = 0; i < 3; i++)
            {
                Configure(i == 0 ? GroundDataPath : "Assets/_Project/Data/Player/GroundAttack" + (i + 1) + ".asset", i);
                Configure(i == 0 ? AirDataPath : "Assets/_Project/Data/Player/AirAttack" + (i + 1) + ".asset", i);
            }
            ApplyComboRecoveryTiming();
            var contents = PrefabUtility.LoadPrefabContents(PlayerPath);
            try
            {
                foreach (var old in contents.GetComponentsInChildren<MonoBehaviour>(true))
                    if (old != null && old.GetType().Name == "PlayerWeaponMotion") UnityEngine.Object.DestroyImmediate(old.gameObject);
                var origin = contents.transform.Find("AttackEffectOrigin");
                if (origin == null)
                {
                    origin = new GameObject("AttackEffectOrigin").transform;
                    origin.SetParent(contents.transform, false);
                }
                Ref(contents.GetComponent<PlayerAttackEffects>(), "effectOrigin", origin);
                var adapter = contents.GetComponent<PlayerAttackAnimator>();
                if (adapter == null) adapter = contents.AddComponent<PlayerAttackAnimator>();
                Ref(adapter, "attack", contents.GetComponent<PlayerAttack>());
                var visual = contents.transform.Find("Visual");
                if (visual != null && visual.GetComponent<Animator>() != null)
                    Ref(adapter, "characterAnimator", visual.GetComponent<Animator>());
                PrefabUtility.SaveAsPrefabAsset(contents, PlayerPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            if (!AssetDatabase.CopyAsset(SourceScene, ScenePath)) throw new IOException("Timed scene copy failed.");
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try { EditorSceneManager.SaveScene(scene); }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
            DeleteOwned("Assets/_Project/Prefabs/Player/Greatsword.prefab");
            string[] clips = { "Carry", "Hit1_Lift", "Hit1_Down", "Hit1_Recover", "Hit2_Brace", "Hit2_Up", "Hit2_Recover", "Hit3_DrawBack", "Hit3_Across", "Hit3_Recover" };
            foreach (var name in clips) DeleteOwned(OldMotions + "/" + name + ".anim");
            if (Directory.Exists(OldMotions) && Directory.GetFileSystemEntries(OldMotions).Length == 0) AssetDatabase.DeleteAsset(OldMotions);
            DeleteOwned("Assets/_Project/Scripts/Runtime/Player/Combat/PlayerWeaponMotion.cs");
        }
        [MenuItem("ETHEREAN/Combat/Apply Combo Recovery Timing")]
        public static void ApplyComboRecoveryTiming()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode before applying saved tuning.");
            foreach (var prefix in new[] { "GroundAttack", "AirAttack" })
            {
                for (int i = 0; i < 2; i++)
                {
                    string path = "Assets/_Project/Data/Player/" + prefix + (i == 0 ? "" : "2") + ".asset";
                    var data = AssetDatabase.LoadAssetAtPath<PlayerAttackData>(path);
                    if (data == null) throw new InvalidOperationException("Attack data missing: " + path);
                    var settings = new SerializedObject(data);
                    settings.FindProperty("recoveryTime").floatValue = i == 0 ? 0.38f : 0.65f;
                    settings.FindProperty("comboLinkStart").floatValue = i == 0 ? 0.6f : 0.8f;
                    settings.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(data);
                    AssetDatabase.SaveAssetIfDirty(data);
                }
            }
        }
        private static void Configure(string path, int index)
        {
            var data = AssetDatabase.LoadAssetAtPath<PlayerAttackData>(path);
            if (data == null) throw new InvalidOperationException("Attack data missing: " + path);
            var settings = new SerializedObject(data);
            settings.FindProperty("hitboxOffset").vector2Value = new[] { new Vector2(1f, 0.6f), new Vector2(0.95f, 0.8f), new Vector2(0f, 0.1f) }[index] * PlayerScaleReviewBuilder.ReferenceRatio;
            settings.FindProperty("hitboxSize").vector2Value = new[] { new Vector2(1.65f, 2.5f), new Vector2(1.8f, 2.4f), new Vector2(4.2f, 1.2f) }[index] * PlayerScaleReviewBuilder.ReferenceRatio;
            settings.FindProperty("hitboxAngle").floatValue = 0f;
            settings.FindProperty("animationStateName").stringValue = "Base Layer.Attack" + (index + 1);
            settings.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssetIfDirty(data);
        }
        private static void Ref(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            var settings = new SerializedObject(target);
            settings.FindProperty(field).objectReferenceValue = value;
            settings.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void DeleteOwned(string relative)
        {
            string root = Path.GetFullPath("Assets/_Project") + Path.DirectorySeparatorChar;
            string absolute = Path.GetFullPath(relative);
            if (!absolute.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new IOException("Invalid cleanup path.");
            if (File.Exists(relative) && !AssetDatabase.DeleteAsset(relative)) throw new IOException("Asset deletion failed: " + relative);
        }
    }
}
