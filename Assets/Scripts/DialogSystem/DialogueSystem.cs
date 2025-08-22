using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using TMPro;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
    public enum DialogType
    {
        Intro,
        Dialog_1,
        Dialog_2,
        Dialog_3,
        Dialog_4,
        Dialog_5
    }

    [Serializable]
    public class DialogTimelineTrigger
    {
        public DialogType dialogType;
        public int timelineIndex;
    }

    public class DialogueSystem : MonoBehaviour
    {
        [SerializeField] private GameObject dialoguePanel; // Панель с текстом
        [SerializeField] private TextMeshProUGUI dialogueText; // Текст для вывода
        [SerializeField] private float textSpeed = 0.05f; // Скорость появления текста
        [SerializeField] private float delayAfterLines = 1.5f; // Задержка после последней строки

        [SerializeField] private List<DialogTimelineTrigger> timelineTriggers = new List<DialogTimelineTrigger>();

        public bool isDialogRunning = false;

        private List<string> lines = new List<string>(); // Список строк диалога
        private int currentLine = 0; // Текущая строка

        private LinesContainer linesContainer;
        private DialogType currentType;

        [Inject]
        private InputHandler _inputHandler;
        [Inject]
        private TimelineManager _timelineManager;

        void Start()
        {
            linesContainer = GetComponent<LinesContainer>();
            InputLines(linesContainer.dialogLines[(int)DialogType.Intro].lines);
        }

        public void InitDialogue(int index)
        {
            if (isDialogRunning) return;
            currentType = (DialogType)index;

            ClearLines();
            InputLines(linesContainer.dialogLines[index].lines);

            StartDialogue();
        }

        private void ClearLines()
        {
            lines.Clear();
        }

        private void InputLines(List<string> newLines)
        {
            this.lines.AddRange(newLines);
        }

        public void StartDialogue()
        {
            dialoguePanel.SetActive(true);
            currentLine = 0;
            StartCoroutine(TypeLine());
        }

        IEnumerator TypeLine()
        {
            isDialogRunning = true;
            // Очищаем текст перед началом новой строки
            dialogueText.text = "";

            // Постепенно выводим каждый символ текущей строки
            foreach (char c in lines[currentLine].ToCharArray())
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(textSpeed);
            }

            bool isPlayingCutscene = _timelineManager.IsCutscenePlaying();

            if (isPlayingCutscene)
                if (_timelineManager.GetCutscene() != null)
                    _timelineManager.GetCutscene().Pause();

            while (!_inputHandler.JumpPressed)
                yield return null;

            // Переходим к следующей строке или закрываем диалог
            currentLine++;
            if (currentLine < lines.Count)
            {
                StartCoroutine(TypeLine());
            }
            else
            {
                while (!_inputHandler.JumpPressed)
                    yield return null;

                if (_timelineManager.IsCutscenePaused())
                    _timelineManager.GetCutscene().Resume();

                EndDialogue();
            }
        }

        void EndDialogue()
        {
            dialoguePanel.SetActive(false);
            isDialogRunning = false;

            // Проверяем все триггеры и запускаем соответствующие таймлайны
            foreach (var trigger in timelineTriggers)
            {
                if (trigger.dialogType == currentType)
                {
                    _timelineManager.PlayCutscene(trigger.timelineIndex);
                    break; // Запускаем только первый подходящий триггер
                }
            }
        }

        // Метод для добавления триггера программно
        public void AddTimelineTrigger(DialogType dialogType, int timelineIndex)
        {
            timelineTriggers.Add(new DialogTimelineTrigger 
            { 
                dialogType = dialogType, 
                timelineIndex = timelineIndex 
            });
        }

        // Метод для удаления триггера
        public void RemoveTimelineTrigger(DialogType dialogType)
        {
            timelineTriggers.RemoveAll(trigger => trigger.dialogType == dialogType);
        }

        // Метод для проверки наличия триггера
        public bool HasTimelineTrigger(DialogType dialogType)
        {
            return timelineTriggers.Exists(trigger => trigger.dialogType == dialogType);
        }

        // Метод для получения индекса таймлайна по типу диалога
        public int GetTimelineIndex(DialogType dialogType)
        {
            var trigger = timelineTriggers.Find(t => t.dialogType == dialogType);
            return trigger != null ? trigger.timelineIndex : -1;
        }
    }
}