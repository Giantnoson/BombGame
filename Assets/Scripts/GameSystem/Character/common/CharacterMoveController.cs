using GameSystem.EventSystem;
using UnityEngine;

namespace GameSystem.Character
{
    public class CharacterMoveController : MonoBehaviour
    {
        public string ownerId = "P001";

        [Tooltip("鼠标灵敏度")] public float mouseSensitivity = 2.0f;

        [Tooltip("水平视角限制角度")] public float verticalRotationMaxLimit = 90.0f; // 垂直视角限制角度

        [Tooltip("垂直视角限制角度")] public float verticalRotationMinLimit = 25f; // 垂直视角限制角度

        public float rotationX; // 垂直旋转角度 
        public bool isMainCameraExists;
        public float MoveSpeed = 10f;

        private CharacterController controller;

        private bool isDie;
        private bool isMiniMapExists; //判断是否含有小地图
        private Camera mainCamera;
        private Camera miniMapCamera;


        //其他变量
        private float mouseX;
        private float mouseY;

        private Vector3 moveDirection = Vector3.zero;

        [Tooltip("角色移动速度")] public float a { get; set; } // SimpleMove 的速度单位是 米/秒


        private void Start()
        {
            controller = GetComponent<CharacterController>();
            var cameras = GetComponentsInChildren<Camera>();
            foreach (var mapCamera in cameras)
                if (mapCamera.CompareTag("MiniMapCamera"))
                {
                    miniMapCamera = mapCamera;
                    isMiniMapExists = true;
                    ;
                }
                else if (mapCamera.CompareTag("MainCamera"))
                {
                    mainCamera = mapCamera;
                    isMainCameraExists = true;
                }

            if (mainCamera == null) Debug.LogWarning("未找到MainCamera");
            if (!isMiniMapExists)
                Debug.LogError("在PlayerMoveController中未找到MiniMapCamera");
            else
                miniMapCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
            // 隐藏并锁定鼠标光标到屏幕中心
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            if (isDie) return;
            // 鼠标水平旋转控制（左右看）
            mouseX *= mouseSensitivity;
            // 鼠标垂直旋转控制（上下看）
            mouseY *= mouseSensitivity;
            rotationX -= mouseY; // 减号确保鼠标向上移动时视角向上
            rotationX = Mathf.Clamp(rotationX, verticalRotationMinLimit, verticalRotationMaxLimit); // 限制垂直视角范围
            controller.SimpleMove(moveDirection * MoveSpeed);
            //角色旋转
            transform.Rotate(Vector3.up, mouseX); // 绕Y轴旋转角色 
            // 摄像机垂直旋转
            if (isMainCameraExists) mainCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            if (isMiniMapExists) miniMapCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
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

        public void Init(string playerID)
        {
            ownerId = playerID;
        }

        private void OnSpeedUpdate(CharacterMoveEvent.UpdateSpeedEvent evt)
        {
            if (ownerId == evt.Id) MoveSpeed = evt.Speed;
        }

        private void OnMoveDirectionUpdate(CharacterMoveEvent.UpdateMoveDirectionEvent evt)
        {
            if (ownerId == evt.Id)
                moveDirection = evt.Direction;
        }

        private void OnRotationXUpdate(CharacterMoveEvent.UpdateRotationXEvent evt)
        {
            if (ownerId == evt.Id)
            {
                mouseX = evt.MouseX;
                mouseY = evt.MouseY;
            }
        }

        protected virtual void OnPlayerDie(CharacterDieEvent evt)
        {
            if (isDie) return;
            if (evt.DieId == ownerId)
            {
                isDie = true;
                ownerId = "Die";
                controller.enabled = false;
            }
        }
    }
}