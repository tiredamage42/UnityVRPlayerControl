using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Game.PerkSystem {

#if UNITY_EDITOR
    public static class PerkSystemEditor 
    {
        static AssetSelector<Perk> _itemSelector;
        public static AssetSelector<Perk> itemSelector {
            get {
                if (_itemSelector == null) {
                    Debug.Log("Buildign asset selector for Perks");
                    _itemSelector = new AssetSelector<Perk>(o => o.displayName, o => 0);
                }
                return _itemSelector;
            }
        }
    }
#endif


    [CreateAssetMenu()] public class Perk : ScriptableObject
    {
        public bool playerEdit = true;
        public string displayName;
        [TextArea] public string description;
        [Header("Per Level")] [NeatArray] public NeatStringArray descriptions; 
        public int levels { get { return descriptions.list.Length; } }
        [NeatArray] public PerkBehaviorsArray perkBehaviors;
        [NeatArray] public PerkScriptsArray perkScripts;   
    }
}
