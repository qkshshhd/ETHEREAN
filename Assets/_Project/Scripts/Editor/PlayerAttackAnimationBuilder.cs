using System;
using Etherean.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Etherean.Editor
{
    public static class PlayerAttackAnimationBuilder
    {
        public const string SpriteFolder = PlayerCharacterAnimationBuilder.SpriteFolder + "/Attack1";
        public const string ClipPath = PlayerCharacterAnimationBuilder.AnimationFolder + "/Attack1.anim";

        [MenuItem("ETHEREAN/Player/Connect First Attack Animation")]
        public static void Build()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(PlayerCharacterAnimationBuilder.ControllerPath);
            var attack = AssetDatabase.LoadAssetAtPath<PlayerAttackData>("Assets/_Project/Data/Player/GroundAttack.asset");
            if (controller == null || attack == null) throw new InvalidOperationException("Existing character controller and first attack data are required.");
            var sprites = new Sprite[3];
            for (int i = 0; i < sprites.Length; i++)
            {
                string path = SpriteFolder + "/image_" + (i + 1) + ".png";
                AssetDatabase.ImportAsset(path);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) throw new InvalidOperationException("Missing attack image: " + path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 32;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.alphaIsTransparency = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = new Vector2(.5f, 1f / 32);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings); importer.SaveAndReimport();
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipPath);
            if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, ClipPath); }
            clip.frameRate = 60;
            var keys = new[]
            {
                new ObjectReferenceKeyframe { time = 0, value = sprites[0] },
                new ObjectReferenceKeyframe { time = attack.WindupTime * .75f, value = sprites[1] },
                new ObjectReferenceKeyframe { time = attack.WindupTime + attack.ActiveTime * .5f, value = sprites[2] },
                new ObjectReferenceKeyframe { time = attack.TotalTime, value = sprites[2] }
            };
            AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
            var clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
            clipSettings.loopTime = false; clipSettings.stopTime = attack.TotalTime;
            AnimationUtility.SetAnimationClipSettings(clip, clipSettings);
            var machine = controller.layers[0].stateMachine; AnimatorState state = null;
            foreach (var child in machine.states) if (child.state.name == "Attack1") state = child.state;
            if (state == null) state = machine.AddState("Attack1");
            state.motion = clip;
            EditorUtility.SetDirty(clip); AssetDatabase.SaveAssetIfDirty(clip);
            EditorUtility.SetDirty(controller); AssetDatabase.SaveAssetIfDirty(controller);
        }
    }
}
