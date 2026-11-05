using System;
using System.IO;
using Etherean.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Etherean.Editor
{
    public static class PlayerHudBuilder
    {
        public const string PrefabPath = "Assets/_Project/Prefabs/UI/PlayerHud.prefab";
        public const string DefaultFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        [MenuItem("ETHEREAN/UI/Create and Connect Player HUD")]
        public static void Build()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play first.");
            if (!File.Exists(PrefabPath)) CreatePrefab();
            var player = PrefabUtility.LoadPrefabContents(PlayerCharacterAnimationBuilder.PrefabPath);
            try
            {
                if (player.GetComponentInChildren<PlayerHud>(true) != null) return;
                var hud = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), player.transform);
                hud.name = "PlayerHud";
                PrefabUtility.SaveAsPrefabAsset(player, PlayerCharacterAnimationBuilder.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(player); }
        }

        private static void CreatePrefab()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath)); AssetDatabase.Refresh();
            var root = new GameObject("PlayerHud", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            try
            {
                var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.pixelPerfect = true;
                var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(640, 360); scaler.matchWidthOrHeight = .5f;
                var group = root.GetComponent<CanvasGroup>(); group.interactable = false; group.blocksRaycasts = false;
                var content = Rect("Content", root.transform, new Vector2(12, -12), new Vector2(220, 64));
                var health = Label("HealthText", content, Vector2.zero);
                var count = Label("ItemCountText", content, new Vector2(0, -20));
                var progress = Rect("HealingProgress", content, new Vector2(0, -40), new Vector2(220, 20));
                var progressText = Label("ProgressText", progress, Vector2.zero);
                var view = root.AddComponent<PlayerHudView>();
                Set(view, "content", content.gameObject); Set(view, "healthText", health); Set(view, "itemCountText", count);
                Set(view, "healingProgressRoot", progress.gameObject); Set(view, "healingProgressText", progressText);
                root.AddComponent<PlayerHud>();
                health.text = "HP -- / --"; count.text = "Heal -- / --"; progressText.text = "Healing --%";
                progress.gameObject.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1); rect.anchoredPosition = position; rect.sizeDelta = size;
            return rect;
        }

        private static TMP_Text Label(string name, Transform parent, Vector2 position)
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DefaultFontPath);
            if (font == null) throw new InvalidOperationException("Import TMP Essential Resources before creating the HUD.");
            var rect = Rect(name, parent, position, new Vector2(220, 20)); var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font; text.fontSize = 14; text.alignment = TextAlignmentOptions.TopLeft;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false; text.richText = false; return text;
        }

        private static void Set(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            var data = new SerializedObject(target); data.FindProperty(field).objectReferenceValue = value; data.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
