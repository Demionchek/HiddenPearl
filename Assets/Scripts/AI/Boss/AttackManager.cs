using System.Collections;
using AI.BossPatterns.Patterns;
using Player;
using UnityEngine;
using Zenject;

namespace AI.BossPatterns
{
    public class AttackManager : MonoBehaviour
    {
        [System.Serializable]
        public class AttackPhase
        {
            public string phaseName;
            public AttackPattern[] attackPatterns;
            public int minAttacks = 1;
            public int maxAttacks = 3;
            public float phaseCooldown = 2f;
        }

        [Header("Attack Settings")]
        public AttackPhase[] phases;
        public float timeBetweenAttacks = 1.5f;
        public bool loopPhases = true;

        [Header("References")]
        public Transform player;
        public Animator snakeAnimator;
        public BossHealthBar bossHealthBar;

        private Rigidbody2D rb;
        private SnakeAI snakeAI;
        private int currentPhase = 0;
        private bool isAttacking = false;
        private Coroutine attackCoroutine;

        [Inject]
        private PlayerController playerController;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            snakeAI = GetComponent<SnakeAI>();
            InitializePatterns();

            bossHealthBar.OnDeath += InitDeath;
            bossHealthBar.OnHit += IncreasePhase;
            playerController.OnDeath += InterruptAttacks;
            playerController.OnRevive += StartAttacks;
        }

        private void InitDeath()
        {
            InterruptAttacks();
            snakeAI.PlayDeath();
        }

        public void StartAttacks()
        {
            currentPhase = 0;
            if (!isAttacking)
            {
                attackCoroutine = StartCoroutine(AttackCycle());
            }
        }

        private void InitializePatterns()
        {
            foreach (AttackPhase phase in phases)
            {
                foreach (AttackPattern pattern in phase.attackPatterns)
                {
                    pattern.Initialize(snakeAI, rb, snakeAnimator);

                    // Передаем ссылку на игрока если нужно
                    if (pattern is DashArcAttack dashAttack && player != null)
                    {
                        dashAttack.playerTarget = player;
                    }
                }
            }
        }

        private IEnumerator AttackCycle()
        {
            isAttacking = true;

            while (loopPhases)
            {
                yield return StartCoroutine(ExecutePhase(currentPhase));

                // Переход к следующей фазе
                //currentPhase = (currentPhase + 1) % phases.Length;
                yield return new WaitForSeconds(phases[currentPhase].phaseCooldown);
            }

            isAttacking = false;
        }

        public void ExecuteSingleAttack(AttackPattern pattern, Transform target)
        {
            pattern.playerTarget = target;
            StartCoroutine(pattern.ExecutePattern());
        }

        public void IncreasePhase()
        {
            if (currentPhase == phases.Length - 1) return;

            currentPhase = (currentPhase + 1) % phases.Length;
        }

        private IEnumerator ExecutePhase(int phaseIndex)
        {
            AttackPhase phase = phases[phaseIndex];
            int attackCount = Random.Range(phase.minAttacks, phase.maxAttacks + 1);

            for (int i = 0; i < attackCount; i++)
            {
                // Выбор случайного паттерна из фазы
                AttackPattern pattern = phase.attackPatterns[Random.Range(0, phase.attackPatterns.Length)];

                pattern.playerTarget = player;

                // Запуск паттерна
                yield return StartCoroutine(pattern.ExecutePattern());

                // Пауза между атаками
                if (i < attackCount - 1)
                {
                    yield return new WaitForSeconds(timeBetweenAttacks);
                }
            }
        }

        public void InterruptAttacks()
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
            }

            // Останавливаем все паттерны
            foreach (AttackPhase phase in phases)
            {
                foreach (AttackPattern pattern in phase.attackPatterns)
                {
                    pattern.StopAllCoroutines();
                    pattern.DisableExtras();
                }
            }

            isAttacking = false;
        }

        public void SetPhase(int newPhase)
        {
            currentPhase = Mathf.Clamp(newPhase, 0, phases.Length - 1);
        }

        public bool IsAttacking()
        {
            return isAttacking;
        }
    }
}