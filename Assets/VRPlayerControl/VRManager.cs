using UnityEngine;
using Valve.VR;

using _GAME_MANAGER_TYPE_ = VRPlayerDemo.DemoGameManager;

namespace VRPlayer{
    
    /*
        add this to the same game object as any normal game manager

        acts as an interface to whatever game manager 
        for vr components

        creates visuals during pause sequence for vr

        - turns off all lights except for one above player
        - increases black fog and turns sky black

        Todo : 
            show controllers, hide on unpause

    */
    
    public class VRManager : MonoBehaviour
    {

        public static bool steamVRWorking {
            get {
                return SteamVR.initializedState == SteamVR.InitializedStates.InitializeSuccess;
            }
        }
        public static bool headsetIsOnPlayerHead{
            get {
                return StandardizedVRInput.instance != null && StandardizedVRInput.instance.headsetIsOnPlayerHead;
            }
        }
        public static Transform trackingOrigin {
            get {
                return Player.instance.trackingOriginTransform;
            }
        }
        public static float playerRealLifeHeight {
            get {
                return Player.instance.realLifeHeight;
            }
        }
        public static float gamespaceHeight {
            get {
                return Player.instance.gamespaceHeight;
            }
        }

        
        
        static _GAME_MANAGER_TYPE_ _gameManager;
        static _GAME_MANAGER_TYPE_ gameManager {
            get {
                if (_gameManager == null) {
                    _gameManager = GameObject.FindObjectOfType<_GAME_MANAGER_TYPE_>();
                }
                return _gameManager;
            }
        }

        public static bool gamePaused { get { return gameManager.isPaused; } }
        public static event System.Action<bool> onGamePaused;

    
        void OnEnable () {
            gameManager.onPauseRoutineStart += OnPauseRoutineStart;
            gameManager.onPauseRoutineEnd += OnPauseRoutineEnd;

            gameManager.onUISelect += OnUISelection;
            gameManager.onUISubmit += OnUISubmit;

            gameManager.onShowGameMessage += OnShowGameMessage;
        }






        void OnDisable () {
            gameManager.onPauseRoutineStart -= OnPauseRoutineStart;
            gameManager.onPauseRoutineEnd -= OnPauseRoutineEnd;

            gameManager.onUISelect -= OnUISelection;
            gameManager.onUISubmit -= OnUISubmit;

            gameManager.onShowGameMessage -= OnShowGameMessage;
            
        }
        public static void ToggleGamePause () {
            gameManager.TogglePause();
        }



        

        public static event System.Action<string, int> onShowGameMessage;
        void OnShowGameMessage (string message, int key) {
            
            if (onShowGameMessage != null) {
                Debug.LogError("callingbakc on show message");
                onShowGameMessage (message, key);
            }
        }
        

        public static void ShowGameMessage (string message, int key) {

        }


        public static event System.Action<GameObject[], object[]> onUISelection, onUISubmit;
        void OnUISelection (GameObject[] data, object[] customData) {
            if (onUISelection != null) {
                Debug.LogError("callign back on ui selectin");
                onUISelection (data, customData);
            }
        }
        void OnUISubmit (GameObject[] data, object[] customData) {
            if (onUISubmit != null) {
                onUISubmit (data, customData);
            }
        }
        
        

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

        public float pauseLightRange = 5;
        public float pauseLightIntensity = 1;
        public Color pauseLightColor = Color.white;



        CameraClearFlags lastClearFlags;
        Camera hmdCamera;
        Color lastClearColor;
        FogComponent pauseFog, lastFog;
        Light pauseLight;
        Light[] allLights;



        



        void Awake () {
            pauseFog = new FogComponent(true, Color.black, FogMode.Exponential, 1, 0, 0);   
        }
        void Start () {
            hmdCamera = Player.instance.hmdTransform.GetComponent<Camera>();
        }

        void OnPauseRoutineStart (bool isPaused, float routineTime) {
            
            SteamVR_Fade.Start( Color.clear, 0 );
            SteamVR_Fade.Start( Color.black, routineTime );

            if (onGamePaused != null) {
                onGamePaused(isPaused);
            }
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
            pauseLight.transform.position = Player.instance.hmdTransform.position + Vector3.up;
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



        public void ShowTextHints ( GameObject[] data )
		{
            Debug.LogError("showing texxt hints");
			StandardizedVRInput.instance.PlayDebugRoutine();
		}
		public void DisableHints ( GameObject[] data )
		{
			StandardizedVRInput.instance.StopHintRoutine();
		}
        public void SetRenderModel(GameObject[] data)
        {
            if (data.Length < 2) {
                return;
            }
            for (int handIndex = 0; handIndex < Player.instance.hands.Length; handIndex++)
            {
                Hand hand = Player.instance.hands[handIndex];
                if (hand.handType == SteamVR_Input_Sources.RightHand)
                    hand.SetRenderModel(data[0]);
                if (hand.handType == SteamVR_Input_Sources.LeftHand)
                    hand.SetRenderModel(data[1]);
            }
        }
        public void RecalibrateRealLifeHeight (GameObject[] data) {
            Player.instance.RecalibrateRealLifeHeight();
        }

        


        
        
    }
}

