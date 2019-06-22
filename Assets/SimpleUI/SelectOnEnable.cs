using UnityEngine;
using UnityEngine.EventSystems;

namespace SimpleUI {

    public class SelectOnEnable : MonoBehaviour
    {
        bool hasSelected;
        EventSystem eventSystem;
        public GameObject toSelect;

        void OnDisable () {
            hasSelected = false;
            eventSystem.SetSelectedGameObject(null);
        }

        void Awake () {
            eventSystem = GameObject.FindObjectOfType<EventSystem>();
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
