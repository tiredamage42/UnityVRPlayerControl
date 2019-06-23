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
		
		
		// private Player player = null;
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
		
		private float loopingAudioMaxVolume = 0.0f;

		
		private Vector3 startingFeetOffset = Vector3.zero;
		private bool movedFeetFarEnough = false;

		
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
			
			pointerLineRenderer = GetComponentInChildren<LineRenderer>();
			teleportPointerObject = pointerLineRenderer.gameObject;

			int tintColorID = Shader.PropertyToID( "_TintColor" );
			fullTintAlpha = pointVisibleMaterial.GetColor( tintColorID ).a;

			teleportArc = GetComponent<TeleportArc>();
			teleportArc.traceLayerMask = traceLayerMask;

			loopingAudioMaxVolume = loopingAudioSource.volume;

			float invalidReticleStartingScale = invalidReticleTransform.localScale.x;
			invalidReticleMinScale *= invalidReticleStartingScale;
			invalidReticleMaxScale *= invalidReticleStartingScale;
		}


		//-------------------------------------------------
		void Start()
        {
            teleportMarkers = GameObject.FindObjectsOfType<TeleportPoint>();

			HidePointer();
		}

		void OnDisable()
		{
			HidePointer();
		}


		void Update()
		{
			SmoothTeleport(Time.deltaTime);

			bool teleportNewlyPressed = false;

			if ( visible )
			{

				if (teleportAction.GetStateUp(teleportHand))
					
				{
						TryTeleportPlayer();
				}
			}

			if (!VRManager.gamePaused && teleportationAllowed) {

				if (!StandardizedVRInput.ActionOccupied(teleportAction, teleportHand) && teleportAction.GetStateDown(teleportHand) )
					
				{
					teleportNewlyPressed = true;
				}
			}

		
			if ( !visible && teleportNewlyPressed )
			
			{
				//Begin showing the pointer
				ShowPointer( );
			}
			else if ( visible )
			{

				
				if ( !teleportNewlyPressed && !teleportAction.GetState(teleportHand) )
				{
					//Hide the pointer
					HidePointer();
				}
			}

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
		void ShowPlayArea (bool validLocation, Vector3 playerFeetOffset, Vector3 pointedAtPosition) {
			if (!validLocation) {
				SceneChaperone.Activate(false);
				return;
			}

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
			SceneChaperone.SetTransform(pointedAtPosition + offsetToUse, Player.instance.transform.rotation);
			SceneChaperone.Activate(true);
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
			
			Vector3 playerFeetOffset = Player.instance.trackingOriginTransform.position - Player.instance.feetPositionGuess;

			Vector3 arcVelocity = pointerDir * arcDistance;

			TeleportPoint hitTeleportMarker = null;

			//Check pointer angle
			float dotUp = Vector3.Dot( pointerDir, Vector3.up );
			float dotForward = Vector3.Dot( pointerDir, Player.instance.hmdTransform.forward );
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




			ShowPlayArea(validAreaTargeted, playerFeetOffset, pointedAtPosition);

			
			
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
			float distanceFromPlayer = Vector3.Distance( pointerEnd, Player.instance.hmdTransform.position );
			float invalidReticleCurrentScale = Util.RemapNumberClamped( distanceFromPlayer, invalidReticleMinScaleDistance, invalidReticleMaxScaleDistance, invalidReticleMinScale, invalidReticleMaxScale );
			reticle.transform.localScale = Vector3.one * invalidReticleCurrentScale;

				
		}
		private void HidePointer()
		{
			
			if ( visible )
			{
				pointerHideStartTime = Time.time;
			}

			visible = false;

			if ( isPointing )
			{
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
					if ( teleportMarker.markerActive && teleportMarker.ShouldActivate( Player.instance.feetPositionGuess ) )
					{
						teleportMarker.gameObject.SetActive( true );
						teleportMarker.Highlight( false );
					}
				}

				startingFeetOffset = Player.instance.trackingOriginTransform.position - Player.instance.feetPositionGuess;
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

				pointerStartTransform = pointerHand.transform;


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
				StandardizedVRInput.instance.TriggerHapticPulse( 
					teleportHand,
					// this,
					 (ushort)(validLocation ? 800 : 100) );
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
			Vector3 playerFeetOffset = Player.instance.trackingOriginTransform.position - Player.instance.feetPositionGuess;
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



			headAudioSource.transform.SetParent( Player.instance.hmdTransform );
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
			
			Vector3 playerFeetOffset = Player.instance.trackingOriginTransform.position - Player.instance.feetPositionGuess;
			Player.instance.trackingOriginTransform.position = (teleportPosition + playerFeetOffset) + Vector3.up * trackingTransformOffset;
			
			
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
	}
}
