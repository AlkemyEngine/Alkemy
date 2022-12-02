using System;
using System.Collections.Generic;
using System.Linq;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Lobby
{
    public class LobbyController : MonoBehaviour
    {
        public static LobbyController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        [SerializeField] private GameObject[] characterPrefabs;

        private VisualElement _rootElement;
        private VisualElement _playButtonElement;
        private VisualElement _quitButtonElement;
        private VisualElement _secondPlayerSelectionLineElement;
        
        private readonly LobbySlot[] _lobbySlots = new LobbySlot[8];
        private PlayerConfigurationManager _playerConfigurationManager;
        private SelectableCharacterRegister _selectableCharacterRegister;

        private void Start()
        {
            _playerConfigurationManager = PlayerConfigurationManager.Instance;
            _selectableCharacterRegister = SelectableCharacterRegister.Instance;
            
            foreach (GameObject t in characterPrefabs)
            {
                _selectableCharacterRegister.Register(t);
            }
            
            SetupUI();
        }

        private void SetupUI()
        {
            _rootElement = GetComponent<UIDocument>().rootVisualElement;
            
            _secondPlayerSelectionLineElement = _rootElement.Q<VisualElement>("SecondLine");
            _playButtonElement = _rootElement.Q<VisualElement>("StartButton");
            _quitButtonElement = _rootElement.Q<VisualElement>("ReturnButton");
            
            _playButtonElement.RegisterCallback<ClickEvent>(OnPlayButtonClicked);
            _quitButtonElement.RegisterCallback<ClickEvent>(OnQuitButtonClicked);

            for (int i = 0; i < 8; i++)
            {
                VisualElement lobbySlotRoot = _rootElement.Q<VisualElement>("player_selector_" + (i + 1));
                LobbySlot slot = new LobbySlot(lobbySlotRoot);
                slot.OnNextButtonClickedEvent += OnNextButtonClicked;
                slot.OnPreviousButtonClickedEvent += OnPreviousButtonClicked;
                _lobbySlots[i] = slot;
            }
            
            UpdateUI();
        }

        public void UpdateUI()
        {
            List<PlayerConfiguration> playerConfigurations = _playerConfigurationManager.PlayerConfigurations;
            int playerCount = playerConfigurations.Count;
            
            _secondPlayerSelectionLineElement.style.display = playerCount > 4 ? DisplayStyle.Flex : DisplayStyle.None;
            
            for (int i = 0; i < 8; i++)
            {
                LobbySlot lobbySlot = _lobbySlots[i];
                
                if (i >= playerCount)
                {
                    lobbySlot.SetupPlayer(null);
                    continue;
                }
                
                PlayerConfiguration playerConfiguration = playerConfigurations[i];
                
                lobbySlot.SetupPlayer(playerConfiguration.PlayerIndex);
                lobbySlot.SetReady(playerConfiguration.IsReady);
            }

            _playButtonElement.SetEnabled(CanStartTheGame());
        }

        public bool CanStartTheGame()
        {
            int playerCount = _playerConfigurationManager.PlayerConfigurations.Count;
            return  playerCount > 1 && 
                    _playerConfigurationManager.PlayerConfigurations.All(p => p.IsReady);
        }
        
        private void OnNextButtonClicked(int playerIndex)
        {
            SelectNextCharacter(playerIndex);
        }

        public void SelectNextCharacter(int playerIndex)
        {
            PlayerConfiguration playerConfiguration = _playerConfigurationManager.GetPlayerConfiguration(playerIndex);
            
            if (playerConfiguration.IsReady) return;
            
            int currentCharacterIndex = playerConfiguration.CharacterIndex;
            int nextCharacterIndex = _selectableCharacterRegister.GetNextSelectableCharacterIndex(currentCharacterIndex);
            _selectableCharacterRegister.SetSelectable(currentCharacterIndex, true);
            _selectableCharacterRegister.SetSelectable(nextCharacterIndex, false);
            _playerConfigurationManager.SetPlayerCharacterIndex(playerIndex, nextCharacterIndex);
            UpdateUI();
        }
        
        private void OnPreviousButtonClicked(int playerIndex)
        {
            SelectPreviousCharacter(playerIndex);
        }
        
        public void SelectPreviousCharacter(int playerIndex)
        {
            PlayerConfiguration playerConfiguration = _playerConfigurationManager.GetPlayerConfiguration(playerIndex);
            
            if (playerConfiguration.IsReady) return;
            
            int currentCharacterIndex = playerConfiguration.CharacterIndex;
            int previousCharacterIndex = _selectableCharacterRegister.GetPreviousSelectableCharacterIndex(currentCharacterIndex);
            _selectableCharacterRegister.SetSelectable(currentCharacterIndex, true);
            _selectableCharacterRegister.SetSelectable(previousCharacterIndex, false);
            _playerConfigurationManager.SetPlayerCharacterIndex(playerIndex, previousCharacterIndex);
            
            UpdateUI();
        }

        private void OnPlayButtonClicked(ClickEvent evt)
        {
            StartGame();
        }

        private void OnQuitButtonClicked(ClickEvent evt)
        {
            ReturnToMainMenu();
        }

        public void StartGame()
        {
            List<PlayerConfiguration> playerConfigurations = _playerConfigurationManager.PlayerConfigurations;
            _playerConfigurationManager.gameObject.GetComponent<PlayerInputManager>().DisableJoining();

            foreach (PlayerConfiguration playerConfiguration in playerConfigurations)
            {
                PlayerInput playerInput = playerConfiguration.Input;
                GameObject player = playerInput.gameObject;
                player.GetComponent<LobbyCharacterInputHandler>().enabled = false;
                player.GetComponent<PlayerInputHandler>().enabled = true;
                playerInput.SwitchCurrentActionMap("Player");
            }
            
            SceneManager.LoadScene("MainScene");
        }
        
        public void ReturnToMainMenu()
        {
            // TODO: Go back to main menu
        }

        private void OnDestroy()
        {
            _playButtonElement.UnregisterCallback<ClickEvent>(OnPlayButtonClicked);
            _quitButtonElement.UnregisterCallback<ClickEvent>(OnQuitButtonClicked);
        }
    }
}
