using System.Collections;
using AI.BossPatterns.Patterns;
using DG.Tweening;
using Player;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Zenject;

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
        public AudioClip deathSound;
        public GameObject clouds1Prefab;
        public GameObject clouds2Prefab;
        public GameObject rainPrefab;
        public Light2D light2D;

        [Header("DeathSettings")]
        public int sceneToLoad = 4;

        private Rigidbody2D rb;
        private AttackManager attackManager;
        private AudioSource audioSource;
        private bool inCombat = true; // Всегда в режиме боя

        [Inject]
        private PlayerController playerController;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            attackManager = GetComponent<AttackManager>();
            audioSource = GetComponent<AudioSource>();

            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player").transform;

            healthPrefab.SetActive(false);

            playerController.OnDeath += ReturnHome;
            //StartAttackPattern();
        }

        private void ReturnHome() => StartCoroutine(MoveToPosition(defaultCombatPosition.position, 1f));

        public void PlayTargetSound(AudioClip clip, bool loop = false)
        {
            if (clip == null) audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
            audioSource.loop = loop;
        }

        public void StopTargetSound()
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        public void PlayDeath()
        {
            Animator animator = GetComponent<Animator>();
            animator.SetTrigger("Roar");
            audioSource.PlayOneShot(deathSound);
            Vector2 targetPostition = transform.position - Vector3.up * 5;
            StartCoroutine(MoveToPosition(targetPostition, 5));
            StartCoroutine(OnDeathCoroutine());
        }

        private IEnumerator OnDeathCoroutine()
        {
            yield return new WaitForSeconds(3f);
            healthPrefab.SetActive(false);
            rainPrefab.SetActive(false);
            Vector3 clouds1Pos = clouds1Prefab.transform.position - Vector3.left * 5;
            Vector3 clouds2Pos = clouds2Prefab.transform.position - Vector3.right * 5;
            clouds1Prefab.transform.DOMove(clouds1Pos, 2f);
            clouds2Prefab.transform.DOMove(clouds2Pos, 2f);
            while (light2D.intensity < 1f)
            {
                light2D.intensity += 0.05f;
                yield return new WaitForSeconds(0.2f);
            }

            SceneManager.LoadScene(sceneToLoad);
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