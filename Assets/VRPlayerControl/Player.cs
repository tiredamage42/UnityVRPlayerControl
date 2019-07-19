
using UnityEngine;
using System.Collections;
using Valve.VR;

using GameBase;

namespace VRPlayer
{
	/*

	XRSettings.eyeTextureResolutionScale;
	Controls the actual size of eye textures as a multiplier of the device's default resolution.
	A value of 1.0 will use the default eye texture resolution specified by the XR device. Values less than 1.0 will use lower resolution eye textures, which may improve performance at the expense of a less sharp image. Values greater than 1.0 will use higher resolution eye textures, resulting in a potentially sharper image at a cost to performance and increased memory usage.


	XRSettings.renderViewportScale

	Controls how much of the allocated eye texture should be used for rendering.
	Valid range is 0.0 to 1.0. This value can be changed at runtime without reallocating eye textures

	
	


	11.1 milliseconds in the case of a refresh rate of 90 hz.

If your app is GPU-bound, the Unity Profiler
 displays XR.WaitForGPU in excess of one frame’s time in milliseconds.
If your app is CPU-bound, a frame takes longer than the designated frame time, but the Unity Profiler displays XR.WaitForGPU shorter than the one frame.
	
	 */
    public enum ReleaseStyle
    {
        NoChange, GetFromHand, ShortEstimation, AdvancedEstimation,
    }
	
	public class Player : MonoBehaviour
	{
		public float lodBias = 10;
		public SteamVR_Input_Sources mainHand;
		public SteamVR_Input_Sources offHand { get { return VRManager.OtherHand(mainHand); } }
		public SteamVR_Action_Boolean pauseAction;

		
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

		[Tooltip("World scale around the player")]
		[Range(.1f, 10)] public float worldScale = 1.0f;

		
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
					
        	_realLifeHeight = hmdTransform.localPosition.y;
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
		
    void UpdateWorldScale () {
        transform.localScale = Vector3.one * (1.0f/worldScale);
    }


		void CheckForInitialScaling () {

			if (_realLifeHeight == -1 || recalibrateHeight) {
				RecalibrateRealLifeHeight();
			}
			recalibrateHeight = false;
		}

		public Transform trackingOriginTransform { get { return transform; } }
		public Transform hmdTransform;

		[Tooltip( "List Hands *RIGHT FIRST*" )]
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
		public Vector3 feetPositionGuess { get { return trackingOriginTransform.position + Vector3.ProjectOnPlane( hmdTransform.position - trackingOriginTransform.position, trackingOriginTransform.up ); } }

		void Awake()
		{
			moveScript = GetComponent<SimpleCharacterController>();

			/*


QualitySettings.lodBias = 3.8;

the main camera that renders in editor has FOV at 60d, 
while the VR device FOV is 90d. 

LODBias1 * (tan(FOV2/2)/tan(FOV1/2)) where:
LODBias1 is the LODBias for the first camera you are coming from (in this case main camera)
FOV1 is the FOV for the first camera (again main camera) in radians.
FOV2 is the FOV for the second camera (in this case Daydream) in radians.

For main camera we have FOV1 = 60 degrees = pi/3 radians, LODBias = 2
For the Daydream camera we have FOV2 = 90 degrees = pi/2 radians

So: 2 * (tan(pi/2/2)/tan(pi/3/2)) = 
2 * (tan(pi/4)/tan(pi/6)) = ~3.46

 */

 Debug.Log("Setting lod bias");
//  QualitySettings.lodBias = 3.46f;
QualitySettings.lodBias = lodBias;



			
		}


		IEnumerator Start()
		{
			_instance = this;
			QualitySettings.vSyncCount = 0;

            while (SteamVR.initializedState == SteamVR.InitializedStates.None || SteamVR.initializedState == SteamVR.InitializedStates.Initializing)
                yield return null;

			if ( SteamVR.instance == null )
				Debug.LogError("there was a problem initializing steam vr");
        }

        void Update()
        {
            if (SteamVR.initializedState != SteamVR.InitializedStates.InitializeSuccess)
                return;

			CheckForInitialScaling();
			
			handsTogether = Vector3.SqrMagnitude(hands[0].transform.position - hands[1].transform.position) <= (handsTogetherThreshold * handsTogetherThreshold);
			
			if (pauseAction.GetStateDown(offHand)) {
                GameManager.TogglePause();
            }
		
			UpdateWorldScale();        
        }
	}
}

