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
        }

        protected override void Update()
        {
            // 不做任何输入处理，完全由网络同步位置和旋转
        }
        
        protected override void Die()
        {
            base.Die();
            Destroy(gameObject.GetComponent<BoxCollider>());
        }
    }
}