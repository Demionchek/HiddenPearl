using UnityEngine;
using System.Collections;
using Interfaces;
using TMPro;

namespace DialogSystem
{
    public class TextTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject textPanel;
        [SerializeField] private TMP_Text textDisplay;
        [SerializeField] private string baseText = "Текст сообщения";
        [SerializeField] private float displaySpeed = 0.05f;
        public bool changeTextWithTrigger = false;
        public Opener[]  openers;
        public string triggerText = "";

        private Coroutine displayCoroutine;

        
        private bool AreAllOpenersActive()
        {
            foreach (var opener in openers)
            {
                if (opener == null || !opener.isActive)
                    return false;
            }
            return true;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player")) // Проверяем, что вошёл игрок
            {
                textPanel.SetActive(true); // Включаем панель
                if (displayCoroutine != null)
                {
                    StopCoroutine(displayCoroutine);
                }
                displayCoroutine = StartCoroutine(DisplayText());
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player")) // Проверяем, что вышел игрок
            {
                if (displayCoroutine != null)
                {
                    StopCoroutine(displayCoroutine);
                }
                textDisplay.text = ""; // Очищаем текст
                textPanel.SetActive(false); // Выключаем панель
            }
        }

        private IEnumerator DisplayText()
        {
            textDisplay.text = ""; // Очищаем текст перед началом
            
            string textToDisplay = changeTextWithTrigger && AreAllOpenersActive() ? triggerText : baseText;

            // Постепенно выводим текст посимвольно
            foreach (char letter in textToDisplay.ToCharArray())
            {
                textDisplay.text += letter;
                yield return new WaitForSeconds(displaySpeed);
            }
        }
    }
}