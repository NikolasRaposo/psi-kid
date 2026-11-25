using UnityEngine;

/// <summary>
/// A conduit drawn behind the walls from a pressure plate to its door, so players
/// (and viewers) can see which plate opens what. Dim red while idle, bright cyan
/// with a travelling pulse while the plate is satisfied.
/// </summary>
public class PlateWire : MonoBehaviour {
    private static readonly Color Off = new Color(0.55f, 0.16f, 0.2f, 0.55f);
    private static readonly Color On = new Color(0.37f, 0.92f, 1f, 0.95f);

    private LineRenderer line;
    private bool lit;
    private float blend;

    /// <summary>Builds an L-shaped wire between the two transforms.</summary>
    public static PlateWire Create(Transform from, Transform to) {
        var go = new GameObject("PlateWire");
        go.transform.SetParent(from, false);
        var wire = go.AddComponent<PlateWire>();
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.widthMultiplier = 0.08f;
        lr.numCornerVertices = 2;
        lr.sortingOrder = -5; // behind the tilemap walls: it only shows in open space
        Vector3 a = from.position + new Vector3(0, -0.35f, 0);
        Vector3 b = to.position + new Vector3(0, -0.9f, 0);
        lr.positionCount = 3;
        lr.SetPosition(0, a);
        lr.SetPosition(1, new Vector3(a.x, b.y, 0));
        lr.SetPosition(2, b);
        wire.line = lr;
        wire.Apply(0f);
        return wire;
    }

    public void SetLit(bool value) => lit = value;

    private void Update() {
        blend = Mathf.MoveTowards(blend, lit ? 1f : 0f, Time.deltaTime * 6f);
        Apply(blend);
    }

    private void Apply(float k) {
        if (line == null) {
            return;
        }
        float pulse = lit ? 0.85f + Mathf.Sin(Time.time * 10f) * 0.15f : 1f;
        Color c = Color.Lerp(Off, On, k) * pulse;
        c.a = Mathf.Lerp(Off.a, On.a, k);
        line.startColor = c;
        line.endColor = c;
        line.widthMultiplier = Mathf.Lerp(0.07f, 0.11f, k);
    }
}
