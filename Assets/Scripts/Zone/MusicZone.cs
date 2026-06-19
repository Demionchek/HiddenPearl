using UnityEngine;

namespace Zone
{
    [RequireComponent(typeof(Collider2D))]
    public class MusicZone : MonoBehaviour
    {
        public enum Axis { X, Y }

        [Header("Direction")]
        [SerializeField] private Axis axis = Axis.X;
        [SerializeField] private bool invert = false;

        [Header("Music")]
        [SerializeField] private AudioSource sourceFrom;
        [SerializeField] private AudioSource sourceTo;
        [SerializeField] private float masterVolume = 1f;

        private Collider2D _zone;
        private bool _playerInside;

        private void Awake()
        {
            _zone = GetComponent<Collider2D>();
            _zone.isTrigger = true;
        }

        private void Start()
        {
            if (sourceFrom != null && !sourceFrom.isPlaying) sourceFrom.Play();
            if (sourceTo != null && !sourceTo.isPlaying) sourceTo.Play();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player")) _playerInside = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInside = false;
        }

        private void Update()
        {
            if (!_playerInside) return;

            float t = Sample(Player.PlayerController.Instance.transform.position);
            SetVolumes(t);
        }

        private float Sample(Vector3 playerPos)
        {
            Bounds b = _zone.bounds;

            float t = axis == Axis.X
                ? Mathf.InverseLerp(b.min.x, b.max.x, playerPos.x)
                : Mathf.InverseLerp(b.min.y, b.max.y, playerPos.y);

            return invert ? 1f - t : t;
        }

        // t=0 → только A, t=1 → только B
        private void SetVolumes(float t)
        {
            if (sourceFrom != null) sourceFrom.volume = Mathf.Lerp(masterVolume, 0f, t);
            if (sourceTo != null) sourceTo.volume = Mathf.Lerp(0f, masterVolume, t);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (TryGetComponent(out Collider2D col))
            {
                Gizmos.color = new Color(0.2f, 0.8f, 0.4f, 0.25f);
                Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            }
        }
#endif
    }
}
