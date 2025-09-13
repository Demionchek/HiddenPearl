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
        private AudioSource audioSource;
        private bool inCombat = true; // Всегда в режиме боя

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            attackManager = GetComponent<AttackManager>();
            audioSource = GetComponent<AudioSource>();

            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player").transform;

            StartAttackPattern();
        }

        public void PlayTargetSound(AudioClip clip)
        {
            if (clip == null) audioSource.Stop();

            audioSource.PlayOneShot(clip);
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