using UnityEngine;

namespace Game {

    // TODO: make this an initialization perk script
    [CreateAssetMenu()] public class GameValueTemplate : ScriptableObject {
        [NeatArray] public GameValueArray gameValueTemplates;
    }
}
