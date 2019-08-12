using UnityEngine;

namespace ActorSystem {
    [CreateAssetMenu()] public class GameValueTemplate : ScriptableObject {
        [NeatArray] public GameValueArray gameValueTemplates;
    }
}
