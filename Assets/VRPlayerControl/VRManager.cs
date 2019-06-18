using UnityEngine;
using Valve.VR;
namespace VRPlayer {
    public static class VRManager
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
    }
}