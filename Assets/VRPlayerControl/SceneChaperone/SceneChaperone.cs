using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;
using Valve.VR.InteractionSystem;

namespace VRPlayer {
    public class SceneChaperone : MonoBehaviour
    {
		public static void SetTransform (Vector3 position, Quaternion rotation) {
			instanc.transform.position = position;
			instanc.transform.rotation = rotation;
		}
		public static void Activate (bool activated) {
			instanc.gameObject.SetActive(activated);
		}

        // public bool initialized { get; private set; }
		// public float playAreaSizeX { get; private set; }
		// public float playAreaSizeZ { get; private set; }
		// public bool roomscale { get; private set; }

		
		//-------------------------------------------------
		// IEnumerator Start()
		// {
		// 	// Uncomment for roomscale testing
		// 	//_instance.initialized = true;
		// 	//_instance.playAreaSizeX = UnityEngine.Random.Range( 1.0f, 4.0f );
		// 	//_instance.playAreaSizeZ = UnityEngine.Random.Range( 1.0f, _instance.playAreaSizeX );
		// 	//_instance.roomscale = true;
		// 	//ChaperoneInfo.Initialized.Send();
		// 	//yield break;

		// 	// Get interface pointer
			
		// 	var chaperone = OpenVR.Chaperone;
		// 	if ( chaperone == null )
		// 	{
		// 		Debug.LogWarning("<b>[SteamVR Interaction]</b> Failed to get IVRChaperone interface.");
		// 		initialized = true;
		// 		yield break;
		// 	}

		// 	// Get play area size
		// 	while ( true )
		// 	{
		// 		float px = 0.0f, pz = 0.0f;
		// 		if ( chaperone.GetPlayAreaSize( ref px, ref pz ) )
		// 		{
		// 			initialized = true;
		// 			playAreaSizeX = px;
		// 			playAreaSizeZ = pz;
		// 			roomscale = Mathf.Max( px, pz ) > 1.01f;

		// 			Debug.LogFormat("<b>[SteamVR Interaction]</b> ChaperoneInfo initialized. {2} play area {0:0.00}m x {1:0.00}m", px, pz, roomscale ? "Roomscale" : "Standing" );

        //             OnChaperoneInfoInitialized();

		// 			yield break;
		// 		}

		// 		yield return null;
		// 	}
		// }

        static SceneChaperone _instance;
        public static SceneChaperone instanc {
            get {
                if (_instance == null) {
                    _instance = GameObject.FindObjectOfType<SceneChaperone>();
                }
                return _instance;
            }
        }
        public GameObject playAreaPreviewCorner;
		public GameObject playAreaPreviewSide;
        // [HideInInspector] public Transform playAreaPreviewTransform;
		// private Transform[] playAreaPreviewCorners;
		// private Transform[] playAreaPreviewSides;
        
        void Awake () {
            
            // initialized = false;
            // playAreaSizeX = 1.0f;
            // playAreaSizeZ = 1.0f;
            // roomscale = false;

            DontDestroyOnLoad( gameObject );


            playAreaPreviewCorner.SetActive( false );
			playAreaPreviewSide.SetActive( false );

        }

        void OnEnable()
		{
			OnChaperoneInfoInitialized(); // In case it's already initialized
		}

        //Maybe adjust this to world scale
		private void OnChaperoneInfoInitialized()
		{
		// 	if ( initialized && roomscale )			
        //     {
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
		// 				playAreaPreviewCorners[i].transform.parent =  playAreaPreviewTransform;
		// 			}
						
		// 			playAreaPreviewSide.SetActive( true );
		// 			playAreaPreviewSides = new Transform[4];

		// 			playAreaPreviewSides[0] = playAreaPreviewSide.transform;
		// 			for (int i = 1; i < 4; i++) {
		// 				playAreaPreviewSides[i] = Instantiate( playAreaPreviewSides[0] );
		// 			}
		// 			for (int i = 0; i < 4; i++) {
		// 				playAreaPreviewSides[i].transform.parent =  playAreaPreviewTransform;
		// 			}
		// 		}

        //         float x = playAreaSizeX;
		// 		float z = playAreaSizeZ;

		// 		playAreaPreviewSides[0].localPosition = new Vector3( 0.0f, 0.0f, 0.5f * z - 0.25f );
		// 		playAreaPreviewSides[1].localPosition = new Vector3( 0.0f, 0.0f, -0.5f * z + 0.25f );
		// 		playAreaPreviewSides[2].localPosition = new Vector3( 0.5f * x - 0.25f, 0.0f, 0.0f );
		// 		playAreaPreviewSides[3].localPosition = new Vector3( -0.5f * x + 0.25f, 0.0f, 0.0f );

		// 		for (int i = 0; i < 4; i++) {
		// 			playAreaPreviewSides[i].localScale = new Vector3( (i < 2 ? x : z) - 0.5f, 1.0f, 1.0f );
		// 		}
				
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
					
		// 		playAreaPreviewTransform.gameObject.SetActive( false );
		// 	}
		}
    }
}
