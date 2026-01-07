using System;
using UnityEngine;

namespace Game_props
{
    public class Explode : MonoBehaviour
    {
        public float destroyTime = 0.5f;
        private void Start()
        {
            Destroy(gameObject, destroyTime);
        }
    }
}