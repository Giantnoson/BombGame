using System;
using UnityEngine;

namespace Game_props
{
    public class Explode : MonoBehaviour
    {
        public float destroyTime = 0.5f;
        private void OnEnable()
        {
            Invoke("ReturnToPool", destroyTime);
        }

        private void ReturnToPool()
        {
            print("回收进池子");
            ExplodePool.Instance.ReturnExplode(gameObject);
        }
    }
}