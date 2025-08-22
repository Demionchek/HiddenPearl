using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class CutsceneTrigger : MonoBehaviour
    {
        public int cutsceneIndex = 0;
        private BoxCollider2D boxCollider2D;

        [Inject]
        private TimelineManager _timelineManager;

        private void Awake()
        {
            boxCollider2D = GetComponent<BoxCollider2D>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _timelineManager.PlayCutscene(cutsceneIndex);
                boxCollider2D.enabled = false;
            }
        }
    }
}