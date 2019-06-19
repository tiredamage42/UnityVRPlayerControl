// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// namespace SimpleUI {

// public class UIPageButton : MonoBehaviour
// {
//     Button button;
//     public UIPage page;
//     [HideInInspector] public UIPage parentPage;
//     void OnButtonClick () {
//         if (page != null) {
//             page.gameObject.SetActive(true);
//             page.parentPage = parentPage;
//             parentPage.gameObject.SetActive(false);
//         }
//     }

//     void Awake () {
//         button = GetComponent<Button>();
//         button.onClick.AddListener(OnButtonClick);
//     }
    
// }
// }
