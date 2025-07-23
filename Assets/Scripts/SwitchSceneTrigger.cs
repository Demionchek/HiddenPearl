using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefaultNamespace
{
    public class SwitchSceneTrigger : MonoBehaviour
    {
        public int sceneIndex;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player") ||
                other.gameObject.layer == LayerMask.NameToLayer("Bird") ||
                other.gameObject.layer == LayerMask.NameToLayer("Rat"))
            {
                SceneManager.LoadScene(sceneIndex);
            }
        }

        public void Switch(int sceneIndex) => SceneManager.LoadScene(sceneIndex);
    }
}