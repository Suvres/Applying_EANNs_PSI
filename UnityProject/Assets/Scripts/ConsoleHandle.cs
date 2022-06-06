using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PSI
{
    public class ConsoleHandle : MonoBehaviour
    {
        private static ConsoleHandle instance;

        public void Awake()
        {
            instance = this;
        }

        public static void Log(string text)
        {
            Debug.Log(text);
        }
    }
}
