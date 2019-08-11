

#if UNITY_EDITOR

using UnityEditor;
namespace InventorySystem {
    public static class InventorySystemEditorUtils 
    {

        public static AssetSelector<ItemBehavior> itemSelector;


        static string NameFromItemBehavior (ItemBehavior item) {
            return "[" + item.category + "] " + item.name;
        }
        // static bool OrderItem (ItemBehavior item) {
        //     return 
        // }

        public static void UpdateItemSelector () {
            if (itemSelector == null)
                itemSelector = new AssetSelector<ItemBehavior>(NameFromItemBehavior, o => o.category);
            else 
                itemSelector.UpdateAssetReferences();
        }
    }
}

#endif