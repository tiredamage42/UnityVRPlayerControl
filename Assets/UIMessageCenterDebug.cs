using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleUI;
public class UIMessageCenterDebug : MonoBehaviour
{
    public float frequency = 1;
    UIMessageCenter center;
    // Start is called before the first frame update
    void Start()
    {
        center = GetComponent<UIMessageCenter>();
        // StartCoroutine(MessageRoutine());
    }

    IEnumerator MessageRoutine () {
        while (true) {
            yield return new WaitForSeconds(frequency);
            center.ShowMessage("test message");
        }
    }

}
