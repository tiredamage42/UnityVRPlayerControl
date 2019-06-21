using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleUI;

namespace VRPlayer{

public class DemoGameManager : MonoBehaviour
{

    public static event System.Action<bool, float> onPauseRoutineStart, onPauseRoutineEnd;

    public static bool isPaused;


    [Header("The base page for the main menu")]
    public UIPage uiMenuFirstPage;

    static DemoGameManager _i;
    public static DemoGameManager instance{
        get {
            if (_i == null) {
                _i = GameObject.FindObjectOfType<DemoGameManager>();
            }
            return _i;
        }
    }

    public float pauseDelay = .1f;


    void Awake () {
        uiMenuFirstPage.onBaseCancel += OnCancelMainMenuPage;
    }

    void OnCancelMainMenuPage () {
        TogglePause();
    }



    static IEnumerator TogglePauseCoroutine () {
        isPaused = !isPaused;
        float delay = instance.pauseDelay;
        
        if (onPauseRoutineStart != null) {
            onPauseRoutineStart(isPaused, delay);
        }
        
        yield return new WaitForSeconds(delay);

        if (isPaused) {
            
            UIManager.ShowUI (instance.uiMenuFirstPage, true, true);

        }
        else {
            UIManager.HideUI (instance.uiMenuFirstPage);
        }


        if (onPauseRoutineEnd != null) {
            onPauseRoutineEnd(isPaused, delay);
        }
    }

    public static void TogglePause () {
        instance.StartCoroutine(TogglePauseCoroutine());
    }
    


}
}
