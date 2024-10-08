using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    public Transform targetCamera; // The original camera this one should follow
    public Vector3 offset; // The offset for the copied camera relative to the original

    void Update()
    {
        if (targetCamera != null)
        {
            // Follow the target camera with the specified offset
            transform.position = targetCamera.position + offset;
            transform.rotation = targetCamera.rotation;
        }
    }
}
