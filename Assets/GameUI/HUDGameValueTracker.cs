using UnityEngine;
using ActorSystem;
using SimpleUI;

namespace GameUI {
    public class HUDGameValueTracker : MonoBehaviour
    {
        public UIValueTracker uiObject;
        public string gameValueName;
        Actor actor;

        void Awake () {
            actor = GetComponent<Actor>();
        }

        void OnEnable () {
            GameValue gv = actor.GetGameValue(gameValueName);
            if (gv != null) gv.AddChangeListener(UpdateUIObject);
        }
            
        void OnDisable () {
            GameValue gv = actor.GetGameValue(gameValueName);
            if (gv != null) gv.RemoveChangeListener(UpdateUIObject);
        }

        public void SetUIObject(UIValueTracker uiObject) {
            this.uiObject = uiObject;

            if (uiObject != null) {
                uiObject.SetText(gameValueName);

                GameValue gv = actor.GetGameValue(gameValueName);
                if (gv != null) UpdateUIObject(0, gv.GetValue(), gv.GetMinValue(), gv.GetMaxValue());
            }
        }

        void UpdateUIObject (float delta, float newValue, float min, float max) {
            if (uiObject != null) uiObject.SetValue(Mathf.InverseLerp(min, max, newValue));
        }
    }
}
