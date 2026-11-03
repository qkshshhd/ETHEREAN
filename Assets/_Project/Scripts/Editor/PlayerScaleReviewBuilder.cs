using System;
using System.IO;
using Etherean.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Etherean.Editor
{
    public static class PlayerScaleReviewBuilder
    {
        public const float ReferenceHeight = 30f / 32f;
        public const float ReferenceRatio = ReferenceHeight / 1.8f;
        public const string ScenePath = "Assets/_Project/Scenes/Tests/PlayerCurrentScaleTest.unity";

        [MenuItem("ETHEREAN/Player/Apply Current Character Scale")]
        public static void Apply()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
            var root = PrefabUtility.LoadPrefabContents(PlayerCharacterAnimationBuilder.PrefabPath);
            try
            {
                var collider = root.GetComponent<CapsuleCollider2D>();
                if (Mathf.Abs(collider.size.y - ReferenceHeight) < 0.0001f) { CreateReviewScene(); return; }
                if (Mathf.Abs(collider.size.y - 1.8f) > 0.0001f)
                    throw new InvalidOperationException("Unexpected collider height. Review current scale before migration.");
                Scale(root.GetComponent<PlayerMovement>(), ReferenceRatio, "horizontalMoveSpeed", "riseGravity", "fallGravity", "maxFallSpeed");
                Scale(root.GetComponent<PlayerJump>(), ReferenceRatio, "jumpSpeed");
                Scale(root.GetComponent<PlayerWallJump>(), ReferenceRatio, "wallJumpHorizontalSpeed", "wallJumpVerticalSpeed");
                Scale(root.GetComponent<PlayerWallSlide>(), ReferenceRatio, "wallSlideSpeed");
                Scale(root.GetComponent<PlayerGroundSensor>(), ReferenceRatio, "groundCheckDistance");
                Scale(root.GetComponent<PlayerWallSensor>(), ReferenceRatio, "wallCheckDistance");
                Scale(root.GetComponent<PlayerCharacterAnimator>(), ReferenceRatio, "idleSpeedThreshold", "runSpeedThreshold");
                collider.size *= ReferenceRatio;
                collider.offset *= ReferenceRatio;
                var visual = root.transform.Find("CharacterVisual");
                visual.localPosition = new Vector3(collider.offset.x, collider.offset.y - collider.size.y * 0.5f, 0f);
                visual.localScale = Vector3.one;
                var origin = root.transform.Find("AttackEffectOrigin");
                if (origin != null) origin.localPosition *= ReferenceRatio;
                foreach (string prefix in new[] { "GroundAttack", "AirAttack" })
                    for (int i = 0; i < 3; i++)
                    {
                        string path = "Assets/_Project/Data/Player/" + prefix + (i == 0 ? "" : (i + 1).ToString()) + ".asset";
                        var data = AssetDatabase.LoadAssetAtPath<PlayerAttackData>(path);
                        if (data == null) throw new InvalidOperationException("Missing attack data: " + path);
                        var settings = new SerializedObject(data);
                        foreach (string field in new[] { "hitboxOffset", "hitboxSize", "knockback" })
                            settings.FindProperty(field).vector2Value *= ReferenceRatio;
                        settings.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(data);
                        AssetDatabase.SaveAssetIfDirty(data);
                    }
                PrefabUtility.SaveAsPrefabAsset(root, PlayerCharacterAnimationBuilder.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            CreateReviewScene();
        }

        private static void Scale(UnityEngine.Object target, float ratio, params string[] fields)
        {
            var settings = new SerializedObject(target);
            foreach (string field in fields) settings.FindProperty(field).floatValue *= ratio;
            settings.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateReviewScene()
        {
            if (File.Exists(ScenePath)) return;
            if (!AssetDatabase.CopyAsset(PlayerCombatTestSceneBuilder.ScenePath, ScenePath))
                throw new IOException("Scale review scene copy failed.");
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    var position = root.transform.position;
                    root.transform.position = new Vector3(position.x * ReferenceRatio, position.y * ReferenceRatio, position.z);
                    if (root.GetComponent<PlayerMovement>() == null && root.GetComponent<Camera>() == null)
                    {
                        var scale = root.transform.localScale;
                        root.transform.localScale = new Vector3(scale.x * ReferenceRatio, scale.y * ReferenceRatio, scale.z);
                    }
                }
                EditorSceneManager.SaveScene(scene);
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
