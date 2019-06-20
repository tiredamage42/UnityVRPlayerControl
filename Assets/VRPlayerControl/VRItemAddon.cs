using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem;
using Valve.VR;

namespace VRPlayer {

    public class VRItemAddon : MonoBehaviour
    {
        [Tooltip("Activates an action set on attach and deactivates on detach")]
        public SteamVR_ActionSet activateActionSetOnAttach;

        [Tooltip("Hide the whole hand on attachment and show on detach")]
        public bool hideHandOnAttach = true;

        // [Tooltip("Hide the skeleton part of the hand on attachment and show on detach")]
        // public bool hideSkeletonOnAttach = false;

        // [Tooltip("Hide the controller part of the hand on attachment and show on detach")]
        // public bool hideControllerOnAttach = false;

        [Tooltip("The integer in the animator to trigger on pickup. 0 for none")]
        public int handAnimationOnPickup = 0;

        [Tooltip("The range of motion to set on the skeleton. None for no change.")]
        public SkeletalMotionRangeChange setRangeOfMotionOnPickup = SkeletalMotionRangeChange.None;

        [HideInInspector]
        public SteamVR_Skeleton_Poser skeletonPoser;

        [Tooltip("Should the rendered hand lock on to and follow the object")]
        public bool handFollowTransform = true;


        private void Awake()
        {
            skeletonPoser = GetComponent<SteamVR_Skeleton_Poser>();
         
        }
    }
}
