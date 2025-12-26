using UnityEngine;

public class MoveBetweenPoints : MonoBehaviour
{
    [Header("Точки перемещения")]
    public Transform pointA;   // Первая точка (можно задать в инспекторе)
    public Transform pointB;   // Вторая точка

    [Header("Настройки движения")]
    public float baseSpeed = 0.5f;              // Максимальная скорость
    public float slowDownDistance = 1f;       // На каком расстоянии начинать замедление
    public float minSpeed = 0.1f;             // Минимальная скорость у точки (чтобы не застревал)
    public float waitTime = 0.5f;               // Пауза в точке

    private Vector3 targetPosition;
    private SpriteRenderer spriteRenderer;
    private bool isWaiting = false;

    void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError("Назначьте pointA и pointB!");
            enabled = false;
            return;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();

        transform.position = pointA.position;
        targetPosition = pointB.position;

        // Разворачиваем спрайт в начальном направлении
        FlipSprite();
    }

    void Update()
    {
        if (isWaiting) return;

        // Расстояние до цели
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        // Вычисляем текущую скорость с замедлением
        float currentSpeed = baseSpeed;

        if (distanceToTarget < slowDownDistance)
        {
            // Плавное замедление: чем ближе, тем медленнее
            float speedFactor = Mathf.Clamp01(distanceToTarget / slowDownDistance);
            currentSpeed = Mathf.Lerp(minSpeed, baseSpeed, speedFactor);
        }

        // Двигаемся
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);

        // Если почти дошли — считаем, что прибыли
        if (distanceToTarget < 0.05f)
        {
            Invoke(nameof(SwitchTarget), waitTime);
            isWaiting = true;
        }

        FlipSprite();
    }

    void SwitchTarget()
    {
        // Меняем цель на противоположную
        if (targetPosition == pointA.position)
            targetPosition = pointB.position;
        else
            targetPosition = pointA.position;

        isWaiting = false;
    }

    void FlipSprite()
    {
        if (spriteRenderer == null) return;

        // Определяем направление движения
        Vector3 direction = targetPosition - transform.position;

        if (direction.x > 0)
            spriteRenderer.flipX = false;
        else if (direction.x < 0)
            spriteRenderer.flipX = true;
    }
}