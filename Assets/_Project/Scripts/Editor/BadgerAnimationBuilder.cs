using System;
using System.IO;
using Etherean.Combat;
using Etherean.Enemies;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
namespace Etherean.Editor
{
    public static class BadgerAnimationBuilder
    {
        public const string SpriteFolder = "Assets/_Project/Art/Sprites/Badger";
        public const string AnimationFolder = "Assets/_Project/Art/Animations/Badger";
        [MenuItem("ETHEREAN/Enemies/Connect Badger Animations")]
        public static void Build() => Build(new[] { "Idle", "Walk", "Attack", "Death" }, true);

        [MenuItem("ETHEREAN/Enemies/Update Badger Walk And Death")]
        public static void RefreshWalkAndDeath() => Build(new[] { "Walk", "Death" }, false);

        private static void Build(string[] clips, bool connectPrefab)
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play first.");
            var data = AssetDatabase.LoadAssetAtPath<BadgerData>(BadgerCombatSceneBuilder.DataPath);
            Directory.CreateDirectory(AnimationFolder); AssetDatabase.Refresh();
            string controllerPath = AnimationFolder + "/Badger.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            foreach (string name in clips)
            {
                PadCanvas(name);
                for (int i = 1; i <= 16; i++)
                {
                    string path = SpriteFolder + "/" + name + "/image_" + i + ".png";
                    AssetDatabase.ImportAsset(path);
                    var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                    importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 32; importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
                    importer.textureCompression = TextureImporterCompression.Uncompressed; importer.npotScale = TextureImporterNPOTScale.None;
                    importer.alphaIsTransparency = true; importer.wrapMode = TextureWrapMode.Clamp;
                    var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
                    settings.spriteAlignment = (int)SpriteAlignment.Custom;
                    // One calibrated anchor per clip preserves authored motion within its frames.
                    settings.spritePivot = name == "Walk" ? new Vector2(46f / 96, 4f / 96) :
                        name == "Death" ? new Vector2(46f / 96, 21f / 96) : new Vector2(0.5f, 21f / 96);
                    settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings); importer.SaveAndReimport();
                }
                string clipPath = AnimationFolder + "/" + name + ".anim";
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, clipPath); }
                bool loop = name == "Idle" || name == "Walk";
                float duration = name == "Attack" ? data.WindupTime + data.ActiveTime + data.RecoveryTime : 16f / (name == "Idle" ? 8 : 12);
                clip.frameRate = name == "Attack" ? 60 : 16 / duration;
                var keys = new ObjectReferenceKeyframe[17];
                for (int i = 0; i <= 16; i++) keys[i] = new ObjectReferenceKeyframe { time = name == "Attack" ?
                    i < 6 ? i * data.WindupTime / 6 : i < 8 ? data.WindupTime + (i - 6) * data.ActiveTime / 2 :
                    data.WindupTime + data.ActiveTime + (i - 8) * data.RecoveryTime / 8 : i * duration / 16,
                    value = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + "/" + name + "/image_" + (i == 16 ? loop ? 1 : 16 : i + 1) + ".png") };
                AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
                var clipSettings = AnimationUtility.GetAnimationClipSettings(clip); clipSettings.loopTime = loop; clipSettings.stopTime = duration; AnimationUtility.SetAnimationClipSettings(clip, clipSettings);
                var machine = controller.layers[0].stateMachine; AnimatorState state = null;
                foreach (var child in machine.states) if (child.state.name == name) state = child.state;
                if (state == null) state = machine.AddState(name); state.motion = clip;
                if (name == "Idle") machine.defaultState = state;
                EditorUtility.SetDirty(clip); AssetDatabase.SaveAssetIfDirty(clip);
            }
            EditorUtility.SetDirty(controller); AssetDatabase.SaveAssetIfDirty(controller);
            if (!connectPrefab) return;
            var root = PrefabUtility.LoadPrefabContents(BadgerCombatSceneBuilder.BadgerPath);
            try
            {
                if (root.GetComponent<EnemyRoaming>() == null) root.AddComponent<EnemyRoaming>();
                var square = root.transform.Find("Square"); if (square != null) square.GetComponent<SpriteRenderer>().enabled = false;
                var visual = root.transform.Find("CharacterVisual");
                if (visual == null) { visual = new GameObject("CharacterVisual").transform; visual.SetParent(root.transform, false); }
                visual.localPosition = new Vector3(0, -root.GetComponent<BoxCollider2D>().size.y / 2, 0); visual.localScale = Vector3.one;
                var renderer = visual.GetComponent<SpriteRenderer>(); if (renderer == null) renderer = visual.gameObject.AddComponent<SpriteRenderer>();
                renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + "/Idle/image_1.png");
                renderer.sharedMaterial = square.GetComponent<SpriteRenderer>().sharedMaterial;
                SpriteSortingSetup.ConfigureEnemy(renderer);
                var animator = visual.GetComponent<Animator>(); if (animator == null) animator = visual.gameObject.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller; animator.applyRootMotion = false; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                var adapter = root.GetComponent<BadgerAnimator>(); if (adapter == null) adapter = root.AddComponent<BadgerAnimator>();
                Set(adapter,"brain",root.GetComponent<BadgerBrain>()); Set(adapter,"motor",root.GetComponent<BadgerMotor>());
                Set(adapter,"health",root.GetComponent<CombatHealth>()); Set(adapter,"reaction",root.GetComponent<CombatReaction>());
                Set(adapter,"characterAnimator",animator); Set(adapter,"characterRenderer",renderer);
                renderer.flipX = new SerializedObject(adapter).FindProperty("invertSpriteFacing").boolValue;
                PrefabUtility.SaveAsPrefabAsset(root,BadgerCombatSceneBuilder.BadgerPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static void Set(UnityEngine.Object target,string field,UnityEngine.Object value)
        { var settings=new SerializedObject(target);settings.FindProperty(field).objectReferenceValue=value;settings.ApplyModifiedPropertiesWithoutUndo(); }
        private static void PadCanvas(string name)
        {
            for (int i = 1; i <= 16; i++)
            {
                string path = SpriteFolder + "/" + name + "/image_" + i + ".png";
                var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                Texture2D padded = null;
                try
                {
                    if (!source.LoadImage(File.ReadAllBytes(path))) throw new InvalidOperationException("Invalid PNG: " + path);
                    if (source.width == 96 && source.height == 96) continue;
                    if (source.width != 64 || source.height != 64) throw new InvalidOperationException("Unexpected canvas: " + path);
                    int bottom = name == "Walk" ? 15 : 16;
                    padded = new Texture2D(96, 96, TextureFormat.RGBA32, false);
                    padded.SetPixels32(new Color32[96 * 96]);
                    padded.SetPixels32(16, bottom, 64, 64, source.GetPixels32());
                    padded.Apply();
                    File.WriteAllBytes(path, padded.EncodeToPNG());
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(source);
                    if (padded != null) UnityEngine.Object.DestroyImmediate(padded);
                }
            }
        }
    }
}
