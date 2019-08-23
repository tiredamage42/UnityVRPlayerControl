/*

	to do:
		disable collider on fast teleport

 */
using UnityEngine;
using Valve.VR;
using GameBase;
using InteractionSystem;
using Game;
namespace VRPlayer
{
	public class Teleport : MonoBehaviour
    {

		public bool teleportationAllowed;

		public int teleportAction = 1;
        // public SteamVR_Action_Boolean teleportAction;
		public SteamVR_Input_Sources teleportHand = SteamVR_Input_Sources.LeftHand;

		Hand teleportHandClass { get { return Player.instance.GetHand(teleportHand); } }

        public LayerMask traceLayerMask;
		public Texture2D teleportPointTexture;
		
		static int tintColorID = Shader.PropertyToID( "_TintColor" );
		static int mainTexID = Shader.PropertyToID( "_MainTex" );
		const string shaderName  = "Valve/VR/Highlight";
		[System.NonSerialized] Material _pointVisibleMaterial, _invalidMaterial, _pointLockedMaterial, _pointHighlightedMaterial;
			
		public Material pointVisibleMaterial {
			get {
				if (_pointVisibleMaterial == null) _pointVisibleMaterial = MakeMaterial(visibleColor);
				return _pointVisibleMaterial;
			}
		}
		public Material pointLockedMaterial {
			get {
				if (_pointLockedMaterial == null) _pointLockedMaterial = MakeMaterial(lockedColor);
				return _pointLockedMaterial;
			}
		}
		public Material pointHighlightedMaterial {
			get {
				if (_pointHighlightedMaterial == null) _pointHighlightedMaterial = MakeMaterial(highlightedColor);
				return _pointHighlightedMaterial;
			}
		}
		public Material invalidMaterial {
			get {
				if (_invalidMaterial == null) _invalidMaterial = MakeMaterial(invalidColor);
				return _invalidMaterial;
			}
		}

		// invalid (255, 34, 57, 128)
		// highlighted (115, 234, 116, 128)
		// locked (201, 141, 35, 128)
		// visible (35, 166, 201, 128)


		public Color32 invalidColor = new Color32(255, 34, 57, 128);
		public Color32 highlightedColor = new Color32(115, 234, 116, 128);
		public Color32 lockedColor = new Color32(201, 141, 35, 128);
		public Color32 visibleColor = new Color32(35, 166, 201, 128);

		Material MakeMaterial (Color32 color) {

			Material m = new Material(Shader.Find(shaderName));
			m.hideFlags = HideFlags.HideAndDontSave;
			m.SetColor(tintColorID, color);
			m.SetTexture(mainTexID, teleportPointTexture);
			return m;
		}
		
		public Transform destinationReticleTransform;
		public Transform invalidReticleTransform;

		public float teleportFadeTime = 0.1f;
		public float arcDistance = 10.0f;


		AudioSource loopSource, oneShotSource;
		
		[Header( "Sounds" )]
		public AudioClip teleportSound;
		public AudioClip pointerStartSound;
		public AudioClip pointerLoopSound;
		public AudioClip pointerStopSound;
		public AudioClip goodHighlightSound;
		public AudioClip badHighlightSound;

		
		
		
		TeleportArc teleportArc = null;
		bool visible = false;
		TeleportPoint[] teleportMarkers;
		TeleportPoint pointedAtTeleportMarker;
		TeleportPoint teleportingToMarker;
		Vector3 pointedAtPosition;
		Vector3 prevPointedAtPosition;
		bool teleporting = false;
		float currentFadeTime = 0.0f;
		float invalidReticleMinScale = 0.2f;
		float invalidReticleMaxScale = 1.0f;
		float invalidReticleMinScaleDistance = 0.4f;
		float invalidReticleMaxScaleDistance = 2.0f;
		Vector3 startingFeetOffset = Vector3.zero;
		bool movedFeetFarEnough = false;

		public float maxGroundAngle = 45;

		static Teleport _instance;
		public static Teleport instance {
			get {
				if ( _instance == null ) _instance = GameObject.FindObjectOfType<Teleport>();
				return _instance;
			}
		}

		CharacterController playerCC;
		void Awake()
        {
            _instance = this;
			teleportArc = GetComponent<TeleportArc>();
			teleportArc.traceLayerMask = traceLayerMask;
			teleportArc.material = pointHighlightedMaterial;

			destinationReticleTransform.GetComponentInChildren<MeshRenderer>().sharedMaterial = pointHighlightedMaterial;
			invalidReticleTransform.GetComponentInChildren<MeshRenderer>().sharedMaterial = invalidMaterial;
			
			float invalidReticleStartingScale = invalidReticleTransform.localScale.x;
			invalidReticleMinScale *= invalidReticleStartingScale;
			invalidReticleMaxScale *= invalidReticleStartingScale;

			loopSource = gameObject.AddComponent<AudioSource>();
			oneShotSource = gameObject.AddComponent<AudioSource>();
			
			loopSource.loop = true;
			oneShotSource.loop = false;

			loopSource.spatialBlend = 0;
			oneShotSource.spatialBlend = 0;

			loopSource.volume = 1;
			oneShotSource.volume = 1;
		}

		void Start()
        {
			playerCC = Player.instance.GetComponent<CharacterController>();
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

			if (!teleporting && teleportationAllowed) {
				if ( visible && validAreaTargeted)
				{
					// if (teleportAction.GetStateUp(teleportHand))
					if (ControlsManager.GetActionEnd(teleportAction, VRManager.Hand2Int(teleportHand) ))
					{
						//Pointing at an unlocked teleport marker
						teleportingToMarker = pointedAtTeleportMarker;
						InitiateTeleportFade();
					}
				}
				if (!GameManager.isPaused) {

					// if ((!StandardizedVRInput.ActionOccupied(teleportAction, teleportHand) || Player.instance.GetComponent<Interactor>().interactionMode == 1) && teleportAction.GetStateDown(teleportHand) )
					if ((!ControlsManager.ActionOccupied(teleportAction, VRManager.Hand2Int(teleportHand)) || Player.instance.GetComponent<Interactor>().interactionMode == 1) && ControlsManager.GetActionStart(teleportAction, VRManager.Hand2Int(teleportHand) ) )

					{
						teleportNewlyPressed = true;
					}
				}
			}

			if ( !visible && teleportNewlyPressed )
			{
				ShowPointer( );
			}
			else if ( visible )
			{

				// if ( !teleportNewlyPressed && !teleportAction.GetState(teleportHand) )
				if ( !teleportNewlyPressed && !ControlsManager.GetActionUpdate(teleportAction, VRManager.Hand2Int(teleportHand) ))
				{
					HidePointer();
				}
			}

			if ( visible )
			{
				UpdatePointer();
			}	
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

		void ActivateReticles (bool destination, bool invalid) {
			destinationReticleTransform.gameObject.SetActive( destination );
			invalidReticleTransform.gameObject.SetActive( invalid );
		}

		bool validAreaTargeted;

		void UpdatePointer()
		{
			Vector3 pointerStart = teleportHandClass.transform.position;
			Vector3 pointerDir = teleportHandClass.transform.forward;
			validAreaTargeted = false;

			Vector3 pointerEnd;
			bool hitSomething = false;
			
			Vector3 arcVelocity = pointerDir * arcDistance;

			TeleportPoint hitTeleportMarker = null;

			//Check pointer angle
			float dotUp = Vector3.Dot( pointerDir, Vector3.up );
			float dotForward = Vector3.Dot( pointerDir, VRManager.instance.hmdTransform.forward );
			bool pointerAtBadAngle = ( ( dotForward > 0 && dotUp > 0.75f ) || ( dotForward < 0.0f && dotUp > 0.5f ) );
			
			//Trace to see if the pointer hit anything
			RaycastHit hitInfo = new RaycastHit();

			teleportArc.SetArcData( pointerStart, arcVelocity, true, pointerAtBadAngle );
			
			if (!pointerAtBadAngle) {
				if ( teleportArc.DrawArc( out hitInfo ) )
				{
					hitSomething = true;
					hitTeleportMarker = hitInfo.collider.GetComponentInParent<TeleportPoint>();
				}
			}

			HighlightSelected( hitTeleportMarker );

			if ( hitTeleportMarker != null ) //Hit a teleport marker
			{
				if ( !hitTeleportMarker.locked ) validAreaTargeted = true;
				
				ActivateReticles (false, false);
				
				pointedAtTeleportMarker = hitTeleportMarker;
				pointedAtPosition = hitInfo.point;

				pointerEnd = hitInfo.point;
			}
			else 
			{
				Vector3 normalToUse = Vector3.up;
				float angle = 0;

				if (hitSomething) {
					normalToUse = hitInfo.normal;
					angle = Vector3.Angle( normalToUse, Vector3.up );
				}
				
				validAreaTargeted = !pointerAtBadAngle && hitSomething && (angle <= maxGroundAngle); // not locked area

				ActivateReticles (validAreaTargeted, !validAreaTargeted && !pointerAtBadAngle);
				
				pointedAtTeleportMarker = null;

				pointerEnd = hitSomething ? hitInfo.point : teleportArc.GetArcPositionAtTime( teleportArc.arcDuration );
				
				if (invalidReticleTransform.gameObject.activeSelf) ResizeReticleBasedOnDistance (invalidReticleTransform, pointerEnd, normalToUse);
				if (destinationReticleTransform.gameObject.activeSelf) ResizeReticleBasedOnDistance (destinationReticleTransform, pointerEnd, normalToUse);
				
				pointedAtPosition = pointerEnd;
			}

			teleportArc.SetColor( validAreaTargeted ? highlightedColor : invalidColor );


			Vector3 playerFeetOffset = VRManager.instance.trackingOriginTransform.position - Player.instance.feetPositionGuess;
			ShowPlayArea(validAreaTargeted, playerFeetOffset, pointedAtPosition);
			
			destinationReticleTransform.position = pointedAtPosition;
			invalidReticleTransform.position = pointerEnd;
			
			
		}
		void ResizeReticleBasedOnDistance (Transform reticle, Vector3 pointerEnd, Vector3 hitNormal) {
			//Orient the reticle to the normal of the trace hit point
			Quaternion targetRotation = Quaternion.FromToRotation( Vector3.up, hitNormal );
			reticle.rotation = Quaternion.Slerp( reticle.rotation, targetRotation, 0.1f );

			//Scale the invalid reticle based on the distance from the player
			float distanceFromPlayer = Vector3.Distance( pointerEnd, VRManager.instance.hmdTransform.position );
			float invalidReticleCurrentScale = Mathf.Lerp(invalidReticleMinScale, invalidReticleMaxScale, Mathf.InverseLerp(invalidReticleMinScaleDistance, invalidReticleMaxScaleDistance, distanceFromPlayer)); 
			reticle.transform.localScale = Vector3.one * invalidReticleCurrentScale;
		}

				
		void HidePointer()
		{
			visible = false;

			loopSource.Stop();
			PlayAudioClip(oneShotSource, pointerStopSound);
			
			teleportArc.Hide();

			ActivateTeleportMarkers(false);
			ActivateReticles (false, false);

			SceneChaperone.Activate(false);
		}

		void ActivateTeleportMarkers (bool activated) {
			foreach ( TeleportPoint teleportMarker in teleportMarkers ) {
				// if ( teleportMarker != null && teleportMarker.markerActive && teleportMarker.gameObject != null )
				teleportMarker.gameObject.SetActive( activated );
			}
		}
			

		void ShowPointer ( )
		{
			pointedAtTeleportMarker = null;
			visible = true;
			
			teleportArc.Show();

			foreach ( TeleportPoint teleportMarker in teleportMarkers )
			{
				if ( teleportMarker.markerActive && teleportMarker.ShouldActivate( Player.instance.feetPositionGuess ) )
				{
					teleportMarker.gameObject.SetActive( true );
					teleportMarker.Highlight( false );
				}
			}

			startingFeetOffset = VRManager.instance.trackingOriginTransform.position - Player.instance.feetPositionGuess;
			movedFeetFarEnough = false;

			loopSource.clip = pointerLoopSound;
			loopSource.Play();
			
			PlayAudioClip( oneShotSource, pointerStartSound );
		}



		private void PlayAudioClip( AudioSource source, AudioClip clip )
		{
			source.clip = clip;
			source.Play();
		}

		private void PlayPointerHaptic( bool validLocation )
		{
			Player.instance.TriggerHapticPulse(teleportHand, (ushort)(validLocation ? 800 : 100) );
		}

		float teleportLerp;
		public bool fastTeleport = true;
		public float fastTeleportTime = .25f;
		Vector3 teleportStartPosition;

		void SmoothTeleport (float deltaTime) {
			if (teleporting && fastTeleport) {

				teleportLerp += deltaTime * (1.0f/fastTeleportTime);
				
				if (teleportLerp > 1) teleportLerp = 1;
				

				Vector3 teleportPosition = pointedAtPosition;
				if ( teleportingToMarker != null ) teleportPosition = teleportingToMarker.transform.position;

				Vector3 targetPos = GetNewPlayAreaPosition(teleportPosition);

				VRManager.instance.trackingOriginTransform.position = Vector3.Lerp(teleportStartPosition, targetPos, teleportLerp);

				if (teleportLerp == 1) {
					teleporting = false;
					if (playerCC != null) playerCC.enabled = true;
				}
			}
		}
		
		void InitiateTeleportFade()
		{
			teleporting = true;

			bool doFade = true;

			currentFadeTime = teleportFadeTime;

				if (fastTeleport) {
					doFade = false;
					teleportStartPosition = VRManager.instance.trackingOriginTransform.position;
					teleportLerp = 0;
					if (playerCC != null) playerCC.enabled = false;
				}
			
			if (doFade) {
				SteamVR_Fade.Start( Color.clear, 0 );
				SteamVR_Fade.Start( Color.black, currentFadeTime );
			}

			PlayAudioClip( oneShotSource, teleportSound );

			if (doFade) {

				Invoke( "TeleportPlayer", currentFadeTime );
			}

		}

		void TeleportPlayer()
		{
			teleporting = false;

			SteamVR_Fade.Start( Color.clear, currentFadeTime );

			Vector3 teleportPosition = pointedAtPosition;

			if ( teleportingToMarker != null )
			{
				teleportPosition = teleportingToMarker.transform.position;
			}			
			VRManager.instance.trackingOriginTransform.position = GetNewPlayAreaPosition(teleportPosition);
		}

		Vector3 GetNewPlayAreaPosition (Vector3 teleportPosition) {
			Vector3 playerFeetOffset = VRManager.instance.trackingOriginTransform.position - Player.instance.feetPositionGuess;
			return (teleportPosition + playerFeetOffset) + Vector3.up * Player.instance.totalHeightOffset;
		}


		void HighlightSelected( TeleportPoint marker )
		{
			if ( pointedAtTeleportMarker != marker ) //Pointing at a new teleport marker
			{
				if ( pointedAtTeleportMarker != null )
				{
					pointedAtTeleportMarker.Highlight( false );
				}

				if ( marker != null )
				{
					marker.Highlight( true );

					prevPointedAtPosition = pointedAtPosition;
					PlayPointerHaptic( !marker.locked );
					PlayAudioClip( oneShotSource, goodHighlightSound );

				}
				else if ( pointedAtTeleportMarker != null )
				{
					PlayAudioClip( oneShotSource, badHighlightSound );
				}
			}
			else if ( marker != null ) //Pointing at the same teleport marker
			{
				if ( Vector3.Distance( prevPointedAtPosition, pointedAtPosition ) > 1.0f )
				{
					prevPointedAtPosition = pointedAtPosition;
					PlayPointerHaptic( !marker.locked );
				}
			}
		}		
	}
}
