using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lobby
{
    public class LobbyPlayerManager : MonoBehaviour
    {
        public static LobbyPlayerManager Instance { get; private set; }
        
        [SerializeField] private GameObject[] selectableCharacters;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.Log("More than one LobbyPlayerManager in scene!");
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
        
        public void AddPlayer()
        {
            
        }
    }
}