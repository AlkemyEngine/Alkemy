using System.Collections.Generic;
using System.Linq;
using Lobby;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace Player
{
    public class PlayerConfigurationManager : MonoBehaviour
    {
        public static PlayerConfigurationManager Instance { get; private set; }
        public List<PlayerConfiguration> PlayerConfigurations { get; private set; }
        
        private SelectableCharacterRegister _characterRegister;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.Log("Instance already exists. More than one PlayerConfigurationManager in the scene.");
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            _characterRegister = SelectableCharacterRegister.Instance;
            PlayerConfigurations = new List<PlayerConfiguration>();
        }

        public PlayerConfiguration GetPlayerConfiguration(int playerIndex)
        {
            return PlayerConfigurations[playerIndex];
        }
        
        public void SetPlayerCharacterIndex(int playerIndex, int characterPrefab)
        {
            PlayerConfiguration playerConfiguration = PlayerConfigurations[playerIndex];
            playerConfiguration.CharacterIndex = characterPrefab;
        }
        
        public void SetPlayerReady(int playerIndex, bool ready)
        {
            PlayerConfigurations[playerIndex].IsReady = ready;
        }
        
        public void HandlePlayerJoined(PlayerInput playerInput)
        {
            playerInput.transform.SetParent(transform);
            
            if (PlayerConfigurations.Any(p => p.PlayerIndex == playerInput.playerIndex)) return;
            
            int characterIndex = _characterRegister.GetNextSelectableCharacterIndex(0);
            _characterRegister.SetSelectable(characterIndex, false);
            PlayerConfiguration playerConfiguration = new PlayerConfiguration(playerInput, playerInput.playerIndex);
            PlayerConfigurations.Add(playerConfiguration);

            LobbyCharacterInputHandler characterInputHandler =
                playerInput.gameObject.GetComponent<LobbyCharacterInputHandler>();
            characterInputHandler.InitializePlayer(playerConfiguration);
            LobbyController.Instance.UpdateUI();
        }
        
        public void HandlePlayerLeft(PlayerInput playerInput)
        {
            PlayerConfigurations.Remove(PlayerConfigurations.Find(x => x.PlayerIndex == playerInput.playerIndex));
        }
    }
}