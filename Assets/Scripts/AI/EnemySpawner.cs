using System;
using System.Threading.Tasks;
using DefaultNamespace;
using NUnit.Framework;
using Player;
using Zenject;

namespace AI
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class EnemySpawner : MonoBehaviour
    {
        [Header("Enemy References")]
        [SerializeField] private List<GameObject> enemyPool = new List<GameObject>();
        
        [Header("Enemy Patroll Points")]
        [SerializeField] private List<Transform> patrollPoints = new List<Transform>();
        
        [Header("Enemy Spawn Points")]
        [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

        [Header("Activation Settings")]
        [SerializeField] private float activationInterval = 3f;
        [SerializeField] private int totalEnemiesToActivate = 10;
        [SerializeField] private bool autoStartActivation = false;

        public GameObject dialogTrigger;
        public GameObject walls;

        private List<GameObject> activeEnemies = new List<GameObject>();
        private Coroutine activationCoroutine;
        private int enemiesActivatedCount = 0;
        private bool allEnemiesActivated = false;

        // Событие для оповещения об уничтожении всех врагов
        public System.Action OnAllEnemiesEliminated;
        
        [Inject]
        PlayerController playerController;
        
        [Inject]
        DialogueSystem dialogSystem;

        void Start()
        {
            if (enemyPool.Count == 0)
            {
                Debug.LogError("No enemy references assigned!");
                return;
            }

            // Деактивируем всех врагов в пуле при старте
            foreach (var enemy in enemyPool)
            {
                if (enemy != null)
                {
                    enemy.SetActive(false);
                }
            }

            if (autoStartActivation)
            {
                StartActivation();
            }

            playerController.OnRevive += OnRevivePlayer;
        }

        public void StartActivation()
        {
            if (activationCoroutine == null)
            {
                activationCoroutine = StartCoroutine(ActivateEnemiesRoutine());
            }
        }

        public void StopActivation()
        {
            if (activationCoroutine != null)
            {
                StopCoroutine(activationCoroutine);
                activationCoroutine = null;
            }
        }

        private IEnumerator ActivateEnemiesRoutine()
        {
            while (enemiesActivatedCount < enemyPool.Count)
            {
                yield return new WaitForSeconds(activationInterval);

                ActivateEnemy();
                enemiesActivatedCount++;

                // Проверяем, если это последний враг
                if (enemiesActivatedCount >= enemyPool.Count)
                {
                    allEnemiesActivated = true;
                    break;
                }
            }
        }

        private void ActivateEnemy()
        {
            // Находим первого неактивного врага в пуле
            GameObject enemyToActivate = null;
            foreach (var enemy in enemyPool)
            {
                if (enemy != null && !enemy.activeInHierarchy && !activeEnemies.Contains(enemy))
                {
                    enemyToActivate = enemy;
                    break;
                }
            }

            if (enemyToActivate == null)
            {
                Debug.LogWarning("No available enemies to activate!");
                return;
            }

            // Активируем врага
            enemyToActivate.SetActive(true);
            activeEnemies.Add(enemyToActivate);

            // Добавляем обработчик смерти
            BaseEnemy baseEnemy = enemyToActivate.GetComponent<BaseEnemy>();
            if (baseEnemy != null)
            {
                baseEnemy.patrolPoints = patrollPoints;
                baseEnemy.SetPlayerAsTarget();
                baseEnemy.Init();
                baseEnemy.OnDeath += HandleEnemyDeath;
            }
        }

        private void HandleEnemyDeath(BaseEnemy baseEnemy)
        {
            // Удаляем из списка активных врагов
            activeEnemies.Remove(baseEnemy.gameObject);

            // TODO: Счетчик смертей

            // Проверяем, все ли враги уничтожены и все ли были активированы
            if (allEnemiesActivated && activeEnemies.Count == 0)
            {
                AllEnemiesEliminated();
                foreach (GameObject enemyObj in enemyPool)
                {
                    BaseEnemy enemy = enemyObj.GetComponent<BaseEnemy>();
                    enemy.Revive();
                    enemy.loopOverrideState = true;
                    enemy.overrideAIState = AIState.idle;
                }
            }
        }

        private void AllEnemiesEliminated()
        {
            Debug.Log("Все враги уничтожены!");
            
            // Вызываем событие
            OnAllEnemiesEliminated?.Invoke();
            
            dialogSystem.InitDialogue(1);
        }

        // Метод для активации конкретного врага по индексу
        public void ActivateSpecificEnemy(int index)
        {
            if (index >= 0 && index < enemyPool.Count && enemyPool[index] != null)
            {
                if (!enemyPool[index].activeInHierarchy && !activeEnemies.Contains(enemyPool[index]))
                {
                    enemyPool[index].SetActive(true);
                    activeEnemies.Add(enemyPool[index]);
                    enemiesActivatedCount++;

                    // Добавляем обработчик смерти
                    BaseEnemy baseEnemy = enemyPool[index].GetComponent<BaseEnemy>();
                    if (baseEnemy != null)
                    {
                        baseEnemy.OnDeath += HandleEnemyDeath;
                    }
                }
            }
        }

        private void OnRevivePlayer()
        {
            StopActivation();
            dialogTrigger.SetActive(true);
            walls.SetActive(false);
            enemiesActivatedCount = 0;
            activeEnemies.Clear();
            foreach (GameObject enemyObj in enemyPool)
            {
                try
                {
                    BaseEnemy enemy = enemyObj.GetComponent<BaseEnemy>();
                    enemy.Revive();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                enemyObj.SetActive(false);
                enemyObj.transform.position = spawnPoints[Random.Range(0, spawnPoints.Count)].position;
            }
            
        }

        // Метод для деактивации всех врагов
        public void DeactivateAllEnemies()
        {
            StopActivation();

            foreach (var enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    enemy.SetActive(false);
                    
                    // Убираем обработчики событий
                    BaseEnemy baseEnemy = enemy.GetComponent<BaseEnemy>();
                    if (baseEnemy != null)
                    {
                        baseEnemy.OnDeath -= HandleEnemyDeath;
                    }
                }
            }

            activeEnemies.Clear();
            enemiesActivatedCount = 0;
            allEnemiesActivated = false;
        }

        // Методы для динамического изменения параметров
        public void AddEnemyToPool(GameObject newEnemy)
        {
            if (!enemyPool.Contains(newEnemy))
            {
                enemyPool.Add(newEnemy);
                if (newEnemy != null)
                {
                    newEnemy.SetActive(false);
                }
            }
        }

        public void RemoveEnemyFromPool(GameObject enemyToRemove)
        {
            if (enemyPool.Contains(enemyToRemove))
            {
                enemyPool.Remove(enemyToRemove);
                
                // Если враг был активен, удаляем его из активного списка
                if (activeEnemies.Contains(enemyToRemove))
                {
                    activeEnemies.Remove(enemyToRemove);
                    
                    // Убираем обработчики событий
                    BaseEnemy baseEnemy = enemyToRemove.GetComponent<BaseEnemy>();
                    if (baseEnemy != null)
                    {
                        baseEnemy.OnDeath -= HandleEnemyDeath;
                    }
                }
            }
        }

        public void SetActivationInterval(float newInterval)
        {
            activationInterval = newInterval;
        }

        public void SetTotalEnemiesToActivate(int newTotal)
        {
            totalEnemiesToActivate = newTotal;
        }

        public int GetRemainingEnemies()
        {
            return activeEnemies.Count;
        }

        public int GetTotalEnemiesActivated()
        {
            return enemiesActivatedCount;
        }

        public bool AreAllEnemiesEliminated()
        {
            return allEnemiesActivated && activeEnemies.Count == 0;
        }

        // Очистка при уничтожении объекта
        void OnDestroy()
        {
            StopActivation();
            
            playerController.OnRevive -= OnRevivePlayer;
            
            // Убираем все обработчики событий
            foreach (var enemy in enemyPool)
            {
                if (enemy != null)
                {
                    BaseEnemy baseEnemy = enemy.GetComponent<BaseEnemy>();
                    if (baseEnemy != null)
                    {
                        baseEnemy.OnDeath -= HandleEnemyDeath;
                    }
                }
            }
        }
    }
}