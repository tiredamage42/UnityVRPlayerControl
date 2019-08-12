using UnityEngine;
using Valve.VR;
using GameUI;
using SimpleUI;
using DialogueSystem;


namespace VRPlayer.UI {
    public class VRDialogueUI : MonoBehaviour
    {
        public SteamVR_Input_Sources attachHand = SteamVR_Input_Sources.RightHand;
        public TransformBehavior equipBehavior;
        public DialoguePlayerUIHandler myUIHandler;
        protected virtual void OnEnable () {
            if (myUIHandler != null) myUIHandler.onUIOpen += OnOpenUI;   
        }
        protected virtual void OnDisable () {
            if (myUIHandler != null) myUIHandler.onUIOpen -= OnOpenUI;
        }
        void OnOpenUI (UIElementHolder uiObject) {
            TransformBehavior.AdjustTransform(uiObject.baseObject.transform, Player.instance.GetHand(attachHand).transform, equipBehavior, 0);
            VRUIInput.SetUIHand(attachHand);
        }
    }
}
