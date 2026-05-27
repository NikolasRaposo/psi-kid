using UnityEngine;

/// <summary>
/// Opens an associated door (by disabling its GameObject) while a player or a
/// movable object rests on the plate, and closes it again when they leave.
/// </summary>
public class PressurePlate : MonoBehaviour {
    [Tooltip("Door GameObject toggled by this plate (disabled means open).")]
    [SerializeField] private GameObject door;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player") || collision.CompareTag("Movable")) {
            door.SetActive(false);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Player") || collision.CompareTag("Movable")) {
            door.SetActive(true);
        }
    }
}
