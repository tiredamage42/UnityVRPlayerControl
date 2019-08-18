using UnityEngine;

namespace Game.GameUI {
    public class UIObjectInitializer : MonoBehaviour
    {
        public Actor associatedActor;
        void Awake () {
            if (associatedActor == null) {
                Debug.LogError("Please specify an associate actor for ui object " + name);
            }
            UIHandler[] allUIHandlers = GetComponents<UIHandler>();
            for (int i = 0; i < allUIHandlers.Length; i++) {
                allUIHandlers[i].myActor = associatedActor;
            }
        }
    }
}
