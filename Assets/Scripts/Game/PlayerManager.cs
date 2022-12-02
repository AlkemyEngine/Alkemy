using System;
using System.Collections;
using System.Collections.Generic;
using Character;
using Lobby;
using Player;
using Terrain;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Game
{
    [RequireComponent(typeof(TerrainManager), typeof(GameManager))]
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance { get; private set; }

        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameObject[] playerSpawns;

        private List<GameObject> _instantiatedPlayer;
        private int _alivePlayerCount;

        private void Awake()
        {
            Instance = this;
            _instantiatedPlayer = new List<GameObject>();
        }

        private void GeneratePlayers()
        {
            ClearOldPlayer();
            DoSpawnPlayers();
        }

        private void ClearOldPlayer()
        {
            foreach (GameObject player in _instantiatedPlayer)
            {
                Destroy(player);
            }
            
            _instantiatedPlayer.Clear();
        }
        
        private void DoSpawnPlayers()
        {
            SelectableCharacterRegister selectableCharacterRegister = SelectableCharacterRegister.Instance;
            List<PlayerConfiguration> playerConfigurations = PlayerConfigurationManager.Instance.PlayerConfigurations;
            
            Debug.Log("playerConfigurations.Count: " + playerConfigurations.Count);
            
            for (int i = 0; i < playerConfigurations.Count; i++)
            {
                PlayerConfiguration playerConfiguration = playerConfigurations[i];
                GameObject playerConfigurationGameObject = playerConfiguration.Input.gameObject;
                
                GameObject characterPrefab = selectableCharacterRegister[playerConfigurations[i].CharacterIndex].prefab;
                int spawnIndex = i % playerSpawns.Length;
                GameObject player = Instantiate(characterPrefab, playerSpawns[spawnIndex].transform.position, Quaternion.identity);
                
                player.GetComponent<EntityHealth>().OnDeath += OnPlayerDeath;
                foreach (MonoBehaviour component in playerConfigurationGameObject.GetComponents<MonoBehaviour>())
                {
                    Debug.Log(component.GetType().FullName);
                }
                Debug.Log(playerConfigurationGameObject.GetComponent<PlayerInputHandler>());
                playerConfigurationGameObject.GetComponent<PlayerInputHandler>().InitializePlayer(playerConfiguration, player);
                
                _instantiatedPlayer.Add(player);
            }
            
            _alivePlayerCount = _instantiatedPlayer.Count;
        }

        private void OnPlayerDeath(GameObject player)
        {
            Destroy(player);
            _alivePlayerCount--;
            
            if (ShouldEndTheGame())
            {
                gameManager.EndTheGame();
            }
        }
        
        private bool ShouldEndTheGame()
        {
            if (_alivePlayerCount > 1) return false;
            return true;
        }
        
        private void OnEnable()
        {
            gameManager.GameStateChanged += OnGameStateChanged;
        }

        private void OnDisable()
        {
            gameManager.GameStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.TerrainGenerated:
                    OnTerrainGenerated();
                    break;
                case GameState.Ended:
                    OnGameEnded();
                    break;
            }
        }
        
        private void OnTerrainGenerated()
        {
            GeneratePlayers();
            gameManager.SetGameState(GameState.Ready);
            gameManager.SetGameState(GameState.Playing);
        }

        private void OnGameEnded()
        {
            StartCoroutine(Restart());
        }

        private IEnumerator Restart()
        {
            yield return new WaitForSeconds(3);
            gameManager.ReloadGame();
        }
        
        private void OnValidate()
        {
            if (gameManager == null) gameManager = GetComponent<GameManager>();
        }
    }
}