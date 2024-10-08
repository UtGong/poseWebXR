using UnityEngine;

public class CameraPCController : MonoBehaviour
{
    public float moveSpeed = 5.0f; // 移动速度
    public float rotateSpeed = 90.0f; // 旋转速度
    public GameObject selectBox;
    private bool is_Selected = false; // 是否选中
    private Transform selectedTransform; // 选中的立方体的Transform

    void Update()
    {
        // 检测鼠标左键点击
        if (Input.GetMouseButtonDown(0)) // 鼠标左键按下
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                // 检查点击的对象是否为立方体
                if (hit.transform.CompareTag("CameraHandle"))
                {
                    if (selectedTransform != null)
                    {
                        // 如果已经选中了一个立方体，先取消选中
                        selectedTransform = null;
                        is_Selected = false;
                        selectBox.SetActive(is_Selected);
                    }
                    else
                    {
                        // 选中点击的立方体
                        selectedTransform = hit.transform;
                        is_Selected = true;
                        
                    }

                    if(selectedTransform != null) selectBox.SetActive(selectedTransform.gameObject == gameObject);
                }
            }
        }

        // 检测鼠标右键点击
        if (Input.GetMouseButtonDown(1)) // 鼠标右键按下
        {
            if (selectedTransform != null)
            {
                // 取消选中
                selectedTransform = null;
                is_Selected = false;
                selectBox.SetActive(is_Selected);
            }
        }

        // 如果选中了立方体，则进行移动和旋转操作
        if (is_Selected && selectedTransform != null)
        {
            float horizontal = Input.GetAxis("Horizontal"); // A/D 或者 Left/Right 箭头
            float vertical = Input.GetAxis("Vertical"); // W/S 或者 Up/Down 箭头
            float mouseX = Input.GetAxis("Mouse X"); // 鼠标X轴移动
            float mouseY = Input.GetAxis("Mouse Y"); // 鼠标Y轴移动

            // 前后左右移动
            Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
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
    }
}