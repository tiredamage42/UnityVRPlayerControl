using UnityEngine;
using System.Collections;

using Valve.VR;
using Valve.VR.InteractionSystem;

using InteractionSystem;
using Game.InventorySystem;

namespace VRPlayer
{
    public class Hand : MonoBehaviour
    {
        [HideInInspector] public VelocityEstimator velocityEstimator;


        void OnGamePaused (bool isPaused, float routineTime) {
            if (isPaused) {
                ShowController(true);
            }
            else {
                HideController(true);
            }
        }


            

        public SteamVR_Input_Sources handType { get { return trackedObject.inputSource; } }
        SteamVR_Behaviour_Pose _trackedObject;

        SteamVR_Behaviour_Pose trackedObject {
            get {
                if (_trackedObject == null) _trackedObject = GetComponent<SteamVR_Behaviour_Pose>();
                return _trackedObject;
            }
        }
        public GameObject renderModelPrefab;
        protected RenderModel mainRenderModel;
        
        
        private TextMesh debugText;
        public bool isActive { get { return trackedObject.isActive; } }
        public bool isPoseValid { get { return trackedObject.isValid; } }

        int myEquipIndex { get { return VRManager.Hand2Int(handType); } }
        int otherHandEquipIndex { get { return 1 - myEquipIndex; } } 

        public SteamVR_Behaviour_Skeleton skeleton { get { return mainRenderModel != null ? mainRenderModel.GetSkeleton() : null; } }

        public void ShowController(bool permanent = false) {
            if (mainRenderModel != null) mainRenderModel.SetControllerVisibility(true, permanent);
        }
        public void HideController(bool permanent = false) {
            if (mainRenderModel != null) mainRenderModel.SetControllerVisibility(false, permanent);
        }

        public void ShowSkeleton(bool permanent = false) {
            if (mainRenderModel != null) mainRenderModel.SetHandVisibility(true, permanent);
        }

        public void HideSkeleton(bool permanent = false) {
            if (mainRenderModel != null) mainRenderModel.SetHandVisibility(false, permanent);
        }

        public bool HasSkeleton() {
            return mainRenderModel != null && mainRenderModel.GetSkeleton() != null;
        }

        public void Show() {
            SetVisibility(true);
        }

        public void Hide() {
            SetVisibility(false);
        }

        public void SetVisibility(bool visible) {
            if (mainRenderModel != null) mainRenderModel.SetVisibility(visible);
        }

        public void SetSkeletonRangeOfMotion(EVRSkeletalMotionRange newRangeOfMotion, float blendOverSeconds = 0.1f) {
            mainRenderModel.SetSkeletonRangeOfMotion(newRangeOfMotion, blendOverSeconds);
        }

        public void SetTemporarySkeletonRangeOfMotion(SkeletalMotionRangeChange temporaryRangeOfMotionChange, float blendOverSeconds = 0.1f) {
            mainRenderModel.SetTemporarySkeletonRangeOfMotion(temporaryRangeOfMotionChange, blendOverSeconds);
        }

        public void ResetTemporarySkeletonRangeOfMotion(float blendOverSeconds = 0.1f) {
            mainRenderModel.ResetTemporarySkeletonRangeOfMotion(blendOverSeconds);
        }

        // public void SetAnimationState(int stateValue) {
        //     mainRenderModel.SetAnimationState(stateValue);
        // }
        // public void StopAnimation() {
        //     mainRenderModel.StopAnimation();
        // }

        protected float blendToPoseTime = 0.1f;
        protected float releasePoseBlendTime = 0.2f;



        void OnItemEquipped (Inventory inventory, SceneItem item, int equipSlot, bool quickEquipped){
            if (equipSlot != myEquipIndex) {
                return;
            }

            if (quickEquipped) {

                if (item.itemBehavior.equipType != InventoryEquipper.EquipType.Static && item.rigidbody != null) {
                    velocityEstimator.BeginEstimatingVelocity();
                }
            }
                        
            VRItemAddon vr_item = item.GetComponent<VRItemAddon>();
            if (!vr_item) {
                Debug.LogError(item.name + " :: does not have a vr item component");
                return;
            }
            
            if (vr_item.hideHandOnAttach) {
                Hide();
            }

            if (vr_item.setRangeOfMotionOnPickup != SkeletalMotionRangeChange.None) {
                SetTemporarySkeletonRangeOfMotion(vr_item.setRangeOfMotionOnPickup);
            }

            if (vr_item.usePose) {
                skeleton.BlendToPoser(vr_item.poseToUse, 1, blendToPoseTime);
            }

            if (vr_item.activateActionSetOnAttach != null)
                vr_item.activateActionSetOnAttach.Activate(handType);
        }
        
        void OnItemUnequipped(Inventory inventory, SceneItem item, int slotIndex, bool quickEquipped){
            if (slotIndex != myEquipIndex) {
                return;
            }

            if (quickEquipped) {

                if (item.itemBehavior.equipType != InventoryEquipper.EquipType.Static) {
                    Rigidbody rigidbody = item.rigidbody;
                    if (rigidbody != null) {

                        Vector3 velocity;
                        Vector3 angularVelocity;

                        GetReleaseVelocities(rigidbody, out velocity, out angularVelocity);

                        rigidbody.velocity = velocity;
                        rigidbody.angularVelocity = angularVelocity;
                    }
                }
            }

            VRItemAddon vr_item = item.GetComponent<VRItemAddon>();
            if (!vr_item) {
                Debug.LogError(item.name + " :: does not have a vr item component");
                return;
            }
            
            if (vr_item.hideHandOnAttach) {
                Show();
            }

            if (vr_item.setRangeOfMotionOnPickup != SkeletalMotionRangeChange.None) {
                ResetTemporarySkeletonRangeOfMotion();
            }
              
            if (mainRenderModel != null) {
                mainRenderModel.ReturnHandToOrigin();
                // mainRenderModel.MatchHandToTransform(mainRenderModel.transform);
            }

            //move to vr interacable
            if (vr_item.usePose) 
            {
                if (skeleton != null) {
                    skeleton.BlendToSkeleton(releasePoseBlendTime);
                }
            }
            
            if (vr_item.activateActionSetOnAttach != null)
            {
                if (equipper.equippedSlots[otherHandEquipIndex] == null || equipper.equippedSlots[otherHandEquipIndex].sceneItem.GetComponent<VRItemAddon>().activateActionSetOnAttach != vr_item.activateActionSetOnAttach)
                {
                    vr_item.activateActionSetOnAttach.Deactivate(handType);
                }
            }

            
        }

        public virtual void GetReleaseVelocities(Rigidbody rigidbody, out Vector3 velocity, out Vector3 angularVelocity)
        {
            Player player = Player.instance;
            switch (player.releaseVelocityStyle)
            {
                case ReleaseStyle.ShortEstimation:
                    velocityEstimator.FinishEstimatingVelocity();
                    velocity = velocityEstimator.GetVelocityEstimate();
                    angularVelocity = velocityEstimator.GetAngularVelocityEstimate();
                    break;
                case ReleaseStyle.AdvancedEstimation:
                    GetEstimatedPeakVelocities(out velocity, out angularVelocity);
                    break;
                case ReleaseStyle.GetFromHand:
                    velocity = GetTrackedObjectVelocity(player.releaseVelocityTimeOffset);
                    angularVelocity = GetTrackedObjectAngularVelocity(player.releaseVelocityTimeOffset);
                    break;
                default:
                case ReleaseStyle.NoChange:
                    velocity = rigidbody.velocity;
                    angularVelocity = rigidbody.angularVelocity;
                    break;
            }

            if (player.releaseVelocityStyle != ReleaseStyle.NoChange)
                velocity *= player.scaleReleaseVelocity;
        }



        //-------------------------------------------------
        // Get the world velocity of the VR Hand.
        //-------------------------------------------------
        public Vector3 GetTrackedObjectVelocity(float timeOffset = 0)
        {
            if (timeOffset == 0)
                return VRManager.trackingOrigin.TransformVector(trackedObject.GetVelocity());
         
            Vector3 velocity;
            Vector3 angularVelocity;

            trackedObject.GetVelocitiesAtTimeOffset(timeOffset, out velocity, out angularVelocity);
            return VRManager.trackingOrigin.TransformVector(velocity);
        }
        
        //-------------------------------------------------
        // Get the world space angular velocity of the VR Hand.
        //-------------------------------------------------
        public Vector3 GetTrackedObjectAngularVelocity(float timeOffset = 0)
        {

            if (timeOffset == 0)
                return VRManager.trackingOrigin.TransformDirection(trackedObject.GetAngularVelocity());
            
            Vector3 velocity;
            Vector3 angularVelocity;

            trackedObject.GetVelocitiesAtTimeOffset(timeOffset, out velocity, out angularVelocity);
            return VRManager.trackingOrigin.TransformDirection(angularVelocity);
        }
            

        public void GetEstimatedPeakVelocities(out Vector3 velocity, out Vector3 angularVelocity)
        {
            trackedObject.GetEstimatedPeakVelocities(out velocity, out angularVelocity);
            velocity = VRManager.trackingOrigin.TransformVector(velocity);
            angularVelocity = VRManager.trackingOrigin.TransformDirection(angularVelocity);
        }



        protected virtual void Awake()
        {
            velocityEstimator = GetComponent<VelocityEstimator>();
            
            interactor = GetComponent<InteractionPoint>();
            interactor.interactorID = VRManager.Hand2Int(handType);

            GetComponent<EquipPoint>().equipID = interactor.interactorID;
            
            VRManager.onPauseRoutineStart += OnGamePaused;
        }

        InteractionPoint interactor;

        protected virtual IEnumerator Start()
        {
            // Debug.Log( "<b>[SteamVR Interaction]</b> Hand - initializing connection routine" );
            while (true)
            {
                if (isPoseValid)
                {
                    InitController();
                    break;
                }
                yield return null;
            }
            HideController(true);
        }

        InventoryEquipper equipper;
        protected virtual void OnEnable()
        {
         
            
            equipper = Player.instance.GetComponent<InventoryEquipper>();
            equipper.onEquip += OnItemEquipped;
            equipper.onUnequip += OnItemUnequipped;
            equipper.onEquipUpdate += OnEquippedUpdate;

            interactor.onInspectUpdate += OnInspectUpdate;
            interactor.onInspectStart += OnInspectStart;
            interactor.onInspectEnd += OnInspectEnd;

        }

        protected virtual void OnDisable()
        {
            CancelInvoke();

            equipper.onEquip -= OnItemEquipped;
            equipper.onUnequip -= OnItemUnequipped;
            equipper.onEquipUpdate -= OnEquippedUpdate;

            interactor.onInspectUpdate -= OnInspectUpdate;
            interactor.onInspectStart -= OnInspectStart;
            interactor.onInspectEnd -= OnInspectEnd;
        }



        void OnInspectUpdate (InteractionPoint interactor, Interactable hoveringInteractable) {
        
        }
        void OnInspectStart (InteractionPoint interactor, Interactable hoveringInteractable) {
            // for (int i = 0; i < Player.instance.actions.Length; i++) {
            //     if (hoveringInteractable.actionNames.Length > i) {
            //         if (i != equipper.quickEquipAction || equipper.equippedSlots[myEquipIndex] == null) {
            //             StandardizedVRInput.instance.ShowHint(handType, Player.instance.actions[i], hoveringInteractable.actionNames[i]);
            //         }
            //     }
            // }
        }


        void OnInspectEnd (InteractionPoint interactor, Interactable hoveringInteractable) {
            // for (int i = 0; i < Player.instance.actions.Length; i++) {
            //     StandardizedVRInput.instance.HideHint(handType, Player.instance.actions[i]);
            // }
        }

        // void ControlInteractorAndEquipper (SteamVR_Action_Boolean action, int actionKey) {
        //     bool useDown = action.GetStateDown(handType);
        //     bool useUp = action.GetStateUp(handType);
        //     bool useHeld = action.GetState(handType);
                
        //     if (useDown) {
        //         StandardizedVRInput.instance.HideHint(handType, action);
        //         // interactor.OnUseStart(actionKey);
        //     }
        //     if (useUp) {
        //         // interactor.OnUseEnd(actionKey);
        //     }
        //     if (useHeld) {
        //         // interactor.OnUseUpdate(actionKey);
        //     }         
        // }

            


        // protected virtual void Update()
        // {
        //     bool handOccupied = VRUIInput.HandOccupied(handType);
        //     if (handOccupied)
        //         return;

        //     for (int i = 0; i < Player.instance.actions.Length; i++) {
        //         ControlInteractorAndEquipper (Player.instance.actions[i], i);
        //     }        
        // }

        protected virtual void OnEquippedUpdate( Inventory inventory, SceneItem item, int slotIndex, bool quickEquipped )
        {
            if (slotIndex != myEquipIndex) {
                return;
            }
            
            
            if (mainRenderModel != null) 
            {
                VRItemAddon vr_item = item.GetComponent<VRItemAddon>();
                if (vr_item != null)
                {
                    if (vr_item.handFollowTransform)                    
                    {
                        Quaternion targetHandRotation;
                        Vector3 targetHandPosition;

                        // Transform equipPoint = inventory.equippedItem.item.transform;
                        // Quaternion offset = Quaternion.Inverse(this.transform.rotation) * equipPoint.rotation;
                        targetHandRotation = item.transform.rotation;
                                
                        // Vector3 worldOffset = (this.transform.position - equipPoint.position);
                        // Quaternion rotationDiff = mainRenderModel.GetHandRotation() * Quaternion.Inverse(this.transform.rotation);
                        // Vector3 localOffset = rotationDiff * worldOffset;
                        targetHandPosition = item.transform.position;// + localOffset;
                        
                        mainRenderModel.SetHandRotation(targetHandRotation);
                        mainRenderModel.SetHandPosition(targetHandPosition);
                    }
                }
            }
        }


        private void HandDebugLog(string msg)
        {
            Debug.Log("<b>[SteamVR Interaction]</b> Hand (" + this.name + "): " + msg);
        }
        private void InitController()
        {
            HandDebugLog("Hand " + name + " connected with type " + handType.ToString());

            bool hadOldRendermodel = mainRenderModel != null;
            EVRSkeletalMotionRange oldRM_rom = EVRSkeletalMotionRange.WithController;
            if(hadOldRendermodel)
                oldRM_rom = mainRenderModel.GetSkeletonRangeOfMotion;

            if (mainRenderModel != null) {
                Destroy(mainRenderModel.gameObject);
            }

            GameObject renderModelInstance = GameObject.Instantiate(renderModelPrefab);
            renderModelInstance.layer = gameObject.layer;
            renderModelInstance.tag = gameObject.tag;
            renderModelInstance.transform.parent = this.transform;
            renderModelInstance.transform.localPosition = Vector3.zero;
            renderModelInstance.transform.localRotation = Quaternion.identity;
            renderModelInstance.transform.localScale = renderModelPrefab.transform.localScale;

            //TriggerHapticPulse(800);  //pulse on controller init

            int deviceIndex = trackedObject.GetDeviceIndex();

            mainRenderModel = renderModelInstance.GetComponent<RenderModel>();

            mainRenderModel.SetPoser(GetComponent<HandPoser>());

            if (hadOldRendermodel) {
                Debug.Log("setting skeletal range of motion");
                mainRenderModel.SetSkeletonRangeOfMotion(oldRM_rom);
            }

            this.BroadcastMessage("SetInputSource", handType, SendMessageOptions.DontRequireReceiver); // let child objects know we've initialized
            this.BroadcastMessage("OnHandInitialized", deviceIndex, SendMessageOptions.DontRequireReceiver); // let child objects know we've initialized
        }

        public void SetRenderModel(GameObject prefab)
        {
            renderModelPrefab = prefab;

            if (mainRenderModel != null && isPoseValid)
                InitController();
        }

        // public int GetDeviceIndex()
        // {
        //     return trackedObject.GetDeviceIndex();
        // }
    }
}
