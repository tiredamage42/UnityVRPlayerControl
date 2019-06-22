using UnityEngine;
using UnityEngine.EventSystems;

namespace SimpleUI {

    public class SelectOnEnable : MonoBehaviour
    {
        bool hasSelected;
        
        EventSystem _eventSystem;
        EventSystem eventSystem {
            get {
                if (_eventSystem == null) {
                    _eventSystem = GameObject.FindObjectOfType<EventSystem>();
                }
                return _eventSystem;
            }
        }
        public GameObject toSelect;

        void OnDisable () {
            hasSelected = false;
            eventSystem.SetSelectedGameObject(null);
        }

        void Awake () {
            // eventSystem = GameObject.FindObjectOfType<EventSystem>();
            // Debug.LogError("found eent syste " + eventSystem);
        }
        
        // Update is called once per frame
        void Update()
        {
            if (!hasSelected){
                // Debug.LogError("selected :: " + toSelect.name+ " on " + name);
                eventSystem.SetSelectedGameObject(toSelect);
                hasSelected = true;
            }
            
        }
    }
}
