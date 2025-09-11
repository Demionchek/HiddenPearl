using UnityEngine;

public class BossBody : MonoBehaviour
{
    [Header("Boss Settings")]
    public int maxHealth = 100;
    public GameObject deathEffect;

    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void Hit()
    {
        currentHealth -= 25; // Урон от ядра

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            Debug.Log("Boss hit! Health: " + currentHealth);
            // Можно добавить анимацию получения урона
        }
    }

    private void Die()
    {
        Debug.Log("Boss defeated!");

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}