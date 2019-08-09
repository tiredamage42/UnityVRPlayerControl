using UnityEngine;
using System.Collections;

using Valve.VR;



using GameBase;
using _GAME_MANAGER_TYPE_ = GameBase.GameManager;

namespace VRPlayer{
    /*


    ADD TO CONDITIONAL CHECKS:
        contains keywords
        self values, supplied values

    Add TO MODIFIERS:
        incoming change multipliers

    perks examples:


        weapon types:
            
            melee
            pistol
            rifle
            shotgun
            etc...



        game modifiers for values checked against actor:

        modify outer value (string valueType, string valueName, float baseValue) {
            
            foreach mod in valueMods[valueType][valueName]
                baseValue = mod.modify(baseValue);
        }


        for instance armor perk adds modifier:
            valueMods["Armor"]["Armor"] = new gamemodifier(multiply base by 1.5);

        rifle damage + 20% perk:
            valueMods["Rifle"]["Damage"] = new gamemodifier(multiply base by 1.2);



        damage modifier perks:

            rifle do 15 more damage:




        animal friend

            consider threat on ai check:
                if hostiles contains(actor)
                    
                    return true (so player can trigger hostile on teammates dynamically)
                    forgivness period for each hostile on same team (after a couple in game days)

                    or when hostile dead.

                    when forgiveness happens (if not dead and on same or non hostile faction, possible start relationship stats)
                    based on actors personality values, nice guy will forgive easily, stern people will require favors to better build relationship...


                else

                    is threat = check faction table for threat

                    then any:


                        is threat = considerThreatModifier(our values, actor values, is threat)
                            on already faction threat:
                                return 
                                    gamevalue: "Perk_AnimalFriend" <= 0 

                            on faction threat denied:
                                no onditionals = return already is threat








        stats page:

            value tracker panel:
                health, ap, thirst, hunger


            inventory:
                item pages and description panel

            
            perks:

                option 1: 
                    perks are game value

                    perk table is actor value template

                    foreach perk in perk table:
                        button for each level (flag ones already had... game value > i for each level)



    
    
    
    
    
    
    
    
    
    
    
    
    
    */

    
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
                return Player.instance.transform;
            }
        }
        
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
        public static event System.Action<bool, float> onPauseRoutineStart, onPauseRoutineEnd;


        public float lodBias = 10;
		

    
        
        
        
        


        



        void Awake () {
            

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
    void OnEnable () {
            GameManager.onPauseRoutineStart += OnPauseRoutineStart;
            GameManager.onPauseRoutineEnd += OnPauseRoutineEnd;
        }

        void OnDisable () {
            GameManager.onPauseRoutineStart -= OnPauseRoutineStart;
            GameManager.onPauseRoutineEnd -= OnPauseRoutineEnd;
        }


    void OnPauseRoutineStart (bool isPaused, float routineTime) {
            
           
            if (onPauseRoutineStart != null) {
                onPauseRoutineStart(isPaused, routineTime);
            }
        }
        void OnPauseRoutineEnd (bool isPaused, float routineTime) {
            if (onPauseRoutineEnd != null) {
                onPauseRoutineEnd(isPaused, routineTime);
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

