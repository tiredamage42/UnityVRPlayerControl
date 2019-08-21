using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Game.RadioSystem {

    [CreateAssetMenu(menuName="Game/Radio System/Radio Station")]
    public class RadioStation : ScriptableObject {
        public string displayName = "New Radio Station";
        public AudioClip[] songs;
        public bool randomizeSongs = true;
    }
}