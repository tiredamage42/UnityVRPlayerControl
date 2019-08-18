using UnityEngine;
using SimpleUI;

namespace Game.GameUI {
    public class HUDGameValueTracker : MonoBehaviour
    {
        public UIValueTracker uiObject;
        
        [Header("Invalid/Warning Thresholds")]
        [NeatArray] public NeatFloatArray colorSchemeThresholds;
        public string gameValueName;
        Actor actor;

        void GetActor () {
            if (actor == null) actor = GetComponent<UIObjectInitializer>().associatedActor;
        }

        void Awake () {
            GetActor();
        }

        void Start () {
            GameValue gv = actor.GetGameValue(gameValueName);
            if (gv != null) gv.AddChangeListener(UpdateUIObject);
        }
            
        void OnDisable () {
            GameValue gv = actor.GetGameValue(gameValueName);
            if (gv != null) gv.RemoveChangeListener(UpdateUIObject);
        }

        public void SetUIObject(UIValueTracker uiObject) {
            this.uiObject = uiObject;
            if (uiObject == null) return;
            uiObject.SetText(gameValueName);
            GetActor();
            GameValue gv = actor.GetGameValue(gameValueName);
            if (gv != null) UpdateUIObject(0, gv.GetValue(), gv.GetMinValue(), gv.GetMaxValue());
        }

        void UpdateUIObject (float delta, float newValue, float min, float max) {
            
            UIColorScheme scheme = UIColorScheme.Normal;
            int l = Mathf.Min(2, colorSchemeThresholds.list.Length);
            for (int i = 0; i < l; i++) {
                if (newValue <= colorSchemeThresholds.list[i]) {
                    scheme = i == 0 ? UIColorScheme.Invalid : UIColorScheme.Warning;
                    break;
                }
            }

            if (uiObject != null) uiObject.SetValue(Mathf.InverseLerp(min, max, newValue), scheme);
        }
    }
}
