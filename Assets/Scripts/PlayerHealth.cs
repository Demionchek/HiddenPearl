using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private Image bar;
    [SerializeField] private Image[] extraHealthImages; // Массив дополнительных сердечек
    [SerializeField] private int maxBaseHealth = 3; // Максимальное здоровье для основной шкалы

    private IHealth health;

    private void Start()
    {
        health = target.GetComponent<IHealth>();

        if (health == null)
        {
            Debug.LogError("IHealth component doesn't exist on gameobject! " + gameObject.name);
            return;
        }

        UpdateHealthDisplay();
        health.healthChanged += OnHealthChanged;
    }

    private void OnHealthChanged(int value)
    {
        UpdateHealthDisplay();
    }

    private void UpdateHealthDisplay()
    {
        int currentHealth = health.GetHealth();
        int maxHealth = health.GetMaxHealth();

        // Обновляем основную шкалу (показываем до maxBaseHealth)
        if (currentHealth <= maxBaseHealth)
        {
            // Если здоровье в пределах базового - показываем фактическое заполнение
            bar.fillAmount = (float)currentHealth / maxBaseHealth;
        }
        else
        {
            // Если здоровье больше базового - шкала всегда полная
            bar.fillAmount = 1f;
        }

        UpdateExtraHealthImages(currentHealth);
    }

    private void UpdateExtraHealthImages(int currentHealth)
    {
        if (extraHealthImages == null || extraHealthImages.Length == 0)
            return;

        // Количество дополнительных сердечек = здоровье сверх базового максимума
        int extraHealth = currentHealth - maxBaseHealth;

        // Ограничиваем количеством доступных изображений
        int activeExtraImages = Mathf.Clamp(extraHealth, 0, extraHealthImages.Length);

        // Активируем/деактивируем сердечки
        for (int i = 0; i < extraHealthImages.Length; i++)
        {
            if (extraHealthImages[i] != null)
            {
                bool shouldBeActive = i < activeExtraImages;
                extraHealthImages[i].gameObject.SetActive(shouldBeActive);

                // Можно добавить анимацию или изменение цвета
                if (shouldBeActive)
                {
                    extraHealthImages[i].color = Color.white;
                }
            }
        }
    }

    // Метод для проверки, можно ли добавить еще сердечко
    public bool CanAddExtraHealth()
    {
        if (health == null) return false;

        int currentHealth = health.GetHealth();
        int maxHealth = health.GetMaxHealth();
        int currentExtraHealth = currentHealth - maxBaseHealth;

        return currentHealth < maxHealth && currentExtraHealth < extraHealthImages.Length;
    }

    private void OnDestroy()
    {
        if (health != null)
            health.healthChanged -= OnHealthChanged;
    }
}
}