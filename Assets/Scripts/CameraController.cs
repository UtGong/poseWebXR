using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera mainCamera;        // 主摄像头
    public Camera cameraA;           // 摄像头A
    public Camera cameraB;           // 摄像头B
    public Camera cameraC;           // 摄像头C

    public Transform modelA;         // 模特A
    public Transform modelB;         // 模特B
    public Transform modelC;         // 模特C

    public float followSpeed = 5f;   // 摄像头跟随速度
    
    private Transform closestModel;   // 最近的模特
    private float minDistance = float.MaxValue;

    void LateUpdate()
    {
        // 重置最小距离
        minDistance = float.MaxValue;
        closestModel = null;

        // 找出主摄像头中最近的模特
        float distanceA = Vector3.Distance(mainCamera.transform.position, modelA.position);
        float distanceB = Vector3.Distance(mainCamera.transform.position, modelB.position);
        float distanceC = Vector3.Distance(mainCamera.transform.position, modelC.position);

        if (distanceA < minDistance)
        {
            minDistance = distanceA;
            closestModel = modelA;
        }
        if (distanceB < minDistance)
        {
            minDistance = distanceB;
            closestModel = modelB;
        }
        if (distanceC < minDistance)
        {
            minDistance = distanceC;
            closestModel = modelC;
        }

        // 获取主摄像头相对于最近模特的方向和距离
        Vector3 mainCameraDirection = mainCamera.transform.position - closestModel.position;
        float mainCameraDistance = mainCameraDirection.magnitude;
        
        // 更新每个摄像头的位置和朝向
        UpdateCamera(cameraA, modelA, mainCameraDistance);
        UpdateCamera(cameraB, modelB, mainCameraDistance);
        UpdateCamera(cameraC, modelC, mainCameraDistance);
    }

    void UpdateCamera(Camera camera, Transform target, float distance)
    {
        // 计算目标位置
        Vector3 targetPosition = target.position + (camera.transform.position - target.position).normalized * distance;
        
        // 平滑更新摄像头位置
        camera.transform.position = Vector3.Lerp(
            camera.transform.position, 
            targetPosition, 
            Time.deltaTime * followSpeed
        );

        // 让摄像头始终看向目标模特
        Vector3 targetDirection = target.position - camera.transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        
        // 平滑更新摄像头旋转
        camera.transform.rotation = Quaternion.Slerp(
            camera.transform.rotation,
            targetRotation,
            Time.deltaTime * followSpeed
        );
    }
}