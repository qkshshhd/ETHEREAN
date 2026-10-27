using UnityEngine;

namespace Etherean.Player
{
    [DisallowMultipleComponent]
    public sealed class PixelScalePreview : MonoBehaviour
    {
        [SerializeField] private Camera sourceCamera;
        [SerializeField] private RenderTexture previewTexture;

        private void OnEnable()
        {
            if (sourceCamera == null || previewTexture == null || sourceCamera.targetTexture != previewTexture)
            {
                Debug.LogError("Connect the review camera and its preview RenderTexture.", this);
                enabled = false;
                return;
            }
            sourceCamera.aspect = (float)previewTexture.width / previewTexture.height;
        }

        private void OnGUI()
        {
            int scale = Mathf.Max(1, Mathf.FloorToInt(Mathf.Min(
                (float)Screen.width / previewTexture.width, (float)Screen.height / previewTexture.height)));
            float width = previewTexture.width * scale;
            float height = previewTexture.height * scale;
            var rect = new Rect(Mathf.Floor((Screen.width - width) / 2f), Mathf.Floor((Screen.height - height) / 2f), width, height);
            GUI.DrawTexture(rect, previewTexture, ScaleMode.StretchToFill, false);
            GUILayout.BeginArea(new Rect(8f, 8f, 440f, 90f), GUI.skin.box);
            GUILayout.Label($"640x360 / 32 PPU / display {scale}x / camera {sourceCamera.orthographicSize:F3}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("View A: Size 5.5")) sourceCamera.orthographicSize = 5.5f;
            if (GUILayout.Button("View B: Size 5.625")) sourceCamera.orthographicSize = 5.625f;
            GUILayout.EndHorizontal();
            GUILayout.Label(Screen.width < 640 || Screen.height < 360
                ? "Use a Game view of at least 640x360; the image is cropped here."
                : "A/D + Space: move and jump. Gray figures are proportion guides.");
            GUILayout.EndArea();
        }
    }
}