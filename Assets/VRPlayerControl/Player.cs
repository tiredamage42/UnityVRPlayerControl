
using UnityEngine;
using System.Collections;
using Valve.VR;

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

		public SteamVR_Action_Boolean[] actions = new SteamVR_Action_Boolean[3];
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
		}


        void Update()
        {
            // if (SteamVR.initializedState != SteamVR.InitializedStates.InitializeSuccess)
            //     return;

			CheckForInitialScaling();
			
			handsTogether = Vector3.SqrMagnitude(hands[0].transform.position - hands[1].transform.position) <= (handsTogetherThreshold * handsTogetherThreshold);

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
				
				bool handOccupied = VRUIInput.HandOccupied(hand);
				if (handOccupied)
					continue;

				for (int x = 0; x < actions.Length; x++) {
					ControlInteractorAndEquipper (hand, i, actions[x], x);
				}        
			}
        }	
	}
}

