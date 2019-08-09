using System.Collections;
using UnityEngine;


namespace GameBase{
    public class GameManager : MonoBehaviour
    {

        public float pauseDelay = .1f;

        public static event System.Action<bool, float> onPauseRoutineStart, onPauseRoutineEnd;
        public static bool isPaused;

        static GameManager _i;
        public static GameManager instance{
            get {
                if (_i == null) {
                    _i = GameObject.FindObjectOfType<GameManager>();
                }
                return _i;
            }
        }


        static IEnumerator TogglePauseCoroutine () {
            isPaused = !isPaused;
            float delay = instance.pauseDelay;
            
            if (onPauseRoutineStart != null) {
                onPauseRoutineStart(isPaused, delay);
            }
            
            yield return new WaitForSeconds(delay);

            if (onPauseRoutineEnd != null) {
                onPauseRoutineEnd(isPaused, delay);
            }
        }

        public static void TogglePause () {
            instance.StartCoroutine(TogglePauseCoroutine());
        }

        public static void QuitApplication () {
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit ();
    #endif
    
        }
    }
}
