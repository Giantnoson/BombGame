using System.Collections.Generic;
using GameSystem.GameScene.MessageScene;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu.Pool
{
    public class MessagePool : ObjectPool<Message>
    {
        public static MessagePool Instance { get; private set; }
        
        protected override void InitializeSingleton()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public  GameObject GetMessage()
        {
            return base.GetObjectFromPool();
        }
    }
}