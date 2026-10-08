using UnityEngine;

public class Paralax : MonoBehaviour
{
    [Tooltip("Kamera")]
    public Transform mainCamera;
    public float parallaxMultiplier;
    private Vector3 startPosition;
    private Vector3 startCameraPosition;

    void Start()
    {
        // if (mainCamera == null) 
        // {
        //     mainCamera = Camera.main.transform;
        // }
        // startPosition = transform.position;
        // startCameraPosition = mainCamera.position;
    }

    void LateUpdate()
    {
        Vector3 cameraMovement = mainCamera.position - startCameraPosition;
        Vector3 newPosition = startPosition + (cameraMovement * parallaxMultiplier);
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
    }
}