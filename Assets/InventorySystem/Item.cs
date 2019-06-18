using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractionSystem;
using VRPlayer;
using Valve.VR;
namespace InventorySystem {

    public class Item : MonoBehaviour, IInteractable
    {
        public Inventory.EquipType equipType = Inventory.EquipType.Normal;
        public EquipBehavior equipBehavior;
        public bool hoverLockOnEquip = true;

        void Awake () {
            InitializeInteractableComponents();
        }

        public int useActionForEquip = 0;

        [Header("Set to -1 for no stash")]
        public int useActionForStash = 1;

        public void OnInspectStart(Interactor interactor) {}
        public void OnInspectEnd(Interactor interactor) {}
        public void OnInspectUpdate(Interactor interactor) {}
        public void OnUseStart(Interactor interactor, int useIndex) {
            if (useIndex == useActionForEquip) {
                Inventory inventory = interactor.GetComponent<Inventory>();
                if (inventory != null) {
                    if (hoverLockOnEquip)
                        interactor.HoverLock( GetComponent<Interactable>() );
            
                    inventory.EquipItem(this);//, attachmentFlags);
                    // this.parentInventory = inventory;
                }
            }
            else if (useIndex == useActionForStash) {

            }
            else {
                if (useIndex != 0 && useIndex != 1) 
                    Debug.LogError("unknown action for item " + name + "\naction: " + useIndex + "\ninteractor:" + interactor.name);
            }
        }
        public bool unequipOnUseEnd = true;
        public void OnUseEnd(Interactor interactor, int useIndex) {
            if (!unequipOnUseEnd)
                return;
            // Debug.LogError("use end");
                
            if (useIndex == useActionForEquip) {
                // Debug.LogError("should beeee");
                Inventory inventory = interactor.GetComponent<Inventory>();
                if (inventory != null) {
                    // Debug.LogError("inventory not null");
                    if (inventory == parentInventory) {
                        // Debug.LogError("same as parent");

                        if (hoverLockOnEquip)
                            interactor.HoverUnlock(GetComponent<Interactable>());
                        inventory.UnequipItem(this);
                        

                        // Uncomment to detach ourselves late in the frame.
                        // This is so that any vehicles the player is attached to
                        // have a chance to finish updating themselves.
                        // If we detach now, our position could be behind what it
                        // will be at the end of the frame, and the object may appear
                        // to teleport behind the hand when the player releases it.
                        //StartCoroutine( LateDetach( hand ) );

                    }
                }
            }
            else if (useIndex == useActionForStash) {
                Debug.LogError("stash!");

            }
            else {
                if (useIndex != 0 && useIndex != 1) 
                    Debug.LogError("unknown action for item " + name + "\naction: " + useIndex + "\ninteractor:" + interactor.name);
            }
        }
        public void OnUseUpdate(Interactor interactor, int useIndex) {

        }

        protected virtual IEnumerator LateDetach( Inventory inventory )
		{
			yield return new WaitForEndOfFrame();
            inventory.UnequipItem(this);
		}

        public SteamVR_ActionSet activateActionSetOnAttach
        {
            get 
            {
                return GetComponent<Interactable>().activateActionSetOnAttach;
            }
        }

        public bool hideHandOnAttach
        {
            get 
            {
                return GetComponent<Interactable>().hideHandOnAttach;
            }
        }


        public bool hideSkeletonOnAttach
        {
            get 
            {
                return GetComponent<Interactable>().hideSkeletonOnAttach;
            }
        }

        public bool hideControllerOnAttach 
        {
            get 
            {
                return GetComponent<Interactable>().hideControllerOnAttach;
            }
        }

        public int handAnimationOnPickup 
        {
            get 
            {
                return GetComponent<Interactable>().handAnimationOnPickup;
            }
        }

        public SkeletalMotionRangeChange setRangeOfMotionOnPickup 
        {
            get 
            {
                return GetComponent<Interactable>().setRangeOfMotionOnPickup;
            }
        }


        














        // [Tooltip("Specify whether you want to snap to the inventory's object attachment point, or just the raw inventory transform")]
        // public bool useAlternateAttachementPoint {
        //     get {
        //         return GetComponent<Interactable>().useHandObjectAttachmentPoint;
        //     }
        // }
        // public bool attachEaseIn{// = false;
        // get {
        //         return GetComponent<Interactable>().attachEaseIn;
        //     }
        // }
        // [HideInInspector] public AnimationCurve snapAttachEaseInCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
        // public float snapAttachEaseInTime{// = 0.15f;
        // get {
        //         return GetComponent<Interactable>().snapAttachEaseInTime;
        //     }
        // }
        // public Hand.AttachmentFlags attachmentFlags; 
        // {
        //     get {
        //         return GetComponent<Interactable>().attachmentFlags;
        //     }
        // }
        // public bool snapAttachEaseInCompleted {// = false;
        // get {
        //         return GetComponent<Interactable>().snapAttachEaseInCompleted;
        //     }
        // }
        public SteamVR_Skeleton_Poser skeletonPoser {// = false;
        get {
                return GetComponent<Interactable>().skeletonPoser;
            }
        }

        public bool handFollowTransform {// = false;
        get {
                return GetComponent<Interactable>().handFollowTransform;
            }
        }














        public Inventory parentInventory;

        public void OnEquipped (Inventory inventory) {
            this.parentInventory = inventory;
            
            for (int i = 0; i < itemComponents.Count; i++) {
                itemComponents[i].OnEquipped(inventory);
            }
        }
        public void OnUnequipped (Inventory inventory) {
            this.parentInventory = null;
            
            for (int i = 0; i < itemComponents.Count; i++) {
                itemComponents[i].OnUnequipped(inventory);
            }
        }
        public void OnEquippedUpdate(Inventory inventory) {
            for (int i = 0; i < itemComponents.Count; i++) {
                itemComponents[i].OnEquippedUpdate(inventory);
            }
        }


        public void OnEquippedUseStart (Inventory interactor, int useIndex) {
            for (int i = 0; i < itemComponents.Count; i++) {
                itemComponents[i].OnEquippedUseStart(interactor, useIndex);
            }
            // if (onUseStart != null) {
            //     onUseStart(interactor, useIndex);
            // }
        }
        public void OnEquippedUseEnd (Inventory interactor, int useIndex) {
            for (int i = 0; i < itemComponents.Count; i++) {
                itemComponents[i].OnEquippedUseEnd(interactor, useIndex);
            }
            // if (onUseEnd != null) {
            //     onUseEnd(interactor, useIndex);
            // }
        }
        public void OnEquippedUseUpdate(Inventory interactor, int useIndex) {
            for (int i = 0; i < itemComponents.Count; i++) {
                itemComponents[i].OnEquippedUseUpdate(interactor, useIndex);
            }
            // if (onUseUpdate != null) {
            //     onUseUpdate(interactor, useIndex);
            // }
        }

        List<IInventoryItem> itemComponents = new List<IInventoryItem>();
        void InitializeInteractableComponents() {
            // itemComponents = GetComponents<IInventoryItem>().ToList();

            IInventoryItem[] itemComponents = GetComponents<IInventoryItem>();

            for (int i = 0; i< itemComponents.Length; i++) {
                this.itemComponents.Add(itemComponents[i]);
            }
        }
        public void AddItemComponent (IInventoryItem itemComponent) {
            itemComponents.Add(itemComponent);
        }

        void OnDestroy()
        {
            // isDestroying = true;

            if (parentInventory != null)
            {
                parentInventory.UnequipItem(this, false);//this.gameObject, false);
                // parentInventory.skeleton.BlendToSkeleton(0.1f);
            }

            
        }




    }
}
