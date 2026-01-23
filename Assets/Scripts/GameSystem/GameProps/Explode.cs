using System;
using GameSystem.Pool;
using UnityEngine;

namespace GameSystem.GameProps
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
            ExplodePool.Instance.ReturnExplode(gameObject);
        }
    }
}