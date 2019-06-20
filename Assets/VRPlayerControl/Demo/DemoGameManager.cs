using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleUI;

namespace VRPlayer{

public class DemoGameManager : MonoBehaviour
{

    public static event System.Action<bool, float> onPauseRoutineStart, onPauseRoutineEnd;

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
        float delay = instance.pauseDelay;
        
        if (onPauseRoutineStart != null) {
            onPauseRoutineStart(isPaused, delay);
        }
        
        yield return new WaitForSeconds(delay);

        instance.uiMenu.SetActive(isPaused);
        if (isPaused) {
            yield return null;// new WaitForNextFrame();
            instance.uiMenu.SetActive(!isPaused);
            yield return null;// new WaitForNextFrame();
            instance.uiMenu.SetActive(isPaused);
            yield return null;// new WaitForNextFrame();
            instance.uiMenu.SetActive(!isPaused);
            yield return null;// new WaitForNextFrame();
            instance.uiMenu.SetActive(isPaused);
            
            
            // yield return new WaitForNextFrame();
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
