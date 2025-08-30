using Interfaces;
using UnityEngine;

namespace AI.BossPatterns
{
    public class Projectile : MonoBehaviour
    {
        protected Vector2 direction;
        protected float speed;
        protected int damage = 5;
    
        public void Initialize(Vector2 moveDirection, float moveSpeed, int projectileDamage = 5)
        {
            direction = moveDirection.normalized;
            speed = moveSpeed;
            damage = projectileDamage;
        
            // Поворот спрайта в направлении движения
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    
        void Update()
        {
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }
    
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                other.GetComponent<IHittable>().Hit();
                Destroy(gameObject);
            }
        }
    }
}