using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Teleports the player to a destination point when they trigger this elevator
/// and request an interaction. Subscribes to the player's teleport event only
/// while the player is standing on the elevator.
/// </summary>
public class ElevatorController : MonoBehaviour {
    [Tooltip("Where the player is teleported to when they interact on this elevator.")]
    [FormerlySerializedAs("pontoDestino")]
    [SerializeField] private Transform destinationPoint;

    private bool playerOnElevator;
    private PlayerController playerController;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!collision.CompareTag("Player")) {
            return;
        }

        playerOnElevator = true;
        playerController = collision.GetComponent<PlayerController>();
        if (playerController != null) {
            playerController.OnTeleportRequest += TeleportPlayer;
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (!collision.CompareTag("Player")) {
            return;
        }

        playerOnElevator = false;
        if (playerController != null) {
            playerController.OnTeleportRequest -= TeleportPlayer;
        }
    }

    private void TeleportPlayer(GameObject player) {
        if (playerOnElevator) {
            player.transform.position = destinationPoint.position;
        }
    }
}
