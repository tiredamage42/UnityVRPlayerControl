using UnityEngine;
using UnityEngine.EventSystems;

namespace SimpleUI {

    public class SelectOnEnable : MonoBehaviour
    {
        bool hasSelected;
        // EventSystem _eventSystem;
        // EventSystem eventSystem {
        //     get {
        //         InitializeEventSystemReference();
        //         return _eventSystem;
        //     }
        // }
        public GameObject toSelect;

        // void InitializeEventSystemReference () {
        //     if (_eventSystem == null) _eventSystem = GameObject.FindObjectOfType<EventSystem>();
        // }

        void OnDisable () {
            if (hasSelected) {
                hasSelected = false;
                UIManager.SetSelection(null);
            }
        }

        // void Awake () {
        //     InitializeEventSystemReference();
        // }
        
        void Update()
        {
            if (!hasSelected){
                // Debug.LogError("selected :: " + toSelect.name+ " on " + name);
             
                UIManager.SetSelection(toSelect);
                hasSelected = true;
            }
        }
    }
}
