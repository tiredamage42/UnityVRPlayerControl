using UnityEngine;

namespace SimpleUI{
    [ExecuteInEditMode] public class ElementHolderCollection : UIElementHolder
    {

        
        protected override Transform ElementsParent() { return null; }
        protected override SelectableElement ElementPrefab() { return null; }
    }
}
