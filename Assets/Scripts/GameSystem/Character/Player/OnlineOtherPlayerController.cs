using GameSystem.GameScene.MainMenu.EventSystem;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu.Character.Player
{
    public class OnlineOtherPlayerController : BaseOnlinePlayerController
    {
        protected override void Awake()
        {
            base.Awake();
            var characterController = GetComponent<CharacterController>();
            if (characterController != null) Destroy(characterController); // 移除CharacterController组件，避免物理碰撞干扰
            gameObject.AddComponent<BoxCollider>();
            GameEventSystem.AddListener<MoveEvents.PlayerMoveEvent>(OnMoveEvent);
        }

        void OnDisable()
        {
            GameEventSystem.RemoveListener<MoveEvents.PlayerMoveEvent>(OnMoveEvent);
        }
        
        protected override void Update()
        {
            // 不做任何输入处理，完全由网络同步位置和旋转
            return;
        }
        
        private void OnMoveEvent(MoveEvents.PlayerMoveEvent e)
        {
            if (e.PlayerId != PlayerId) return; // 只处理其他玩家的移动事件
            transform.position = e.position; // 更新位置
            transform.rotation = Quaternion.Euler(0, e.angle, 0); // 更新旋转，假设只需要更新Y轴旋转
        }

        protected void Die()
        {
            base.Die();
            Destroy(gameObject.GetComponent<BoxCollider>());
        }
    }
}