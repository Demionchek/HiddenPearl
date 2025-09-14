using System.Collections;
using AI.BossPatterns.Patterns;
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
        public Transform timelineTarget;
        public AttackPattern timeLinePattern;
        public GameObject healthPrefab;

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

            healthPrefab.SetActive(false);

            //StartAttackPattern();
        }

        public void PlayTargetSound(AudioClip clip)
        {
            if (clip == null) audioSource.Stop();

            audioSource.PlayOneShot(clip);
        }

        public void PlayDeath()
        {
            Animator animator = GetComponent<Animator>();
            animator.SetTrigger("Roar");
            Vector2 targetPostition = transform.position - Vector3.up * 5;
            MoveToPosition(targetPostition, 5);
        }

        public void TimeLineAttackExecute()
        {
            attackManager.ExecuteSingleAttack(timeLinePattern, timelineTarget);
        }

        public void StartAttackPattern()
        {
            if (attackManager != null)
            {
                attackManager.StartAttacks();
            }
            healthPrefab.SetActive(true);
        }

        public void StopAttackPattern()
        {
            if (attackManager != null)
            {
                attackManager.InterruptAttacks();
            }
        }

        public IEnumerator MoveToPosition(Vector2 targetPosition, float duration, float rotationSpeed = 5f)
        {
            float elapsed = 0f;
            Vector2 startPosition = rb.position;

            while (elapsed < duration)
            {
                elapsed += Time.fixedDeltaTime;
                float t = elapsed / duration;

                // Плавное перемещение
                Vector2 newPosition = Vector2.Lerp(startPosition, targetPosition, t);
                rb.MovePosition(newPosition);

                // Плавный поворот к цели
                Vector2 direction = (targetPosition - newPosition).normalized;
                float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
                rb.MoveRotation(Quaternion.Slerp(rb.transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));

                yield return new WaitForFixedUpdate();
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