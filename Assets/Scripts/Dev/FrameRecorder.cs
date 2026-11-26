using System.IO;
using UnityEngine;

/// <summary>
/// Records the main camera to numbered PNG frames at a fixed frame rate
/// (Time.captureFramerate), independent of the Game view size. Used to capture
/// trailer footage from a scripted run; encode with ffmpeg afterwards.
/// </summary>
public class FrameRecorder : MonoBehaviour {
    public string folder;
    public int width = 1920;
    public int height = 1080;
    public int fps = 30;
    public bool recording;

    private RenderTexture rt;
    private Texture2D tex;
    private int frame;

    public int FrameCount => frame;

    public void Begin(string outFolder) {
        folder = outFolder;
        Directory.CreateDirectory(folder);
        rt = new RenderTexture(width, height, 24);
        tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        Time.captureFramerate = fps;
        frame = 0;
        recording = true;
    }

    public void End() {
        recording = false;
        Time.captureFramerate = 0;
    }

    private void LateUpdate() {
        if (!recording) {
            return;
        }
        var cam = Camera.main;
        if (cam == null) {
            return;
        }
        var prev = cam.targetTexture;
        cam.targetTexture = rt;
        cam.Render();
        cam.targetTexture = prev;
        var active = RenderTexture.active;
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply(false);
        RenderTexture.active = active;
        File.WriteAllBytes(Path.Combine(folder, $"f{frame:00000}.png"), tex.EncodeToPNG());
        frame++;
    }
}
