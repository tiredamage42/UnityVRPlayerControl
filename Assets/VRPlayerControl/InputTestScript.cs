using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class InputTestScript : MonoBehaviour
{
    
    // [SteamVR_DefaultAction("Squeeze")]
    public SteamVR_Action_Boolean booleanAction;

    public SteamVR_Action_Single squeezeAction;
    public SteamVR_Action_Vector2 touchpadAction;
    
    
    
    void Awake () {

        //events
        booleanAction[SteamVR_Input_Sources.Any].onStateDown += OnBooleanDown;
        booleanAction[SteamVR_Input_Sources.Any].onState += OnBoolean;
        booleanAction[SteamVR_Input_Sources.Any].onStateUp += OnBooleanUp;

        //fires when axis isn on zero
        touchpadAction[SteamVR_Input_Sources.Any].onAxis += OnAxis;
    }

    void OnAxis(SteamVR_Action_Vector2 action, SteamVR_Input_Sources source, 
        Vector2 axis, Vector2 delta) {

            Debug.Log( " event axis value: " + axis);

    }
    

    void OnBoolean(SteamVR_Action_Boolean action, SteamVR_Input_Sources source) {
Debug.Log( " boolean pressed event " );
    }
    void OnBooleanDown(SteamVR_Action_Boolean action, SteamVR_Input_Sources source) {
Debug.Log( " boolean down event " );
    }
    void OnBooleanUp(SteamVR_Action_Boolean action, SteamVR_Input_Sources source) {
Debug.Log( " boolean up event " );
    }

    

    // Update is called once per frame
    void Update()
    {
        if (booleanAction.GetStateDown(SteamVR_Input_Sources.Any)) {
            Debug.Log( " boolean down " );
        }

        if (booleanAction[SteamVR_Input_Sources.LeftHand].stateDown) {
            Debug.Log( " boolean down shortcut " );
        }

        if (SteamVR_Actions._default.Teleport.GetStateUp(SteamVR_Input_Sources.Any)) {
            Debug.Log( " Teleport up " );
        }
        

        float triggerValue = squeezeAction.GetAxis(SteamVR_Input_Sources.Any);

        if (triggerValue > 0) {
            print(triggerValue);
        }
    }


}
