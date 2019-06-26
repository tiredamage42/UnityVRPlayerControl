using UnityEngine;
using Valve.VR;

namespace VRPlayer {

    /*
        add to any items that can be equipped as part of an inventory system
    */

    public class VRItemAddon : MonoBehaviour
    {
        [Tooltip("Activates an action set on attach and deactivates on detach")]
        public SteamVR_ActionSet activateActionSetOnAttach;

        [Tooltip("Hide the whole hand on attachment and show on detach")]
        public bool hideHandOnAttach = true;

        // [Tooltip("The integer in the animator to trigger on pickup. 0 for none")]
        // public int handAnimationOnPickup = 0;
        [Tooltip("Should the rendered hand lock on to and follow the object")]
        public bool handFollowTransform = true;


        [Tooltip("The range of motion to set on the skeleton. None for no change.")]
        public SkeletalMotionRangeChange setRangeOfMotionOnPickup = SkeletalMotionRangeChange.None;
        
        [Space]
        public bool usePose = true;
        public string poseName;
        [Range(0,1)] public float poseInfluence = 1;
        // [HideInInspector] public SteamVR_Skeleton_HandMask poseMask = new SteamVR_Skeleton_HandMask();

        // [HideInInspector] public SteamVR_Skeleton_Poser skeletonPoser;



        // private void Awake()
        // {
        //     skeletonPoser = GetComponent<SteamVR_Skeleton_Poser>();
        // }
    }
}
