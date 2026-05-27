using UnityEngine;

/// <summary>
/// A platform that descends while the player stands on it, carrying the player
/// down at the same speed. The player is parented to the platform during the ride.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Elevator : MonoBehaviour {
    [Tooltip("Descent speed in units per second.")]
    [SerializeField] private float speed = 4f;

    private Rigidbody2D rb;
    private Transform player;
    private bool isPlayerOnPlatform;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        if (!isPlayerOnPlatform || player == null) {
            return;
        }

        float delta = speed * Time.deltaTime;
        rb.MovePosition(rb.position + new Vector2(0f, -delta));
        player.position = new Vector2(player.position.x, player.position.y - delta);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            isPlayerOnPlatform = true;
            player = collision.transform;
            player.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            isPlayerOnPlatform = false;
            if (player != null) {
                player.SetParent(null);
            }
            player = null;
        }
    }
}
