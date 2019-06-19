using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleUI;

namespace VRPlayer{

public class DemoGameManager : MonoBehaviour
{

    public static event System.Action<bool> onPauseRoutineStart, onPauseRoutineEnd;

    public static bool isPaused;


    public GameObject uiMenu;

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



    static IEnumerator TogglePauseCoroutine () {
        isPaused = !isPaused;
        
        if (onPauseRoutineStart != null) {
            onPauseRoutineStart(isPaused);
        }
        
        yield return new WaitForSeconds(instance.pauseDelay);

        instance.uiMenu.SetActive(isPaused);

        if (onPauseRoutineEnd != null) {
            onPauseRoutineEnd(isPaused);
        }
    }

    public static void TogglePause () {
        instance.StartCoroutine(TogglePauseCoroutine());
    }
    


}
}
