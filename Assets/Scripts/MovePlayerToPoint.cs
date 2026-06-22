using System.Collections;
using Animations;
using Player;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace DefaultNamespace
{
    public class MovePlayerToPoint : MonoBehaviour
    {
        [SerializeField] private Transform targetPoint;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private bool oneShot = true;
        [SerializeField] private UnityEvent onArrived;

        [Inject] private InputHandler _inputHandler;

        private bool _triggered;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (oneShot && _triggered) return;

            LayerMask playerMask = LayerMask.GetMask("Player");
            if ((playerMask.value & (1 << other.gameObject.layer)) == 0) return;

            if (targetPoint == null) return;

            var animController = other.GetComponentInChildren<PlayerAnimationController>()
                                 ?? other.GetComponent<PlayerAnimationController>();
            var spriteRenderer = other.GetComponentInChildren<SpriteRenderer>()
                                 ?? other.GetComponent<SpriteRenderer>();

            _triggered = true;
            StartCoroutine(MoveRoutine(other.transform, animController, spriteRenderer));
        }

        private IEnumerator MoveRoutine(Transform player, PlayerAnimationController animController, SpriteRenderer spriteRenderer)
        {
            _inputHandler.SetInputEnabled(false);

            Vector2 target = targetPoint.position;

            while (((Vector2)player.position - target).sqrMagnitude > 0.05f * 0.05f)
            {
                float directionX = Mathf.Sign(target.x - player.position.x);

                if (spriteRenderer != null)
                    spriteRenderer.flipX = directionX < 0;

                // Speed +1 вправо, -1 влево
                animController?.SetMovementParameters(directionX, true);

                Vector2 newPos = Vector2.MoveTowards(player.position, target, moveSpeed * Time.deltaTime);
                player.position = new Vector3(newPos.x, newPos.y, player.position.z);

                yield return null;
            }

            player.position = new Vector3(target.x, target.y, player.position.z);

            animController?.SetMovementParameters(0f, true);

            _inputHandler.SetInputEnabled(true);

            onArrived?.Invoke();
        }
    }
}
