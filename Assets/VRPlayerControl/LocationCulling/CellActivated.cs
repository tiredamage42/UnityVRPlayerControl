using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnvironmentTools {
	// public class LocationCulled : MonoBehaviour {
	// 	[HideInInspector] public LocationCuller parentCuller;
	// }
    public class CellActivated : MonoBehaviour {

		void Awake () {
			// WorldGrid.RegisterCellObject(this);
		}

		public void OnPlayerGridChange (Vector2Int newGrid, int distance) {
			if (distance <= WorldGrid.instance.cellActivateDistance) {
				if (!gameObject.activeSelf) {
					gameObject.SetActive(true);
				}
				
			}
			else {
				if (gameObject.activeSelf) {
					gameObject.SetActive(false);
				}
			}
		}
	}
}