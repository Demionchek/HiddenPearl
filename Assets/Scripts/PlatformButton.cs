using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class PlatformButton : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private UnityEvent onPressed;
    [SerializeField] private UnityEvent onReleased;

    private Animator _animator;
    private bool _isPressed;
    private int _overlappingCount; // считаем вошедших, чтобы выход одного из нескольких не снимал нажатие

    private static readonly int PressedHash = Animator.StringToHash("pressed");

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        // Триггер коллайдер обязателен
        var col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"[PlatformButton] Коллайдер на {name} должен быть триггером.", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        _overlappingCount++;

        if (!_isPressed)
            Press();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        // Страховка: если кнопка почему-то не нажата, а игрок внутри — нажимаем
        if (!_isPressed)
            Press();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        _overlappingCount = Mathf.Max(0, _overlappingCount - 1);

        if (_overlappingCount == 0 && _isPressed)
            Release();
    }

    private void Press()
    {
        _isPressed = true;
        _animator.SetBool(PressedHash, true);
        onPressed?.Invoke();
    }

    private void Release()
    {
        _isPressed = false;
        _animator.SetBool(PressedHash, false);
        onReleased?.Invoke();
    }

    private static bool IsPlayer(Collider2D other)
    {
        return other.CompareTag("Player");
    }
}
