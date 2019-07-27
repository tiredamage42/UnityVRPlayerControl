using UnityEngine;
using System.Collections;

using Valve.VR;



using GameBase;
using _GAME_MANAGER_TYPE_ = GameBase.GameManager;

namespace VRPlayer{

    
    /*
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
                return Player.instance.transform;//trackingOriginTransform;
            }
        }
        // public static float playerRealLifeHeight {
        //     get {
        //         return Player.instance.realLifeHeight;
        //     }
        // }
        // public static float gamespaceHeight {
        //     get {
        //         return Player.instance.gamespaceHeight;
        //     }
        // }

        public static int Hand2Int(SteamVR_Input_Sources hand) {
            if (hand == SteamVR_Input_Sources.RightHand) 
                return 0;
            else if (hand == SteamVR_Input_Sources.LeftHand) 
                return 1;
            
            Debug.LogError("no integer defined for vr source :: " + hand + ", only left and right hands are supported");
            return -1;
        }
        public static SteamVR_Input_Sources Int2Hand (int hand) {
            if (hand == 0)
                return SteamVR_Input_Sources.RightHand;
            else if (hand == 1)
                return SteamVR_Input_Sources.LeftHand;
        
            Debug.LogError("no vr source defined for integer :: " + hand + ", only 0 and 1 are supported");
            return errorVRSource;
        }
        public static SteamVR_Input_Sources OtherHand (SteamVR_Input_Sources hand) {
            if (hand == SteamVR_Input_Sources.RightHand) 
                return SteamVR_Input_Sources.LeftHand;
            else if (hand == SteamVR_Input_Sources.LeftHand) 
                return SteamVR_Input_Sources.RightHand;
            
            Debug.LogError("no other vr source for vr source :: " + hand + ", only left and right hands are supported");
            return errorVRSource;
        }

        public const SteamVR_Input_Sources errorVRSource = SteamVR_Input_Sources.Keyboard;

        
        
        static _GAME_MANAGER_TYPE_ _gameManager;
        static _GAME_MANAGER_TYPE_ gameManager {
            get {
                if (_gameManager == null) {
                    _gameManager = GameObject.FindObjectOfType<_GAME_MANAGER_TYPE_>();
                }
                return _gameManager;
            }
        }

        // public static bool gamePaused { get { return GameBase.GameManager.isPaused; } }
        public static event System.Action<bool> onGamePaused;


        public float lodBias = 10;
		

    
        void OnEnable () {
            
            GameManager.onPauseRoutineStart += OnPauseRoutineStart;
            GameManager.onPauseRoutineEnd += OnPauseRoutineEnd;

            // gameManager.onUISelect += OnUISelection;
            // gameManager.onUISubmit += OnUISubmit;

            // gameManager.onShowGameMessage += OnShowGameMessage;
        }






        void OnDisable () {
            GameManager.onPauseRoutineStart -= OnPauseRoutineStart;
            GameManager.onPauseRoutineEnd -= OnPauseRoutineEnd;

            // gameManager.onUISelect -= OnUISelection;
            // gameManager.onUISubmit -= OnUISubmit;

            // gameManager.onShowGameMessage -= OnShowGameMessage;
            
        }
        // public static void ToggleGamePause () {
        //     gameManager.TogglePause();
        // }



        

        // public static event System.Action<string, int> onShowGameMessage;
        // void OnShowGameMessage (string message, int key) {
            
        //     if (onShowGameMessage != null) {
        //         // Debug.LogError("callingbakc on show message");
        //         onShowGameMessage (message, key);
        //     }
        // }
        
// public static void ShowGameMessage (string message) {
//     ShowGameMessage(message, 0);
// }
        // public static void ShowGameMessage (string message, int key) {
        //     gameManager.ShowGameMessage(message, key);
        // }


        // public static event System.Action<GameObject[], object[]> onUISelection, onUISubmit;
        // void OnUISelection (GameObject[] data, object[] customData) {
        //     if (onUISelection != null) {
        //         // Debug.LogError("callign back on ui selectin");
        //         onUISelection (data, customData);
        //     }
        // }
        // void OnUISubmit (GameObject[] data, object[] customData) {
        //     if (onUISubmit != null) {
        //         onUISubmit (data, customData);
        //     }
        // }
        
        

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


			/*
                QualitySettings.lodBias = 3.8;

                the main camera that renders in editor has FOV at 60d, 
                while the VR device FOV is 90d. 

                LODBias1 * (tan(FOV2/2)/tan(FOV1/2)) where:
                LODBias1 is the LODBias for the first camera you are coming from (in this case main camera)
                FOV1 is the FOV for the first camera (again main camera) in radians.
                FOV2 is the FOV for the second camera (in this case Daydream) in radians.

                For main camera we have FOV1 = 60 degrees = pi/3 radians, LODBias = 2
                For the Daydream camera we have FOV2 = 90 degrees = pi/2 radians

                So: 2 * (tan(pi/2/2)/tan(pi/3/2)) = 
                2 * (tan(pi/4)/tan(pi/6)) = ~3.46
            */
            QualitySettings.lodBias = lodBias;

        }
        // void Start () {
        //     hmdCamera = Player.instance.hmdTransform.GetComponent<Camera>();
        // }
        IEnumerator Start()
		{
			// _instance = this;
			QualitySettings.vSyncCount = 0;
            hmdCamera = hmdTransform.GetComponent<Camera>();

            while (SteamVR.initializedState == SteamVR.InitializedStates.None || SteamVR.initializedState == SteamVR.InitializedStates.Initializing)
                yield return null;

			if ( SteamVR.instance == null )
				Debug.LogError("there was a problem initializing steam vr");

        
        }

        public Transform trackingOriginTransform { get { return Player.instance.transform; } }
		public Transform hmdTransform;


        public static Transform hmd_Transform {
            get {
                return instance.hmdTransform;
            }
        }
        static VRManager _instance;
		public static VRManager instance {
			get {
				if ( _instance == null )
					_instance = FindObjectOfType<VRManager>();
				return _instance;
			}
		}



        
[Tooltip("World scale around the player")]
		[Range(.1f, 10)] public float worldScale = 1.0f;

        public SteamVR_Input_Sources mainHand;
		public SteamVR_Input_Sources offHand { get { return VRManager.OtherHand(mainHand); } }
		public SteamVR_Action_Boolean pauseAction;


        
        void Update()
        {
            if (SteamVR.initializedState != SteamVR.InitializedStates.InitializeSuccess)
                return;

			if (pauseAction.GetStateDown(offHand)) {
                GameManager.TogglePause();
            }
		
			UpdateWorldScale();        
        }
        void UpdateWorldScale () {
        trackingOriginTransform.localScale = Vector3.one * (1.0f/worldScale);
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
            pauseLight.transform.position = hmdTransform.position + Vector3.up;
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

