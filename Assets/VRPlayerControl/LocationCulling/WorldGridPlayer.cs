using UnityEngine;

namespace EnvironmentTools {

	[ExecuteInEditMode]
	public class WorldGridPlayer : MonoBehaviour {
		void OnEnable () {
			WorldGrid.playerCamera = this;
		}
		void OnDisable () {
			if (WorldGrid.playerCamera == this)
				WorldGrid.playerCamera = null;
		}
	}
}