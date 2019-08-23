
using UnityEngine;
using System.Collections;
using Valve.VR;
using System.Collections.Generic;
using GameBase;


using Game;
namespace VRPlayer
{
	
    public enum ReleaseStyle
    {
        NoChange, GetFromHand, ShortEstimation, AdvancedEstimation,
    }
	
	public class Player : MonoBehaviour
	{

		public float trackpadMiddleZone = .1f;
		public List<int> dPadActions = new List<int>();
		public int trackpadButtonAction = 2;

		bool CheckTrackPadButtonValue (int actionCheck, int controllerIndex) {
			if (actionCheck == trackpadButtonAction) {
				return inMiddle[controllerIndex];
			}
			return true;
		}
		bool CheckDpadButtonValue (int actionCheck, int controllerIndex) {
			if (dPadActions.Contains(actionCheck)) {
				return !inMiddle[controllerIndex];
			}
			return true;
		}
		
		public SteamVR_Action_Vector2 trackpadAxis;

        [Tooltip("This action lets you know when the player has placed the headset on their head")]
        public SteamVR_Action_Boolean headsetOnHead = SteamVR_Input.GetBooleanAction("HeadsetOnHead");
        public SteamVR_Action_Vibration hapticAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic");

		public Vector2 GetTrackpadAxis (int hand) {
			return lastTrackpadTouchValues[hand];
		}

		public Vector2 GetTrackpadAxis (SteamVR_Input_Sources hand) {
			return GetTrackpadAxis(VRManager.Hand2Int(hand));
		}
		public bool GetTrackpadIsMiddleValue (int hand) {
			return inMiddle[hand];
		}

		Vector2[] lastTrackpadTouchValues = new Vector2[2];
		bool[] inMiddle = new bool[2];
		void UpdateTrackpadTouchValues () {
			for (int i = 0; i < 2; i++) {
				lastTrackpadTouchValues[i] = trackpadAxis.GetAxis(VRManager.Int2Hand(i));
			}
		}
		void CalculateIfMiddle () {
			float trackpadMiddleZone2 = trackpadMiddleZone * trackpadMiddleZone;
			for (int i = 0; i < 2; i++) {
				inMiddle[i] = lastTrackpadTouchValues[i].sqrMagnitude <= trackpadMiddleZone2;
			}
		}
		Vector4 savedAxis, lastAxis;
        void CalculateScrollDeltas () {
            for (int i = 0; i < 2; i++) {
                int startIndex = 2*i;

				Vector2 current = lastTrackpadTouchValues[i];
                // Vector2 current = TrackpadAxis.GetAxis(VRManager.Int2Hand(i));
                for (int x = 0; x < 2; x++) savedAxis[startIndex+x] = GetAxisRaw(startIndex+x, current[x], deltaThresholdForScroll[x]);
            }
        }


		void UpdateTrackPad () {
			UpdateTrackpadTouchValues();
			CalculateScrollDeltas();
			CalculateIfMiddle ();

		}




		public bool headsetIsOnPlayerHead;
        [Tooltip(".15f is a good setting when\nstandalone input module has 60 actions per second")]  
        public Vector2 deltaThresholdForScroll = new Vector2(.15f, .15f);
        


        public Vector2 GetScrollDelta (SteamVR_Input_Sources hand) {
            if (hand == SteamVR_Input_Sources.Any) return new Vector2(savedAxis.x, savedAxis.y) + new Vector2(savedAxis.z, savedAxis.w);
			if (hand != SteamVR_Input_Sources.LeftHand && hand != SteamVR_Input_Sources.RightHand) {
				Debug.LogError("wrong hand");
			}
		    int handOffset = 2*VRManager.Hand2Int(hand);
		    return new Vector2(savedAxis[handOffset], savedAxis[handOffset+1]);
        }
        
        /*
            make axis react to scrolling action on trackpad
        */
        float GetAxisRaw(int axisIndex, float currentAxis, float currentThreshold) {

            float delta = currentAxis - lastAxis[axisIndex];
            float returnAxis = 0;
            if (delta != 0 && Mathf.Abs(delta) >= currentThreshold){
                if (lastAxis[axisIndex] == 0 || currentAxis == 0) {
                    // if (lastAxis[axisIndex] == 0) 
                    //     Debug.LogError("on scroll start");
                    // else 
                    //     Debug.LogError("on scroll end");
                    
                }
                else {
                    returnAxis = Mathf.Clamp(delta * 99999, -1, 1);
                }
                lastAxis[axisIndex] = currentAxis;
            }
            return returnAxis;
        }


        // static Dictionary<SteamVR_Action, SteamVR_Input_Sources> occupiedActions = new Dictionary<SteamVR_Action, SteamVR_Input_Sources>();

        // public static void MarkActionOccupied(SteamVR_Action action, SteamVR_Input_Sources forHand) {
        //     occupiedActions[action] = forHand;
        // }
        // public static void MarkActionUnoccupied(SteamVR_Action action) {
        //     occupiedActions[action] = VRManager.errorVRSource;
        // }
        // public static bool ActionOccupied (SteamVR_Action action, SteamVR_Input_Sources forHand) {
        //     if (VRUIInput.ActionOccupied(action, forHand)) {
        //         return true;
        //     }
            
        //     SteamVR_Input_Sources handValue;
        //     if (occupiedActions.TryGetValue(action, out handValue)) {
        //         return handValue != SteamVR_Input_Sources.Keyboard && forHand == handValue;
        //     }
        //     else {
        //         return false;
        //     }
        // }
        



        public void TriggerHapticPulse(SteamVR_Input_Sources hand, ushort microSecondsDuration)
        {
            float seconds = (float)microSecondsDuration / 1000000f;
            hapticAction.Execute(0, seconds, 1f / seconds, 1, hand);
        }

        public void TriggerHapticPulse(SteamVR_Input_Sources hand, float duration, float frequency, float amplitude)
        {
            hapticAction.Execute(0, duration, frequency, amplitude, hand);
        }


























		public SteamVR_Action_Boolean[] actions = new SteamVR_Action_Boolean[3];

		bool CheckAction (int action) {
			if (action < 0 || action >= actions.Length) {
				Debug.LogError("action  " + action + " is not defined in player");
				return false;
			}
			return true;
		}
		SteamVR_Input_Sources GetHand (int controllerIndex) {
			return (controllerIndex < 0) ? SteamVR_Input_Sources.Any : VRManager.Int2Hand(controllerIndex);
		}
	
		public bool GetActionStart (int action, int controllerIndex) {
			if (!CheckAction(action)) return false;
			if (!CheckTrackPadButtonValue (action, controllerIndex)) return false;
			if (!CheckDpadButtonValue (action, controllerIndex)) return false;



			return actions[action].GetStateDown(GetHand(controllerIndex));
		}
		public bool GetActionUpdate (int action, int controllerIndex) {
			if (!CheckAction(action)) return false;
			return actions[action].GetState(GetHand(controllerIndex));
		}
		public bool GetActionEnd (int action, int controllerIndex) {
			if (!CheckAction(action)) return false;
			return actions[action].GetStateUp(GetHand(controllerIndex));
		}

		void InitializeControls () {
			ControlsManager.instance.InitializeActionGetters (GetActionStart, GetActionUpdate, GetActionEnd);
		}

		public float handsTogetherThreshold = .25f;
		[HideInInspector] public bool handsTogether;

		public ReleaseStyle releaseVelocityStyle = ReleaseStyle.GetFromHand;

        [Tooltip("The time offset used when releasing the object with the RawFromHand option")]
        public float releaseVelocityTimeOffset = -0.011f;
        public float scaleReleaseVelocity = 1.1f;

		SimpleCharacterController moveScript;

		[Tooltip("Resize the player in game to be defaultPlayerHeight height?")]
		public bool resizePlayer;
		[Tooltip("How tall the player should be in game when resizing")]
		public float defaultPlayerHeight = 1.8f;

		// any extra height offset to add to the player (crouching or prone, etc...)
		[HideInInspector] public float extraHeightOffset;

		// how tall the player is standing up in the game
		public float gamespaceHeight {
			get {
				return resizePlayer ? defaultPlayerHeight : realLifeHeight;
			}
		}
		// how tall the player is in real life
		[SerializeField] float _realLifeHeight = -1;
		public float realLifeHeight {
			get {
				return _realLifeHeight == -1 ? 1 : _realLifeHeight;
			}
		}

		// how much offset to put on the transform to simulate the player being
		// the default height
		// negative if lpayer is taller (push the play area down), 
		// positive if shorter (push the play area up)
		float resizePlayerOffset {
			get {
				return resizePlayer ? defaultPlayerHeight - realLifeHeight : 0;
			}
		}

		public void KeepTransformFlushWithGround(float groundY) {
			Vector3 origPos = transform.position;
			transform.position = new Vector3(origPos.x, groundY + totalHeightOffset, origPos.z);
		}

		public void RecalibrateRealLifeHeight () {
			if (_realLifeHeight == -1) {
				KeepTransformFlushWithGround(moveScript.GetFloor().y);
			}
			if (VRManager.headsetIsOnPlayerHead) {
				StartCoroutine( _RecalibrateRealLifeHeight() );
			}
		}

		bool recalibrateHeight;
		public IEnumerator _RecalibrateRealLifeHeight () {
					
        	_realLifeHeight = VRManager.hmd_Transform.localPosition.y;
			Debug.LogError("Recalibrating Player real life hieght " + _realLifeHeight);
			if (totalHeightOffset != 0) {
				yield return new WaitForFixedUpdate();
				float floorY = moveScript.GetFloor().y;
				KeepTransformFlushWithGround(floorY);
			}
			yield return null;
		}

		public float totalHeightOffset {
			get {
				return extraHeightOffset + resizePlayerOffset;
			}
		}

		void CheckForInitialScaling () {

			if (_realLifeHeight == -1 || recalibrateHeight) {
				RecalibrateRealLifeHeight();
			}
			recalibrateHeight = false;
		}

		[Header( "* RIGHT FIRST *" )]
		public Hand[] hands;
		static Player _instance;
		public static Player instance {
			get {
				if ( _instance == null )
					_instance = FindObjectOfType<Player>();
				return _instance;
			}
		}

		public Hand GetHand( SteamVR_Input_Sources sources )
		{
			for ( int j = 0; j < hands.Length; j++ )
			{
				if ( hands[j].handType == sources)
					return hands[j];
			}
			return null;
		}


		public Hand leftHand { get { return GetHand(SteamVR_Input_Sources.LeftHand); } }
		public Hand rightHand { get { return GetHand(SteamVR_Input_Sources.RightHand); } }

		// Guess for the world-space position of the player's feet, directly beneath the HMD.
		public Vector3 feetPositionGuess { get { return transform.position + Vector3.ProjectOnPlane( VRManager.hmd_Transform.position - transform.position, transform.up ); } }


		Actor actor;
		void Awake()
		{
			actor = GetComponent<Actor>();
			moveScript = GetComponent<SimpleCharacterController>();		
			InitializeControls();	
		}

        void Update()
        {
            // if (SteamVR.initializedState != SteamVR.InitializedStates.InitializeSuccess)
            //     return;

			CheckForInitialScaling();
			
			handsTogether = Vector3.SqrMagnitude(hands[0].transform.position - hands[1].transform.position) <= (handsTogetherThreshold * handsTogetherThreshold);

			if (headsetOnHead.GetStateDown(SteamVR_Input_Sources.Head)) {
                // Debug.Log("<b>SteamVR Interaction System</b> Headset placed on head");
                headsetIsOnPlayerHead = true;
            }
            else if (headsetOnHead.GetStateUp(SteamVR_Input_Sources.Head)) {
                // Debug.Log("<b>SteamVR Interaction System</b> Headset removed");
                headsetIsOnPlayerHead = false;
            }
            else if (headsetOnHead.GetState(SteamVR_Input_Sources.Head)) {
                headsetIsOnPlayerHead = true;
            }
            UpdateTrackPad();

			UpdateInputActions();
        }

		void ControlInteractorAndEquipper (SteamVR_Input_Sources hand, int handID, SteamVR_Action_Boolean action, int actionKey) {
            bool useDown = action.GetStateDown(hand);
            bool useUp = action.GetStateUp(hand);
            bool useHeld = action.GetState(hand);
                
            if (useDown) actor.BroadcastActionStart(handID, actionKey);
            if (useUp) actor.BroadcastActionEnd(handID, actionKey);
            if (useHeld) actor.BroadcastActionUpdate(handID, actionKey);
        }


        void UpdateInputActions()
        {
			for (int i = 0; i < 2; i++) {
				SteamVR_Input_Sources hand = VRManager.Int2Hand(i);
				
				// bool handOccupied = VRUIInput.HandOccupied(hand);
				// if (handOccupied)
				// 	continue;

				for (int x = 0; x < actions.Length; x++) {
					ControlInteractorAndEquipper (hand, i, actions[x], x);
				}        
			}
        }	
	}
}

