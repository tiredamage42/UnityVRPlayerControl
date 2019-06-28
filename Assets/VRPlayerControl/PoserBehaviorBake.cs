using UnityEngine;
using Valve.VR;

namespace VRPlayer
{
    public class PoserBehaviorBake : MonoBehaviour {
        public SteamVR_Input_Sources handToUse = SteamVR_Input_Sources.LeftHand;

        public SteamVR_Skeleton_Pose[] posesToBake;
    }
}