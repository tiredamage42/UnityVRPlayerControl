using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnvironmentTools {
	
	public class WorldGridPlayer : MonoBehaviour {
		void OnEnable () {
            WorldGrid.playerCamera = this;
			// LocationCuller.cameras.Add(this);

		}
		void OnDisable () {
            if (WorldGrid.playerCamera == this)
			    WorldGrid.playerCamera = null;
			
            // LocationCuller.cameras.Remove(this);
		}
	}
}