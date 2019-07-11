using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnvironmentTools {
	// public class LocationCulled : MonoBehaviour {
	// 	[HideInInspector] public LocationCuller parentCuller;
	// }
    public class CellActivated : MonoBehaviour {

		// void Awake () {
			// WorldGrid.RegisterCellObject(this);
		// }



		bool subscribed;
		Vector2Int myGrid;
		public int activateCellDistance = 2;

		[Header("+ cell size")]
		public float activateCellDistance_f = 20;
		public bool useRawDistance;


		void OnEnable () {
			if (!subscribed) {
				WorldGrid.instance.onPlayerGridChange += OnPlayerGridChange;
				myGrid = WorldGrid.GetGrid(transform.position);
				subscribed = true;
			}
		}

		void OnDestroy () {
			subscribed = false;
			if (WorldGrid.instance != null)
				WorldGrid.instance.onPlayerGridChange -= OnPlayerGridChange;
		}

		float Abs (float a) {
            return a >= 0.0f ? a : -a;
        }

		public void OnPlayerGridChange (Vector2Int newGrid, Vector3 cameraPos, float cellSize){//, int distance) {
			bool withinDistance = false;
			if (useRawDistance) {
				//add cell size buffer so we dont activate / deactivate while inside range
				float distCheck = activateCellDistance_f + cellSize; 
				Vector3 myPos = transform.position;
				withinDistance = Abs(cameraPos.x - myPos.x) <= distCheck && Abs(cameraPos.z - myPos.z) <= distCheck;
			}
			else {
				int distance = WorldGrid.GetDistance(myGrid, newGrid);
				withinDistance = distance <= activateCellDistance;
			}

			if (withinDistance) {
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