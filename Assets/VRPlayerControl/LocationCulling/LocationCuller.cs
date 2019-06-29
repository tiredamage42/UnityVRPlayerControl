using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace EnvironmentTools {
	public class LocationCulled : MonoBehaviour {
		[HideInInspector] public LocationCuller parentCuller;
	}

	public class LocationCullCamera : MonoBehaviour {
		void OnEnable () {
			LocationCuller.cameras.Add(this);
		}
		void OnDisable () {
			LocationCuller.cameras.Remove(this);
		}
	}

		
	public class LocationCuller : MonoBehaviour {

		public static List<LocationCullCamera> cameras = new List<LocationCullCamera>();

		public float encapsulateRange = 10;

		public float enableDistance = 5f;

		List<LocationCulled> myLocationCulled = new List<LocationCulled>();


		void Awake () {
			LocationCulled[] allLocationCulled = GameObject.FindObjectsOfType<LocationCulled>();

			for (int i = 0; i < allLocationCulled.Length; i++) {
				if (allLocationCulled[i].parentCuller != null) {
				if (Vector3.Distance(transform.position, allLocationCulled[i].transform.position) <= encapsulateRange) {

					allLocationCulled[i].parentCuller = this;
					myLocationCulled.Add(allLocationCulled[i]);
				}
				}
			}
		}

		bool currentlyVisible = true;

		void Update () {
			bool visible = false;
			for (int i =0; i < cameras.Count; i++) {

				float dist = Vector3.Distance(cameras[i].transform.position, transform.position);

				if (dist <= encapsulateRange + enableDistance) {
					visible = true;
					break;
				}
			}

			if (visible != currentlyVisible) {
				SetVisible(visible);
			}
		}

		void SetVisible (bool visible) {
			currentlyVisible = visible;
			for (int i = 0; i < myLocationCulled.Count; i++) {
				myLocationCulled[i].gameObject.SetActive(visible);
			}
		}



	}
}