using System.Collections;
using System.Collections.Generic;
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
    }

    void Awake () {
        eventSystem = GameObject.FindObjectOfType<EventSystem>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasSelected && UIManager.input.GetAxisRaw(UIManager.verticalAxis) != 0) {
            eventSystem.SetSelectedGameObject(toSelect);
            hasSelected = true;
        }
        
    }
}
}
