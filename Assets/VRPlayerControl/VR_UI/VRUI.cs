using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using InventorySystem;

using SimpleUI;

using Valve.VR;

namespace VRPlayer {

    public class VRUI : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            SetUpMessageCenter();
        }

        // Update is called once per frame
        void Update()
        {
            
            SetUpMessageCenter();

            UpdateQuickInventory();
        }



        void OnEnable () {
        
            VRManager.onUISelection += OnUISelection;
            VRManager.onGamePaused += OnGamePaused;
            VRManager.onShowGameMessage += OnShowGameMessage;


            quickInventory.onBaseCancel += CloseQuickInventory;
			
            
        }
        void OnDisable () {
            VRManager.onGamePaused -= OnGamePaused;
            VRManager.onUISelection -= OnUISelection;
            VRManager.onShowGameMessage -= OnShowGameMessage;
        }


        void OnQuickInventorySubmit (GameObject[] data, object[] customData) {
			CloseQuickInventory();
		}

		void CloseQuickInventory () {
			UIManager.HideUI(quickInventory);
			VRManager.onUISubmit -= OnQuickInventorySubmit;
		}

		void OpenQuickInventory (SteamVR_Input_Sources hand) {
			UIManager.ShowUI(quickInventory, true, false);

            EquipBehavior.AdjustTransform(quickInventory.baseObject.transform, Player.instance.GetHand(hand).transform, quickInventoryEquip, 0);

			VRUIInput.SetUIHand(hand);

			VRManager.onUISubmit += OnQuickInventorySubmit;
		}

        bool CheckHandForWristRadialOpen (SteamVR_Input_Sources hand) {
			if (quickInventoryAction.GetStateDown(hand)) {
				OpenQuickInventory(hand);
				return true;
			}
			return false;
		}
        void UpdateQuickInventory () {
			if (!VRManager.gamePaused && !quickInventoryOpen && Player.instance.handsTogether) {

                StandardizedVRInput.MarkActionOccupied(quickInventoryAction, SteamVR_Input_Sources.Any);

                if (!CheckHandForWristRadialOpen(SteamVR_Input_Sources.LeftHand)) {
                    CheckHandForWristRadialOpen(SteamVR_Input_Sources.RightHand);
                }
            }
            else {
                StandardizedVRInput.MarkActionUnoccupied(quickInventoryAction);
            }   
        } 

		
			



        public SteamVR_Input_Sources messagesHand = SteamVR_Input_Sources.LeftHand;

        public EquipBehavior messagesEquip, quickInventoryEquip;

        public UIElementHolder quickInventory;
        public UIMessageCenter messageCenter;


        public bool quickInventoryOpen { get { return quickInventory.gameObject.activeInHierarchy; } }
		public SteamVR_Action_Boolean quickInventoryAction;



        void SetUpMessageCenter () {
            Transform handTransform = Player.instance.GetHand(messagesHand).transform;

            EquipBehavior.AdjustTransform(messageCenter.transform, handTransform, messagesEquip, 0);

        }
        
        void OnUISelection (GameObject[] data, object[] customData) {
            // float duration,  float frequency, float amplitude
            StandardizedVRInput.instance.TriggerHapticPulse( VRUIInput.GetUIHand (), .1f, 1.0f, 1.0f );   
        }
        void OnShowGameMessage (string message, int key) {
            StandardizedVRInput.instance.TriggerHapticPulse( messagesHand, .1f, 1.0f, 1.0f );   
        }

        
        void OnGamePaused(bool isPaused) {
            if (isPaused) {
                if (quickInventoryOpen) {
					CloseQuickInventory();
				}
			
                VRUIInput.SetUIHand(SteamVR_Input_Sources.Any);
            }
        }

        
    }
}
