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

        void Start () {
            GameValue gv = actor.GetGameValue(gameValueName);
            if (gv != null) {
                Debug.LogError("adding change listener");
                gv.AddChangeListener(UpdateUIObject);
            }
            else
            {
                                Debug.LogError("couldnt find " + gameValueName);

            }
        }
            
        void OnDisable () {
            GameValue gv = actor.GetGameValue(gameValueName);
            if (gv != null) gv.RemoveChangeListener(UpdateUIObject);
        }

        public void SetUIObject(UIValueTracker uiObject) {
            this.uiObject = uiObject;

            if (uiObject != null) {
                uiObject.SetText(gameValueName);
                if (actor == null)
                    actor = GetComponent<Actor>();

                GameValue gv = actor.GetGameValue(gameValueName);
                if (gv != null) UpdateUIObject(0, gv.GetValue(), gv.GetMinValue(), gv.GetMaxValue(), null);
            }
        }

        void UpdateUIObject (float delta, float newValue, float min, float max, string msg) {
            if (uiObject != null) uiObject.SetValue(Mathf.InverseLerp(min, max, newValue));
        }
    }
}
