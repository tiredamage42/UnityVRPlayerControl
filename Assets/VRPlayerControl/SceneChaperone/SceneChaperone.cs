using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;
using Valve.VR.InteractionSystem;

namespace VRPlayer {
    public class SceneChaperone : MonoBehaviour
    {
        SteamVR_Events.Action chaperoneInfoInitializedAction;

        static SceneChaperone _instance;
        public static SceneChaperone instance {
            get {
                if (_instance == null) {
                    _instance = GameObject.FindObjectOfType<SceneChaperone>();
                }
                return _instance;
            }
        }
        public GameObject playAreaPreviewCorner;
		public GameObject playAreaPreviewSide;
        // private Transform playAreaPreviewTransform;
		private Transform[] playAreaPreviewCorners;
		private Transform[] playAreaPreviewSides;
        bool built;

        void Awake () {
            chaperoneInfoInitializedAction = ChaperoneInfo.InitializedAction( OnChaperoneInfoInitialized );

            playAreaPreviewCorner.SetActive( false );
			playAreaPreviewSide.SetActive( false );

        }

        void OnEnable()
		{
			chaperoneInfoInitializedAction.enabled = true;
			OnChaperoneInfoInitialized(); // In case it's already initialized
		}


		//-------------------------------------------------
		void OnDisable()
		{
			chaperoneInfoInitializedAction.enabled = false;
		}

        //Maybe adjust this to world scale
		//-------------------------------------------------
		private void OnChaperoneInfoInitialized()
		{
			ChaperoneInfo chaperone = ChaperoneInfo.instance;

			if ( chaperone.initialized && chaperone.roomscale )
			{
				//Set up the render model for the play area bounds

				if ( !built)// playAreaPreviewTransform == null )
				{
                    built = true;
					// playAreaPreviewTransform = new GameObject( "PlayAreaPreviewTransform" ).transform;
					// playAreaPreviewTransform.parent = transform;
					// Util.ResetTransform( playAreaPreviewTransform );

					playAreaPreviewCorner.SetActive( true );
					playAreaPreviewCorners = new Transform[4];

					playAreaPreviewCorners[0] = playAreaPreviewCorner.transform;
					for (int i = 1; i < 4; i++) {
						playAreaPreviewCorners[i] = Instantiate( playAreaPreviewCorners[0] );
					}
					for (int i = 0; i < 4; i++) {
						playAreaPreviewCorners[i].transform.parent = transform;// playAreaPreviewTransform;
					}
						
					playAreaPreviewSide.SetActive( true );
					playAreaPreviewSides = new Transform[4];

					playAreaPreviewSides[0] = playAreaPreviewSide.transform;
					for (int i = 1; i < 4; i++) {
						playAreaPreviewSides[i] = Instantiate( playAreaPreviewSides[0] );
					}
					for (int i = 0; i < 4; i++) {
						playAreaPreviewSides[i].transform.parent = transform;// playAreaPreviewTransform;
					}
				}

				float x = chaperone.playAreaSizeX;
				float z = chaperone.playAreaSizeZ;

				playAreaPreviewSides[0].localPosition = new Vector3( 0.0f, 0.0f, 0.5f * z - 0.25f );
				playAreaPreviewSides[1].localPosition = new Vector3( 0.0f, 0.0f, -0.5f * z + 0.25f );
				playAreaPreviewSides[2].localPosition = new Vector3( 0.5f * x - 0.25f, 0.0f, 0.0f );
				playAreaPreviewSides[3].localPosition = new Vector3( -0.5f * x + 0.25f, 0.0f, 0.0f );

				for (int i = 0; i < 4; i++) {
					playAreaPreviewSides[i].localScale = new Vector3( (i < 2 ? x : z) - 0.5f, 1.0f, 1.0f );
				}
				
				playAreaPreviewSides[0].localRotation = Quaternion.Euler( 0.0f, 0.0f, 0.0f );
				playAreaPreviewSides[1].localRotation = Quaternion.Euler( 0.0f, 180.0f, 0.0f );
				playAreaPreviewSides[2].localRotation = Quaternion.Euler( 0.0f, 90.0f, 0.0f );
				playAreaPreviewSides[3].localRotation = Quaternion.Euler( 0.0f, 270.0f, 0.0f );

				playAreaPreviewCorners[0].localPosition = new Vector3( 0.5f * x - 0.25f, 0.0f, 0.5f * z - 0.25f );
				playAreaPreviewCorners[1].localPosition = new Vector3( 0.5f * x - 0.25f, 0.0f, -0.5f * z + 0.25f );
				playAreaPreviewCorners[2].localPosition = new Vector3( -0.5f * x + 0.25f, 0.0f, -0.5f * z + 0.25f );
				playAreaPreviewCorners[3].localPosition = new Vector3( -0.5f * x + 0.25f, 0.0f, 0.5f * z - 0.25f );

				for (int i = 0; i < 4; i++) {
					playAreaPreviewCorners[i].localRotation = Quaternion.Euler( 0.0f, i * 90.0f, 0.0f );
				}
					
				// playAreaPreviewTransform.gameObject.SetActive( false );
                gameObject.SetActive( false );
			}
		}





    }
}
