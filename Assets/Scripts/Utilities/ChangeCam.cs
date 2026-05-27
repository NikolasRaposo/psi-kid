using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Trigger volume at a room boundary. When the player enters, it asks the
/// <see cref="CameraController"/> to switch to the camera assigned to this room.
/// </summary>
public class ChangeCam : MonoBehaviour {
    [Tooltip("Index of the camera (in CameraController) to activate for this room.")]
    [FormerlySerializedAs("CamIndex")]
    [SerializeField] private int cameraIndex;

    private CameraController cameraController;

    private void Start() {
        cameraController = Camera.main.GetComponent<CameraController>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            cameraController.ChangeCam(cameraIndex);
        }
    }
}
