using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using SimpleUI;

namespace GameUI {
    public class UISelectableElementHandler : MonoBehaviour
    {
        public UIElementHolder uiObject;
        public bool UIObjectActive () {
            return uiObject.gameObject.activeInHierarchy;
        }
        public void SetUIObject (UIElementHolder uiObject) {
            this.uiObject = uiObject;
        }
    

        protected void MakeButton (SelectableElement element, string text, object[] customData) {
            element.elementText = text;
            element.uiText.SetText(text, -1);
            element.customData = customData;
        }

        protected void BroadcastUIOpen() {
            if (onUIOpen != null) {
                onUIOpen (uiObject);
            }
        }
        protected void BroadcastUIClose() {
            if (onUIClose != null) {
                onUIClose (uiObject);
            }
        }
        public event System.Action<UIElementHolder> onUIOpen, onUIClose;

    }
}
