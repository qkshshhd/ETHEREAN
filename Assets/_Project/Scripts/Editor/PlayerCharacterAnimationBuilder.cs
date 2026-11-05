using System;
using System.IO;
using Etherean.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Etherean.Editor
{
    public static class PlayerCharacterAnimationBuilder
    {
        public const string SpriteFolder = "Assets/_Project/Art/Sprites/Player";
        public const string AnimationFolder = "Assets/_Project/Art/Animations/Player";
        public const string ControllerPath = AnimationFolder + "/PlayerCharacter.controller";
        public const string PrefabPath = "Assets/_Project/Prefabs/Player/Player.prefab";

        [MenuItem("ETHEREAN/Player/Connect Character Animations")]
        public static void Build()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
            foreach (string file in Directory.GetFiles(SpriteFolder, "*.png", SearchOption.AllDirectories))
            {
                string path = file.Replace('\\', '/');
                AssetDatabase.ImportAsset(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 32f;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.alphaIsTransparency = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = new Vector2(0.5f, 1f / 32f);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }
            Directory.CreateDirectory(AnimationFolder);
            AssetDatabase.Refresh();
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            var machine = controller.layers[0].stateMachine;
            foreach (string name in new[] { "Idle", "Walk", "Run" })
            {
                string path = AnimationFolder + "/" + name + ".anim";
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, path); }
                clip.frameRate = name == "Idle" ? 8f : name == "Walk" ? 12f : 16f;
                var frames = new ObjectReferenceKeyframe[17];
                for (int i = 0; i <= 16; i++)
                    frames[i] = new ObjectReferenceKeyframe { time = i / clip.frameRate,
                        value = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + "/" + name + "/image_" + (i % 16 + 1) + ".png") };
                AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), frames);
                var clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
                clipSettings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, clipSettings);
                AnimatorState state = null;
                foreach (var child in machine.states) if (child.state.name == name) state = child.state;
                if (state == null) state = machine.AddState(name);
                state.motion = clip;
                if (name == "Idle") machine.defaultState = state;
                EditorUtility.SetDirty(clip);
                AssetDatabase.SaveAssetIfDirty(clip);
            }
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssetIfDirty(controller);
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var old = root.transform.Find("Visual");
                if (old != null && old.TryGetComponent<SpriteRenderer>(out var oldRenderer)) oldRenderer.enabled = false;
                var visual = root.transform.Find("CharacterVisual");
                if (visual == null) { visual = new GameObject("CharacterVisual").transform; visual.SetParent(root.transform, false); }
                var collider = root.GetComponent<CapsuleCollider2D>();
                visual.localPosition = new Vector3(collider.offset.x, collider.offset.y - collider.size.y * 0.5f, 0f);
                visual.localScale = Vector3.one;
                var renderer = visual.GetComponent<SpriteRenderer>();
                if (renderer == null) renderer = visual.gameObject.AddComponent<SpriteRenderer>();
                renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + "/CharacterBase.png");
                renderer.color = Color.white;
                if (old != null && old.TryGetComponent<SpriteRenderer>(out var source))
                {
                    renderer.sharedMaterial = source.sharedMaterial;
                }
                SpriteSortingSetup.ConfigurePlayer(renderer);
                var animator = visual.GetComponent<Animator>();
                if (animator == null) animator = visual.gameObject.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                var presenter = root.GetComponent<PlayerCharacterAnimator>();
                if (presenter == null) presenter = root.AddComponent<PlayerCharacterAnimator>();
                Set(presenter, "movement", root.GetComponent<PlayerMovement>());
                Set(presenter, "locomotion", root.GetComponent<PlayerLocomotion>());
                Set(presenter, "attack", root.GetComponent<PlayerAttack>());
                Set(presenter, "attackAnimator", root.GetComponent<PlayerAttackAnimator>());
                Set(presenter, "characterAnimator", animator);
                Set(presenter, "characterRenderer", renderer);
                var adapter = root.GetComponent<PlayerAttackAnimator>();
                Set(adapter, "characterAnimator", animator);
                var adapterSettings = new SerializedObject(adapter);
                adapterSettings.FindProperty("returnStateName").stringValue = "Base Layer.Idle";
                adapterSettings.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void Set(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
