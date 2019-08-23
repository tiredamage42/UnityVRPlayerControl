using UnityEngine;

namespace VRPlayer.UI {
    public class TrackpadGhostHandler : MonoBehaviour
    {
        public TrackpadGhostUI trackpadGhostPrefab;
        public TransformBehavior trackpadGhostEquip;
        TrackpadGhostUI[] trackpadGhosts;

        void UpdateTrackpadTransforms () {
            for (int i = 0; i < trackpadGhosts.Length; i++) {
                TransformBehavior.AdjustTransform(trackpadGhosts[i].transform, Player.instance.GetHand(VRManager.Int2Hand(i)).transform, trackpadGhostEquip, 0);
            }
        }

        void OnEnable () {
            trackpadGhosts = new TrackpadGhostUI[] {
                Instantiate(trackpadGhostPrefab),
                Instantiate(trackpadGhostPrefab),
            };
            UpdateTrackpadTransforms();
        }

        void Update()
        {

#if UNITY_EDITOR

            UpdateTrackpadTransforms();
#endif
            for (int i = 0; i < trackpadGhosts.Length; i++) {
                // trackpadGhosts[i].axis = StandardizedVRInput.instance.TrackpadAxis.GetAxis(VRManager.Int2Hand(i));
                trackpadGhosts[i].axis = Player.instance.GetTrackpadAxis(i);
                trackpadGhosts[i].middleZone = Player.instance.trackpadMiddleZone;
                trackpadGhosts[i].inMiddle = Player.instance.GetTrackpadIsMiddleValue(i);
            }
            
        }
    }

}
