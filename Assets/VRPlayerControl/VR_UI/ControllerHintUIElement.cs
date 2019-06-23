using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleUI;
namespace VRPlayer.UI {
    public class ControllerHintUIElement : MonoBehaviour
    {
        public UIText text;

        void Awake () {
            text = GetComponentInChildren<UIText>();

            Hide();
        }
        
        public void Hide () {
            gameObject.SetActive(false);
        }
        public void Show (string message) {
            gameObject.SetActive(true);
            text.SetText(message);
        }
    }
}
