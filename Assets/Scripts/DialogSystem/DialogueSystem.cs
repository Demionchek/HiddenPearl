using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using TMPEffects.Components;
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
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private float textSpeed = 0.05f;
        [SerializeField] private float delayAfterLines = 1.5f;

        [SerializeField] private List<DialogTimelineTrigger> timelineTriggers = new List<DialogTimelineTrigger>();

        public bool isDialogRunning = false;

        private List<DialogueLine> lines = new List<DialogueLine>();
        private int currentLine = 0;

        private LinesContainer linesContainer;
        private DialogType currentType;
        private TMPWriter writer;
        private bool timelinePausedByDialogue = false;

        [Inject]
        private InputHandler _inputHandler;

        [Inject]
        private TimelineManager _timelineManager;

        void Start()
        {
            linesContainer = GetComponent<LinesContainer>();
            InputLines(linesContainer.dialogLines[(int)DialogType.Intro].DialogueLines);

            writer = dialogueText.gameObject.GetComponent<TMPWriter>();
            if (writer == null)  writer = dialogueText.gameObject.AddComponent<TMPWriter>();
        }

        public void InitDialogue(int index)
        {
            if (isDialogRunning) return;
            currentType = (DialogType)index;

            ClearLines();
            InputLines(linesContainer.dialogLines[index].DialogueLines);

            StartDialogue();
        }

        private void ClearLines()
        {
            lines.Clear();
        }

        private void InputLines(List<DialogueLine> newLines)
        {
            this.lines.AddRange(newLines);
        }

        public void StartDialogue()
        {
            isDialogRunning = true;
            timelinePausedByDialogue = _timelineManager.PauseCurrentCutscene();
            dialoguePanel.SetActive(true);
            currentLine = 0;
            StartCoroutine(TypeLine());
        }

        IEnumerator TypeLine()
        {
            int lineIndex = currentLine;
            DialogueLine line = lines[lineIndex];

            line.onLineStart?.Invoke();
            dialogueText.text = line.text;

            yield return new WaitWhile(() => _inputHandler.JumpPressed);

            writer.StartWriter();

            yield return new WaitUntil(() => writer.IsWriting == false || _inputHandler.JumpPressed);

            if (writer.IsWriting && _inputHandler.JumpPressed)
            {
                writer.SkipWriter();
                yield return new WaitUntil(() => writer.IsWriting == false);
                //line.onLineEnd?.Invoke();
            }

            currentLine++;

            yield return new WaitWhile(() => _inputHandler.JumpPressed);


            if (currentLine < lines.Count)
                yield return new WaitUntil(() => _inputHandler.JumpPressed);

            yield return null;

            yield return new WaitWhile(() => _inputHandler.JumpPressed);

            line.onLineEnd?.Invoke();

            if (currentLine < lines.Count)
            {
                StartCoroutine(TypeLine());
            } else
            {
                yield return new WaitWhile(() => _inputHandler.JumpPressed);
                yield return new WaitUntil(() => _inputHandler.JumpPressed);
                yield return null;
                yield return new WaitWhile(() => _inputHandler.JumpPressed);

                if (timelinePausedByDialogue)
                    _timelineManager.ResumeCurrentCutscene();

                EndDialogue();
            }
        }

        void EndDialogue()
        {
            dialoguePanel.SetActive(false);
            isDialogRunning = false;
            timelinePausedByDialogue = false;

            foreach (var trigger in timelineTriggers)
            {
                if (trigger.dialogType == currentType)
                {
                    _timelineManager.PlayCutscene(trigger.timelineIndex);
                    break;
                }
            }
        }

        public void AddTimelineTrigger(DialogType dialogType, int timelineIndex)
        {
            timelineTriggers.Add(new DialogTimelineTrigger { dialogType = dialogType, timelineIndex = timelineIndex });
        }

        public void RemoveTimelineTrigger(DialogType dialogType)
        {
            timelineTriggers.RemoveAll(trigger => trigger.dialogType == dialogType);
        }

        public bool HasTimelineTrigger(DialogType dialogType)
        {
            return timelineTriggers.Exists(trigger => trigger.dialogType == dialogType);
        }

        public int GetTimelineIndex(DialogType dialogType)
        {
            var trigger = timelineTriggers.Find(t => t.dialogType == dialogType);
            return trigger != null ? trigger.timelineIndex : -1;
        }
    }
}
