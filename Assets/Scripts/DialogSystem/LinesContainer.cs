using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class LinesContainer : MonoBehaviour
    {
        public List<Lines> dialogLines = new List<Lines>();
    }

    [Serializable]
    public class Lines
    {
        public List<string> lines = new List<string>();
    }
}