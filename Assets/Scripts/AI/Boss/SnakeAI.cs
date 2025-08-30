using UnityEngine;

namespace AI.BossPatterns
{

    public class SnakeAI : MonoBehaviour
    {
        [Header("Combat Settings")]
        public float combatDistance = 3f;
        public float rotationSpeed = 5f;
        public float rotationCorrection = 45;
        
        [Header("References")]
        public Transform player;
        public Transform defaultCombatPosition;
        
        private Rigidbody2D rb;
        private AttackManager attackManager;
        private bool inCombat = true; // Всегда в режиме боя
        
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            attackManager = GetComponent<AttackManager>();
            
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player").transform;
            
            StartAttackPattern();
        }
        
        void FixedUpdate()
        {
            if (inCombat && !attackManager.IsAttacking() && player != null)
            {
                //MaintainCombatPosition();
                //FacePlayer();
            }
        }
        
        private void MaintainCombatPosition()
        {
            // Держим дистанцию от игрока
            Vector2 desiredPosition = (Vector2)player.position + (Vector2)(defaultCombatPosition.position - transform.position).normalized * combatDistance;
            rb.MovePosition(Vector2.Lerp(rb.position, desiredPosition, 0.1f * Time.fixedDeltaTime));
        }
        
        private void FacePlayer()
        {
            // Поворачиваемся к игроку
            Vector2 direction = (player.position - transform.position).normalized;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rb.MoveRotation(Mathf.LerpAngle(rb.rotation, targetAngle - rotationCorrection, rotationSpeed * Time.fixedDeltaTime));
        }
        
        public void StartAttackPattern()
        {
            if (attackManager != null)
            {
                attackManager.StartAttacks();
            }
        }
        
        public void StopAttackPattern()
        {
            if (attackManager != null)
            {
                attackManager.InterruptAttacks();
            }
        }
        
        // Для визуализации в редакторе
        void OnDrawGizmosSelected()
        {
            if (player != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(player.position, combatDistance);
            }
        }
    }
}