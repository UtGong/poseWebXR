using UnityEngine;
using System.Linq;

public class SyncCameraManager : MonoBehaviour
{
    public Camera mainCamera;
    public Camera[] trackingCameras;
    public Transform[] targets;

    public float rotationSpeed = 5f;
    public float moveSpeed = 5f;

    private Transform nearestTarget;
    private float distanceToNearestTarget;
    private Vector3 directionToNearestTarget;

    private void Update()
    {
        UpdateNearestTarget();
        UpdateTrackingCameras();
    }

    private void HandleMainCameraInput()
    {
        // 旋转主摄像机
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        mainCamera.transform.Rotate(Vector3.up, mouseX, Space.World);
        mainCamera.transform.Rotate(Vector3.right, -mouseY, Space.Self);

        // 移动主摄像机
        float moveHorizontal = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveVertical = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        Vector3 movement = mainCamera.transform.right * moveHorizontal + mainCamera.transform.forward * moveVertical;
        mainCamera.transform.position += movement;
    }

    private void UpdateNearestTarget()
    {
        if (targets.Length == 0) return;

        nearestTarget = targets.OrderBy(t => Vector3.Distance(t.position, mainCamera.transform.position)).First();
        directionToNearestTarget = nearestTarget.position - mainCamera.transform.position;
        distanceToNearestTarget = directionToNearestTarget.magnitude;
        directionToNearestTarget = directionToNearestTarget.normalized;
    }

    private void UpdateTrackingCameras()
    {
        if (nearestTarget == null) return;

        for (int i = 0; i < trackingCameras.Length && i < targets.Length; i++)
        {
            if (targets[i] != null)
            {
                // 计算跟踪摄像机的位置
                Vector3 targetToCamera = mainCamera.transform.position - nearestTarget.position;
                Vector3 newCameraPosition = targets[i].position + targetToCamera;
                trackingCameras[i].transform.position = newCameraPosition;

                // 设置跟踪摄像机的旋转，与主摄像机保持一致
                trackingCameras[i].transform.rotation = mainCamera.transform.rotation;
            }
        }
    }
}