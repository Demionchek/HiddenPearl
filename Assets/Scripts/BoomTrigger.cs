using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Zenject;

namespace DefaultNamespace
{
    public class BoomTrigger : MonoBehaviour
    {
        public GameObject boom;
        public int cutsceneIndex = 1;

        public List<Light2D> lights;
        public AudioSource audioSource;

        [Inject]
        private TimelineManager _timelineManager;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Interact"))
            {
                Destroy(other.gameObject);
                boom.SetActive(true);
                _timelineManager.PlayCutscene(cutsceneIndex);
                StartCoroutine(LightToggle());
            }
        }

        private IEnumerator LightToggle()
        {
            yield return new WaitForSeconds(0.5f);
            foreach (Light2D light in lights)
            {
                light.color = Color.red;
            }
            audioSource.gameObject.SetActive(true);
            audioSource.Play();
        }
    }
}