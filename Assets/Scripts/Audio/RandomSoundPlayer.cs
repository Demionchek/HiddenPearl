using UnityEngine;

public class RandomSoundPlayer : MonoBehaviour
{
    [Header("Sound Settings")]
    [SerializeField] private AudioClip[] soundClips; // Массив звуков для воспроизведения
    [SerializeField] private AudioSource audioSource; // Источник звука
    [SerializeField] private float cooldownDuration = 0.5f; // Длительность cooldown в секундах

    public bool doNotInterrupt = false;

    private float _nextPlayTime;

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void PlayRandomSound()
    {
        // Проверяем cooldown, если включен режим doNotInterrupt
        if (doNotInterrupt)
        {
            if (Time.time < _nextPlayTime)
            {
                return;
            }
        }

        int randomIndex = Random.Range(0, soundClips.Length);
        audioSource.PlayOneShot(soundClips[randomIndex]);

        // Устанавливаем время следующего возможного воспроизведения
        if (doNotInterrupt)
        {
            _nextPlayTime = Time.time + cooldownDuration;
        }
    }

    // Метод для ручного воспроизведения
    public void PlayRandomSoundNow()
    {
        if (soundClips.Length > 0)
        {
            PlayRandomSound();
        }
    }

    // Метод для принудительного сброса cooldown
    public void ResetCooldown()
    {
        _nextPlayTime = 0f;
    }

    // Свойство для проверки, активен ли cooldown
    public bool IsOnCooldown => doNotInterrupt && Time.time < _nextPlayTime;
}