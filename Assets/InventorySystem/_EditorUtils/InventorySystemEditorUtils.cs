

#if UNITY_EDITOR

using UnityEditor;
namespace InventorySystem {
    public static class InventorySystemEditorUtils 
    {
        public static AssetSelector<ItemBehavior> itemSelector;

        public static void UpdateItemSelector () {
            if (itemSelector == null)
                itemSelector = new AssetSelector<ItemBehavior>();
            else 
                itemSelector.UpdateAssetReferences();
        }
    }
}

#endif