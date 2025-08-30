using Interfaces;
using UnityEngine;

namespace AI.BossPatterns
{
    // Контроллер луча
    public class BeamController : MonoBehaviour
    {
        private float duration;
        private float width;
        private Vector2 direction;
        private float elapsed;
    
        public void Initialize(Vector2 beamDirection, float beamDuration, float beamWidth)
        {
            direction = beamDirection;
            duration = beamDuration;
            width = beamWidth;
            elapsed = 0f;
        
            // Настройка визуала луча
            transform.localScale = new Vector3(width, 1f, 1f);
            transform.right = direction;
        }
    
        void Update()
        {
            elapsed += Time.deltaTime;
        
            if (elapsed >= duration)
            {
                Destroy(gameObject);
            }
        }
    
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                // Нанесение урона игроку
                other.GetComponent<IHittable>().Hit();
            }
        }
    }
}