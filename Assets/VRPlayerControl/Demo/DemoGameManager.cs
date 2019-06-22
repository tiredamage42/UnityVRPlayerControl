using System.Collections;
using UnityEngine;

// using SimpleUI;

namespace VRPlayerDemo{
    public class DemoGameManager : MonoBehaviour
    {

        public float pauseDelay = .1f;

        public event System.Action<bool, float> onPauseRoutineStart, onPauseRoutineEnd;
        public bool isPaused;

        // [Header("The base page for the main menu")]
        // public UIPage uiMenuFirstPage;

        static DemoGameManager _i;
        public static DemoGameManager instance{
            get {
                if (_i == null) {
                    _i = GameObject.FindObjectOfType<DemoGameManager>();
                }
                return _i;
            }
        }


        public event System.Action<GameObject[], object[]> onUISelect, onUISubmit;

        public System.Delegate[] GetUISelectInvocations () {
            return onUISelect.GetInvocationList();
        }
        public System.Delegate[] GetUISubmitInvocations () {
            return onUISubmit.GetInvocationList();
        }




        public event System.Action<string, int> onShowGameMessage;
        public void ShowGameMessage (string message, int key) {

            if (onShowGameMessage != null) {
                onShowGameMessage(message, key);
            }
        }



        // void Awake () {
        //     uiMenuFirstPage.onBaseCancel += OnCancelMainMenuPage;
        // }

        // void OnCancelMainMenuPage () {
        //     TogglePause();
        // }



        IEnumerator TogglePauseCoroutine () {
            isPaused = !isPaused;
            float delay = instance.pauseDelay;
            
            if (onPauseRoutineStart != null) {
                onPauseRoutineStart(isPaused, delay);
            }
            
            yield return new WaitForSeconds(delay);

            // if (isPaused)
            //     UIManager.ShowUI (instance.uiMenuFirstPage, true, true);
            // else
            //     UIManager.HideUI (instance.uiMenuFirstPage);

            if (onPauseRoutineEnd != null) {
                onPauseRoutineEnd(isPaused, delay);
            }
        }

        public void TogglePause () {
            StartCoroutine(TogglePauseCoroutine());
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
