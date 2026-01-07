using UnityEngine;

namespace Game_props
{
    public class Bomb : MonoBehaviour
    {
        
        public float fuseTime = 3f;
        public float explosionRadius = 5f;
        public float explosionForce = 10f;
        public GameObject explosionEffect;

        private void Start()
        {
            Invoke("Explode", fuseTime);
        }
        
        
        public void Explode()
        {
            // TODO: Add explosion effect
            Destroy(gameObject);
        }
    }
}