using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace player
{
    public class PlayerMoveController : MonoBehaviour
{
    public float moveSpeed = 6.0f;
    public float mouseSensitivity = 2.0f;
    public float verticalRotationLimit = 90.0f; // 垂直视角限制角度
    
    private CharacterController controller;
    private float rotationX = 0f; // 垂直旋转角度 
    private float rotationY = 0f; // 水平旋转角度
    private Camera miniMapCamera;
    private bool isMiniMapExists = false;
    
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        Camera[] cameras = GetComponentsInChildren<Camera>();
        foreach (var mapCamera in cameras)
        {
            if (mapCamera.CompareTag("MiniMapCamera"))
            {
               miniMapCamera = mapCamera; 
               isMiniMapExists = true;
               break;
            }
        }

        if (!isMiniMapExists)
        {
            Debug.LogError("MiniMap Camera not found!");
        }
        else
        {
            miniMapCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
        }
        // 隐藏并锁定鼠标光标到屏幕中心
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
 
    void Update()
    {
        // 获取WASD输入 
        float moveHorizontal = Input.GetAxis("Horizontal"); // A/D
        float moveVertical = Input.GetAxis("Vertical");     // W/S 
        
        print(moveHorizontal + " " + moveVertical);
        
        // 构建移动向量（相对于角色本地坐标系）
        Vector3 moveDirection = new Vector3(moveHorizontal, 0, moveVertical);
        
        // 将移动向量从角色本地坐标系转换到世界坐标系
        moveDirection = transform.TransformDirection(moveDirection);
        
        // 应用移动速度和时间增量
        moveDirection *= moveSpeed * Time.deltaTime;
        
        controller.Move(moveDirection);
        
        // 鼠标水平旋转控制（左右看）
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up, mouseX); // 绕Y轴旋转角色 
        if (isMiniMapExists)
        {
            miniMapCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
        }
        
        // 鼠标垂直旋转控制（上下看）
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotationX -= mouseY; // 减号确保鼠标向上移动时视角向上
        rotationX = Mathf.Clamp(rotationX, -verticalRotationLimit, verticalRotationLimit); // 限制垂直视角范围
        
        // 将垂直旋转应用到相机
        Camera.main.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }
}

}
