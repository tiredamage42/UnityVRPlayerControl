using UnityEngine;
using Valve.VR;

namespace VRPlayer {

    //TODO: pause TOD when game pauses, or this fog gets all messed up

    public class PauseVisuals : MonoBehaviour
    {
        void OnEnable () {
            VRManager.onPauseRoutineStart += OnPauseRoutineStart;
            VRManager.onPauseRoutineEnd += OnPauseRoutineEnd;
        }

        void OnDisable () {
            VRManager.onPauseRoutineStart -= OnPauseRoutineStart;
            VRManager.onPauseRoutineEnd -= OnPauseRoutineEnd;
        }
        

        public float pauseLightRange = 5;
        public float pauseLightIntensity = 1;
        public Color pauseLightColor = Color.white;

        CameraClearFlags lastClearFlags;
        Camera hmdCamera;
        Color lastClearColor;
        FogComponent pauseFog, lastFog;
        Light pauseLight;
        Light[] allLights;

        

        struct FogComponent {

            Color fogColor;
            float startDistance, endDistance, density;
            bool useFog;
            FogMode fogMode;

            public void SetFog () {
                RenderSettings.fog = useFog;
                RenderSettings.fogColor = fogColor;
                RenderSettings.fogMode = fogMode;
                RenderSettings.fogDensity = density;
                RenderSettings.fogStartDistance = startDistance;
                RenderSettings.fogEndDistance = endDistance;
            }

            public FogComponent(bool useFog, Color32 color, FogMode fogMode, float density, float startDistance, float endDistance) {
                this.useFog = useFog;
                this.fogColor = color;
                this.fogMode = fogMode;
                this.density = density;
                this.startDistance = startDistance;
                this.endDistance = endDistance;
            }
            public FogComponent (bool e) : this (
                RenderSettings.fog, RenderSettings.fogColor, RenderSettings.fogMode, RenderSettings.fogDensity, RenderSettings.fogStartDistance, RenderSettings.fogEndDistance
            ) { }
        }
        void Awake () {
            pauseFog = new FogComponent(true, Color.black, FogMode.Exponential, 1, 0, 0); 
        }
        void Start () {

            hmdCamera = VRManager.instance.hmdTransform.GetComponent<Camera>();
        }
  
        
        void OnPauseRoutineStart (bool isPaused, float routineTime) {
            
            SteamVR_Fade.Start( Color.clear, 0 );
            SteamVR_Fade.Start( Color.black, routineTime );

        }
        void OnPauseRoutineEnd (bool isPaused, float routineTime) {
            SteamVR_Fade.Start( Color.clear, routineTime );
            if (!isPaused){
                hmdCamera.clearFlags = lastClearFlags;
                hmdCamera.backgroundColor = lastClearColor;
                
                lastFog.SetFog();
                EnableAllLights();
                DisablePauseRoomLight();
            }
            else {
                lastClearFlags = hmdCamera.clearFlags;
                lastClearColor = hmdCamera.backgroundColor;

                hmdCamera.clearFlags = CameraClearFlags.SolidColor;
                hmdCamera.backgroundColor = Color.black;

                lastFog = new FogComponent(true);
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

        void EnablePauseRoomLight () {
            BuildPauseRoomLightIfNull ();

            pauseLight.gameObject.SetActive(true);
            pauseLight.transform.position = hmdCamera.transform.position + Vector3.up;
            pauseLight.color = pauseLightColor;
            pauseLight.intensity = pauseLightIntensity;
            pauseLight.range = pauseLightRange;
        }
        void DisablePauseRoomLight () {
            pauseLight.gameObject.SetActive(false);
        }
        void BuildPauseRoomLightIfNull () {
            if (pauseLight == null) {
                GameObject lightGO = new GameObject("pauseSceneLight");
                pauseLight = lightGO.AddComponent<Light>();
                pauseLight.type = LightType.Point;
                pauseLight.shadows = LightShadows.None;

            }
        }
    }

}