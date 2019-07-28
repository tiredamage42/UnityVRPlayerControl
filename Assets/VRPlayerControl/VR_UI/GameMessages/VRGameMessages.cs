// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;


using Valve.VR;
using GameUI;
using SimpleUI;
namespace VRPlayer.UI {
    public class VRGameMessages : MonoBehaviour
    {

        public SteamVR_Input_Sources messagesHand = SteamVR_Input_Sources.LeftHand;
        public TransformBehavior messagesEquip;
        // public UIMessageCenter messageCenterPrefab;

        GameMessageInbox inbox;

        void GetMessageCenterReference () {
            inbox = Player.instance.GetComponent<GameMessageInbox>();
        }

        void Start () {
            GetMessageCenterReference();
            SetUpMessageCenter();
        }
        void OnEnable() {
            GetMessageCenterReference();
            inbox.onShowMessage += OnShowGameMessage;

        }
        void OnDisable () {
            inbox.onShowMessage -= OnShowGameMessage;
        }


        void Update () {
#if UNITY_EDITOR
            SetUpMessageCenter();
#endif
        }

        void SetUpMessageCenter () {

            // if (messageCenter == null) {
            //     messageCenter = Instantiate(messageCenterPrefab);// as UIMessageCenter;
            // }
            Transform handTransform = Player.instance.GetHand(messagesHand).transform;
            TransformBehavior.AdjustTransform(inbox.messageCenterUI.transform, handTransform, messagesEquip, 0);
        }
        void OnShowGameMessage (string message) {            
            StandardizedVRInput.instance.TriggerHapticPulse( messagesHand, .1f, 1.0f, 1.0f );   
        }
        // void OnEnable () {
        //     UIManager.onShowGameMessage += OnShowGameMessage;    
        // }
        // void OnDisable () {
        //     UIManager.onShowGameMessage -= OnShowGameMessage;
        // }
    }
}

