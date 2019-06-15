//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Handles all the teleport logic
//
//=============================================================================
/*

	to do:
		disable collider on fast teleport

 */
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Valve.VR.InteractionSystem;
using Valve.VR;
namespace VRPlayer
{
	//-------------------------------------------------------------------------
	public class Teleport : MonoBehaviour
    {

		public bool teleportationAllowed;


        public SteamVR_Action_Boolean teleportAction;
		public SteamVR_Input_Sources teleportHand = SteamVR_Input_Sources.LeftHand;

		Hand teleportHandClass {
			get {
				return Player.instance.GetHand(teleportHand);
			}
		}

        public LayerMask traceLayerMask;
		public Material pointVisibleMaterial;
		public Material pointLockedMaterial;
		public Material pointHighlightedMaterial;
		public Transform destinationReticleTransform;
		public Transform invalidReticleTransform;
		// public GameObject playAreaPreviewCorner;
		// public GameObject playAreaPreviewSide;
		public Color pointerValidColor;
		public Color pointerInvalidColor;
		public Color pointerLockedColor;
		public bool showPlayAreaMarker = true;
		public float teleportFadeTime = 0.1f;
		public float meshFadeTime = 0.2f;
		public float arcDistance = 10.0f;


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

		
		private LineRenderer pointerLineRenderer;
		private GameObject teleportPointerObject;
		private Transform pointerStartTransform;


		bool isPointing;
		
		
		private Player player = null;
		private TeleportArc teleportArc = null;

		private bool visible = false;

		private TeleportPoint[] teleportMarkers;
		private TeleportPoint pointedAtTeleportMarker;
		private TeleportPoint teleportingToMarker;
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
		
		// private Transform playAreaPreviewTransform;
		// private Transform[] playAreaPreviewCorners;
		// private Transform[] playAreaPreviewSides;

		private float loopingAudioMaxVolume = 0.0f;

		// private Coroutine hintCoroutine = null;

		// private bool originalHoverLockState = false;
		// private Interactable originalHoveringInteractable = null;
		// private AllowTeleportWhileAttachedToHand allowTeleportWhileAttached = null;

		private Vector3 startingFeetOffset = Vector3.zero;
		private bool movedFeetFarEnough = false;

		// SteamVR_Events.Action chaperoneInfoInitializedAction;

		public float maxGroundAngle = 45;

		
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

		float trackingTransformOffset;
		public void SetCurrentTrackingTransformOffset (float offset) {
			trackingTransformOffset = offset;
		}


		//-------------------------------------------------
		void Awake()
        {
            _instance = this;

			// chaperoneInfoInitializedAction = ChaperoneInfo.InitializedAction( OnChaperoneInfoInitialized );

			pointerLineRenderer = GetComponentInChildren<LineRenderer>();
			teleportPointerObject = pointerLineRenderer.gameObject;

			int tintColorID = Shader.PropertyToID( "_TintColor" );
			fullTintAlpha = pointVisibleMaterial.GetColor( tintColorID ).a;

			teleportArc = GetComponent<TeleportArc>();
			teleportArc.traceLayerMask = traceLayerMask;

			loopingAudioMaxVolume = loopingAudioSource.volume;

			// playAreaPreviewCorner.SetActive( false );
			// playAreaPreviewSide.SetActive( false );

			float invalidReticleStartingScale = invalidReticleTransform.localScale.x;
			invalidReticleMinScale *= invalidReticleStartingScale;
			invalidReticleMaxScale *= invalidReticleStartingScale;
		}


		//-------------------------------------------------
		void Start()
        {
            teleportMarkers = GameObject.FindObjectsOfType<TeleportPoint>();

			HidePointer();

			player = Player.instance;

			if ( player == null )
			{
				Debug.LogError("<b>[SteamVR Interaction]</b> Teleport: No Player instance found in map.");
				Destroy( this.gameObject );
				return;
			}

			// CheckForSpawnPoint();

			// Invoke( "ShowTeleportHint", 5.0f );
		}


		//-------------------------------------------------
		// void OnEnable()
		// {
		// 	chaperoneInfoInitializedAction.enabled = true;
		// 	OnChaperoneInfoInitialized(); // In case it's already initialized
		// }


		// //-------------------------------------------------
		void OnDisable()
		{
		// 	chaperoneInfoInitializedAction.enabled = false;
			HidePointer();
		}



			



		//-------------------------------------------------
		// private void CheckForSpawnPoint()
		// {
		// 	foreach ( TeleportPoint teleportMarker in teleportMarkers )
		// 	{
		// 		// TeleportPoint teleportPoint = teleportMarker as TeleportPoint;
		// 		if ( teleportMarker.playerSpawnPoint )
		// 		{
		// 			teleportingToMarker = teleportMarker;
		// 			TeleportPlayer();
		// 			break;
		// 		}
		// 	}
		// }

		//-------------------------------------------------
		void Update()
		{
			SmoothTeleport(Time.deltaTime);

			bool teleportNewlyPressed = false;

			if ( visible )
			{
				if ( WasTeleportButtonReleased(  ) )
					
				{
						TryTeleportPlayer();
				}
			}

			if ( WasTeleportButtonPressed(  ) )
				
			{
				teleportNewlyPressed = true;
			}
		
			//If something is attached to the hand that is preventing teleport
			// if ( allowTeleportWhileAttached && !allowTeleportWhileAttached.teleportAllowed )
			// {
			// 	HidePointer();
			// }
			// else
			// {
				if ( !visible && teleportNewlyPressed )
				
				{
					//Begin showing the pointer
					ShowPointer( );
				}
				else if ( visible )
				{


					if ( !teleportNewlyPressed && !IsTeleportButtonDown( ) )
					{
						//Hide the pointer
						HidePointer();
					}
				}
			// }

			if ( visible )
			{
				UpdatePointer();

				if ( meshFading )
				{
					UpdateTeleportColors();
				}
			}
			
		}


		//-------------------------------------------------


		void SetArcColor (Color32 color) {
			teleportArc.SetColor( color );
			pointerLineRenderer.startColor = color;
			pointerLineRenderer.endColor = color;
		}


		bool ShowPlayArea (Vector3 playerFeetOffset, Vector3 pointedAtPosition) {
			if ( !showPlayAreaMarker ) return false;
			// if ( playAreaPreviewTransform == null ) return false;
			
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

			// SceneChaperone.instance.transform.position = pointedAtPosition + offsetToUse;
			// SceneChaperone.instance.transform.rotation = Player.instance.transform.rotation;

			SceneChaperone.SetTransform(
pointedAtPosition + offsetToUse,
Player.instance.transform.rotation

			);
			

			return true;
		}


		void ActivateReticles (bool destination, bool invalid){//, bool offset) {

			destinationReticleTransform.gameObject.SetActive( destination );
			invalidReticleTransform.gameObject.SetActive( invalid );
			
		}




		bool validAreaTargeted;

		
		private void UpdatePointer()
		{

			validAreaTargeted = false;


			Vector3 pointerStart = pointerStartTransform.position;
			Vector3 pointerEnd;
			Vector3 pointerDir = pointerStartTransform.forward;
			bool hitSomething = false;
			
			Vector3 playerFeetOffset = player.trackingOriginTransform.position - player.feetPositionGuess;

			Vector3 arcVelocity = pointerDir * arcDistance;

			TeleportPoint hitTeleportMarker = null;

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
				hitTeleportMarker = hitInfo.collider.GetComponentInParent<TeleportPoint>();
			}




			if ( pointerAtBadAngle )
			{
				hitTeleportMarker = null;
			}

			HighlightSelected( hitTeleportMarker );


			if ( hitTeleportMarker != null ) //Hit a teleport marker
			{
				if ( hitTeleportMarker.locked )
				{
					SetArcColor( pointerLockedColor );					
				}
				else
				{
					validAreaTargeted = true;
					SetArcColor( pointerValidColor );
				}





				ActivateReticles (false, false);//, true);
				

				pointedAtTeleportMarker = hitTeleportMarker;
				pointedAtPosition = hitInfo.point;


			

				pointerEnd = hitInfo.point;
			}
			else //Hit neither
			{

				Vector3 normalToUse = Vector3.up;
				float angle = 0;

				if (hitSomething) {
					normalToUse = hitInfo.normal;
					angle = Vector3.Angle( normalToUse, Vector3.up );
				}

				
				validAreaTargeted = !pointerAtBadAngle && hitSomething && (angle <= maxGroundAngle); // not locked area

				
				ActivateReticles (validAreaTargeted, !validAreaTargeted && !pointerAtBadAngle);//, !pointerAtBadAngle);
				SetArcColor( validAreaTargeted ? pointerValidColor : pointerInvalidColor );

				
				pointedAtTeleportMarker = null;

				if ( hitSomething )
				{
					pointerEnd = hitInfo.point;
				}
				else
				{
					pointerEnd = teleportArc.GetArcPositionAtTime( teleportArc.arcDuration );
				}


				
				
				if (invalidReticleTransform.gameObject.activeSelf) {

					ResizeReticleBasedOnDistance (invalidReticleTransform, pointerEnd, normalToUse);


					
				}

				if (destinationReticleTransform.gameObject.activeSelf) {

					ResizeReticleBasedOnDistance (destinationReticleTransform, pointerEnd, normalToUse);


				}








				
				pointedAtPosition = pointerEnd;

			}




			bool showPlayAreaPreview = validAreaTargeted && ShowPlayArea(playerFeetOffset, pointedAtPosition);

			SceneChaperone.Activate(showPlayAreaPreview);
			// SceneChaperone.instance.playAreaPreviewTransform.gameObject.SetActive(showPlayAreaPreview);
			// if (showPlayAreaPreview) {
			// 	Debug.Log("showing preview");
			// }
			// if ( playAreaPreviewTransform != null )
			// {
			// 	playAreaPreviewTransform.gameObject.SetActive( showPlayAreaPreview );
			// }

			
			destinationReticleTransform.position = pointedAtPosition;
			invalidReticleTransform.position = pointerEnd;
			
			
			reticleAudioSource.transform.position = pointedAtPosition;

			pointerLineRenderer.SetPosition( 0, pointerStart );
			pointerLineRenderer.SetPosition( 1, pointerEnd );
		}
		void ResizeReticleBasedOnDistance (Transform reticle, Vector3 pointerEnd, Vector3 hitNormal) {

			//Orient the reticle to the normal of the trace hit point
			
			Quaternion targetRotation = Quaternion.FromToRotation( Vector3.up, hitNormal );
			reticle.rotation = Quaternion.Slerp( reticle.rotation, targetRotation, 0.1f );

			//Scale the invalid reticle based on the distance from the player
			float distanceFromPlayer = Vector3.Distance( pointerEnd, player.hmdTransform.position );
			float invalidReticleCurrentScale = Util.RemapNumberClamped( distanceFromPlayer, invalidReticleMinScaleDistance, invalidReticleMaxScaleDistance, invalidReticleMinScale, invalidReticleMaxScale );
			reticle.transform.localScale = Vector3.one * invalidReticleCurrentScale;

				
		}




		// //Maybe adjust this to world scale
		// //-------------------------------------------------
		// private void OnChaperoneInfoInitialized()
		// {
		// 	ChaperoneInfo chaperone = ChaperoneInfo.instance;

		// 	if ( chaperone.initialized && chaperone.roomscale )
		// 	{
		// 		//Set up the render model for the play area bounds

		// 		if ( playAreaPreviewTransform == null )
		// 		{
		// 			playAreaPreviewTransform = new GameObject( "PlayAreaPreviewTransform" ).transform;
		// 			playAreaPreviewTransform.parent = transform;
		// 			Util.ResetTransform( playAreaPreviewTransform );

		// 			playAreaPreviewCorner.SetActive( true );
		// 			playAreaPreviewCorners = new Transform[4];

		// 			playAreaPreviewCorners[0] = playAreaPreviewCorner.transform;
		// 			for (int i = 1; i < 4; i++) {
		// 				playAreaPreviewCorners[i] = Instantiate( playAreaPreviewCorners[0] );
		// 			}
		// 			for (int i = 0; i < 4; i++) {
		// 				playAreaPreviewCorners[i].transform.parent = playAreaPreviewTransform;
		// 			}
						
		// 			playAreaPreviewSide.SetActive( true );
		// 			playAreaPreviewSides = new Transform[4];

		// 			playAreaPreviewSides[0] = playAreaPreviewSide.transform;
		// 			for (int i = 1; i < 4; i++) {
		// 				playAreaPreviewSides[i] = Instantiate( playAreaPreviewSides[0] );
		// 			}
		// 			for (int i = 0; i < 4; i++) {
		// 				playAreaPreviewSides[i].transform.parent = playAreaPreviewTransform;
		// 			}
		// 		}

		// 		float x = chaperone.playAreaSizeX;
		// 		float z = chaperone.playAreaSizeZ;

		// 		playAreaPreviewSides[0].localPosition = new Vector3( 0.0f, 0.0f, 0.5f * z - 0.25f );
		// 		playAreaPreviewSides[1].localPosition = new Vector3( 0.0f, 0.0f, -0.5f * z + 0.25f );
		// 		playAreaPreviewSides[2].localPosition = new Vector3( 0.5f * x - 0.25f, 0.0f, 0.0f );
		// 		playAreaPreviewSides[3].localPosition = new Vector3( -0.5f * x + 0.25f, 0.0f, 0.0f );

		// 		for (int i = 0; i < 4; i++) {
		// 			playAreaPreviewSides[i].localScale = new Vector3( (i < 2 ? x : z) - 0.5f, 1.0f, 1.0f );
		// 		}
				
		// 		// playAreaPreviewSides[0].localScale = new Vector3( x - 0.5f, 1.0f, 1.0f );
		// 		// playAreaPreviewSides[1].localScale = new Vector3( x - 0.5f, 1.0f, 1.0f );
		// 		// playAreaPreviewSides[2].localScale = new Vector3( z - 0.5f, 1.0f, 1.0f );
		// 		// playAreaPreviewSides[3].localScale = new Vector3( z - 0.5f, 1.0f, 1.0f );

		// 		playAreaPreviewSides[0].localRotation = Quaternion.Euler( 0.0f, 0.0f, 0.0f );
		// 		playAreaPreviewSides[1].localRotation = Quaternion.Euler( 0.0f, 180.0f, 0.0f );
		// 		playAreaPreviewSides[2].localRotation = Quaternion.Euler( 0.0f, 90.0f, 0.0f );
		// 		playAreaPreviewSides[3].localRotation = Quaternion.Euler( 0.0f, 270.0f, 0.0f );

		// 		playAreaPreviewCorners[0].localPosition = new Vector3( 0.5f * x - 0.25f, 0.0f, 0.5f * z - 0.25f );
		// 		playAreaPreviewCorners[1].localPosition = new Vector3( 0.5f * x - 0.25f, 0.0f, -0.5f * z + 0.25f );
		// 		playAreaPreviewCorners[2].localPosition = new Vector3( -0.5f * x + 0.25f, 0.0f, -0.5f * z + 0.25f );
		// 		playAreaPreviewCorners[3].localPosition = new Vector3( -0.5f * x + 0.25f, 0.0f, 0.5f * z - 0.25f );

		// 		for (int i = 0; i < 4; i++) {
		// 			playAreaPreviewCorners[i].localRotation = Quaternion.Euler( 0.0f, i * 90.0f, 0.0f );
		// 		}
					
		// 		// playAreaPreviewCorners[0].localRotation = Quaternion.Euler( 0.0f, 0.0f, 0.0f );
		// 		// playAreaPreviewCorners[1].localRotation = Quaternion.Euler( 0.0f, 90.0f, 0.0f );
		// 		// playAreaPreviewCorners[2].localRotation = Quaternion.Euler( 0.0f, 180.0f, 0.0f );
		// 		// playAreaPreviewCorners[3].localRotation = Quaternion.Euler( 0.0f, 270.0f, 0.0f );

		// 		playAreaPreviewTransform.gameObject.SetActive( false );
		// 	}
		// }



		//-------------------------------------------------
		private void HidePointer()
		{
			
			if ( visible )
			{
				pointerHideStartTime = Time.time;
			}

			visible = false;

			if ( isPointing )
			{
				// if ( ShouldOverrideHoverLock() )
				// {
				// 	//Restore the original hovering interactable on the hand
				// 	if ( originalHoverLockState == true )
				// 	{
				// 		teleportHandClass.HoverLock( originalHoveringInteractable );
						
				// 	}
				// 	else
				// 	{
						
				// 		teleportHandClass.HoverUnlock( null );

				// 	}
				// }

				//Stop looping sound
				loopingAudioSource.Stop();
				PlayAudioClip( pointerAudioSource, pointerStopSound );
			}
			teleportPointerObject.SetActive( false );

			teleportArc.Hide();

			foreach ( TeleportPoint teleportMarker in teleportMarkers )
			{
				if ( teleportMarker != null && teleportMarker.markerActive && teleportMarker.gameObject != null )
				{
					teleportMarker.gameObject.SetActive( false );
				}
			}

			destinationReticleTransform.gameObject.SetActive( false );
			invalidReticleTransform.gameObject.SetActive( false );

			SceneChaperone.Activate(false);
			
			// if (SceneChaperone.instance.playAreaPreviewTransform != null) {

			// SceneChaperone.instance.playAreaPreviewTransform.gameObject.SetActive(false);
			// }
			// Debug.Log("no preview");
			// if ( playAreaPreviewTransform != null )
			// {
			// 	playAreaPreviewTransform.gameObject.SetActive( false );
			// }

			
			isPointing = false;
			
		}


		//-------------------------------------------------
		private void ShowPointer ( )
		{
			if ( !visible )
			{
				pointedAtTeleportMarker = null;
				pointerShowStartTime = Time.time;
				visible = true;
				meshFading = true;

				teleportPointerObject.SetActive( false );
				teleportArc.Show();

				foreach ( TeleportPoint teleportMarker in teleportMarkers )
				{
					if ( teleportMarker.markerActive && teleportMarker.ShouldActivate( player.feetPositionGuess ) )
					{
						teleportMarker.gameObject.SetActive( true );
						teleportMarker.Highlight( false );
					}
				}

				startingFeetOffset = player.trackingOriginTransform.position - player.feetPositionGuess;
				movedFeetFarEnough = false;


				loopingAudioSource.clip = pointerLoopSound;
				loopingAudioSource.loop = true;
				loopingAudioSource.Play();
				loopingAudioSource.volume = 0.0f;
			}





			isPointing = true;


			if ( visible  )
			{
				PlayAudioClip( pointerAudioSource, pointerStartSound );
			}

				Hand pointerHand = teleportHandClass;

				pointerStartTransform = GetPointerStartTransform( pointerHand );

				// if ( pointerHand.currentAttachedObject != null )
				// {
				// 	allowTeleportWhileAttached = pointerHand.currentAttachedObject.GetComponent<AllowTeleportWhileAttachedToHand>();
				// }

				//Keep track of any existing hovering interactable on the hand
				// originalHoverLockState = pointerHand.hoverLocked;
				// originalHoveringInteractable = pointerHand.hoveringInteractable;

				// if ( ShouldOverrideHoverLock() )
				// {
				// 	pointerHand.HoverLock( null );
				// }

				pointerAudioSource.transform.SetParent( pointerStartTransform );
				pointerAudioSource.transform.localPosition = Vector3.zero;

				loopingAudioSource.transform.SetParent( pointerStartTransform );
				loopingAudioSource.transform.localPosition = Vector3.zero;
			
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
			foreach ( TeleportPoint teleportMarker in teleportMarkers )
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
				{
					//Pointing at an unlocked teleport marker
					teleportingToMarker = pointedAtTeleportMarker;
					InitiateTeleportFade();

					// CancelTeleportHint();
				}
			}
		}


		float teleportLerp;
		public bool fastTeleport = true;
		public float fastTeleportTime = .25f;
		Vector3 teleportStartPosition;



		void SmoothTeleport (float deltaTime) {
			if (teleporting && fastTeleport) {

				teleportLerp += deltaTime * (1.0f/fastTeleportTime);
				if (teleportLerp > 1) {
					teleportLerp = 1;
				}








				Vector3 teleportPosition = pointedAtPosition;

			TeleportPoint teleportPoint = teleportingToMarker as TeleportPoint;
			if ( teleportPoint != null )
			{
				teleportPosition = teleportPoint.transform.position;

				
			}
			Vector3 playerFeetOffset = player.trackingOriginTransform.position - player.feetPositionGuess;
			Vector3 targetPos = (teleportPosition + playerFeetOffset) + Vector3.up * trackingTransformOffset;








				Player.instance.trackingOriginTransform.position = Vector3.Lerp(
					teleportStartPosition, 
					targetPos

					,
					teleportLerp
				);



				if (teleportLerp == 1) {

					teleporting = false;
					Player.instance.GetComponent<CharacterController>().enabled = true;
					
				}
			}
		}
		


		//-------------------------------------------------
		private void InitiateTeleportFade()
		{
			teleporting = true;

			bool doFade = true;

			currentFadeTime = teleportFadeTime;

			TeleportPoint teleportPoint = teleportingToMarker as TeleportPoint;
			if ( teleportPoint != null && teleportPoint.teleportType == TeleportPoint.TeleportPointType.SwitchToNewScene )
			{
				currentFadeTime *= 3.0f;
			}
			else {

				if (fastTeleport) {
					doFade = false;
					teleportStartPosition = Player.instance.trackingOriginTransform.position;
					teleportLerp = 0;

					Player.instance.GetComponent<CharacterController>().enabled = false;
				}

			}
			if (doFade) {

				SteamVR_Fade.Start( Color.clear, 0 );
				SteamVR_Fade.Start( Color.black, currentFadeTime );
			}



			headAudioSource.transform.SetParent( player.hmdTransform );
			headAudioSource.transform.localPosition = Vector3.zero;
			PlayAudioClip( headAudioSource, teleportSound );

			if (doFade) {

				Invoke( "TeleportPlayer", currentFadeTime );
			}

		}


		//-------------------------------------------------
		private void TeleportPlayer()
		{
			teleporting = false;


			SteamVR_Fade.Start( Color.clear, currentFadeTime );

			Vector3 teleportPosition = pointedAtPosition;

			TeleportPoint teleportPoint = teleportingToMarker as TeleportPoint;
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
			
			Vector3 playerFeetOffset = player.trackingOriginTransform.position - player.feetPositionGuess;
			player.trackingOriginTransform.position = (teleportPosition + playerFeetOffset) + Vector3.up * trackingTransformOffset;
			
			
		}


		//-------------------------------------------------
		private void HighlightSelected( TeleportPoint hitTeleportMarker )
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
		// public void ShowTeleportHint()
		// {
		// 	CancelTeleportHint();

		// 	hintCoroutine = StartCoroutine( TeleportHintCoroutine() );
		// }


		//-------------------------------------------------
		// public void CancelTeleportHint()
		// {
		// 	if ( hintCoroutine != null )
        //     {
        //         ControllerButtonHints.HideTextHint(player.leftHand, teleportAction);
        //         ControllerButtonHints.HideTextHint(player.rightHand, teleportAction);

		// 		StopCoroutine( hintCoroutine );
		// 		hintCoroutine = null;
		// 	}

		// 	CancelInvoke( "ShowTeleportHint" );
		// }


		//-------------------------------------------------
		// private IEnumerator TeleportHintCoroutine()
		// {
		// 	float prevBreakTime = Time.time;
		// 	float prevHapticPulseTime = Time.time;

		// 	while ( true )
		// 	{
		// 		bool pulsed = false;

		// 		//Show the hint on each eligible hand

		// 			Hand hand = teleportHandClass;
		// 			bool showHint = true;// IsEligibleForTeleport( hand );
		// 			bool isShowingHint = !string.IsNullOrEmpty( ControllerButtonHints.GetActiveHintText( hand, teleportAction) );
		// 			if ( showHint )
		// 			{
		// 				if ( !isShowingHint )
		// 				{
		// 					ControllerButtonHints.ShowTextHint( hand, teleportAction, "Teleport" );
		// 					prevBreakTime = Time.time;
		// 					prevHapticPulseTime = Time.time;
		// 				}

		// 				if ( Time.time > prevHapticPulseTime + 0.05f )
		// 				{
		// 					//Haptic pulse for a few seconds
		// 					pulsed = true;

		// 					hand.TriggerHapticPulse( 500 );
		// 				}
		// 			}
		// 			else if ( !showHint && isShowingHint )
		// 			{
		// 				ControllerButtonHints.HideTextHint( hand, teleportAction);
		// 			}
				

		// 		if ( Time.time > prevBreakTime + 3.0f )
		// 		{
		// 			//Take a break for a few seconds
		// 			yield return new WaitForSeconds( 3.0f );

		// 			prevBreakTime = Time.time;
		// 		}

		// 		if ( pulsed )
		// 		{
		// 			prevHapticPulseTime = Time.time;
		// 		}

		// 		yield return null;
		// 	}
		// }


		//-------------------------------------------------
		// public bool IsEligibleForTeleport( Hand hand )
		// {
		// 	if ( hand == null )
		// 	{
		// 		return false;
		// 	}

		// 	if ( !hand.gameObject.activeInHierarchy )
		// 	{
		// 		return false;
		// 	}

		// 	if ( hand.hoveringInteractable != null )
		// 	{
		// 		return false;
		// 	}

		// 	if ( hand.noSteamVRFallbackCamera == null )
		// 	{
		// 		if ( hand.isActive == false)
		// 		{
		// 			return false;
		// 		}

		// 		//Something is attached to the hand
		// 		if ( hand.currentAttachedObject != null )
		// 		{
		// 			AllowTeleportWhileAttachedToHand allowTeleportWhileAttachedToHand = hand.currentAttachedObject.GetComponent<AllowTeleportWhileAttachedToHand>();

		// 			return allowTeleportWhileAttachedToHand != null && allowTeleportWhileAttachedToHand.teleportAllowed == true;
					
		// 		}
		// 	}

		// 	return true;
		// }


		//-------------------------------------------------
		// private bool ShouldOverrideHoverLock()
		// {
		// 	if ( !allowTeleportWhileAttached || allowTeleportWhileAttached.overrideHoverLock )
		// 	{
		// 		return true;
		// 	}

		// 	return false;
		// }


		//-------------------------------------------------
		private bool WasTeleportButtonReleased()// Hand hand )
		{
			Hand hand = teleportHandClass;

			// if ( IsEligibleForTeleport( hand ) )
			// {
				// if ( hand.noSteamVRFallbackCamera != null )
				// {
				// 	return Input.GetKeyUp( KeyCode.T );
				// }
				// else
                // {
                    return teleportAction.GetStateUp(hand.handType);

                    //return hand.controller.GetPressUp( SteamVR_Controller.ButtonMask.Touchpad );
                // }
			// }

			// return false;
		}

		//-------------------------------------------------
		private bool IsTeleportButtonDown( )//Hand hand )
		{
			Hand hand = teleportHandClass;
			
			// if ( IsEligibleForTeleport( hand ) )
			// {
				// if ( hand.noSteamVRFallbackCamera != null )
				// {
				// 	return Input.GetKey( KeyCode.T );
				// }
				// else
                // {
                    return teleportAction.GetState(hand.handType);
				// }
			// }

			// return false;
		}


		//-------------------------------------------------
		private bool WasTeleportButtonPressed( )//Hand hand )
		{
			Hand hand = teleportHandClass;
			
			// if ( IsEligibleForTeleport( hand ) )
			// {
				// if ( hand.noSteamVRFallbackCamera != null )
				// {
				// 	return Input.GetKeyDown( KeyCode.T );
				// }
				// else
                // {
                    return teleportAction.GetStateDown(hand.handType);

                    //return hand.controller.GetPressDown( SteamVR_Controller.ButtonMask.Touchpad );
				// }
			// }

			// return false;
		}


		//-------------------------------------------------
		private Transform GetPointerStartTransform( Hand hand )
		{
			// if ( hand.noSteamVRFallbackCamera != null )
			// {
			// 	return hand.noSteamVRFallbackCamera.transform;
			// }
			// else
			// {
				return hand.transform;
			// }
		}
	}
}
