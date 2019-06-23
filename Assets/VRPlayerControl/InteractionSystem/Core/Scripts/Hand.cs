using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Valve.VR;
using Valve.VR.InteractionSystem;

using InteractionSystem;
using InventorySystem;


using VRPlayer.UI;
namespace VRPlayer
{
    public class Hand : MonoBehaviour
    {
        [HideInInspector] public VelocityEstimator velocityEstimator;


        void OnGamePaused (bool isPaused) {
            if (isPaused) {
                ShowController(true);
            }
            else {
                HideController(true);
            }
        }

        void AdditionalInitialization () {
            velocityEstimator = GetComponent<VelocityEstimator>();
            VRManager.onGamePaused += OnGamePaused;
        }

        public SteamVR_Input_Sources handType;
        SteamVR_Behaviour_Pose trackedObject;
        public SteamVR_Action_Boolean useAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");
        public SteamVR_Action_Boolean stashAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");
        public SteamVR_Action_Boolean dropAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");
        
        public GameObject renderModelPrefab;
        protected RenderModel mainRenderModel;
        
        
        private TextMesh debugText;
        public bool isActive { get { return trackedObject.isActive; } }
        public bool isPoseValid { get { return trackedObject.isValid; } }


        int myEquipIndex {
            get {
                return GetComponent<EquipPoint>().equipSlotOnBase;
            }
        }
        int otherHandEquipIndex {
            get {
                return 1-myEquipIndex;
            }
        }

        public SteamVR_Behaviour_Skeleton skeleton
        {
            get
            {
                if (mainRenderModel != null)
                    return mainRenderModel.GetSkeleton();
                return null;
            }
        }

        public void ShowController(bool permanent = false)
        {
            if (mainRenderModel != null)
                mainRenderModel.SetControllerVisibility(true, permanent);
        }

        public void HideController(bool permanent = false)
        {
            if (mainRenderModel != null)
                mainRenderModel.SetControllerVisibility(false, permanent);
        }

        public void ShowSkeleton(bool permanent = false)
        {
            if (mainRenderModel != null)
                mainRenderModel.SetHandVisibility(true, permanent);
        }

        public void HideSkeleton(bool permanent = false)
        {
            if (mainRenderModel != null)
                mainRenderModel.SetHandVisibility(false, permanent);
        }

        public bool HasSkeleton()
        {
            return mainRenderModel != null && mainRenderModel.GetSkeleton() != null;
        }

        public void Show()
        {
            SetVisibility(true);
        }

        public void Hide()
        {
            SetVisibility(false);
        }

        public void SetVisibility(bool visible)
        {
            if (mainRenderModel != null)
                mainRenderModel.SetVisibility(visible);
        }

        public void SetSkeletonRangeOfMotion(EVRSkeletalMotionRange newRangeOfMotion, float blendOverSeconds = 0.1f)
        {
            mainRenderModel.SetSkeletonRangeOfMotion(newRangeOfMotion, blendOverSeconds);
        }

        public void SetTemporarySkeletonRangeOfMotion(SkeletalMotionRangeChange temporaryRangeOfMotionChange, float blendOverSeconds = 0.1f)
        {
            mainRenderModel.SetTemporarySkeletonRangeOfMotion(temporaryRangeOfMotionChange, blendOverSeconds);
        }

        public void ResetTemporarySkeletonRangeOfMotion(float blendOverSeconds = 0.1f)
        {
            mainRenderModel.ResetTemporarySkeletonRangeOfMotion(blendOverSeconds);
        }

        public void SetAnimationState(int stateValue)
        {
            mainRenderModel.SetAnimationState(stateValue);
        }

        public void StopAnimation()
        {
            mainRenderModel.StopAnimation();
        }

        protected float blendToPoseTime = 0.1f;
        protected float releasePoseBlendTime = 0.2f;



        void OnItemEquipped (Inventory inventory, Item item, int equipSlot, bool quickEquipped){
            if (equipSlot != myEquipIndex) {
                return;
            }

            if (quickEquipped) {

                if (item.itemBehavior.equipType != Inventory.EquipType.Static && item.rigidbody != null) {
                    GetComponent<VelocityEstimator>().BeginEstimatingVelocity();
                }
            }
                        
            VRItemAddon vr_item = item.GetComponent<VRItemAddon>();
            if (!vr_item) {
                Debug.LogError(item.name + " :: does not have a vr item component");
                return;
            }
            
            if (vr_item.hideHandOnAttach)
                Hide();

            if (vr_item.handAnimationOnPickup != 0)
                SetAnimationState(vr_item.handAnimationOnPickup);

            if (vr_item.setRangeOfMotionOnPickup != SkeletalMotionRangeChange.None)
                SetTemporarySkeletonRangeOfMotion(vr_item.setRangeOfMotionOnPickup);

            if (vr_item.skeletonPoser != null && skeleton != null)
                skeleton.BlendToPoser(vr_item.skeletonPoser, blendToPoseTime);

            if (vr_item.activateActionSetOnAttach != null)
                vr_item.activateActionSetOnAttach.Activate(handType);
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

        void OnItemUnequipped(Inventory inventory, Item item, int slotIndex, bool quickEquipped){
            if (slotIndex != myEquipIndex) {
                return;
            }

            if (quickEquipped) {

                if (item.itemBehavior.equipType != Inventory.EquipType.Static) {
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
            
            if (vr_item.hideHandOnAttach)
                Show();

            if (vr_item.handAnimationOnPickup != 0)
                StopAnimation();

            if (vr_item.setRangeOfMotionOnPickup != SkeletalMotionRangeChange.None)
                ResetTemporarySkeletonRangeOfMotion();
              
            if (mainRenderModel != null)
                mainRenderModel.MatchHandToTransform(mainRenderModel.transform);
            
            if (vr_item.activateActionSetOnAttach != null)
            {
                int otherHandEquipIndex = 1-GetComponent<EquipPoint>().equipSlotOnBase;
                // if (inventory.otherInventory.equippedItem == null || inventory.otherInventory.equippedItem.item.GetComponent<VRItemAddon>().activateActionSetOnAttach != vr_item.activateActionSetOnAttach)
                if (inventory.equippedSlots[otherHandEquipIndex] == null || inventory.equippedSlots[otherHandEquipIndex].sceneItem.GetComponent<VRItemAddon>().activateActionSetOnAttach != vr_item.activateActionSetOnAttach)
                
                {
                    vr_item.activateActionSetOnAttach.Deactivate(handType);
                }
            }

            //move to vr interacable
            if (vr_item.skeletonPoser != null)
            {
                if (skeleton != null) {
                    skeleton.BlendToSkeleton(releasePoseBlendTime);
                }
            }
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
            AdditionalInitialization();
            
            trackedObject = GetComponent<SteamVR_Behaviour_Pose>();
            
            interactor = GetComponent<Interactor>();
            inventory = Player.instance.GetComponent<Inventory>();
        }

        Inventory inventory;
        Interactor interactor;

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

       
        private void UpdateDebugText()
        {
                if (debugText == null)
                {
                    debugText = new GameObject("_debug_text").AddComponent<TextMesh>();
                    debugText.fontSize = 120;
                    debugText.characterSize = 0.001f;
                    debugText.transform.parent = transform;

                    debugText.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
                }

                if (handType == SteamVR_Input_Sources.RightHand)
                {
                    debugText.transform.localPosition = new Vector3(-0.05f, 0.0f, 0.0f);
                    debugText.alignment = TextAlignment.Right;
                    debugText.anchor = TextAnchor.UpperRight;
                }
                else
                {
                    debugText.transform.localPosition = new Vector3(0.05f, 0.0f, 0.0f);
                    debugText.alignment = TextAlignment.Left;
                    debugText.anchor = TextAnchor.UpperLeft;
                }

                debugText.text = string.Format(
                    "Hovering: {0}\n" +
                    "Hover Lock: {1}\n" +
                    "Attached: {2}\n" +
                    "Type: {3}\n",
                    (interactor.hoveringInteractable ? interactor.hoveringInteractable.gameObject.name : "null"),
                    interactor.hoverLocked,

                    // (inventory.equippedItem != null ? inventory.equippedItem.item.name : "null"),
                    (inventory.equippedSlots[myEquipIndex] != null ? inventory.equippedSlots[myEquipIndex].item.itemName : "null"),
                    
                    handType.ToString());
           
        }


        protected virtual void OnEnable()
        {
         
            Inventory inventory = Player.instance.GetComponent<Inventory>();
            inventory.onEquip += OnItemEquipped;
            inventory.onUnequip += OnItemUnequipped;
            inventory.onEquipUpdate += OnEquippedUpdate;

            Interactor interactor = GetComponent<Interactor>();
            interactor.onInspectUpdate += OnInspectUpdate;
            interactor.onInspectStart += OnInspectStart;
            interactor.onInspectEnd += OnInspectEnd;

        }

        protected virtual void OnDisable()
        {
            CancelInvoke();

            
            Inventory inventory = Player.instance.GetComponent<Inventory>();
            inventory.onEquip -= OnItemEquipped;
            inventory.onUnequip -= OnItemUnequipped;
            inventory.onEquipUpdate -= OnEquippedUpdate;

            Interactor interactor = GetComponent<Interactor>();
            interactor.onInspectUpdate -= OnInspectUpdate;
            interactor.onInspectStart -= OnInspectStart;
            interactor.onInspectEnd -= OnInspectEnd;
        }



        void OnInspectUpdate (Interactor interactor, Interactable hoveringInteractable) {
        
        }
        void OnInspectStart (Interactor interactor, Interactable hoveringInteractable) {
            
            if (canQuickEquip) {
                StandardizedVRInput.instance.ShowHint(handType, useAction, hoveringInteractable.actionNames[Inventory.GRAB_ACTION]);
            }
            else {

                // if (!inventory.equippedSlots[myEquipIndex].isQuickEquipped) {

                //     StandardizedVRInput.instance.ShowHint(handType, dropAction, hoveringInteractable.actionNames[Inventory.DROP_ACTION]);
                // }
            }
            

            if (hoveringInteractable.actionNames.Length > Inventory.STASH_ACTION) {
                StandardizedVRInput.instance.ShowHint(handType, stashAction, hoveringInteractable.actionNames[Inventory.STASH_ACTION]);
            }
            // StandardizedVRInput.instance.ShowHint(handType, useAction, "Use");
        }



        void OnInspectEnd (Interactor interactor, Interactable hoveringInteractable) {
            StandardizedVRInput.instance.HideHint(handType, useAction);
            StandardizedVRInput.instance.HideHint(handType, stashAction);
        }

        bool canQuickEquip {
            get {
                return inventory.equippedSlots[myEquipIndex] == null;
            }
        }

            


        protected virtual void Update()
        {
            UpdateDebugText();

            bool useDown = useAction.GetStateDown(handType);
            bool useUp = useAction.GetStateUp(handType);
            bool useHeld = useAction.GetState(handType);
                
            if (useDown) {
                StandardizedVRInput.instance.HideHint(handType, useAction);
                inventory.SetMainEquipPointIndex(myEquipIndex);

                if (canQuickEquip) {
                    interactor.OnUseStart(Inventory.GRAB_ACTION);
                }

                inventory.OnUseStart(myEquipIndex, Inventory.GRAB_ACTION);
                inventory.SetMainEquipPointIndex(-1);
            }
            if (useUp) {
                inventory.SetMainEquipPointIndex(myEquipIndex);

                if (canQuickEquip) {
                    interactor.OnUseEnd(Inventory.GRAB_ACTION);
                }
                
                
                inventory.OnUseEnd(myEquipIndex, Inventory.GRAB_ACTION);
                inventory.SetMainEquipPointIndex(-1);
            }
            if (useHeld) {
                inventory.SetMainEquipPointIndex(myEquipIndex);
                
                if (canQuickEquip) {
                    interactor.OnUseUpdate(Inventory.GRAB_ACTION);
                }

                inventory.OnUseUpdate(myEquipIndex, Inventory.GRAB_ACTION);
                inventory.SetMainEquipPointIndex(-1);
            }         



            useDown = stashAction.GetStateDown(handType);
            useUp = stashAction.GetStateUp(handType);
            useHeld = stashAction.GetState(handType);
                
            if (useDown) {
                StandardizedVRInput.instance.HideHint(handType, stashAction);
                inventory.SetMainEquipPointIndex(myEquipIndex);
                interactor.OnUseStart(Inventory.STASH_ACTION);
                inventory.OnUseStart(myEquipIndex, Inventory.STASH_ACTION);
                inventory.SetMainEquipPointIndex(-1);
            }
            if (useUp) {
                inventory.SetMainEquipPointIndex(myEquipIndex);
                interactor.OnUseEnd(Inventory.STASH_ACTION);
                inventory.OnUseEnd(myEquipIndex, Inventory.STASH_ACTION);
                inventory.SetMainEquipPointIndex(-1);
            }
            if (useHeld) {
                inventory.SetMainEquipPointIndex(myEquipIndex);
                interactor.OnUseUpdate(Inventory.STASH_ACTION);
                inventory.OnUseUpdate(myEquipIndex, Inventory.STASH_ACTION);
                inventory.SetMainEquipPointIndex(-1);
            }



            

            useDown = dropAction.GetStateDown(handType);
            useUp = dropAction.GetStateUp(handType);
            useHeld = dropAction.GetState(handType);
                
            if (useDown) {
                StandardizedVRInput.instance.HideHint(handType, dropAction);
                inventory.SetMainEquipPointIndex(myEquipIndex);
                interactor.OnUseStart(Inventory.DROP_ACTION);
                inventory.OnUseStart(myEquipIndex, Inventory.DROP_ACTION);
                inventory.SetMainEquipPointIndex(-1);
            }
            if (useUp) {
                inventory.SetMainEquipPointIndex(myEquipIndex);
                interactor.OnUseEnd(Inventory.DROP_ACTION);
                inventory.OnUseEnd(myEquipIndex, Inventory.DROP_ACTION);
                inventory.SetMainEquipPointIndex(-1);
            }
            if (useHeld) {
                inventory.SetMainEquipPointIndex(myEquipIndex);
                interactor.OnUseUpdate(Inventory.DROP_ACTION);
                inventory.OnUseUpdate(myEquipIndex, Inventory.DROP_ACTION);
                inventory.SetMainEquipPointIndex(-1);
            }           
        }

        protected virtual void OnEquippedUpdate( Inventory inventory, Item item, int slotIndex, bool quickEquipped )
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

            if (hadOldRendermodel)
                mainRenderModel.SetSkeletonRangeOfMotion(oldRM_rom);

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
