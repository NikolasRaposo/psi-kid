using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Dresses the level at runtime so it stops looking like a flat colour:
/// a tiled back wall behind every tilemap, a dark camera background, a vignette
/// that follows the camera and slow floating dust. Lives on one scene object.
/// </summary>
public class LabAmbience : MonoBehaviour {
    [SerializeField] private Color wallTint = new Color(0.85f, 0.8f, 1f, 1f);
    [SerializeField] private Color cameraBackground = new Color(0.05f, 0.04f, 0.09f, 1f);
    [SerializeField] private Color dustColor = new Color(0.8f, 0.7f, 1f, 0.35f);
    [SerializeField] private float margin = 6f;

    private Camera cam;
    private SpriteRenderer vignette;

    private void Start() {
        cam = Camera.main;
        if (cam != null) {
            cam.backgroundColor = cameraBackground;
            cam.clearFlags = CameraClearFlags.SolidColor;
        }
        BuildBackWall();
        BuildVignette();
        BuildDust();
    }

    private Bounds LevelBounds() {
        var maps = FindObjectsByType<TilemapRenderer>(FindObjectsSortMode.None);
        bool any = false;
        var b = new Bounds();
        foreach (var r in maps) {
            if (!any) {
                b = r.bounds;
                any = true;
            } else {
                b.Encapsulate(r.bounds);
            }
        }
        if (!any) {
            b = new Bounds(Vector3.zero, new Vector3(80, 40, 0));
        }
        b.Expand(margin * 2);
        return b;
    }

    private void BuildBackWall() {
        var sprite = Resources.Load<Sprite>("Art/back_wall");
        if (sprite == null) {
            return;
        }
        var b = LevelBounds();
        var sr = new GameObject("BackWall").AddComponent<SpriteRenderer>();
        sr.transform.SetParent(transform, false);
        sr.sprite = sprite;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(Mathf.Ceil(b.size.x / 2) * 2, Mathf.Ceil(b.size.y / 2) * 2);
        sr.transform.position = new Vector3(b.center.x, b.center.y, 5f);
        sr.color = wallTint;
        sr.sortingOrder = -50;
    }

    private void BuildVignette() {
        var sprite = Resources.Load<Sprite>("Art/vignette");
        if (sprite == null || cam == null) {
            return;
        }
        vignette = new GameObject("Vignette").AddComponent<SpriteRenderer>();
        vignette.sprite = sprite;
        vignette.sortingOrder = 90;
    }

    private void LateUpdate() {
        if (vignette == null || cam == null) {
            return;
        }
        float h = cam.orthographicSize * 2f;
        float w = h * cam.aspect;
        var s = vignette.sprite.bounds.size;
        vignette.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
        vignette.transform.localScale = new Vector3(w / s.x, h / s.y, 1f);
    }

    private void BuildDust() {
        var b = LevelBounds();
        var ps = new GameObject("Dust").AddComponent<ParticleSystem>();
        ps.transform.SetParent(transform, false);
        ps.transform.position = new Vector3(b.center.x, b.center.y, 0f);
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.loop = true;
        main.prewarm = true;
        main.startLifetime = 12f;
        main.startSpeed = 0.15f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.1f);
        main.startColor = dustColor;
        main.maxParticles = 600;
        var em = ps.emission;
        em.rateOverTime = Mathf.Clamp(b.size.x * b.size.y * 0.02f, 10f, 50f);
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(b.size.x, b.size.y, 1f);
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.25f;
        noise.frequency = 0.2f;
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                  new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 0.3f), new GradientAlphaKey(0, 1) });
        col.color = g;
        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.material = new Material(Shader.Find("Sprites/Default"));
        rend.sortingOrder = -10;
        ps.Play();
    }
}
