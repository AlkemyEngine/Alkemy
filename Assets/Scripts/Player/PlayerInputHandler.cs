using Character;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

namespace Player
{
    public class PlayerInputHandler : MonoBehaviour
    {
        private bool _shouldUpdateInput = false;
        private PlayerConfiguration _playerConfiguration;
        private GameInputController _controls;
        private PlayerMovement _playerMovement;
        private PlayerBombController _playerBombController;
        
        
        public void InitializePlayer(PlayerConfiguration pc, GameObject player)
        {
            _controls = new GameInputController();
            _controls.Player.Enable();
            _controls.Menu.Disable();

            EntityHealth entityHealth = player.GetComponent<EntityHealth>();
            entityHealth.OnDeath += OnDeath;
            
            _playerMovement = player.GetComponent<PlayerMovement>();
            _playerBombController = player.GetComponent<PlayerBombController>();
            
            _playerConfiguration = pc;
            _playerConfiguration.Input.onActionTriggered += OnActionTriggered;
            
            _shouldUpdateInput = true;
        }
        
        private void OnActionTriggered(CallbackContext context)
        {
            if (!_shouldUpdateInput) return;
            
            if (context.action.name == _controls.Player.Move.name) 
            {
                OnMove(context);
            } 
            else if (context.action.name == _controls.Player.PlaceBomb.name)
            {
                OnPlaceBomb(context);
            }
        }

        private void OnMove(CallbackContext context)
        {
            Vector2 inputVector = context.ReadValue<Vector2>();
            _playerMovement.UpdateInputVector(inputVector);
        }
        
        private void OnPlaceBomb(CallbackContext context)
        {
            if (context.performed)
            {
                _playerBombController.TryPlaceBomb();
            }
        }

        private void OnDeath(GameObject player)
        {
            _shouldUpdateInput = false;
        }
    }
}