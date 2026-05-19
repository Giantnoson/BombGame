using System;
using System.Linq;
using Config;
using GameSystem.EventSystem;
using GameSystem.EventSystem.Event;
using GameSystem.GameScene.GameRuntimeScene;
using GameSystem.Manager;
using GameSystem.Timer;
using UnityEngine;

namespace GameSystem.GameProps.Item
{
    public class PropsStatus : BaseObject
    {
        [Tooltip("所有者ID")]
        public string ownerId;
        [Tooltip("道具属性面板")]
        public PropsConfig propsConfig;
        [Tooltip("是否处于活跃状态")]
        public bool isActive = false;

        private void OnDestroy()
        {
            var timerKey = GetTimerKey();
            if (GlobalTimerManager.Instance != null && GlobalTimerManager.Instance.HasTimer(timerKey))
            {
                GlobalTimerManager.Instance.CancelTimer(timerKey);
            }
        }

        /// <summary>
        /// 是否已经被销毁/失效，防止重复调用 Disable
        /// </summary>
        private bool isDisposed = false;
        
        /// <summary>
        /// 初始化道具，设置所有者和配置
        /// </summary>
        public void InitProps(string ownerId, PropsConfig propsConfig)
        {
            this.ownerId = ownerId;
            this.propsConfig = propsConfig;
            this.isDisposed = false;
        }

        /// <summary>
        /// 使用道具，根据 validTime 决定行为：
        /// validTime == 0: 立即生效，定时触发（永久，与 -1 相同语义）
        /// validTime == -1: 一次性立即生效（永久）
        /// validTime > 0: 限时生效，到期自动失效
        /// Timer key 采用 ownerId_propsId 组合，确保同玩家同类型道具互斥（后拾取覆盖前一个）
        /// </summary>
        public void UseProps()
        {
            if (propsConfig.validTime < 0f)
            {
                // validTime < 0：一次性立即生效，永不自动失效
                PropsEnable();
            }
            
            // validTime == 0：立即生效，定时触发（到期时间戳 = 当前时间）
            // validTime > 0：限时生效，到期后自动调用 PropsDisable

            GlobalTimerManager.Instance.CreateOrResetTimer(GetTimerKey(), LocalTime.Now + (long)(propsConfig.validTime * 1000), PropsEnable, PropsDisable, propsConfig.validTime == 0f, GameModeSelect.CurrentModeType == GameModeType.Offline);
            HidePropsVisual();
        }

        /// <summary>
        /// 生成 Timer 唯一键：玩家ID + "_" + 道具ID
        /// 同玩家同类型道具共享同一个 Timer，后拾取会重置前一个的计时
        /// </summary>
        private string GetTimerKey() => $"{ownerId}_{propsConfig.propsId}";

        /// <summary>
        /// 道具生效，广播启用事件
        /// </summary>
        public void PropsEnable()
        {
            if (isDisposed) return;
            isActive = true;
            GameEventSystem.Broadcast(new PropsEvent.PropsStatusEnable(ownerId, this));
        }
        /// <summary>
        /// 隐藏道具的视觉组件，但保持 GameObject 存活以支持 Timer 回调
        /// </summary>
        private void HidePropsVisual()
        {

            var renderers = GetComponentsInChildren<Renderer>();
            if (renderers != null)
            {
                foreach (var renderer in renderers)
                {
                    renderer.enabled = false;
                }
            }
            
            var colliders = GetComponentsInChildren<Collider>();
            if (colliders != null)
            {
                foreach (var collider in colliders)
                {
                    collider.enabled = false;
                }
            }
        }
        /// <summary>
        /// 道具失效，广播禁用事件（防重复调用），随后销毁自身 GameObject
        /// </summary>
        public void PropsDisable()
        {
            if (isDisposed) return;
            if (!isActive) return;
            
            isActive = false;
            isDisposed = true;
            GameEventSystem.Broadcast(new PropsEvent.PropsStatusDisable(ownerId, this));
            
            // 在广播之后销毁自身（广播会同步调用 BaseState.OnPropsDisable，
            // 因此 activeProps.Remove 和 ApplyPropsEffect 在销毁前完成）
            Destroy(gameObject);
        }

        /// <summary>
        /// 获取道具是否处于活跃状态
        /// </summary>
        public bool IsActive => isActive;

        /// <summary>
        /// 获取道具类型
        /// </summary>
        public PropsType GetPropsType() => propsConfig.propsType;

        /// <summary>
        /// 获取道具名称
        /// </summary>
        public string GetPropsName() => propsConfig.propsName;

        /// <summary>
        /// 获取道具配置
        /// </summary>
        public PropsConfig GetPropsConfig() => propsConfig;

        /// <summary>
        /// 强制移除道具效果（用于玩家死亡等情况）
        /// 同时取消关联的 Timer，防止后续回调触发
        /// </summary>
        public void ForceDisable()
        {
            // 取消关联的 Timer（使用 ownerId_propsId 组合键）
            var timerKey = GetTimerKey();
            if (GlobalTimerManager.Instance != null && GlobalTimerManager.Instance.HasTimer(timerKey))
            {
                GlobalTimerManager.Instance.CancelTimer(timerKey);
            }
            
            // 调用正常的失效流程
            if (isActive && !isDisposed)
            {
                PropsDisable();
            }
        }

        /// <summary>
        /// 检查道具是否属于指定玩家
        /// </summary>
        public bool BelongsTo(string playerId) => ownerId == playerId;
        
        /// <summary>
        /// 检查道具是否已被释放
        /// </summary>
        public bool IsDisposed => isDisposed;
    }
}
