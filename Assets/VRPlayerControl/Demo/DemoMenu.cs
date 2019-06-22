using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace VRPlayerDemo {

    public class DemoMenu : MonoBehaviour
    {
        public void Quit()
        {
            DemoGameManager.QuitApplication();
        }
    }
}
