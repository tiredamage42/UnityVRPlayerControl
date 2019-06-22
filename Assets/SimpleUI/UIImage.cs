using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleUI {

/*
    used for keeping ui image color consistent
*/

[ExecuteInEditMode]
public class UIImage : MonoBehaviour
{
    public bool useDark;
    void OnEnable () {
        GetComponent<Image>().color = useDark ? UIManager.instance.mainDarkColor : UIManager.instance.mainLightColor;

    }



    
}
}
