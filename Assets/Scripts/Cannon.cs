using Interfaces;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class Cannon : MonoBehaviour, IHittable
{
    [Header("References")]
    public GameObject attackSign;
    public Image chargeFillImage;
    public GameObject cannonballPrefab;
    public Transform firePoint;
    public Animator cannonAnimator;
    public Light2D cannonLight;

    [Header("Settings")]
    public float chargeSpeed = 0.5f;
    public float maxChargeTime = 3f;

    [Header("Audio")]
    public AudioClip chargingSound;
    public AudioClip fireSound;
    [SerializeField] private AudioSource shotSource;
    [SerializeField] private AudioSource chargeSource;

    private float currentCharge = 0f;
    private bool isPlayerNear = false;
    private bool isCharging = false;
    private bool isFullyCharged = false;

    private void Start()
    {
        chargeFillImage.fillAmount = 0f;
        attackSign.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerNear)
        {
            HandleCannonInteraction();
        }
    }

    private void HandleCannonInteraction()
    {
        if (!isFullyCharged)
        {
            // Начало зарядки
            if (!isCharging)
            {
                StartCharging();
            }

            // Процесс зарядки
            if (isCharging)
            {
                currentCharge += chargeSpeed * Time.deltaTime;
                chargeFillImage.fillAmount = Mathf.Clamp01(currentCharge / maxChargeTime);

                if (currentCharge >= maxChargeTime)
                {
                    CompleteCharging();
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            isPlayerNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            isPlayerNear = false;

            // Останавливаем зарядку если игрок отошел
            if (isCharging)
            {
                StopCharging();
            }
        }
    }

    private void StartCharging()
    {
        isCharging = true;
        if (chargingSound != null)
        {
            if (chargeSource.clip != chargingSound)
            {
                chargeSource.clip = chargingSound;
                chargeSource.loop = true;
            }
            chargeSource.Play();
        }
    }

    private void StopCharging()
    {
        isCharging = false;
        chargeSource.Pause();
    }

    private void CompleteCharging()
    {
        StopCharging();
        isFullyCharged = true;
        currentCharge = maxChargeTime;
        chargeFillImage.fillAmount = 1f;
        attackSign.SetActive(true);
    }

    private void FireCannon()
    {
        if (isFullyCharged)
        {
            cannonAnimator.SetTrigger("Fire");

            if (fireSound != null)
            {
                shotSource.PlayOneShot(fireSound);
                shotSource.loop = false;
            }

            ResetCannon();
        }
    }

    public void Shot()
    {
        if (cannonballPrefab != null && firePoint != null)
        {
            cannonLight.enabled = true;
            GameObject cannonball = Instantiate(cannonballPrefab, firePoint.position, firePoint.rotation);
            Cannonball cannonballScript = cannonball.GetComponent<Cannonball>();
            if (cannonballScript != null)
            {
                cannonballScript.SetDirection(transform.right);
            }
        }
    }

    public void StopShot()
    {
        cannonLight.enabled = false;
    }

    private void ResetCannon()
    {
        isFullyCharged = false;
        isCharging = false;
        currentCharge = 0f;
        chargeFillImage.fillAmount = 0f;
        attackSign.SetActive(false);
    }

    public void Hit()
    {
        if (isFullyCharged)
        {
            FireCannon();
        }
    }

    public void Kill()
    {
        throw new System.NotImplementedException();
    }
}