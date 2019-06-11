//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Handles all the teleport logic
//
//=============================================================================

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class Teleport : MonoBehaviour
    {
        public SteamVR_Action_Boolean teleportAction;
		public SteamVR_Input_Sources teleportHand = SteamVR_Input_Sources.LeftHand;

		Hand teleportHandClass {
			get {
				return Player.instance.GetHand(teleportHand);
			}
		}

        public LayerMask traceLayerMask;
		public LayerMask floorFixupTraceLayerMask;
		public float floorFixupMaximumTraceDistance = 1.0f;
		// public Material areaVisibleMaterial;
		// public Material areaLockedMaterial;
		// public Material areaHighlightedMaterial;
		public Material pointVisibleMaterial;
		public Material pointLockedMaterial;
		public Material pointHighlightedMaterial;
		public Transform destinationReticleTransform;
		public Transform invalidReticleTransform;
		public GameObject playAreaPreviewCorner;
		public GameObject playAreaPreviewSide;
		public Color pointerValidColor;
		public Color pointerInvalidColor;
		public Color pointerLockedColor;
		public bool showPlayAreaMarker = true;

		public float teleportFadeTime = 0.1f;
		public float meshFadeTime = 0.2f;

		public float arcDistance = 10.0f;

		// [Header( "Effects" )]
		// public Transform onActivateObjectTransform;
		// public Transform onDeactivateObjectTransform;
		// public float activateObjectTime = 1.0f;
		// public float deactivateObjectTime = 1.0f;

		[Header( "Audio Sources" )]
		public AudioSource pointerAudioSource;
		public AudioSource loopingAudioSource;
		public AudioSource headAudioSource;
		public AudioSource reticleAudioSource;

		[Header( "Sounds" )]
		public AudioClip teleportSound;
		public AudioClip pointerStartSound;
		public AudioClip pointerLoopSound;
		public AudioClip pointerStopSound;
		public AudioClip goodHighlightSound;
		public AudioClip badHighlightSound;

		[Header( "Debug" )]
		public bool debugFloor = false;
		public bool showOffsetReticle = false;
		public Transform offsetReticleTransform;
		public MeshRenderer floorDebugSphere;
		public LineRenderer floorDebugLine;

		private LineRenderer pointerLineRenderer;
		private GameObject teleportPointerObject;
		private Transform pointerStartTransform;


		bool isPointing;
		// private Hand pointerHand = null;
		
		
		private Player player = null;
		private TeleportArc teleportArc = null;

		private bool visible = false;

		private TeleportMarkerBase[] teleportMarkers;
		private TeleportMarkerBase pointedAtTeleportMarker;
		private TeleportMarkerBase teleportingToMarker;
		private Vector3 pointedAtPosition;
		private Vector3 prevPointedAtPosition;
		private bool teleporting = false;
		private float currentFadeTime = 0.0f;

		private float meshAlphaPercent = 1.0f;
		private float pointerShowStartTime = 0.0f;
		private float pointerHideStartTime = 0.0f;
		private bool meshFading = false;
		private float fullTintAlpha;

		private float invalidReticleMinScale = 0.2f;
		private float invalidReticleMaxScale = 1.0f;
		private float invalidReticleMinScaleDistance = 0.4f;
		private float invalidReticleMaxScaleDistance = 2.0f;
		// private Vector3 invalidReticleScale = Vector3.one;
		// private Quaternion invalidReticleTargetRotation = Quaternion.identity;

		private Transform playAreaPreviewTransform;
		private Transform[] playAreaPreviewCorners;
		private Transform[] playAreaPreviewSides;

		private float loopingAudioMaxVolume = 0.0f;

		private Coroutine hintCoroutine = null;

		private bool originalHoverLockState = false;
		private Interactable originalHoveringInteractable = null;
		private AllowTeleportWhileAttachedToHand allowTeleportWhileAttached = null;

		private Vector3 startingFeetOffset = Vector3.zero;
		private bool movedFeetFarEnough = false;

		SteamVR_Events.Action chaperoneInfoInitializedAction;

		// Events

		// public static SteamVR_Events.Event< float > ChangeScene = new SteamVR_Events.Event< float >();
		// public static SteamVR_Events.Action< float > ChangeSceneAction( UnityAction< float > action ) { return new SteamVR_Events.Action< float >( ChangeScene, action ); }

		// public static SteamVR_Events.Event< TeleportMarkerBase > Player = new SteamVR_Events.Event< TeleportMarkerBase >();
		// public static SteamVR_Events.Action< TeleportMarkerBase > PlayerAction( UnityAction< TeleportMarkerBase > action ) { return new SteamVR_Events.Action< TeleportMarkerBase >( Player, action ); }

		// public static SteamVR_Events.Event< TeleportMarkerBase > PlayerPre = new SteamVR_Events.Event< TeleportMarkerBase >();
		// public static SteamVR_Events.Action< TeleportMarkerBase > PlayerPreAction( UnityAction< TeleportMarkerBase > action ) { return new SteamVR_Events.Action< TeleportMarkerBase >( PlayerPre, action ); }

		//-------------------------------------------------
		private static Teleport _instance;
		public static Teleport instance
		{
			get
			{
				if ( _instance == null )
				{
					_instance = GameObject.FindObjectOfType<Teleport>();
				}

				return _instance;
			}
		}


		//-------------------------------------------------
		void Awake()
        {
            _instance = this;

			chaperoneInfoInitializedAction = ChaperoneInfo.InitializedAction( OnChaperoneInfoInitialized );

			pointerLineRenderer = GetComponentInChildren<LineRenderer>();
			teleportPointerObject = pointerLineRenderer.gameObject;

			int tintColorID = Shader.PropertyToID( "_TintColor" );
			fullTintAlpha = pointVisibleMaterial.GetColor( tintColorID ).a;

			teleportArc = GetComponent<TeleportArc>();
			teleportArc.traceLayerMask = traceLayerMask;

			loopingAudioMaxVolume = loopingAudioSource.volume;

			playAreaPreviewCorner.SetActive( false );
			playAreaPreviewSide.SetActive( false );

			float invalidReticleStartingScale = invalidReticleTransform.localScale.x;
			invalidReticleMinScale *= invalidReticleStartingScale;
			invalidReticleMaxScale *= invalidReticleStartingScale;
		}


		//-------------------------------------------------
		void Start()
        {
            teleportMarkers = GameObject.FindObjectsOfType<TeleportMarkerBase>();

			HidePointer();

			player = InteractionSystem.Player.instance;

			if ( player == null )
			{
				Debug.LogError("<b>[SteamVR Interaction]</b> Teleport: No Player instance found in map.");
				Destroy( this.gameObject );
				return;
			}

			CheckForSpawnPoint();

			Invoke( "ShowTeleportHint", 5.0f );
		}


		//-------------------------------------------------
		void OnEnable()
		{
			chaperoneInfoInitializedAction.enabled = true;
			OnChaperoneInfoInitialized(); // In case it's already initialized
		}


		//-------------------------------------------------
		void OnDisable()
		{
			chaperoneInfoInitializedAction.enabled = false;
			HidePointer();
		}


		//-------------------------------------------------
		private void CheckForSpawnPoint()
		{
			foreach ( TeleportMarkerBase teleportMarker in teleportMarkers )
			{
				TeleportPoint teleportPoint = teleportMarker as TeleportPoint;
				if ( teleportPoint && teleportPoint.playerSpawnPoint )
				{
					teleportingToMarker = teleportMarker;
					TeleportPlayer();
					break;
				}
			}
		}


		//-------------------------------------------------
		// public void HideTeleportPointer()
		// {
		// 	if ( pointerHand != null )
		// 	{
		// 		HidePointer();
		// 	}
		// }


		//-------------------------------------------------
		void Update()
		{

			bool teleportNewlyPressed = false;

			// Hand oldPointerHand = pointerHand;
			// Hand newPointerHand = null;

			// foreach ( Hand hand in player.hands )
			// {
				if ( visible )
				{
					if ( WasTeleportButtonReleased(  ) )
					// if ( WasTeleportButtonReleased( hand ) )
					
					{
						// if ( pointerHand == hand ) //This is the pointer hand
						// {
							TryTeleportPlayer();
						// }
					}
				}

				if ( WasTeleportButtonPressed(  ) )
				// if ( WasTeleportButtonPressed( hand ) )
				
				{
					teleportNewlyPressed = true;
					// newPointerHand = hand;
				}
			// }

			//If something is attached to the hand that is preventing teleport
			if ( allowTeleportWhileAttached && !allowTeleportWhileAttached.teleportAllowed )
			{
				HidePointer();
			}
			else
			{
				// if ( !visible && newPointerHand != null )
				if ( !visible && teleportNewlyPressed )
				
				{
					//Begin showing the pointer
					ShowPointer( );// newPointerHand, oldPointerHand );
				}
				else if ( visible )
				{


					if ( !teleportNewlyPressed && !IsTeleportButtonDown( ) )// pointerHand ) )
					{
						//Hide the pointer
						HidePointer();
					}


					// if ( newPointerHand == null && !IsTeleportButtonDown( pointerHand ) )
					// {
					// 	//Hide the pointer
					// 	HidePointer();
					// }
					// else if ( newPointerHand != null )
					// {
					// 	//Move the pointer to a new hand
					// 	ShowPointer( newPointerHand, oldPointerHand );
					// }
				}
			}

			if ( visible )
			{
				UpdatePointer();

				if ( meshFading )
				{
					UpdateTeleportColors();
				}

				// if ( onActivateObjectTransform.gameObject.activeSelf && Time.time - pointerShowStartTime > activateObjectTime )
				// {
				// 	onActivateObjectTransform.gameObject.SetActive( false );
				// }
			}
			else
			{
				// if ( onDeactivateObjectTransform.gameObject.activeSelf && Time.time - pointerHideStartTime > deactivateObjectTime )
				// {
				// 	onDeactivateObjectTransform.gameObject.SetActive( false );
				// }
			}
		}


		//-------------------------------------------------


		void SetArcColor (Color32 color) {
			teleportArc.SetColor( color );
// #if (UNITY_5_4)
// 			pointerLineRenderer.SetColors( color, color );
// #else
			pointerLineRenderer.startColor = color;
			pointerLineRenderer.endColor = color;
// #endif

		}


		bool ShowPlayArea (Vector3 playerFeetOffset) {
			if ( !showPlayAreaMarker ) return false;
			if ( playAreaPreviewTransform == null ) return false;
			
			Vector3 offsetToUse = playerFeetOffset;

			//Adjust the actual offset to prevent the play area marker from moving too much
			if ( !movedFeetFarEnough )
			{
				float distanceFromStartingOffset = Vector3.Distance( playerFeetOffset, startingFeetOffset );
				if ( distanceFromStartingOffset < 0.1f )
				{
					offsetToUse = startingFeetOffset;
				}
				else if ( distanceFromStartingOffset < 0.4f )
				{
					offsetToUse = Vector3.Lerp( startingFeetOffset, playerFeetOffset, ( distanceFromStartingOffset - 0.1f ) / 0.3f );
				}
				else
				{
					movedFeetFarEnough = true;
				}
			}

			playAreaPreviewTransform.position = pointedAtPosition + offsetToUse;

			return true;
		}


		void ActivateReticles (bool destination, bool invalid, bool offset) {

			destinationReticleTransform.gameObject.SetActive( destination );
			invalidReticleTransform.gameObject.SetActive( invalid );
			offsetReticleTransform.gameObject.SetActive( offset );

		}



		bool validAreaTargeted;

				

		
		private void UpdatePointer()
		{

			validAreaTargeted = false;


			Vector3 pointerStart = pointerStartTransform.position;
			Vector3 pointerEnd;
			Vector3 pointerDir = pointerStartTransform.forward;
			bool hitSomething = false;
			
			Vector3 playerFeetOffset = player.trackingOriginTransformPosition - player.feetPositionGuess;

			Vector3 arcVelocity = pointerDir * arcDistance;

			TeleportMarkerBase hitTeleportMarker = null;

			//Check pointer angle
			float dotUp = Vector3.Dot( pointerDir, Vector3.up );
			float dotForward = Vector3.Dot( pointerDir, player.hmdTransform.forward );
			bool pointerAtBadAngle = false;
			if ( ( dotForward > 0 && dotUp > 0.75f ) || ( dotForward < 0.0f && dotUp > 0.5f ) )
			{
				pointerAtBadAngle = true;
			}

			//Trace to see if the pointer hit anything
			RaycastHit hitInfo;
			teleportArc.SetArcData( pointerStart, arcVelocity, true, pointerAtBadAngle );
			if ( teleportArc.DrawArc( out hitInfo ) )
			{
				hitSomething = true;
				hitTeleportMarker = hitInfo.collider.GetComponentInParent<TeleportMarkerBase>();
			}




			if ( pointerAtBadAngle )
			{
				hitTeleportMarker = null;
			}

			HighlightSelected( hitTeleportMarker );


			// bool hasValidSpot = false;

			if ( hitTeleportMarker != null ) //Hit a teleport marker
			{
				if ( hitTeleportMarker.locked )
				{

					SetArcColor( pointerLockedColor );
					// destinationReticleTransform.gameObject.SetActive( false );

					
					
				}
				else
				{
					validAreaTargeted = true;

					// hasValidSpot = true;

					SetArcColor( pointerValidColor );
					
					// destinationReticleTransform.gameObject.SetActive( false );//hitTeleportMarker.showReticle );
				}





				ActivateReticles (false, false, true);
				// AdjustArcAndReticles(true, pointerAtBadAngle);
				

				// offsetReticleTransform.gameObject.SetActive( true );
				// invalidReticleTransform.gameObject.SetActive( false );


				pointedAtTeleportMarker = hitTeleportMarker;
				pointedAtPosition = hitInfo.point;


			

				pointerEnd = hitInfo.point;
			}
			else //Hit neither
			{

				validAreaTargeted = !pointerAtBadAngle && hitSomething; // not locked area

				//if valid area targeted, check for angle

				// hasValidSpot = !pointerAtBadAngle; //and not locked etc...



				ActivateReticles (validAreaTargeted, !validAreaTargeted && !pointerAtBadAngle, !pointerAtBadAngle);
				

				// AdjustArcAndReticles(validAreaTargeted, pointerAtBadAngle);
						
					
				
				
				
				// destinationReticleTransform.gameObject.SetActive( false );
				// offsetReticleTransform.gameObject.SetActive( false );

				// SetArcColor( hasValidSpot ? pointerValidColor : pointerInvalidColor );
				SetArcColor( validAreaTargeted ? pointerValidColor : pointerInvalidColor );

				// invalidReticleTransform.gameObject.SetActive( !pointerAtBadAngle );

				
				
				// if (!pointerAtBadAngle && !validAreaTargeted) {
				if (invalidReticleTransform.gameObject.activeSelf) {

					//Orient the invalid reticle to the normal of the trace hit point
					Vector3 normalToUse = hitInfo.normal;
					float angle = Vector3.Angle( hitInfo.normal, Vector3.up );
					if ( angle < 15.0f )
					{
						normalToUse = Vector3.up;
					}
					
					
					Quaternion invalidReticleTargetRotation = Quaternion.FromToRotation( Vector3.up, normalToUse );
					invalidReticleTransform.rotation = Quaternion.Slerp( invalidReticleTransform.rotation, invalidReticleTargetRotation, 0.1f );

					//Scale the invalid reticle based on the distance from the player
					float distanceFromPlayer = Vector3.Distance( hitInfo.point, player.hmdTransform.position );
					float invalidReticleCurrentScale = Util.RemapNumberClamped( distanceFromPlayer, invalidReticleMinScaleDistance, invalidReticleMaxScaleDistance, invalidReticleMinScale, invalidReticleMaxScale );
					// invalidReticleScale.x = invalidReticleCurrentScale;
					// invalidReticleScale.y = invalidReticleCurrentScale;
					// invalidReticleScale.z = invalidReticleCurrentScale;
					invalidReticleTransform.transform.localScale = Vector3.one * invalidReticleCurrentScale;// invalidReticleScale;
				}

				if (destinationReticleTransform.gameObject.activeSelf) {

					//Orient the invalid reticle to the normal of the trace hit point
					Vector3 normalToUse = hitInfo.normal;
					float angle = Vector3.Angle( hitInfo.normal, Vector3.up );
					if ( angle < 15.0f )
					{
						normalToUse = Vector3.up;
					}
					
					
					Quaternion invalidReticleTargetRotation = Quaternion.FromToRotation( Vector3.up, normalToUse );
					destinationReticleTransform.rotation = Quaternion.Slerp( destinationReticleTransform.rotation, invalidReticleTargetRotation, 0.1f );

					//Scale the invalid reticle based on the distance from the player
					float distanceFromPlayer = Vector3.Distance( hitInfo.point, player.hmdTransform.position );
					float invalidReticleCurrentScale = Util.RemapNumberClamped( distanceFromPlayer, invalidReticleMinScaleDistance, invalidReticleMaxScaleDistance, invalidReticleMinScale, invalidReticleMaxScale );
					// invalidReticleScale.x = invalidReticleCurrentScale;
					// invalidReticleScale.y = invalidReticleCurrentScale;
					// invalidReticleScale.z = invalidReticleCurrentScale;
					destinationReticleTransform.transform.localScale = Vector3.one * invalidReticleCurrentScale;// invalidReticleScale;
				}








				pointedAtTeleportMarker = null;

				if ( hitSomething )
				{
					pointerEnd = hitInfo.point;
				}
				else
				{
					pointerEnd = teleportArc.GetArcPositionAtTime( teleportArc.arcDuration );
				}

				pointedAtPosition = pointerEnd;

				//Debug floor
				if ( debugFloor )
				{
					floorDebugSphere.gameObject.SetActive( false );
					floorDebugLine.gameObject.SetActive( false );
				}
			}


			bool showPlayAreaPreview = validAreaTargeted && ShowPlayArea(playerFeetOffset);


			if ( playAreaPreviewTransform != null )
			{
				playAreaPreviewTransform.gameObject.SetActive( showPlayAreaPreview );
			}

			if ( !showOffsetReticle )
			{
				offsetReticleTransform.gameObject.SetActive( false );
			}

			destinationReticleTransform.position = pointedAtPosition;
			invalidReticleTransform.position = pointerEnd;
			
			// onActivateObjectTransform.position = pointerEnd;
			// onDeactivateObjectTransform.position = pointerEnd;
			
			offsetReticleTransform.position = pointerEnd - playerFeetOffset;

			reticleAudioSource.transform.position = pointedAtPosition;

			pointerLineRenderer.SetPosition( 0, pointerStart );
			pointerLineRenderer.SetPosition( 1, pointerEnd );
		}


		//-------------------------------------------------
		void FixedUpdate()
		{
			if ( !visible )
			{
				return;
			}

			if ( debugFloor )
			{
				//Debug floor
				// TeleportArea teleportArea = pointedAtTeleportMarker as TeleportArea;
				// if ( teleportArea != null )
				{
					if ( floorFixupMaximumTraceDistance > 0.0f )
					{
						floorDebugSphere.gameObject.SetActive( true );
						floorDebugLine.gameObject.SetActive( true );

						RaycastHit raycastHit;
						Vector3 traceDir = Vector3.down;
						traceDir.x = 0.01f;
						if ( Physics.Raycast( pointedAtPosition + 0.05f * traceDir, traceDir, out raycastHit, floorFixupMaximumTraceDistance, floorFixupTraceLayerMask ) )
						{
							floorDebugSphere.transform.position = raycastHit.point;
							floorDebugSphere.material.color = Color.green;
// #if (UNITY_5_4)
// 							floorDebugLine.SetColors( Color.green, Color.green );
// #else
							floorDebugLine.startColor = Color.green;
							floorDebugLine.endColor = Color.green;
// #endif
							floorDebugLine.SetPosition( 0, pointedAtPosition );
							floorDebugLine.SetPosition( 1, raycastHit.point );
						}
						else
						{
							Vector3 rayEnd = pointedAtPosition + ( traceDir * floorFixupMaximumTraceDistance );
							floorDebugSphere.transform.position = rayEnd;
							floorDebugSphere.material.color = Color.red;
// #if (UNITY_5_4)
// 							floorDebugLine.SetColors( Color.red, Color.red );
// #else
							floorDebugLine.startColor = Color.red;
							floorDebugLine.endColor = Color.red;
// #endif
							floorDebugLine.SetPosition( 0, pointedAtPosition );
							floorDebugLine.SetPosition( 1, rayEnd );
						}
					}
				}
			}
		}


		//-------------------------------------------------
		private void OnChaperoneInfoInitialized()
		{
			ChaperoneInfo chaperone = ChaperoneInfo.instance;

			if ( chaperone.initialized && chaperone.roomscale )
			{
				//Set up the render model for the play area bounds

				if ( playAreaPreviewTransform == null )
				{
					playAreaPreviewTransform = new GameObject( "PlayAreaPreviewTransform" ).transform;
					playAreaPreviewTransform.parent = transform;
					Util.ResetTransform( playAreaPreviewTransform );

					playAreaPreviewCorner.SetActive( true );
					playAreaPreviewCorners = new Transform[4];
					playAreaPreviewCorners[0] = playAreaPreviewCorner.transform;
					playAreaPreviewCorners[1] = Instantiate( playAreaPreviewCorners[0] );
					playAreaPreviewCorners[2] = Instantiate( playAreaPreviewCorners[0] );
					playAreaPreviewCorners[3] = Instantiate( playAreaPreviewCorners[0] );

					playAreaPreviewCorners[0].transform.parent = playAreaPreviewTransform;
					playAreaPreviewCorners[1].transform.parent = playAreaPreviewTransform;
					playAreaPreviewCorners[2].transform.parent = playAreaPreviewTransform;
					playAreaPreviewCorners[3].transform.parent = playAreaPreviewTransform;

					playAreaPreviewSide.SetActive( true );
					playAreaPreviewSides = new Transform[4];
					playAreaPreviewSides[0] = playAreaPreviewSide.transform;
					playAreaPreviewSides[1] = Instantiate( playAreaPreviewSides[0] );
					playAreaPreviewSides[2] = Instantiate( playAreaPreviewSides[0] );
					playAreaPreviewSides[3] = Instantiate( playAreaPreviewSides[0] );

					playAreaPreviewSides[0].transform.parent = playAreaPreviewTransform;
					playAreaPreviewSides[1].transform.parent = playAreaPreviewTransform;
					playAreaPreviewSides[2].transform.parent = playAreaPreviewTransform;
					playAreaPreviewSides[3].transform.parent = playAreaPreviewTransform;
				}

				float x = chaperone.playAreaSizeX;
				float z = chaperone.playAreaSizeZ;

				playAreaPreviewSides[0].localPosition = new Vector3( 0.0f, 0.0f, 0.5f * z - 0.25f );
				playAreaPreviewSides[1].localPosition = new Vector3( 0.0f, 0.0f, -0.5f * z + 0.25f );
				playAreaPreviewSides[2].localPosition = new Vector3( 0.5f * x - 0.25f, 0.0f, 0.0f );
				playAreaPreviewSides[3].localPosition = new Vector3( -0.5f * x + 0.25f, 0.0f, 0.0f );

				playAreaPreviewSides[0].localScale = new Vector3( x - 0.5f, 1.0f, 1.0f );
				playAreaPreviewSides[1].localScale = new Vector3( x - 0.5f, 1.0f, 1.0f );
				playAreaPreviewSides[2].localScale = new Vector3( z - 0.5f, 1.0f, 1.0f );
				playAreaPreviewSides[3].localScale = new Vector3( z - 0.5f, 1.0f, 1.0f );

				playAreaPreviewSides[0].localRotation = Quaternion.Euler( 0.0f, 0.0f, 0.0f );
				playAreaPreviewSides[1].localRotation = Quaternion.Euler( 0.0f, 180.0f, 0.0f );
				playAreaPreviewSides[2].localRotation = Quaternion.Euler( 0.0f, 90.0f, 0.0f );
				playAreaPreviewSides[3].localRotation = Quaternion.Euler( 0.0f, 270.0f, 0.0f );

				playAreaPreviewCorners[0].localPosition = new Vector3( 0.5f * x - 0.25f, 0.0f, 0.5f * z - 0.25f );
				playAreaPreviewCorners[1].localPosition = new Vector3( 0.5f * x - 0.25f, 0.0f, -0.5f * z + 0.25f );
				playAreaPreviewCorners[2].localPosition = new Vector3( -0.5f * x + 0.25f, 0.0f, -0.5f * z + 0.25f );
				playAreaPreviewCorners[3].localPosition = new Vector3( -0.5f * x + 0.25f, 0.0f, 0.5f * z - 0.25f );

				playAreaPreviewCorners[0].localRotation = Quaternion.Euler( 0.0f, 0.0f, 0.0f );
				playAreaPreviewCorners[1].localRotation = Quaternion.Euler( 0.0f, 90.0f, 0.0f );
				playAreaPreviewCorners[2].localRotation = Quaternion.Euler( 0.0f, 180.0f, 0.0f );
				playAreaPreviewCorners[3].localRotation = Quaternion.Euler( 0.0f, 270.0f, 0.0f );

				playAreaPreviewTransform.gameObject.SetActive( false );
			}
		}


		//-------------------------------------------------
		private void HidePointer()
		{
			
			if ( visible )
			{
				pointerHideStartTime = Time.time;
			}

			visible = false;

			if ( isPointing )
			// if ( pointerHand )
			{
				if ( ShouldOverrideHoverLock() )
				{
					//Restore the original hovering interactable on the hand
					if ( originalHoverLockState == true )
					{
						teleportHandClass.HoverLock( originalHoveringInteractable );
						// pointerHand.HoverLock( originalHoveringInteractable );
					}
					else
					{
						
						teleportHandClass.HoverUnlock( null );
						// pointerHand.HoverUnlock( null );
					}
				}

				//Stop looping sound
				loopingAudioSource.Stop();
				PlayAudioClip( pointerAudioSource, pointerStopSound );
			}
			teleportPointerObject.SetActive( false );

			teleportArc.Hide();

			foreach ( TeleportMarkerBase teleportMarker in teleportMarkers )
			{
				if ( teleportMarker != null && teleportMarker.markerActive && teleportMarker.gameObject != null )
				{
					teleportMarker.gameObject.SetActive( false );
				}
			}

			destinationReticleTransform.gameObject.SetActive( false );
			invalidReticleTransform.gameObject.SetActive( false );
			offsetReticleTransform.gameObject.SetActive( false );

			// if ( playAreaPreviewTransform != null )
			// {
			// 	playAreaPreviewTransform.gameObject.SetActive( false );
			// }

			// if ( onActivateObjectTransform.gameObject.activeSelf )
			// {
			// 	onActivateObjectTransform.gameObject.SetActive( false );
			// }
			// onDeactivateObjectTransform.gameObject.SetActive( true );

			isPointing = false;
			// pointerHand = null;
		}


		//-------------------------------------------------
		private void ShowPointer ( )// Hand newPointerHand, Hand oldPointerHand )
		{
			if ( !visible )
			{
				pointedAtTeleportMarker = null;
				pointerShowStartTime = Time.time;
				visible = true;
				meshFading = true;

				teleportPointerObject.SetActive( false );
				teleportArc.Show();

				foreach ( TeleportMarkerBase teleportMarker in teleportMarkers )
				{
					if ( teleportMarker.markerActive && teleportMarker.ShouldActivate( player.feetPositionGuess ) )
					{
						teleportMarker.gameObject.SetActive( true );
						teleportMarker.Highlight( false );
					}
				}

				startingFeetOffset = player.trackingOriginTransformPosition - player.feetPositionGuess;
				movedFeetFarEnough = false;

				// if ( onDeactivateObjectTransform.gameObject.activeSelf )
				// {
				// 	onDeactivateObjectTransform.gameObject.SetActive( false );
				// }
				// onActivateObjectTransform.gameObject.SetActive( true );




				loopingAudioSource.clip = pointerLoopSound;
				loopingAudioSource.loop = true;
				loopingAudioSource.Play();
				loopingAudioSource.volume = 0.0f;
			}




			// if ( oldPointerHand )
			// {
			// 	if ( ShouldOverrideHoverLock() )
			// 	{
			// 		//Restore the original hovering interactable on the hand
			// 		if ( originalHoverLockState == true )
			// 		{
			// 			oldPointerHand.HoverLock( originalHoveringInteractable );
			// 		}
			// 		else
			// 		{
			// 			oldPointerHand.HoverUnlock( null );
			// 		}
			// 	}
			// }


			isPointing = true;

			// pointerHand = newPointerHand;

			if ( visible  )//&& oldPointerHand != pointerHand )
			{
				PlayAudioClip( pointerAudioSource, pointerStartSound );
			}

			// if ( pointerHand )
			// {

				Hand pointerHand = teleportHandClass;

				pointerStartTransform = GetPointerStartTransform( pointerHand );

				if ( pointerHand.currentAttachedObject != null )
				{
					allowTeleportWhileAttached = pointerHand.currentAttachedObject.GetComponent<AllowTeleportWhileAttachedToHand>();
				}

				//Keep track of any existing hovering interactable on the hand
				originalHoverLockState = pointerHand.hoverLocked;
				originalHoveringInteractable = pointerHand.hoveringInteractable;

				if ( ShouldOverrideHoverLock() )
				{
					pointerHand.HoverLock( null );
				}

				pointerAudioSource.transform.SetParent( pointerStartTransform );
				pointerAudioSource.transform.localPosition = Vector3.zero;

				loopingAudioSource.transform.SetParent( pointerStartTransform );
				loopingAudioSource.transform.localPosition = Vector3.zero;
			// }
		}


		//-------------------------------------------------
		private void UpdateTeleportColors()
		{
			float deltaTime = Time.time - pointerShowStartTime;
			if ( deltaTime > meshFadeTime )
			{
				meshAlphaPercent = 1.0f;
				meshFading = false;
			}
			else
			{
				meshAlphaPercent = Mathf.Lerp( 0.0f, 1.0f, deltaTime / meshFadeTime );
			}

			//Tint color for the teleport points
			foreach ( TeleportMarkerBase teleportMarker in teleportMarkers )
			{
				teleportMarker.SetAlpha( fullTintAlpha * meshAlphaPercent, meshAlphaPercent );
			}
		}


		//-------------------------------------------------
		private void PlayAudioClip( AudioSource source, AudioClip clip )
		{
			source.clip = clip;
			source.Play();
		}


		//-------------------------------------------------
		private void PlayPointerHaptic( bool validLocation )
		{
			if (isPointing)
			// if ( pointerHand != null )
			{
				Hand pointerHand = teleportHandClass;
				if ( validLocation )
				{
					pointerHand.TriggerHapticPulse( 800 );
				}
				else
				{
					pointerHand.TriggerHapticPulse( 100 );
				}
			}
		}


		//-------------------------------------------------
		private void TryTeleportPlayer()
		{
			if ( visible && !teleporting )
			{
				if ( validAreaTargeted)
				// 	( pointedAtTeleportMarker != null && pointedAtTeleportMarker.locked == false )
				// 	|| (validAreaTargeted)

				// ) 
				
				{
					//Pointing at an unlocked teleport marker
					teleportingToMarker = pointedAtTeleportMarker;
					InitiateTeleportFade();

					CancelTeleportHint();
				}
			}
		}


		//-------------------------------------------------
		private void InitiateTeleportFade()
		{
			teleporting = true;

			currentFadeTime = teleportFadeTime;

			TeleportPoint teleportPoint = teleportingToMarker as TeleportPoint;
			if ( teleportPoint != null && teleportPoint.teleportType == TeleportPoint.TeleportPointType.SwitchToNewScene )
			{
				currentFadeTime *= 3.0f;
				// Teleport.ChangeScene.Send( currentFadeTime );
			}

			SteamVR_Fade.Start( Color.clear, 0 );
			SteamVR_Fade.Start( Color.black, currentFadeTime );


			headAudioSource.transform.SetParent( player.hmdTransform );
			headAudioSource.transform.localPosition = Vector3.zero;
			PlayAudioClip( headAudioSource, teleportSound );

			Invoke( "TeleportPlayer", currentFadeTime );
		}


		//-------------------------------------------------
		private void TeleportPlayer()
		{
			teleporting = false;

			// Teleport.PlayerPre.Send( pointedAtTeleportMarker );

			SteamVR_Fade.Start( Color.clear, currentFadeTime );

			TeleportPoint teleportPoint = teleportingToMarker as TeleportPoint;
			Vector3 teleportPosition = pointedAtPosition;

			if ( teleportPoint != null )
			{
				teleportPosition = teleportPoint.transform.position;

				//Teleport to a new scene
				if ( teleportPoint.teleportType == TeleportPoint.TeleportPointType.SwitchToNewScene )
				{
					teleportPoint.TeleportToScene();
					return;
				}
			}
			else
			// Find the actual floor position below the navigation mesh
			// TeleportArea teleportArea = teleportingToMarker as TeleportArea;
			// if ( teleportArea != null )
			{
				if ( floorFixupMaximumTraceDistance > 0.0f )
				{
					RaycastHit raycastHit;

					//maybe should be down ?
					if ( Physics.Raycast( teleportPosition + 0.05f * Vector3.down, Vector3.down, out raycastHit, floorFixupMaximumTraceDistance, floorFixupTraceLayerMask ) )
					{
						teleportPosition = raycastHit.point;
					}
				}
			}

			// if ( teleportingToMarker.ShouldMovePlayer() )
			// {
				Vector3 playerFeetOffset = player.trackingOriginTransformPosition - player.feetPositionGuess;
				player.trackingOriginTransform.position = teleportPosition + playerFeetOffset;
			// }
			// else
			// {
			// 	teleportingToMarker.TeleportPlayer( pointedAtPosition );
			// }

			// Teleport.Player.Send( pointedAtTeleportMarker );
		}


		//-------------------------------------------------
		private void HighlightSelected( TeleportMarkerBase hitTeleportMarker )
		{
			if ( pointedAtTeleportMarker != hitTeleportMarker ) //Pointing at a new teleport marker
			{
				if ( pointedAtTeleportMarker != null )
				{
					pointedAtTeleportMarker.Highlight( false );
				}

				if ( hitTeleportMarker != null )
				{
					hitTeleportMarker.Highlight( true );

					prevPointedAtPosition = pointedAtPosition;
					PlayPointerHaptic( !hitTeleportMarker.locked );

					PlayAudioClip( reticleAudioSource, goodHighlightSound );

					loopingAudioSource.volume = loopingAudioMaxVolume;
				}
				else if ( pointedAtTeleportMarker != null )
				{
					PlayAudioClip( reticleAudioSource, badHighlightSound );

					loopingAudioSource.volume = 0.0f;
				}
			}
			else if ( hitTeleportMarker != null ) //Pointing at the same teleport marker
			{
				if ( Vector3.Distance( prevPointedAtPosition, pointedAtPosition ) > 1.0f )
				{
					prevPointedAtPosition = pointedAtPosition;
					PlayPointerHaptic( !hitTeleportMarker.locked );
				}
			}
		}


		//-------------------------------------------------
		public void ShowTeleportHint()
		{
			CancelTeleportHint();

			hintCoroutine = StartCoroutine( TeleportHintCoroutine() );
		}


		//-------------------------------------------------
		public void CancelTeleportHint()
		{
			if ( hintCoroutine != null )
            {
                ControllerButtonHints.HideTextHint(player.leftHand, teleportAction);
                ControllerButtonHints.HideTextHint(player.rightHand, teleportAction);

				StopCoroutine( hintCoroutine );
				hintCoroutine = null;
			}

			CancelInvoke( "ShowTeleportHint" );
		}


		//-------------------------------------------------
		private IEnumerator TeleportHintCoroutine()
		{
			float prevBreakTime = Time.time;
			float prevHapticPulseTime = Time.time;

			while ( true )
			{
				bool pulsed = false;

				//Show the hint on each eligible hand
				// foreach ( Hand hand in player.hands )
				// {

					Hand hand = teleportHandClass;
					bool showHint = IsEligibleForTeleport( hand );
					bool isShowingHint = !string.IsNullOrEmpty( ControllerButtonHints.GetActiveHintText( hand, teleportAction) );
					if ( showHint )
					{
						if ( !isShowingHint )
						{
							ControllerButtonHints.ShowTextHint( hand, teleportAction, "Teleport" );
							prevBreakTime = Time.time;
							prevHapticPulseTime = Time.time;
						}

						if ( Time.time > prevHapticPulseTime + 0.05f )
						{
							//Haptic pulse for a few seconds
							pulsed = true;

							hand.TriggerHapticPulse( 500 );
						}
					}
					else if ( !showHint && isShowingHint )
					{
						ControllerButtonHints.HideTextHint( hand, teleportAction);
					}
				// }

				if ( Time.time > prevBreakTime + 3.0f )
				{
					//Take a break for a few seconds
					yield return new WaitForSeconds( 3.0f );

					prevBreakTime = Time.time;
				}

				if ( pulsed )
				{
					prevHapticPulseTime = Time.time;
				}

				yield return null;
			}
		}


		//-------------------------------------------------
		public bool IsEligibleForTeleport( Hand hand )
		{
			if ( hand == null )
			{
				return false;
			}

			if ( !hand.gameObject.activeInHierarchy )
			{
				return false;
			}

			if ( hand.hoveringInteractable != null )
			{
				return false;
			}

			if ( hand.noSteamVRFallbackCamera == null )
			{
				if ( hand.isActive == false)
				{
					return false;
				}

				//Something is attached to the hand
				if ( hand.currentAttachedObject != null )
				{
					AllowTeleportWhileAttachedToHand allowTeleportWhileAttachedToHand = hand.currentAttachedObject.GetComponent<AllowTeleportWhileAttachedToHand>();

					return allowTeleportWhileAttachedToHand != null && allowTeleportWhileAttachedToHand.teleportAllowed == true;
					
				}
			}

			return true;
		}


		//-------------------------------------------------
		private bool ShouldOverrideHoverLock()
		{
			if ( !allowTeleportWhileAttached || allowTeleportWhileAttached.overrideHoverLock )
			{
				return true;
			}

			return false;
		}


		//-------------------------------------------------
		private bool WasTeleportButtonReleased()// Hand hand )
		{
			Hand hand = teleportHandClass;

			if ( IsEligibleForTeleport( hand ) )
			{
				if ( hand.noSteamVRFallbackCamera != null )
				{
					return Input.GetKeyUp( KeyCode.T );
				}
				else
                {
                    return teleportAction.GetStateUp(hand.handType);

                    //return hand.controller.GetPressUp( SteamVR_Controller.ButtonMask.Touchpad );
                }
			}

			return false;
		}

		//-------------------------------------------------
		private bool IsTeleportButtonDown( )//Hand hand )
		{
			Hand hand = teleportHandClass;
			
			if ( IsEligibleForTeleport( hand ) )
			{
				if ( hand.noSteamVRFallbackCamera != null )
				{
					return Input.GetKey( KeyCode.T );
				}
				else
                {
                    return teleportAction.GetState(hand.handType);
				}
			}

			return false;
		}


		//-------------------------------------------------
		private bool WasTeleportButtonPressed( )//Hand hand )
		{
			Hand hand = teleportHandClass;
			
			if ( IsEligibleForTeleport( hand ) )
			{
				if ( hand.noSteamVRFallbackCamera != null )
				{
					return Input.GetKeyDown( KeyCode.T );
				}
				else
                {
                    return teleportAction.GetStateDown(hand.handType);

                    //return hand.controller.GetPressDown( SteamVR_Controller.ButtonMask.Touchpad );
				}
			}

			return false;
		}


		//-------------------------------------------------
		private Transform GetPointerStartTransform( Hand hand )
		{
			if ( hand.noSteamVRFallbackCamera != null )
			{
				return hand.noSteamVRFallbackCamera.transform;
			}
			else
			{
				return hand.transform;
			}
		}
	}
}
