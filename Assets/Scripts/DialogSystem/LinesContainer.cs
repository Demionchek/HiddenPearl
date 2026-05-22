using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace DefaultNamespace
{
    public class LinesContainer : MonoBehaviour
    {
        public List<Lines> dialogLines = new List<Lines>();

        private void OnValidate()
        {
            foreach (var dialogLine in dialogLines)
                dialogLine?.MigrateLegacyLines();
        }
    }

    [Serializable]
    public class DialogueLine
    {
        [TextArea]
        public string text;
        public UnityEvent onLineStart = new UnityEvent();
        [FormerlySerializedAs("onLineSkip")] public UnityEvent onLineEnd = new UnityEvent();
    }

    [Serializable]
    public class Lines
    {
        [SerializeField] private List<DialogueLine> dialogueLines = new List<DialogueLine>();

        [HideInInspector]
        public List<string> lines = new List<string>();

        public List<DialogueLine> DialogueLines
        {
            get
            {
                MigrateLegacyLines();
                return dialogueLines;
            }
        }

        public void MigrateLegacyLines()
        {
            if (dialogueLines.Count > 0 || lines.Count == 0)
                return;

            foreach (var line in lines)
                dialogueLines.Add(new DialogueLine { text = line });
        }
    }
}
