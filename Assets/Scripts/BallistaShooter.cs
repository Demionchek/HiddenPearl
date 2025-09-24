using System;
using System.Collections;
using AI.BossPatterns;
using ObjectPool;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class BallistaShooter : MonoBehaviour
    {
         [SerializeField] private GameObject projectile;
         [SerializeField] private Transform firePoint;
         [SerializeField] private float fireRate = 2f;
         [SerializeField] private float arrowSpeed = 2f;
         private GameObjectPool pool;
         private Animator animator;
         private AudioSource audioSource;

         private void Start()
         {
             pool = new GameObjectPool(projectile, 5);
             animator = GetComponent<Animator>();
             audioSource = GetComponent<AudioSource>();
             StartCoroutine(ShootWithDelay());
         }

         private IEnumerator ShootWithDelay()
         {
             float random = Random.Range(0f, 5f);
             yield return new WaitForSeconds(random);

             while(true)
             {
                 yield return new WaitForSeconds(fireRate);
                 animator.SetTrigger("Attack");
             }
         }

         private void OnAttack()
         {
             GameObject arrow = pool.Get();
             arrow.transform.position = firePoint.position;
             Projectile projectileScript = arrow.GetComponent<Projectile>();
             projectileScript.Initialize(-Vector2.right, arrowSpeed, pool);
             audioSource.Play();
         }
    }
}