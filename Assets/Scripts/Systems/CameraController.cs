using UnityEngine;
using Cinemachine;

/// <summary>
/// Switches between a set of Cinemachine virtual cameras, one per room or
/// section. Only the active camera is enabled at any time.
/// </summary>
public class CameraController : MonoBehaviour {
    [Tooltip("All virtual cameras this controller can switch between, in order.")]
    [SerializeField] private CinemachineVirtualCamera[] cameras;
    [Tooltip("Camera enabled when the level starts.")]
    [SerializeField] private CinemachineVirtualCamera startCamera;

    private CinemachineVirtualCamera currentCamera;

    private void Start() {
        currentCamera = startCamera;
    }

    /// <summary>Activates the camera at <paramref name="index"/> and disables the previous one.</summary>
    public void ChangeCam(int index) {
        if (index < 0 || index >= cameras.Length) {
            Debug.LogWarning($"ChangeCam: camera index {index} is out of range.");
            return;
        }
        SetActiveCamera(cameras[index]);
    }

    /// <summary>Switches to the next camera in the list, wrapping past the end.</summary>
    public void NextCam() {
        int current = System.Array.IndexOf(cameras, currentCamera);
        if (current < 0 || cameras.Length == 0) {
            return;
        }
        SetActiveCamera(cameras[(current + 1) % cameras.Length]);
    }

    /// <summary>Switches to the previous camera in the list, wrapping past the start.</summary>
    public void PreviousCam() {
        int current = System.Array.IndexOf(cameras, currentCamera);
        if (current < 0 || cameras.Length == 0) {
            return;
        }
        SetActiveCamera(cameras[(current - 1 + cameras.Length) % cameras.Length]);
    }

    private void SetActiveCamera(CinemachineVirtualCamera next) {
        if (currentCamera != null) {
            currentCamera.gameObject.SetActive(false);
        }
        currentCamera = next;
        currentCamera.gameObject.SetActive(true);
    }
}
