﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Player interface used to query HMD transforms and VR hands
//
//=============================================================================

using UnityEngine;
using System.Collections;
using UIMessaging;
using Valve.VR;
using Valve.VR.InteractionSystem;

using InventorySystem;
using SimpleUI;
namespace VRPlayer
{
	//-------------------------------------------------------------------------
	// Singleton representing the local VR player/user, with methods for getting
	// the player's hands, head, tracking origin, and guesses for various properties.
	//-------------------------------------------------------------------------
	public class Player : MonoBehaviour
	{
		public UIRadial wristRadial;
		public EquipBehavior radialEquipBehavior;

		public bool wristRadialOpen {
			get {
				return wristRadial.gameObject.activeInHierarchy;
			}
		}
		// ISteamVR_Action_In_Source occupiedAction = null;
		// SteamVR_Input_Sources occupiedHand = SteamVR_Input_Sources.Any;
		public SteamVR_Action_Boolean openEquipSelect;



		void InitializeWristRadial () {
			wristRadial.onBaseCancel += OnWristRadialBaseCancel;
		}

		void OnWristRadialBaseCancel () {
			UIManager.HideUI(wristRadial);
		}
		

        void UpdateWristRadial () {
			bool together = handsTogether;
			if (!wristRadialOpen) {

				if (together) {
					if (openEquipSelect.GetStateDown(SteamVR_Input_Sources.LeftHand)) {
						
						UIManager.ShowUI(wristRadial, true, false);
						wristRadial.GetComponentInParent<LooseTransform>().SetParent(
							GetHand(SteamVR_Input_Sources.LeftHand).transform, 
							radialEquipBehavior.equipSettings[0].position, 
							radialEquipBehavior.equipSettings[0].rotation
						);
						VRInputModuleAddon.SetUIHand(SteamVR_Input_Sources.LeftHand);
						
					}
					else if (openEquipSelect.GetStateDown(SteamVR_Input_Sources.RightHand)) {
						UIManager.ShowUI(wristRadial, true, false);
						wristRadial.GetComponentInParent<LooseTransform>().SetParent(
							GetHand(SteamVR_Input_Sources.LeftHand).transform, 
							radialEquipBehavior.equipSettings[0].position, 
							radialEquipBehavior.equipSettings[0].rotation
						);
						VRInputModuleAddon.SetUIHand(SteamVR_Input_Sources.RightHand);
					}
				}
			}


			/*
				Assumes the open equip button is the side button
				as well as teh teleport button
			*/

			// Teleport.instance.teleportEnabled = !together;

		    
        } 




		public float handsTogetherThreshold = .25f;


		public bool handsTogether {
			get {
				return Vector3.Distance(hands[0].transform.position, hands[1].transform.position) <= handsTogetherThreshold;
			}
		}




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

		bool recalibrateHeight;
		public IEnumerator RecalibrateRealLifeHeight () {
					
        	_realLifeHeight = hmdTransform.localPosition.y;

			Debug.LogError("Recalibrating Player real life hieght " + _realLifeHeight);

			if (totalHeightOffset != 0) {

				yield return new WaitForFixedUpdate();
				// yield return new WaitForFixedUpdate();
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
				if (_realLifeHeight == -1) {
					KeepTransformFlushWithGround(moveScript.GetFloor().y);
				}
				if (VRManager.headsetIsOnPlayerHead) {
					StartCoroutine(

					RecalibrateRealLifeHeight()
					)
					;


				}
				else {
					// Debug.Log("eeeee");
				}

			}
			recalibrateHeight = false;
		}

		
		// [Tooltip( "Virtual transform corresponding to the meatspace tracking origin. Devices are tracked relative to this." )]
		public Transform trackingOriginTransform { get { return transform; } }
		public Transform hmdTransform;

		[Tooltip( "List of possible Hands" )]
		public Hand[] hands;

		//-------------------------------------------------
		// Singleton instance of the Player. Only one can exist at a time.
		//-------------------------------------------------
		private static Player _instance;
		public static Player instance
		{
			get
			{
				if ( _instance == null )
				{
					_instance = FindObjectOfType<Player>();
				}
				return _instance;
			}
		}

		public Hand GetHand( SteamVR_Input_Sources sources )
		{
		
			for ( int j = 0; j < hands.Length; j++ )
			{
				if ( !hands[j].gameObject.activeInHierarchy )
				{
					continue;
				}

				if ( hands[j].handType != sources)
				{
					continue;
				}

				return hands[j];
			}

			return null;
		}


		//-------------------------------------------------
		public Hand leftHand
		{
			get
			{
				
				return GetHand(SteamVR_Input_Sources.LeftHand);
			}
		}


		//-------------------------------------------------
		public Hand rightHand
		{
			get
			{
				return GetHand(SteamVR_Input_Sources.RightHand);
			}
		}

		//-------------------------------------------------
		// Guess for the world-space position of the player's feet, directly beneath the HMD.
		//-------------------------------------------------
		public Vector3 feetPositionGuess
		{
			get
			{
				Transform hmd = hmdTransform;
				if ( hmd )
				{
					return trackingOriginTransform.position + Vector3.ProjectOnPlane( hmd.position - trackingOriginTransform.position, trackingOriginTransform.up );
				}
				return trackingOriginTransform.position;
			}
		}

		//-------------------------------------------------
		private void Awake()
		{
			moveScript = GetComponent<SimpleCharacterController>();
			InitializeMessageCenters();
			InitializeWristRadial();
		}


		//-------------------------------------------------
		private IEnumerator Start()
		{
			_instance = this;

            while (SteamVR.initializedState == SteamVR.InitializedStates.None || SteamVR.initializedState == SteamVR.InitializedStates.Initializing)
                yield return null;

			if ( SteamVR.instance == null )
			{
				Debug.LogError("there was a problem initializing steam vr");
			}
        }

        protected virtual void Update()
        {
            if (SteamVR.initializedState != SteamVR.InitializedStates.InitializeSuccess)
                return;

				Teleport.instance.SetCurrentTrackingTransformOffset(totalHeightOffset);


		
			CheckForInitialScaling();
			UpdateWorldScale();
        

			UpdateMessageCenters();
			UpdateWristRadial();
        }

		void InitializeMessageCenters () {
			MessageCenter r = GetHand(SteamVR_Input_Sources.RightHand).GetComponentInChildren<MessageCenter>();
			r.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
			r.transform.localPosition = new Vector3(-0.05f, 0.0f, 0.0f);
			r.textAlignment = TextAlignment.Right;
			r.textAnchor = TextAnchor.UpperRight;

			MessageCenter l = GetHand(SteamVR_Input_Sources.LeftHand).GetComponentInChildren<MessageCenter>();
			l.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
			l.transform.localPosition = new Vector3(0.05f, 0.0f, 0.0f);
			l.textAlignment = TextAlignment.Left;
			l.textAnchor = TextAnchor.UpperLeft;


			MessageCenter.AddInstance(r);
			MessageCenter.AddInstance(l);			
		}

		//TODO: make the main one whichever is in front of the camera

		void UpdateMessageCenters () {
		
		}
	}
}
