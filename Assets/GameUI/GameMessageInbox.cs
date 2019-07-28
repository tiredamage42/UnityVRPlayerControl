using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleUI;


namespace GameUI {

public class GameMessageInbox : MonoBehaviour
{

    public UIMessageCenter messageCenterUIPrefab;
    public float timeBetweenMessages = 1f;
    [HideInInspector] public UIMessageCenter messageCenterUI;
    public event System.Action<string> onShowMessage;


    Queue<string> messageQ = new Queue<string>();

    void Awake () {
        messageCenterUI = Instantiate(messageCenterUIPrefab);// as UIMessageCenter;
    }
    void Start () {
        StartCoroutine(HandleMessageShowing());
    }


    IEnumerator HandleMessageShowing () {

        while (true) {
            if (messageQ.Count > 0) {
                string msg = messageQ.Dequeue();
                if (messageCenterUI != null) {
                    messageCenterUI.ShowMessage(msg);
                }
#if UNITY_EDITOR
                else {
                    Debug.LogError("Message inbox " + name + " doesnt have a message center ui element!!!");
                    Debug.Log(msg);
                }
#endif

                if (onShowMessage != null) {
                    onShowMessage(msg);
                }



            }


            yield return new WaitForSeconds(timeBetweenMessages);
        }
    }




    public void ShowMessage (string message) {
        messageQ.Enqueue(message);
        // if (messageCenterUI != null) {
        //     messageCenterUI.ShowMessage(message);
        // }
        // if (onShowMessage != null) {
        //     onShowMessage(message);
        // }
    }
}

}