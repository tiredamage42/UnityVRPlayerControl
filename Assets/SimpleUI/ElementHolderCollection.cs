using UnityEngine;

namespace SimpleUI{
    [ExecuteInEditMode] public class ElementHolderCollection : UIElementHolder
    {
        protected override float TextScale() { return 1; }
        protected override Transform ElementsParent() { return null; }
        protected override SelectableElement ElementPrefab() { return null; }
    }
}
