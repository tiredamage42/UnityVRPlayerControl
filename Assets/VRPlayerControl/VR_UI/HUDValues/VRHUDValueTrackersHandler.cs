

using UnityEngine;

using GameUI;
using SimpleUI;
using Valve.VR;

namespace VRPlayer.UI {


    public class VRHUDValueTrackersHandler : MonoBehaviour 
    {
        public GameObject uiTrackersObject;
        public SteamVR_Input_Sources uiHand = SteamVR_Input_Sources.LeftHand;
        public TransformBehavior equipBehaviors;
        HUDGameValueTracker[] foundTrackers;

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

                // textOffset = .05f;
                // trackerSize = new Vector2(2,1);
                // emptyBackground = false;
                // textScale = .005f;
                UIValueTrackerParameters trackerParams = new UIValueTrackerParameters(.05f, new Vector2(.25f,2f), false, .005f);

                for (int i = 0; i < foundTrackers.Length; i++) {

                    UIValueTracker uiObject = VRUI.MakeValueTrackerUI(foundTrackers[i].gameValueName + "TrackerUI", trackerParams, true);
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


