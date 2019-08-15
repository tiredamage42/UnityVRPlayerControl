using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleUI;
namespace GameUI {
    public abstract class UIHandler : MonoBehaviour
    {

        public string context;

        public static UIHandler GetUIHandlerByContext (string context) {
            UIHandler[] allHandlers = GameObject.FindObjectsOfType<UIHandler>();
            for (int i = 0; i < allHandlers.Length; i++) {
                if (allHandlers[i].context == context) {
                    return allHandlers[i];
                }
            }
            return null;
        }
        
        
        #if UNITY_EDITOR
            public static string[] GetHandlerInputNames (string context) {//where X : UIHandler {
                UIHandler handler = GetUIHandlerByContext(context);
                return handler != null ? handler.inputNames : null;
            }
        #endif


        protected System.Func<Vector2Int> customGetInputMethod;
        public void SetUIInputCallback (System.Func<Vector2Int> callback) {
            customGetInputMethod = callback;
        }

        public System.Func<object[], bool> shouldOpenCheck, shouldCloseCheck;

        protected bool OpenUIDenied (object[] parameters) {
            if (UIObjectActive()) return true;
            if (UIManager.AnyUIOpen()) return true;
            if (shouldOpenCheck != null && !shouldOpenCheck(parameters)) return true;
            return false;

        }

        protected bool UICloseDenied (object[] parameters) {
            if (!UIObjectActive()) return true;
            if (shouldCloseCheck != null && !shouldCloseCheck(parameters)) return true;
            return false;
        }

        public abstract bool UIObjectActive ();
        


        
        [NeatArray] public NeatStringArray inputNames;
        
        
        public abstract void OpenUI();
        public abstract void CloseUI();

        protected abstract GameObject GetUIBaseObject ();
        
        protected void BroadcastUIOpen(object[] parameters) {
            if (onUIOpen != null) {
                onUIOpen (GetUIBaseObject(), parameters);
            }
        }
        protected void BroadcastUIClose() {
            if (onUIClose != null) {
                onUIClose (GetUIBaseObject());
            }
        }
        public event System.Action<GameObject, object[]> onUIOpen;
        public event System.Action<GameObject> onUIClose;



    }
}
