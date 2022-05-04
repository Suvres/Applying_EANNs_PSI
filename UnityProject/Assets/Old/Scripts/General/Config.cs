using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

/// Na szybko mam coś takiego, jednka potrzeba aby te pola przechowywały ścieżki do wag, skryptu i pythona
namespace General
{
    public class Config: MonoBehaviour
    {
        public static Config Instance
        {
            get;
            private set;
        }

        [SerializeField]
        public string pythonExecute = "/usr/bin/python3";
        
        public string scriptPath = "./Assets/Scripts/AI/PythonNN/main.py";
        public string weightsPath = "./Assets/Scripts/AI/PythonNN/weights.json";

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("Multiple GameStateManagers in the Scene.");
                return;
            }
            Instance = this;
        }
    }
}