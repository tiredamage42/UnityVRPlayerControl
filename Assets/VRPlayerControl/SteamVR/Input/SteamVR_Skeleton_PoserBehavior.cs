using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;
namespace VRPlayer
{

    [CreateAssetMenu()]
public class SteamVR_Skeleton_PoserBehavior : ScriptableObject
{

 
        public SteamVR_Skeleton_Pose[] allPoses;
        // public SteamVR_Skeleton_Pose skeletonMainPose;
        // public List<SteamVR_Skeleton_Pose> skeletonAdditionalPoses = new List<SteamVR_Skeleton_Pose>();
        public List<PoseBlendingBehaviour> blendingBehaviours = new List<PoseBlendingBehaviour>();

        /// <summary>
        /// A filter applied to the base pose. Blends to a secondary pose by a certain weight. Can be masked per-finger
        /// </summary>
        [System.Serializable]
        public class PoseBlendingBehaviour
        {
            public string name;
            // public bool enabled = true;
            // public float influence = 1;
            
            public int pose = 1;

            // public SteamVR_Skeleton_Pose pose;


            // public float value = 0;
            public SteamVR_Action_Single action_single;
            public SteamVR_Action_Boolean action_bool;
            // public float smoothingSpeed = 0;
            public BlenderTypes type;
            // public bool useMask;
            // public SteamVR_Skeleton_HandMask mask = new SteamVR_Skeleton_HandMask();

            // public bool previewEnabled;


            public PoseBlendingBehaviour()
            {
                // enabled = true;
                // influence = 1;
            }

            public enum BlenderTypes
            {
                Manual, AnalogAction, BooleanAction
            }
        }
    }


    

}
