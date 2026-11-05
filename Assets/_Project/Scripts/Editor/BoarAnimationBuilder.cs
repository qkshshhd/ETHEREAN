using System;
using Etherean.Enemies;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
namespace Etherean.Editor
{
    public static class BoarAnimationBuilder
    {
        public const string SpriteFolder = "Assets/_Project/Art/Sprites/Boar";
        public const string AnimationFolder = "Assets/_Project/Art/Animations/Boar";
        [MenuItem("ETHEREAN/Enemies/Connect Boar Animations")]
        public static void Build()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play first.");
            BoarCombatBuilder.Build();
            foreach (string name in new[] { "Idle", "Walk", "Run", "Death" })
            {
                for (int i = 1; i <= 16; i++)
                {
                    string path = SpriteFolder + "/" + name + "/image_" + i + ".png";
                    AssetDatabase.ImportAsset(path);
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null) throw new InvalidOperationException("Missing supplied frame: " + path);
                    importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 32; importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
                    importer.textureCompression = TextureImporterCompression.Uncompressed; importer.npotScale = TextureImporterNPOTScale.None;
                    importer.alphaIsTransparency = true; importer.wrapMode = TextureWrapMode.Clamp;
                    var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
                    settings.spriteAlignment = (int)SpriteAlignment.Custom;
                    // Keep the supplied 64px canvases unchanged and preserve authored 0-2px foot motion.
                    settings.spritePivot = new Vector2(.5f, 12f / 64);
                    settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings); importer.SaveAndReimport();
                }
                string clipPath = AnimationFolder + "/" + name + ".anim";
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, clipPath); }
                clip.frameRate = name == "Idle" ? 8 : name == "Run" ? 16 : 12;
                bool loop = name != "Death";
                var keys = new ObjectReferenceKeyframe[17];
                for (int i = 0; i <= 16; i++) keys[i] = new ObjectReferenceKeyframe
                {
                    time = i / clip.frameRate,
                    value = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + "/" + name + "/image_" + (i == 16 ? loop ? 1 : 16 : i + 1) + ".png")
                };
                AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
                var clipSettings = AnimationUtility.GetAnimationClipSettings(clip); clipSettings.loopTime = loop; clipSettings.stopTime = 16 / clip.frameRate;
                AnimationUtility.SetAnimationClipSettings(clip, clipSettings); EditorUtility.SetDirty(clip); AssetDatabase.SaveAssetIfDirty(clip);
            }
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(BoarCombatBuilder.ControllerPath);
            foreach (var child in controller.layers[0].stateMachine.states)
            {
                string name = child.state.name == "Charge" ? "Run" : child.state.name == "Walk" ? "Walk" : child.state.name == "Death" ? "Death" : "Idle";
                child.state.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(AnimationFolder + "/" + name + ".anim");
            }
            EditorUtility.SetDirty(controller); AssetDatabase.SaveAssetIfDirty(controller);
            var root = PrefabUtility.LoadPrefabContents(BoarCombatBuilder.PrefabPath);
            try
            {
                var square = root.transform.Find("Square").GetComponent<SpriteRenderer>(); square.enabled = false;
                var renderer = root.transform.Find("CharacterVisual").GetComponent<SpriteRenderer>();
                renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + "/Idle/image_1.png"); renderer.enabled = true;
                SpriteSortingSetup.ConfigureEnemy(renderer);
                var presenter = new SerializedObject(root.GetComponent<BoarAnimator>());
                presenter.FindProperty("invertSpriteFacing").boolValue = false; presenter.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, BoarCombatBuilder.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
