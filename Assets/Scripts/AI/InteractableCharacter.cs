using System;
using DefaultNamespace;
using Interfaces;
using Player;
using UnityEngine;
using Zenject;

namespace AI
{
    public class InteractableCharacter : MonoBehaviour, IInteractable
    {
        [SerializeField] private bool canAttack = false;
        [SerializeField] private bool canHit = false;
        [SerializeField] private float hitDistance = 0.2f;
        [Space(5)]
        [SerializeField] private bool hasDialog = false;
        [SerializeField] private DialogType dialogType;
        [Space(5)]
        [SerializeField] private bool unlocksShape = false;
        [Space(5)]
        [SerializeField] private float interactDelay = 0.5f;
        [Space(5)]
        [SerializeField] private bool isCheckPoint = false;
        [SerializeField] private int checkPointIndex;
        [Space(5)]
        [SerializeField] private GameObject interactSign;
        [SerializeField] private DoorInteractor doorInteractor;
        [SerializeField] private GameObject objToActivate;
        public bool doorCondition = false;
        [Space(5)]
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip attackSound;
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private AudioClip InteractSound_1;
        [SerializeField] private AudioClip InteractSound_2;

        [Inject]
        private DialogueSystem dialogueSystem;
        [Inject]
        private PlayerController playerController;
        [Inject]
        private CheckPoints checkPoints;

        private Animator animator;
        private SpriteRenderer spriteRenderer;

        private float lastTime;
        private float currentTime;
        private bool canInteract = true;

        private bool isFlip;

        public event Action OnInteract;

        private void Start()
        {
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (currentTime - lastTime > lastTime + interactDelay)
            {
                canInteract = true;
            }

            isFlip = spriteRenderer.flipX;

            currentTime = Time.time;
        }

        public void Interact()
        {
            lastTime = Time.time;
            canInteract = false;
            interactSign.SetActive(false);

            OnInteract?.Invoke();

            if(hasDialog && !doorCondition)
                dialogueSystem.InitDialogue((int)dialogType);

            if (canAttack)
                animator.SetTrigger("Attack");

            if (isCheckPoint)
                checkPoints.SetCurrentCheckpoint(checkPointIndex);

            if (objToActivate != null) objToActivate.SetActive(true);

            if (doorInteractor != null)
            {
                if (doorCondition)
                {
                    if (InteractSound_1 != null)
                        audioSource.PlayOneShot(InteractSound_1);

                    doorInteractor.OpenManual(true);
                } else
                {
                    if (InteractSound_2 != null)
                        audioSource.PlayOneShot(InteractSound_2);
                }
            }
        }

        public void PlayAttackSound()
        {
            audioSource?.PlayOneShot(attackSound);
        }

        private void CircleCastAll()
        {
            Vector2 origin = transform.position + new Vector3(0, 0.15f, 0);
            float radius = 0.2f;
            Vector2 direction = (isFlip ? new Vector2(-hitDistance,0) : new Vector2(hitDistance,0));
            origin += direction;

            // Выполняем CircleCast
            // Получаем все коллайдеры в радиусе
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(origin, radius);

            if (hitColliders.Length == 0)
            {
                Debug.Log("В радиусе нет объектов.");
                return;
            }

            // Перебираем все найденные коллайдеры
            foreach (Collider2D collider in hitColliders)
            {
                Debug.Log($"Обнаружен объект: {collider.name}");

                // Проверяем, есть ли у него компонент для взаимодействия
                IHittable hittable = collider.GetComponent<IHittable>();
                if (hittable != null)
                {
                    hittable.Hit(); // Взаимодействуем
                }
            }
        }

        public void SwitchLine(int lineIndex)
        {
            dialogType = (DialogType)lineIndex;
        }

        private void OnDrawGizmos()
        {
            Vector2 origin = transform.position + new Vector3(0, 0.15f, 0);
            float radius = 0.2f;
            float distance = 0.02f;
            Vector2 direction = (isFlip ? new Vector2(-hitDistance,0) : new Vector2(hitDistance,0));
            origin += direction;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(origin, radius);
        }
    }
}