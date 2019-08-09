/*

	to do:
		disable collider on fast teleport

 */
using UnityEngine;
using Valve.VR;
using GameBase;
namespace VRPlayer
{
	public class Teleport : MonoBehaviour
    {

		public bool teleportationAllowed;


        public SteamVR_Action_Boolean teleportAction;
		public SteamVR_Input_Sources teleportHand = SteamVR_Input_Sources.LeftHand;

		Hand teleportHandClass { get { return Player.instance.GetHand(teleportHand); } }

        public LayerMask traceLayerMask;
		public Texture2D teleportPointTexture;
		
		[System.NonSerialized] Material _pointVisibleMaterial, _invalidMaterial;
		[System.NonSerialized] Material _pointLockedMaterial;
		[System.NonSerialized] Material _pointHighlightedMaterial;

		
		static int tintColorID = Shader.PropertyToID( "_TintColor" );
		static int mainTexID = Shader.PropertyToID( "_MainTex" );

		const string shaderName  = "Valve/VR/Highlight";
			
		public Material pointVisibleMaterial {
			get {
				if (_pointVisibleMaterial == null)
					_pointVisibleMaterial = MakeMaterial(visibleColor);
				return _pointVisibleMaterial;
			}
		}
		public Material pointLockedMaterial {
			get {
				if (_pointLockedMaterial == null)
					_pointLockedMaterial = MakeMaterial(lockedColor);
				return _pointLockedMaterial;
			}
		}
		public Material pointHighlightedMaterial {
			get {
				if (_pointHighlightedMaterial == null) 
					_pointHighlightedMaterial = MakeMaterial(highlightedColor);
				return _pointHighlightedMaterial;
			}
		}
		public Material invalidMaterial {
			get {
				if (_invalidMaterial == null) 
					_invalidMaterial = MakeMaterial(invalidColor);
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

		public bool showPlayAreaMarker = true;
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

		
		bool isPointing;
		
		
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

		void Awake()
        {
            _instance = this;
			
		
			teleportArc = GetComponent<TeleportArc>();
			teleportArc.traceLayerMask = traceLayerMask;
			teleportArc.material = pointHighlightedMaterial;

			destinationReticleTransform.GetComponent<MeshRenderer>().sharedMaterial = pointHighlightedMaterial;
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

			if (!GameManager.isPaused && teleportationAllowed) {

				if (!StandardizedVRInput.ActionOccupied(teleportAction, teleportHand) && teleportAction.GetStateDown(teleportHand) )
				{
					teleportNewlyPressed = true;
				}
			}

			if ( !visible && teleportNewlyPressed )
			{
				ShowPointer( );
			}
			else if ( visible )
			{
				if ( !teleportNewlyPressed && !teleportAction.GetState(teleportHand) )
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


		void ActivateReticles (bool destination, bool invalid){//, bool offset) {

			destinationReticleTransform.gameObject.SetActive( destination );
			invalidReticleTransform.gameObject.SetActive( invalid );
			
		}

		bool validAreaTargeted;

		void UpdatePointer()
		{

			validAreaTargeted = false;

			Vector3 pointerStart = teleportHandClass.transform.position;
			Vector3 pointerEnd;
			Vector3 pointerDir = teleportHandClass.transform.forward;
			bool hitSomething = false;
			
			Vector3 playerFeetOffset = VRManager.instance.trackingOriginTransform.position - Player.instance.feetPositionGuess;

			Vector3 arcVelocity = pointerDir * arcDistance;

			TeleportPoint hitTeleportMarker = null;

			//Check pointer angle
			float dotUp = Vector3.Dot( pointerDir, Vector3.up );
			float dotForward = Vector3.Dot( pointerDir, VRManager.instance.hmdTransform.forward );
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
				if ( !hitTeleportMarker.locked )
				{
					validAreaTargeted = true;
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

				ActivateReticles (validAreaTargeted, !validAreaTargeted && !pointerAtBadAngle);
				
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

			teleportArc.SetColor( validAreaTargeted ? highlightedColor : invalidColor );

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
		private void HidePointer()
		{
			

			visible = false;

			if ( isPointing )
			{
				//Stop looping sound
				loopSource.Stop();
				PlayAudioClip(oneShotSource, pointerStopSound);
				
			}

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

		private void ShowPointer ( )
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

			isPointing = true;
		}


		private void PlayAudioClip( AudioSource source, AudioClip clip )
		{
			source.clip = clip;
			source.Play();
		}

		private void PlayPointerHaptic( bool validLocation )
		{
			if (isPointing)
			{
				StandardizedVRInput.instance.TriggerHapticPulse(teleportHand, (ushort)(validLocation ? 800 : 100) );
			}
		}


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
			Vector3 targetPos = GetNewPlayAreaPosition(teleportPosition);


				VRManager.instance.trackingOriginTransform.position = Vector3.Lerp(teleportStartPosition, targetPos, teleportLerp);



				if (teleportLerp == 1) {

					teleporting = false;
					Player.instance.GetComponent<CharacterController>().enabled = true;
					
				}
			}
		}
		
		void InitiateTeleportFade()
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
					teleportStartPosition = VRManager.instance.trackingOriginTransform.position;
					teleportLerp = 0;
					Player.instance.GetComponent<CharacterController>().enabled = false;
				}
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
			
			VRManager.instance.trackingOriginTransform.position = GetNewPlayAreaPosition(teleportPosition);
		}


		Vector3 GetNewPlayAreaPosition (Vector3 teleportPosition) {
			Vector3 playerFeetOffset = VRManager.instance.trackingOriginTransform.position - Player.instance.feetPositionGuess;
			return (teleportPosition + playerFeetOffset) + Vector3.up * Player.instance.totalHeightOffset;
		}


		void HighlightSelected( TeleportPoint hitTeleportMarker )
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
					PlayAudioClip( oneShotSource, goodHighlightSound );

				}
				else if ( pointedAtTeleportMarker != null )
				{
					PlayAudioClip( oneShotSource, badHighlightSound );
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
