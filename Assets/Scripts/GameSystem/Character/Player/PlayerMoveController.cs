using System.Collections;
using System.Collections.Generic;
using GameSystem.EventSystem;
using UnityEngine;

namespace GameSystem.Character.Player
{
    public class PlayerMoveController : MonoBehaviour
    {
        public PlayerController owner;

        public string ownerId = "p001";
        
        [Tooltip("角色移动速度")]
        public float a { get; set; } // SimpleMove 的速度单位是 米/秒
        [Tooltip("鼠标灵敏度")]
        public float mouseSensitivity = 2.0f;
        [Tooltip("水平视角限制角度")]
        public float verticalRotationMaxLimit = 90.0f; // 垂直视角限制角度
        [Tooltip("垂直视角限制角度")]
        public float verticalRotationMinLimit = 25f; // 垂直视角限制角度

        private CharacterController controller;
        private float rotationX = 0f; // 垂直旋转角度 
        private Camera miniMapCamera;
        private bool isMiniMapExists = false;//判断是否含有小地图
        
        private Vector3 moveDirection = Vector3.zero;
        private float MoveSpeed = 10f;
        
        
        
        
        //其他变量
        private float mouseX;
        private float mouseY;        
        

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

            if (Camera.main == null)
            {
                Debug.LogError("全局未找到MainCamera");
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

        private void OnEnable()
        {
            GameEventSystem.AddListener<CharacterMoveEvent.UpdateSpeedEvent>(OnSpeedUpdate);
            GameEventSystem.AddListener<CharacterMoveEvent.UpdateMoveDirectionEvent>(OnMoveDirectionUpdate);
            GameEventSystem.AddListener<CharacterMoveEvent.UpdateRotationXEvent>(OnRotationXUpdate);
        }

        private void OnDisable()
        {
            GameEventSystem.RemoveListener<CharacterMoveEvent.UpdateSpeedEvent>(OnSpeedUpdate);
            GameEventSystem.RemoveListener<CharacterMoveEvent.UpdateMoveDirectionEvent>(OnMoveDirectionUpdate);
            GameEventSystem.RemoveListener<CharacterMoveEvent.UpdateRotationXEvent>(OnRotationXUpdate);
        }

        public void Init(PlayerController owner, string playerID)
        {
            this.owner = owner;
            ownerId = playerID;
        }

        private void OnSpeedUpdate(CharacterMoveEvent.UpdateSpeedEvent evt)
        {
            if (ownerId == evt.OwnerId)
            {
                MoveSpeed = evt.Speed;
            }
        }

        private void OnMoveDirectionUpdate(CharacterMoveEvent.UpdateMoveDirectionEvent evt)
        {
            if (ownerId == evt.OwnerId)
                moveDirection = evt.Direction;
        }

        private void OnRotationXUpdate(CharacterMoveEvent.UpdateRotationXEvent evt)
        {
            if (ownerId == evt.OwnerId)
            {
                mouseX = evt.MouseX;
                mouseY = evt.MouseY;
            }
        }
        
        void Update()
        {
            // 鼠标水平旋转控制（左右看）
            mouseX *= mouseSensitivity;

            
            // 鼠标垂直旋转控制（上下看）
            mouseY *= mouseSensitivity;
            rotationX -= mouseY; // 减号确保鼠标向上移动时视角向上
            rotationX = Mathf.Clamp(rotationX, verticalRotationMinLimit, verticalRotationMaxLimit); // 限制垂直视角范围
            controller.SimpleMove(moveDirection * MoveSpeed);
            //角色旋转
            transform.Rotate(Vector3.up, mouseX); // 绕Y轴旋转角色 
            // 摄像机水平旋转
            Camera.main.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            if (isMiniMapExists)
            {
                miniMapCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        }
    }
}
