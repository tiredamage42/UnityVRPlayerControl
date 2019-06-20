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
            // public bool useFog;
            public FogMode fogMode;

            public void SetFog () {
                // RenderSettings.fog = useFog;
                RenderSettings.fogColor = fogColor;
                RenderSettings.fogMode = fogMode;
                RenderSettings.fogDensity = density;
                RenderSettings.fogStartDistance = startDistance;
                RenderSettings.fogEndDistance = endDistance;
            }

            public FogComponent () {
                // useFog = RenderSettings.fog;
                fogColor = RenderSettings.fogColor;
                fogMode = RenderSettings.fogMode;
                density = RenderSettings.fogDensity;
                startDistance = RenderSettings.fogStartDistance;
                endDistance = RenderSettings.fogEndDistance;
            }
        }


        bool wasUsingFog;
        CameraClearFlags lastClearFlags;
        Camera hmdCamera;
        public Color clearColor;
        Color lastClearColor;

        public FogComponent pauseFog;

        FogComponent lastComponent;



        public Color pauseFlashColor = Color.blue;
        // public float pauseFlashTime = .25f;


        // DemoGameManager gameManager;

        void Awake () {
            // gameManager = GetComponent<DemoGameManager>();

        }
        void OnEnable () {

            DemoGameManager.onPauseRoutineStart += OnPauseRoutineStart;
            DemoGameManager.onPauseRoutineEnd += OnPauseRoutineEnd;
        }
        void OnDisable () {
            DemoGameManager.onPauseRoutineStart -= OnPauseRoutineStart;
            DemoGameManager.onPauseRoutineEnd -= OnPauseRoutineEnd;
        }


        void OnPauseRoutineStart (bool isPaused, float routineTime) {
            SteamVR_Fade.Start( Color.clear, 0 );
            SteamVR_Fade.Start( pauseFlashColor, routineTime );
        }
        void OnPauseRoutineEnd (bool isPaused, float routineTime) {
            hmdCamera = Player.instance.hmdTransform.GetComponent<Camera>();
            SteamVR_Fade.Start( Color.clear, routineTime );
            if (!isPaused){
                RenderSettings.fog = wasUsingFog;
                hmdCamera.clearFlags = lastClearFlags;
                hmdCamera.backgroundColor = lastClearColor;
                
                lastComponent.SetFog();
                lastComponent = null;
                EnableAllLights();
                DisablePauseRoomLight();
            }
            else {
                wasUsingFog = RenderSettings.fog;
                lastClearFlags = hmdCamera.clearFlags;
                lastClearColor = hmdCamera.backgroundColor;

                RenderSettings.fog = true;

                hmdCamera.clearFlags = CameraClearFlags.SolidColor;
                hmdCamera.backgroundColor = clearColor;

            
                lastComponent = new FogComponent();
                pauseFog.SetFog();
                DisableAllLights();
                EnablePauseRoomLight();
            }
        }


        void DisableAllLights () {
            allLights = GameObject.FindObjectsOfType<Light>();
            for (int i = 0; i < allLights.Length;i++) {
                allLights[i].enabled = false;
            }

        }
        void EnableAllLights () {
            for (int i = 0; i < allLights.Length;i++) {
                allLights[i].enabled = true;
            }
        }

        public float pauseLightRange = 5;
        public float pauseLightIntensity = 1;
        public Color pauseLightColor = Color.white;

        void EnablePauseRoomLight () {
            BuildPauseRoomLightIfNull ();

            pauseLight.gameObject.SetActive(true);
            pauseLight.transform.position = Player.instance.hmdTransform.position + Vector3.up;
            pauseLight.color = pauseLightColor;
            pauseLight.intensity = pauseLightIntensity;
            pauseLight.range = pauseLightRange;
        }
        void DisablePauseRoomLight () {
            pauseLight.gameObject.SetActive(false);
        }
        Light pauseLight;
        Light[] allLights;

        void BuildPauseRoomLightIfNull () {
            if (pauseLight == null) {
                GameObject lightGO = new GameObject("pauseSceneLight");
                pauseLight = lightGO.AddComponent<Light>();
                pauseLight.type = LightType.Point;
                pauseLight.shadows = LightShadows.None;

            }
        }

        

        // void FinishPause ( ) {

        //     SteamVR_Fade.Start( Color.clear, pauseFlashTime * .5f );
        //     lastComponent = new FogComponent();
        //     pauseFog.SetFog();
        // }


        // void OnPauseGame(bool paused) {
        //     if (paused) {



				
		// 		Invoke( "FinishPause", pauseFlashTime * .5f );

                

        //     }
        //     else {
        //         lastComponent.SetFog();
        //         lastComponent = null;
        //     }
            
        // }

        // bool isPaused;

        public static bool gamePaused 
        {
            get 
            {
                return DemoGameManager.isPaused;
            }
        }

        static VRGameAddon _vrGame;

        // public static VRGameAddon instance {
        //     get {
        //         if (_vrGame == null) {
        //             _vrGame = GameObject.FindObjectOfType<VRGameAddon>();
        //         }
        //         return _vrGame;
        //     }
        // }
        
        // Update is called once per frame
        void Update()
        {
            if (StandardizedVRInput.instance.MenuButton.GetStateDown(SteamVR_Input_Sources.LeftHand)) {
                DemoGameManager.TogglePause();
            }
        }
    }
}

