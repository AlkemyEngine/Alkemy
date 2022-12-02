using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerConfiguration
    {
        public PlayerInput Input { get; set; }
        public int PlayerIndex => Input.playerIndex;
        
        public bool IsReady { get; set; }
        
        public int CharacterIndex { get; set; }
        
        public PlayerConfiguration(PlayerInput playerInput, int characterIndex)
        {
            Input = playerInput;
            CharacterIndex = characterIndex;
        }
    }
}