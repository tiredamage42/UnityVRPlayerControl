using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

namespace VRPlayer{
    public class VRGameAddon : MonoBehaviour
    {
        [System.Serializable] public class FogComponent {

            public Color fogColor;
            public float startDistance, endDistance, density;
            public bool useFog;
            public FogMode fogMode;

            public void SetFog () {
                RenderSettings.fog = useFog;
                RenderSettings.fogColor = fogColor;
                RenderSettings.fogMode = fogMode;
                RenderSettings.fogDensity = density;
                RenderSettings.fogStartDistance = startDistance;
                RenderSettings.fogEndDistance = endDistance;
            }

            public FogComponent () {
                useFog = RenderSettings.fog;
                fogColor = RenderSettings.fogColor;
                fogMode = RenderSettings.fogMode;
                density = RenderSettings.fogDensity;
                startDistance = RenderSettings.fogStartDistance;
                endDistance = RenderSettings.fogEndDistance;
            }

        }

        public FogComponent pauseFog;

        FogComponent lastComponent;



        public Color pauseFlashColor = Color.blue;
        public float pauseFlashTime = .25f;


        void FinishPause ( ) {

            SteamVR_Fade.Start( Color.clear, pauseFlashTime * .5f );
            lastComponent = new FogComponent();
            pauseFog.SetFog();
        }


        void OnPauseGame(bool paused) {
            if (paused) {



				SteamVR_Fade.Start( Color.clear, 0 );
				SteamVR_Fade.Start( pauseFlashColor, pauseFlashTime * .5f );

				Invoke( "FinishPause", pauseFlashTime * .5f );

                

            }
            else {
                lastComponent.SetFog();
                lastComponent = null;
            }
            
        }

        bool isPaused;

        public static bool gamePaused 
        {
            get 
            {
                return instance.isPaused;
            }
        }

        static VRGameAddon _vrGame;

        public static VRGameAddon instance {
            get {
                if (_vrGame == null) {
                    _vrGame = GameObject.FindObjectOfType<VRGameAddon>();
                }
                return _vrGame;
            }
        }
        
        // Update is called once per frame
        void Update()
        {
            if (StandardizedVRInput.instance.MenuButton.GetStateDown(SteamVR_Input_Sources.LeftHand)) {
                isPaused = !isPaused;
                OnPauseGame(isPaused);
            }
        }
    }
}

