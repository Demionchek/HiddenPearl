using UnityEngine;

public class RandomSoundPlayer : MonoBehaviour
{
    [Header("Sound Settings")]
    [SerializeField] private AudioClip[] soundClips; // Массив звуков для воспроизведения
    [SerializeField] private AudioSource audioSource; // Источник звука

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
        int randomIndex = Random.Range(0, soundClips.Length);
        audioSource.PlayOneShot(soundClips[randomIndex]);
    }

    // Метод для ручного воспроизведения
    public void PlayRandomSoundNow()
    {
        if (soundClips.Length > 0)
        {
            PlayRandomSound();
        }
    }
}