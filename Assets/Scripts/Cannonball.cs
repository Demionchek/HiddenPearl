using UnityEngine;

public class Cannonball : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 15f;
    public float lifetime = 5f;
    public LayerMask bossLayer;
    public GameObject hitEffect;

    private Rigidbody2D rb;
    private Vector2 direction;
    private float destroyTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        destroyTimer = lifetime;
    }

    public void SetDirection(Vector2 shootDirection)
    {
        direction = shootDirection.normalized;
        rb.linearVelocity = direction * speed;
    }

    private void Update()
    {
        destroyTimer -= Time.deltaTime;
        if (destroyTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Проверяем, находится ли коллайдер на нужном слое
        if (((1 << collision.gameObject.layer) & bossLayer) != 0)
        {
            BossBody bossBody = collision.GetComponent<BossBody>();
            if (bossBody != null)
            {
                bossBody.Hit();
            }

            // Создаем эффект попадания
            if (hitEffect != null)
            {
                GameObject hitObj = Instantiate(hitEffect, transform.position, Quaternion.identity);
                Destroy(hitObj, 0.3f);
            }

            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Дополнительная обработка столкновений с другими объектами
        if (((1 << collision.gameObject.layer) & bossLayer) != 0)
        {
            // Если это не босс, просто уничтожаем ядро
            if (hitEffect != null)
            {
                GameObject hitObj = Instantiate(hitEffect, transform.position, Quaternion.identity);
                Destroy(hitObj, 0.3f);
            }
            Destroy(gameObject);
        }
    }
}