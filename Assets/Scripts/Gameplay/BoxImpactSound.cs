using UnityEngine;

/// <summary>Plays a metal thud when a movable box lands hard enough.</summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BoxImpactSound : MonoBehaviour {
    [SerializeField] private float minSpeed = 2.5f;
    [SerializeField] private float cooldown = 0.12f;

    private float lastTime;

    private void OnCollisionEnter2D(Collision2D collision) {
        float speed = collision.relativeVelocity.magnitude;
        if (speed < minSpeed || Time.time - lastTime < cooldown) {
            return;
        }
        lastTime = Time.time;
        Sfx.Play("box_impact", Mathf.Clamp01(speed / 12f) * 0.55f, Random.Range(0.9f, 1.1f));
    }
}
