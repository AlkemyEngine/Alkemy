using System.Collections.Generic;
using UnityEngine;

namespace Lobby
{
    public class SelectableCharacterRegister
    {
        private static SelectableCharacterRegister _instance;
        public static SelectableCharacterRegister Instance => _instance ??= new SelectableCharacterRegister();
        
        private readonly List<SelectableCharacter> _selectableCharacter;
        public int NumberOfSelectableCharacters => _selectableCharacter.Count;
        public SelectableCharacter this[int index] => _selectableCharacter[index];
        public List<SelectableCharacter> SelectableCharacters => _selectableCharacter;

        private SelectableCharacterRegister()
        {
            _selectableCharacter = new List<SelectableCharacter>();
        }
        
        public void Register(GameObject prefab)
        {
            _selectableCharacter.Add(new SelectableCharacter(prefab));
        }
        
        public void Clear()
        {
            _selectableCharacter.Clear();
        }
        
        public int GetNextSelectableCharacterIndex(int currentIndex)
        {
            for (int i = 0; i < _selectableCharacter.Count; i++)
            {
                int index = (i + currentIndex) % _selectableCharacter.Count;
                
                if (_selectableCharacter[index].isSelectable)
                {
                    return index;
                }
            }

            return currentIndex;
        }
        
        public int GetPreviousSelectableCharacterIndex(int currentIndex)
        {
            for (int i = _selectableCharacter.Count - 1; i >= 0; i--)
            {
                int index = (i + currentIndex) % _selectableCharacter.Count;
                
                if (_selectableCharacter[index].isSelectable)
                {
                    return index;
                }
            }

            return currentIndex;
        }
        
        public void SetSelectable(int index, bool isSelectable)
        {
            _selectableCharacter[index].SetSelectable(isSelectable);
        }
    }
}