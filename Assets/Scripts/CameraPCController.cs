using UnityEngine;
public class CameraPCController : MonoBehaviour
{
    public float moveSpeed = 5.0f; // 移动速度
    public float rotateSpeed = 90.0f; // 旋转速度

    public float sensitivity = 2.0f;
    private float rotationY = 0f;
    
    private bool is_Selected = false; // 是否选中
    private Transform selectedTransform; // 选中的立方体的Transform

    public GameObject CameraObj;
    private bool isNormalMode;

    public void ChangeNormalMode(bool _isNormalMode)
    {
        isNormalMode = _isNormalMode;
    }
    
    void Update()
    {
        if (isNormalMode)
        {
            // 检测鼠标左键点击
            if (Input.GetMouseButtonDown(0)) // 鼠标左键按下
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                
                if (selectedTransform != null)
                {
                    selectedTransform.GetChild(4).GetChild(1).GetChild(0).gameObject.SetActive(false);
                }

                if (Physics.Raycast(ray, out hit))
                {
                    // 检查点击的对象是否为立方体
                    if (hit.transform.CompareTag("CameraHandle"))
                    {
                        Debug.Log(hit.transform.GetChild(0).name);
                        var hitTrans = hit.transform.GetChild(4).GetChild(1).GetChild(0);
                        hitTrans.gameObject.SetActive(true);
                        
                        selectedTransform = hit.transform;
                        is_Selected = true;
                    }
                    else
                    {
                        selectedTransform = null;
                        is_Selected = false;
                    }
                }
            }

            float horizontal = Input.GetAxis("Horizontal"); // A/D 或者 Left/Right 箭头
            float vertical = Input.GetAxis("Vertical"); // W/S 或者 Up/Down 箭头
            float mouseX = Input.GetAxis("Mouse X"); // 鼠标X轴移动
            float mouseY = Input.GetAxis("Mouse Y"); // 鼠标Y轴移动

            // 如果选中了立方体，则进行移动和旋转操作
            if (is_Selected && selectedTransform != null)
            {
                // 计算移动方向（基于摄像机的方向）
                Vector3 forward = CameraObj.transform.forward;
                Vector3 right = CameraObj.transform.right;
                forward.y = 0;
                right.y = 0;
                forward.Normalize();
                right.Normalize();

                Vector3 movement = (forward * vertical + right * horizontal) * moveSpeed * Time.deltaTime;
                selectedTransform.Translate(movement, Space.World);

                // 上下移动
                if (Input.GetKey(KeyCode.Space))
                {
                    selectedTransform.Translate(Vector3.up * moveSpeed * Time.deltaTime, Space.World);
                }
                if (Input.GetKey(KeyCode.C))
                {
                    selectedTransform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
                }

                // 360度旋转
                selectedTransform.Rotate(Vector3.up * mouseX * rotateSpeed * Time.deltaTime);
                selectedTransform.Rotate(Vector3.left * mouseY * rotateSpeed * Time.deltaTime);
            }
            else
            {
                // 鼠标控制旋转
                float mouseXFP = Input.GetAxis("Mouse X") * sensitivity;
                rotationY += mouseXFP;
                CameraObj.transform.localEulerAngles = new Vector3(0, rotationY, 0);

                // 获取输入
                float moveHorizontal = Input.GetAxis("Horizontal"); // A/D 或 左/右箭头
                float moveVertical = Input.GetAxis("Vertical"); // W/S 或 上/下箭头

                // 计算移动方向（基于摄像机的方向）
                Vector3 forward = CameraObj.transform.forward;
                Vector3 right = CameraObj.transform.right;
                forward.y = 0;
                right.y = 0;
                forward.Normalize();
                right.Normalize();

                Vector3 moveDirection = (forward * moveVertical + right * moveHorizontal).normalized;

                // 移动摄像机
                CameraObj.transform.position += moveDirection * moveSpeed * Time.deltaTime;
            }
        }
        
    }
}
