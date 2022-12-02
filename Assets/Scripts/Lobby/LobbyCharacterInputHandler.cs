using Player;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

namespace Lobby
{
    public class LobbyCharacterInputHandler : MonoBehaviour
    {
        private const float TimeToRepeat = 1f;
        private float _lastTimeDirectionTriggered;
        
        private LobbyController _lobbyController;
        private PlayerConfiguration _playerConfiguration;
        private GameInputController _controls;

        public void InitializePlayer(PlayerConfiguration playerConfiguration)
        {
            _controls = new GameInputController();
            _controls.Player.Disable();
            _controls.Menu.Enable();
            
            _lobbyController = LobbyController.Instance;
            _playerConfiguration = playerConfiguration;
            _playerConfiguration.Input.onActionTriggered += OnActionTriggered;
        }

        private void OnDisable()
        {
            _controls.Menu.Enable();
            _playerConfiguration.Input.onActionTriggered -= OnActionTriggered;
        }

        private void OnActionTriggered(CallbackContext context)
        {
            if (context.action.name == _controls.Menu.Direction.name)
            {
                OnDirection(context);
            } else if (context.action.name == _controls.Menu.Confirm.name) {
                OnConfirm(context);
            } else if (context.action.name == _controls.Menu.Cancel.name) {
                OnCancel(context);
            }
        }
        
        private void OnDirection(CallbackContext context)
        {
            if (Time.realtimeSinceStartup - _lastTimeDirectionTriggered < TimeToRepeat) return;
            
            Debug.Log("triggered");
            Vector2 direction = context.ReadValue<Vector2>();
            _lastTimeDirectionTriggered = Time.realtimeSinceStartup;

            switch (direction.x)
            {
                case > 0:
                    _lobbyController.SelectNextCharacter(_playerConfiguration.PlayerIndex);
                    break;
                case < 0:
                    _lobbyController.SelectPreviousCharacter(_playerConfiguration.PlayerIndex);
                    break;
            }
        }

        private void OnConfirm(CallbackContext context)
        {
            if (!context.performed) return;

            if (!_playerConfiguration.IsReady)
            {
                _playerConfiguration.IsReady = true;
                _lobbyController.UpdateUI();
                return;
            }
            
            if (_playerConfiguration.PlayerIndex == 0 && _lobbyController.CanStartTheGame())
            {
                _lobbyController.StartGame();
            }
        }
        
        private void OnCancel(CallbackContext context)
        {
            if (!context.performed) return;
            
            if (_playerConfiguration.IsReady)
            {
                _playerConfiguration.IsReady = false;
                _lobbyController.UpdateUI();
                return;
            }

            if (_playerConfiguration.PlayerIndex == 0)
            {
                _lobbyController.ReturnToMainMenu();
            }
        }
    }
}