using UnityEngine;

namespace EnvironmentTools{
    [RequireComponent(typeof(CellActivated))]
    [RequireComponent(typeof(AudioSource))]
    public class CellActivated_AudioSource : MonoBehaviour
    {
        void Awake () {
            AudioSource source = GetComponent<AudioSource>();
            CellActivated cellActivated = GetComponent<CellActivated>();
            cellActivated.useRawDistance = true;
            cellActivated.activateCellDistance_f = source.maxDistance;
        }
    }
}
