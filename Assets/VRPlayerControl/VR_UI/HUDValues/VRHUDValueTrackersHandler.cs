

using UnityEngine;

using SimpleUI;
using Valve.VR;

using Game.UI;
namespace VRPlayer.UI {


    public class VRHUDValueTrackersHandler : MonoBehaviour 
    {
        public GameObject uiTrackersObject;
        public SteamVR_Input_Sources uiHand = SteamVR_Input_Sources.LeftHand;
        public TransformBehavior equipBehaviors;
        HUDGameValueTracker[] foundTrackers;
        public UIValueTrackerParameters trackerParams = new UIValueTrackerParameters();

        bool lengthsOK;
        bool CheckLength () {
            if (foundTrackers.Length > equipBehaviors.transformSettings.Length) {
                Debug.LogError("Found " + foundTrackers.Length + " hud game value trackers, but only supplied " + equipBehaviors.transformSettings.Length + " equip behaviors!!!");
                return false;
            }
            return true;
        }

        void Awake () {
            foundTrackers = uiTrackersObject.GetComponents<HUDGameValueTracker>();
            lengthsOK = CheckLength ();

            if (lengthsOK) {
                for (int i = 0; i < foundTrackers.Length; i++) {
                    UIValueTracker uiObject = VRUIBuilder.MakeValueTrackerUI(foundTrackers[i].gameValueName + "TrackerUI", trackerParams, true);
                    foundTrackers[i].SetUIObject(uiObject);
                    TransformBehavior.AdjustTransform(uiObject.baseObject.transform, Player.instance.GetHand(uiHand).transform, equipBehaviors, i);
                }
            }
        }
#if UNITY_EDITOR
        void Update () {
            if (lengthsOK) {
                for (int i = 0; i < foundTrackers.Length; i++) {
                    TransformBehavior.AdjustTransform(foundTrackers[i].uiObject.baseObject.transform, Player.instance.GetHand(uiHand).transform, equipBehaviors, i);
                }
            }
        }
#endif
    }
}


