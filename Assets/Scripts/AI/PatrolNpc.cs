using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AI
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PatrolNpc : MonoBehaviour
    {
        [Header("Patrol Points")]
        [SerializeField] private Transform[] patrolPoints;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float arrivalThreshold = 0.1f;

        [Header("Player Detection")]
        [SerializeField] private float detectionDistance = 1.5f;
        [SerializeField] private float detectionHeight = 0.5f;

        [Header("Attack")]
        [SerializeField] private float attackDelay = 1f;
        [SerializeField] private float resumeDelay = 0.5f;

        [Header("Other")]
        [SerializeField] private Light2D light2Dr;
        [SerializeField] private Light2D light2Dl;

        private Rigidbody2D _rb;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;

        private Transform _target;
        private int _currentPointIndex;
        private float _facingSign = 1f;

        private enum State { Idle, Moving, Attacking }
        private State _state = State.Idle;

        private static readonly int IdleHash   = Animator.StringToHash("idle");
        private static readonly int RunHash    = Animator.StringToHash("run");
        private static readonly int AttackHash = Animator.StringToHash("attack");

        private int _playerLayerMask;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _playerLayerMask = 1 << LayerMask.NameToLayer("Player");

            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                _currentPointIndex = 0;
                _target = patrolPoints[_currentPointIndex];
                // Сразу ориентируемся в сторону первой точки
                Vector2 toFirst = (Vector2)_target.position - (Vector2)transform.position;
                if (toFirst.sqrMagnitude > 0.0001f)
                    SetFacing(Mathf.Sign(toFirst.x));
            }
        }

        private void Update()
        {
            if (_state == State.Attacking) return;

            if (DetectPlayer())
            {
                StartCoroutine(AttackRoutine());
                return;
            }

            MoveToTarget();
        }

        private void MoveToTarget()
        {
            if (_target == null) return;

            Vector2 toTarget = (Vector2)_target.position - (Vector2)transform.position;

            if (toTarget.magnitude <= arrivalThreshold)
            {
                _rb.linearVelocity = Vector2.zero;
                SetState(State.Idle);
                PickNextPoint();
                return;
            }

            Vector2 moveDir = toTarget.normalized;
            float sign = Mathf.Sign(moveDir.x);
            SetFacing(sign);
            SwitchLight(sign);
            _rb.linearVelocity = moveDir * moveSpeed;
            SetState(State.Moving);
        }

        private void SwitchLight(float sign)
        {
            switch (sign)
            {
                case -1:
                    if (light2Dl != null) light2Dl.enabled = true;
                    if (light2Dr != null) light2Dr.enabled = false;
                    break;
                case 1:
                    if (light2Dl != null) light2Dl.enabled = false;
                    if (light2Dr != null) light2Dr.enabled = true;
                    break;
            }
        }

        private void SetFacing(float sign)
        {
            _facingSign = sign;
            _spriteRenderer.flipX = sign < 0f;
        }

        private void SetState(State newState)
        {
            if (_state == newState) return;
            _state = newState;

            switch (_state)
            {
                case State.Idle:
                    _animator.SetTrigger(IdleHash);
                    break;
                case State.Moving:
                    _animator.SetTrigger(RunHash);
                    break;
                case State.Attacking:
                    _animator.SetTrigger(AttackHash);
                    break;
            }
        }

        private void PickNextPoint()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;
            if (patrolPoints.Length == 1) { _target = patrolPoints[0]; return; }

            int next;
            do { next = Random.Range(0, patrolPoints.Length); }
            while (next == _currentPointIndex);

            _currentPointIndex = next;
            _target = patrolPoints[_currentPointIndex];
        }

        private bool DetectPlayer()
        {
            Vector2 origin = (Vector2)transform.position + Vector2.right * _facingSign * detectionDistance * 0.5f;

            RaycastHit2D hit = Physics2D.BoxCast(
                origin,
                new Vector2(detectionDistance, detectionHeight),
                0f,
                Vector2.right * _facingSign,
                0f,
                _playerLayerMask
            );

            return hit.collider != null;
        }

        private IEnumerator AttackRoutine()
        {
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
            SetState(State.Attacking);

            yield return new WaitForSeconds(attackDelay);

            SetState(State.Idle);

            yield return new WaitForSeconds(resumeDelay);

            _state = State.Idle; // сбрасываем без SetTrigger повторно
        }

        private void OnDrawGizmosSelected()
        {
            if (patrolPoints == null) return;

            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] == null) continue;

                Gizmos.color = i == 0 ? Color.green : Color.red;
                Gizmos.DrawSphere(patrolPoints[i].position, 0.12f);

                if (i + 1 < patrolPoints.Length && patrolPoints[i + 1] != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                }
            }

            // Зона обнаружения
            Gizmos.color = Color.cyan;
            float facing = Application.isPlaying ? _facingSign : 1f;
            Vector3 boxCenter = transform.position + Vector3.right * facing * detectionDistance * 0.5f;
            Gizmos.DrawWireCube(boxCenter, new Vector3(detectionDistance, detectionHeight, 0f));
        }
    }
}
