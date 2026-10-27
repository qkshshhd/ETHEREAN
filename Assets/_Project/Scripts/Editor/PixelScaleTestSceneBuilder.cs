using System;
using System.IO;
using Etherean.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Etherean.Editor
{
    public static class PixelScaleTestSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Tests/PlayerPixelScaleTest.unity";
        public const string ArtFolder = "Assets/_Project/Art/Sprites/PixelScaleTests";
        public const string TexturePath = "Assets/_Project/Data/Player/PixelScalePreview.renderTexture";

        [MenuItem("ETHEREAN/Tests/Create Pixel Scale Review Scene")]
        public static void Create()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
            if (File.Exists(ScenePath) || AssetDatabase.IsValidFolder(ArtFolder) || File.Exists(TexturePath))
                throw new InvalidOperationException("Review assets already exist; open the saved review scene.");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Player/Player.prefab");
            var square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/MovementSquare.png");
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/MovementPlaceholder.mat");
            if (prefab == null || square == null || material == null) throw new InvalidOperationException("Movement assets are missing.");
            AssetDatabase.CreateFolder("Assets/_Project/Art/Sprites", "PixelScaleTests");
            Sprite[] guides = { CreateGuide(30, "Guide2"), CreateGuide(24, "Guide2_5"), CreateGuide(20, "Guide3") };
            Sprite calibration = CreateCalibration();
            var target = new RenderTexture(640, 360, 24, RenderTextureFormat.ARGB32)
            {
                name = "PixelScalePreview", filterMode = FilterMode.Point, antiAliasing = 1,
                useMipMap = false, autoGenerateMips = false, useDynamicScale = false
            };
            AssetDatabase.CreateAsset(target, TexturePath);
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                var cameraObject = new GameObject("Review Camera", typeof(Camera), typeof(AudioListener), typeof(PixelScalePreview));
                cameraObject.tag = "MainCamera";
                Camera camera = cameraObject.GetComponent<Camera>();
                camera.transform.position = new Vector3(0f, 4.625f, -10f);
                camera.orthographic = true;
                camera.orthographicSize = 5.625f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.06f, 0.09f, 0.12f);
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.targetTexture = target;
                var preview = new SerializedObject(cameraObject.GetComponent<PixelScalePreview>());
                preview.FindProperty("sourceCamera").objectReferenceValue = camera;
                preview.FindProperty("previewTexture").objectReferenceValue = target;
                preview.ApplyModifiedPropertiesWithoutUndo();
                for (int x = -10; x <= 10; x++)
                    Rect("Grid X " + x, new Vector2(x, 4.5f), new Vector2(1f / 32f, 11f), new Color(0.16f, 0.2f, 0.23f), square, material, -5);
                for (int y = 0; y <= 10; y++)
                    Rect("Grid Y " + y, new Vector2(0f, y), new Vector2(20f, 1f / 32f), new Color(0.16f, 0.2f, 0.23f), square, material, -5);
                foreach (float x in new[] { -9f, -6f, -3f, 1f, 4f, 7f })
                    Rect("Background Scale Marker", new Vector2(x, 5f), new Vector2(0.5f, 9f), new Color(0.11f, 0.15f, 0.18f), square, material, -10);
                Rect("Ground", new Vector2(0f, -0.5f), new Vector2(20f, 1f), new Color(0.29f, 0.34f, 0.35f), square, material).AddComponent<BoxCollider2D>();
                foreach (float x in new[] { -8f, 8f })
                    Rect("Wall " + x, new Vector2(x, 3f), new Vector2(0.5f, 6f), new Color(0.33f, 0.4f, 0.42f), square, material).AddComponent<BoxCollider2D>();
                foreach (float x in new[] { -4f, 0f, 4f })
                    Rect("Jump Platform " + x, new Vector2(x, 2.75f), new Vector2(2f, 0.25f), new Color(0.31f, 0.37f, 0.39f), square, material).AddComponent<BoxCollider2D>();
                var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                player.transform.position = new Vector3(-6.5f, 0.91f, 0f);
                var ability = new SerializedObject(player.GetComponent<PlayerTraversalAbilities>());
                ability.FindProperty("wallTraversalUnlocked").boolValue = true;
                ability.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.RecordPrefabInstancePropertyModifications(player.GetComponent<PlayerTraversalAbilities>());
                string[] names = { "2 heads", "2.5 heads", "3 heads" };
                for (int i = 0; i < 3; i++)
                {
                    float x = -4f + i * 4f;
                    Rect("Proportion " + names[i], new Vector2(x, 5.5f), Vector2.one, Color.white, guides[i], material, 10);
                    Label(names[i] + " / 60px", new Vector2(x, 4.15f));
                }
                Rect("One Pixel Checker", new Vector2(6.5f, 8f), Vector2.one, Color.white, calibration, material, 10);
                Label("1px checker / 32 PPU", new Vector2(5f, 7f));
                Label("64x64 guides / same 60px height / draft proportions", new Vector2(0f, 9.1f));
                if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Review scene save failed.");
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static Sprite CreateGuide(int head, string name)
        {
            var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            var pixels = new Color32[64 * 64];
            float radius = head / 2f;
            float cy = 62f - radius - 0.5f;
            int halfBody = Mathf.RoundToInt(head * 0.3f);
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, cy));
                    bool circle = distance <= radius && distance >= radius - 1.25f;
                    bool body = y >= 2 && y < 62 - head && x >= 32 - halfBody && x < 32 + halfBody &&
                        (y == 2 || y == 61 - head || x == 32 - halfBody || x == 31 + halfBody);
                    if (circle || body) pixels[y * 64 + x] = new Color32(210, 216, 220, 255);
                }
            texture.SetPixels32(pixels);
            texture.Apply();
            return SaveSprite(texture, name);
        }

        private static Sprite CreateCalibration()
        {
            var texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++) texture.SetPixel(x, y, (x + y) % 2 == 0 ? Color.white : Color.black);
            texture.Apply();
            return SaveSprite(texture, "PixelChecker");
        }

        private static Sprite SaveSprite(Texture2D texture, string name)
        {
            string path = ArtFolder + "/" + name + ".png";
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32f;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static GameObject Rect(string name, Vector2 position, Vector2 scale, Color color, Sprite sprite, Material material, int order = 0)
        {
            var go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.position = position;
            go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = order;
            return go;
        }

        private static void Label(string text, Vector2 position)
        {
            var go = new GameObject("Label " + text, typeof(TextMesh));
            go.transform.position = new Vector3(position.x, position.y, -0.1f);
            var label = go.GetComponent<TextMesh>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 36;
            label.characterSize = 0.1f;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = new Color(0.8f, 0.85f, 0.87f);
            go.GetComponent<MeshRenderer>().sharedMaterial = label.font.material;
        }
    }
}