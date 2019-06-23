using UnityEngine;
using Valve.VR;

namespace VRPlayer {
     [CreateAssetMenu()]
    public class ControllerLayoutHintRoutine : ScriptableObject {

    
        [System.Serializable] public class RoutineNode {
            public SteamVR_Input_Sources hand;
            public string name;
            public StandardizedVRInput.InputType inputType;
        }

        public RoutineNode[] routineNodes;
        public float timeBetweenButtons = 2;
        public float timeBetweenRepeats = 3;
    }
}
