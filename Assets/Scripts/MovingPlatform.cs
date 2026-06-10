using UnityEngine;
using UnityEngine.Events;

public class MovingPlatform : MonoBehaviour
{
    [Header("Points")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float arrivalThreshold = 0.05f;

    [Header("Events")]
    [SerializeField] private UnityEvent onReachA;
    [SerializeField] private UnityEvent onMoveBegin;
    [SerializeField] private UnityEvent onReachB;

    private Transform _target;
    private bool _isMoving;

    private void Awake()
    {
        transform.position = pointA.position;
    }

    public void Activate()
    {
        if(_isMoving) return;

        float distToA = Vector3.Distance(transform.position, pointA.position);
        float distToB = Vector3.Distance(transform.position, pointB.position);

        _target = distToA <= distToB ? pointB : pointA;
        onMoveBegin?.Invoke();
        _isMoving = true;
    }

    public void MoveToA()
    {
        _target = pointA;
        _isMoving = true;
    }

    public void MoveToB()
    {
        _target = pointB;
        _isMoving = true;
    }

    private void FixedUpdate()
    {
        if (!_isMoving || _target == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            _target.position,
            speed * Time.fixedDeltaTime
        );

        if (Vector3.Distance(transform.position, _target.position) <= arrivalThreshold)
        {
            transform.position = _target.position;
            _isMoving = false;

            if (_target == pointA)
                onReachA?.Invoke();
            else if (_target == pointB)
                onReachB?.Invoke();
        }
    }
}
