using System;
using System.IO;
using DG.Tweening;
using Etherean.Player;
using UnityEditor;
using UnityEngine;

namespace Etherean.Editor
{
    public static class PlayerSwordMotionBuilder
    {
        public const string SpritePath = "Assets/_Project/Art/Sprites/Weapons/Greatsword.png";
        public const string PrefabPath = "Assets/_Project/Prefabs/Weapons/Greatsword.prefab";
        public const string DataPath = "Assets/_Project/Data/Player/PlayerSwordMotion.asset";

        [MenuItem("ETHEREAN/Player/Connect Greatsword Tween")]
        public static void Build()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            AssetDatabase.Refresh();
            var importer = (TextureImporter)AssetImporter.GetAtPath(SpritePath);
            if (importer == null) throw new InvalidOperationException("Import the provided sword PNG first.");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.alphaIsTransparency = true;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(16.5f / 32, 54f / 64);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings); importer.SaveAndReimport();

            var data = AssetDatabase.LoadAssetAtPath<PlayerSwordMotionData>(DataPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<PlayerSwordMotionData>();
                AssetDatabase.CreateAsset(data, DataPath);
                AuthorMotion(data);
            }
            var player = PrefabUtility.LoadPrefabContents(PlayerCharacterAnimationBuilder.PrefabPath);
            try
            {
                var body = player.transform.Find("CharacterVisual").GetComponent<SpriteRenderer>();
                var reference = body.sprite;
                var idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(PlayerCharacterAnimationBuilder.AnimationFolder + "/Idle.anim");
                if (idle != null)
                {
                    var bindings = AnimationUtility.GetObjectReferenceCurveBindings(idle);
                    if (bindings.Length > 0)
                    {
                        var frames = AnimationUtility.GetObjectReferenceCurve(idle, bindings[0]);
                        if (frames.Length > 0 && frames[0].value is Sprite first) reference = first;
                    }
                }
                float height = VisibleHeight(AssetDatabase.GetAssetPath(reference), reference.pixelsPerUnit);
                float swordHeight = VisibleHeight(SpritePath, 32);
                SaveSwordPrefab(body, height * data.LengthRatio / swordHeight, data.Carry.OrderOffset);
                var facing = Child(body.transform, "WeaponFacing");
                facing.localPosition = Vector3.zero; facing.localRotation = Quaternion.identity; facing.localScale = Vector3.one;
                var socket = Child(facing, "WeaponSocket");
                socket.localPosition = new Vector3(height * (0.1f / 0.9375f), height * (0.44f / 0.9375f), 0);
                socket.localRotation = Quaternion.identity; socket.localScale = Vector3.one;
                var weapon = socket.Find("Greatsword");
                if (weapon == null)
                    weapon = ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), socket)).transform;
                float scale = height / 0.9375f;
                weapon.localPosition = data.Carry.Position * scale;
                weapon.localRotation = Quaternion.Euler(data.Carry.Rotation);
                weapon.localScale = Vector3.one;
                var motion = player.GetComponent<PlayerSwordMotion>();
                if (motion == null) motion = player.AddComponent<PlayerSwordMotion>();
                var serialized = new SerializedObject(motion);
                Set(serialized, "attack", player.GetComponent<PlayerAttack>());
                Set(serialized, "motionData", data);
                Set(serialized, "facingRoot", facing);
                Set(serialized, "motionRoot", weapon);
                Set(serialized, "characterRenderer", body);
                Set(serialized, "swordRenderer", weapon.GetComponentInChildren<SpriteRenderer>());
                serialized.FindProperty("motionScale").floatValue = scale;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(player, PlayerCharacterAnimationBuilder.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(player); }
        }

        private static void SaveSwordPrefab(SpriteRenderer body, float scale, int order)
        {
            bool exists = File.Exists(PrefabPath);
            var root = exists ? PrefabUtility.LoadPrefabContents(PrefabPath) : new GameObject("Greatsword");
            try
            {
                root.layer = 8;
                var visual = Child(root.transform, "BladeSprite");
                visual.gameObject.layer = 8;
                visual.localPosition = Vector3.zero; visual.localRotation = Quaternion.identity; visual.localScale = Vector3.one * scale;
                var renderer = visual.GetComponent<SpriteRenderer>();
                if (renderer == null) renderer = visual.gameObject.AddComponent<SpriteRenderer>();
                renderer.enabled = false;
                renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
                renderer.sharedMaterial = body.sharedMaterial;
                renderer.sortingLayerID = body.sortingLayerID;
                renderer.sortingOrder = body.sortingOrder + order;
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { if (exists) PrefabUtility.UnloadPrefabContents(root); else UnityEngine.Object.DestroyImmediate(root); }
        }

        private static Transform Child(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null) return child;
            child = new GameObject(name).transform; child.SetParent(parent, false); return child;
        }

        private static float VisibleHeight(string path, float ppu)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!texture.LoadImage(File.ReadAllBytes(path))) throw new InvalidOperationException("Invalid PNG " + path);
                var pixels = texture.GetPixels32(); int min = texture.height, max = -1;
                for (int y = 0; y < texture.height; y++) for (int x = 0; x < texture.width; x++)
                    if (pixels[y * texture.width + x].a > 0) { min = Mathf.Min(min, y); max = Mathf.Max(max, y); }
                if (max < min) throw new InvalidOperationException("Empty sprite " + path);
                return (max - min + 1) / ppu;
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }

        private static void Set(SerializedObject target, string name, UnityEngine.Object value) => target.FindProperty(name).objectReferenceValue = value;

        private static void Pose(SerializedProperty target, Vector3 position, Vector3 rotation, int order)
        {
            target.FindPropertyRelative("position").vector3Value = position;
            target.FindPropertyRelative("rotation").vector3Value = rotation;
            target.FindPropertyRelative("orderOffset").intValue = order;
        }

        private static void AuthorMotion(PlayerSwordMotionData data)
        {
            var serialized = new SerializedObject(data);
            Pose(serialized.FindProperty("carry"), Vector3.zero, new Vector3(0, 0, -80), -1);
            var combo = serialized.FindProperty("combo"); combo.arraySize = 3;
            var first = combo.GetArrayElementAtIndex(0);
            first.FindPropertyRelative("continuousSwing").boolValue = true;
            Pose(first.FindPropertyRelative("lift"), Vector3.zero, new Vector3(0, 0, -170), -1);
            Pose(first.FindPropertyRelative("ready"), Vector3.zero, new Vector3(0, 0, -210), -1);
            Pose(first.FindPropertyRelative("strike"), Vector3.zero, new Vector3(0, 0, -280), 1);
            Pose(first.FindPropertyRelative("recovery"), Vector3.zero, new Vector3(0, 0, -280), 1);
            var second = combo.GetArrayElementAtIndex(1);
            Pose(second.FindPropertyRelative("lift"), new Vector3(0.02f, 0.05f, 0), new Vector3(0, 0, -295), 1);
            Pose(second.FindPropertyRelative("ready"), new Vector3(0.05f, 0.03f, 0), new Vector3(0, 0, -305), 1);
            Pose(second.FindPropertyRelative("strike"), new Vector3(-0.08f, 0.20f, 0), new Vector3(0, 0, -140), -1);
            Pose(second.FindPropertyRelative("recovery"), new Vector3(-0.12f, 0.12f, 0), new Vector3(0, 0, -140), -1);
            var third = combo.GetArrayElementAtIndex(2);
            Pose(third.FindPropertyRelative("lift"), new Vector3(-0.12f, 0.10f, 0), new Vector3(15, -15, -125), -1);
            Pose(third.FindPropertyRelative("ready"), new Vector3(-0.15f, 0.02f, 0), new Vector3(15, -15, -100), -1);
            Pose(third.FindPropertyRelative("strike"), new Vector3(0.05f, 0.02f, 0), new Vector3(15, 165, -100), 1);
            Pose(third.FindPropertyRelative("recovery"), new Vector3(0.02f, 0, 0), new Vector3(0, 180, -90), 1);
            for (int i = 0; i < 3; i++)
            {
                var step = combo.GetArrayElementAtIndex(i);
                step.FindPropertyRelative("liftFraction").floatValue = i == 0 ? 0.75f : 0.55f;
                step.FindPropertyRelative("windupEase").intValue = (int)Ease.InOutSine;
                step.FindPropertyRelative("strikeEase").intValue = (int)(i == 0 ? Ease.InQuad : i == 1 ? Ease.OutQuad : Ease.InOutSine);
                step.FindPropertyRelative("recoveryEase").intValue = (int)Ease.OutSine;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.SaveAssetIfDirty(data);
        }
    }
}
