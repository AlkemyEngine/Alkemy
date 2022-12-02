using UnityEngine.UIElements;

namespace Lobby
{
    public class LobbySlot
    {
        public delegate void LobbySlotEvent(int playerIndex);
        public event LobbySlotEvent OnPreviousButtonClickedEvent;
        public event LobbySlotEvent OnNextButtonClickedEvent;

        public int? playerIndex { get; private set; }
        private readonly VisualElement _root;
        private readonly VisualElement _previousButton;
        private readonly VisualElement _nextButton;
        private readonly VisualElement _readyMark;

        public LobbySlot(VisualElement root)
        {
            _root = root;
            _previousButton = root.Q<Button>("Previous");
            _nextButton = root.Q<Button>("Next");
            _readyMark = root.Q<VisualElement>("ReadyMarker");
            
            _previousButton.RegisterCallback<ClickEvent>(OnPreviousButtonClicked);
            _nextButton.RegisterCallback<ClickEvent>(OnNextButtonClicked);
        }
        
        public void SetupPlayer(int? index)
        {
            playerIndex = index;
            
            _root.style.display = index.HasValue ? DisplayStyle.Flex : DisplayStyle.None;
            _previousButton.SetEnabled(index.HasValue);
            _nextButton.SetEnabled(index.HasValue);
        }
        
        private void OnPreviousButtonClicked(ClickEvent evt)
        {
            if (playerIndex == null) return;
            OnPreviousButtonClickedEvent?.Invoke(playerIndex.Value);
        }
        
        private void OnNextButtonClicked(ClickEvent evt)
        {
            if (playerIndex == null) return;
            OnNextButtonClickedEvent?.Invoke(playerIndex.Value);
        }

        public void SetReady(bool ready)
        {
            _readyMark.style.display = ready ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}