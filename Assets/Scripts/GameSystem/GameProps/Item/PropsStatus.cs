using System;
using Config;
using GameSystem.EventSystem;
using GameSystem.EventSystem.Event;
using GameSystem.GameScene;
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


        public void InitProps(string ownerId, PropsConfig propsConfig)
        {
            this.ownerId = ownerId;
            this.propsConfig = propsConfig;
        }
        

        public void UseProps()
        {
            if (propsConfig.validTime == 0f)
            {
                GlobalTimerManager.Instance.CreateTimer(id, propsConfig.validTime, true ,PropsEnable,PropsDisable);
            }else if (propsConfig.validTime == -1f)
            {
                PropsEnable();
            }
            else
            {
                GlobalTimerManager.Instance.CreateTimer(id,propsConfig.validTime, false ,PropsEnable,PropsDisable);
            }
        }

        public void PropsEnable()
        {
            GameEventSystem.Broadcast(new PropsEvent.PropsStatusEnable(ownerId,this));
        }

        public void PropsDisable()
        {
            GameEventSystem.Broadcast(new PropsEvent.PropsStatusDisable(ownerId,this));
        }
        
        
        
        
        
    }
}

