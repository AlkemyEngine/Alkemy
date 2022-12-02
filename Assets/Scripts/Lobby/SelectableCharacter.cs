using UnityEngine;

namespace Lobby
{
    public class SelectableCharacter
    {
        public GameObject prefab { get; private set; }
        public bool isSelectable { get; private set; }
        
        public SelectableCharacter(GameObject prefab, bool isSelectable)
        {
            this.prefab = prefab;
            this.isSelectable = isSelectable;
        }
        
        public SelectableCharacter(GameObject prefab)
        {
            this.prefab = prefab;
            this.isSelectable = true;
        }
        
        public void SetSelectable(bool isSelectable)
        {
            this.isSelectable = isSelectable;
        }
    }
}