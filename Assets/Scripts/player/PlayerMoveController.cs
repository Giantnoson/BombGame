using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace player
{
    public class PlayerMoveController : MonoBehaviour
    {
        [Tooltip("角色移动速度")]
        public float MoveSpeed { get; set; } // SimpleMove 的速度单位是 米/秒
        [Tooltip("鼠标灵敏度")]
        public float mouseSensitivity = 2.0f;
        [Tooltip("水平视角限制角度")]
        public float verticalRotationMaxLimit = 90.0f; // 垂直视角限制角度
        [Tooltip("垂直视角限制角度")]
        public float verticalRotationMinLimit = 25f; // 垂直视角限制角度

        private CharacterController controller;
        private float rotationX = 0f; // 垂直旋转角度 
        private float rotationY = 0f; // 水平旋转角度
        private Camera miniMapCamera;
        private bool isMiniMapExists = false;//判断是否含有小地图


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
                Debug.LogError("在PlayerMoveController中未找到MiniMapCamera");
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
            
            // 构建移动向量（相对于角色本地坐标系）
            // 注意：SimpleMove 会自动处理重力，所以 Y 轴设为 0
            Vector3 moveDirection = new Vector3(moveHorizontal, 0, moveVertical);
            
            // 将移动向量从角色本地坐标系转换到世界坐标系
            moveDirection = transform.TransformDirection(moveDirection);
            
            // 应用移动速度
            // 关键修改点：SimpleMove 不需要乘以 Time.deltaTime，因为它内部已经处理了
            // 它接受的是速度向量（单位：米/秒）
            controller.SimpleMove(moveDirection * MoveSpeed);
            
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
            rotationX = Mathf.Clamp(rotationX, verticalRotationMinLimit, verticalRotationMaxLimit); // 限制垂直视角范围
            
            // 将垂直旋转应用到相机
            Camera.main.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        }
    }
}
