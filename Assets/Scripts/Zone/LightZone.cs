using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Zone
{
    [RequireComponent(typeof(Collider2D))]
    public class LightZone : MonoBehaviour
    {
        public enum Axis { X, Y }

        [Header("Direction")]
        [SerializeField] private Axis axis = Axis.X;
        [SerializeField] private bool invert = false;

        [Header("Light")]
        [SerializeField] private Light2D targetLight;
        [SerializeField] private float intensityMin = 0f;
        [SerializeField] private float intensityMax = 1f;

        private Collider2D _zone;
        private bool _playerInside;

        private void Awake()
        {
            _zone = GetComponent<Collider2D>();
            _zone.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player")) _playerInside = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player")) _playerInside = false;
        }

        private void Update()
        {
            if (!_playerInside || targetLight == null) return;

            targetLight.intensity = SampleIntensity(Player.PlayerController.Instance.transform.position);
        }

        private float SampleIntensity(Vector3 playerPos)
        {
            Bounds b = _zone.bounds;

            float t = axis == Axis.X
                ? Mathf.InverseLerp(b.min.x, b.max.x, playerPos.x)
                : Mathf.InverseLerp(b.min.y, b.max.y, playerPos.y);

            if (invert) t = 1f - t;

            return Mathf.Lerp(intensityMin, intensityMax, t);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (TryGetComponent(out Collider2D col))
            {
                Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.25f);
                Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            }
        }
#endif
    }
}
