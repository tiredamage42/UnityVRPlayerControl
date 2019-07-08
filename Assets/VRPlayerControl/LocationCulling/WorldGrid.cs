using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace EnvironmentTools {
	
	public class WorldGrid : MonoBehaviour {
		public class CellObject {
			
			System.Action<Vector2Int, int> playerGridChangeCallback;
			System.Func<Vector3> getWorldPosition;

			public CellObject (
				System.Action<Vector2Int, int> playerGridChangeCallback,
				System.Func<Vector3> getWorldPosition
			) {
				this.playerGridChangeCallback = playerGridChangeCallback;
				this.getWorldPosition = getWorldPosition;
			}

			public Vector3 GetWorldPosition() {
				return getWorldPosition();
			}

			public void OnPlayerGridChange (Vector2Int newGrid, int distance) {
				playerGridChangeCallback(newGrid, distance);
			}
		}



		public float cellSize = 20;
		public int cellActivateDistance = 2;

		public static WorldGridPlayer playerCamera;
		Vector2Int lastPlayerGrid;

		// public static event System.Action<Vector2Int> onPlayerGridChange;
		// static Dictionary<Vector2Int, float> cellDistances = new Dictionary<Vector2Int, float>();
		static Dictionary<Vector2Int, List<CellObject>> cellLists = new Dictionary<Vector2Int, List<CellObject>>();


		public static void RegisterCellObject (CellObject cellObject) {
			Vector2Int cell = GetGrid(cellObject.GetWorldPosition(), instance.cellSize);

			if (cellLists.ContainsKey(cell)) {
				cellLists[cell].Add(cellObject);
			}
			else {
				cellLists[cell] = new List<CellObject>(){cellObject};
			}
		}
		public Vector2Int GetPlayerGrid () {
			return GetGrid(playerCamera.transform.position, cellSize);
		}

			
		// public static void RegisterCell (Vector2Int cell) {
		// 	if (!cellDistances.ContainsKey(cell)) {
		// 		cellDistances[cell] = 100;
		// 	}
		// }
		// public static void RegisterCell(Vector3 location) {
		// 	RegisterCell(GetGrid(location));
		// }

		void Update () {
			Vector2Int playerGrid = GetPlayerGrid();

			if (lastPlayerGrid != playerGrid) {

				foreach (var k in cellLists.Keys) {

					int distance = GetDistance(k, playerGrid);

					for (int i =0; i < cellLists[k].Count; i++) {
						cellLists[k][i].OnPlayerGridChange(playerGrid, distance);
					}
					
				}
				
				lastPlayerGrid = playerGrid;
			}
		}

			
		public static int GetDistance (Vector2Int gridA, Vector2Int gridB) {
			return Mathf.Max(Mathf.Abs(gridA.x - gridB.x), Mathf.Abs(gridA.y - gridB.y));
		}

		public static Vector3 GridStartPosition(Vector2Int grid, float cellSize) {
			return new Vector3(grid.x * cellSize, 0, grid.y * cellSize);
		}
		public static Vector3 GridCenterPosition(Vector2Int grid, float cellSize) {
			float halfCell = cellSize * .5f;
			return new Vector3(grid.x * cellSize + halfCell, 0, grid.y * cellSize + halfCell);
		}
		public static Vector3 GridCenterPosition(Vector2Int grid) {
			return GridCenterPosition(grid, instance.cellSize);
		}


		public static Vector2Int GetGrid (Vector3 location, float cellSize) {
			float inv = 1f/cellSize;
			
			float x = location.x * inv;
			int xCell = (int)x;
			if (x < 0) xCell -= 1;
			
			float y = location.z * inv;
			int yCell = (int)y;
			if (y < 0) yCell -= 1;
			
			return new Vector2Int (xCell, yCell);
		}
		public static Vector2Int GetGrid (Vector3 location) {
			return GetGrid(location, instance.cellSize);
		}

			


		static WorldGrid _instance;
		public static WorldGrid instance {
			get {
				if (_instance == null) {
					_instance = GameObject.FindObjectOfType<WorldGrid>();
				}
				return _instance;
			}
		}
	}
	
	// public class LocationCuller : MonoBehaviour {

	// 	public static List<LocationCullCamera> cameras = new List<LocationCullCamera>();

	// 	public float encapsulateRange = 10;

	// 	public float enableDistance = 5f;

	// 	List<LocationCulled> myLocationCulled = new List<LocationCulled>();

	// 	public bool drawGizmo = true;

	// 	void OnDrawGizmosSelected () {
	// 		if (drawGizmo) {

	// 			Gizmos.color = Color.red;
	// 			Gizmos.DrawWireSphere(transform.position, encapsulateRange + enableDistance);
	// 			Gizmos.color = new Color(1,.5f,0,.25f);
	// 			Gizmos.DrawSphere(transform.position, encapsulateRange);
	// 		}

	// 	}

	// 	void Awake () {
	// 		LocationCulled[] allLocationCulled = GameObject.FindObjectsOfType<LocationCulled>();

	// 		for (int i = 0; i < allLocationCulled.Length; i++) {
	// 			if (allLocationCulled[i].parentCuller != null) {
	// 			if (Vector3.Distance(transform.position, allLocationCulled[i].transform.position) <= encapsulateRange) {

	// 				allLocationCulled[i].parentCuller = this;
	// 				myLocationCulled.Add(allLocationCulled[i]);
	// 			}
	// 			}
	// 		}
	// 	}

	// 	bool currentlyVisible = true;

	// 	void Update () {
	// 		bool visible = false;
	// 		for (int i =0; i < cameras.Count; i++) {

	// 			float dist = Vector3.Distance(cameras[i].transform.position, transform.position);

	// 			if (dist <= encapsulateRange + enableDistance) {
	// 				visible = true;
	// 				break;
	// 			}
	// 		}

	// 		if (visible != currentlyVisible) {
	// 			SetVisible(visible);
	// 		}
	// 	}

	// 	void SetVisible (bool visible) {
	// 		currentlyVisible = visible;
	// 		for (int i = 0; i < myLocationCulled.Count; i++) {
	// 			myLocationCulled[i].gameObject.SetActive(visible);
	// 		}
	// 	}
	// }

}








/*
terrraindata.treeInstances

Tree instances

color	Color of this instance.
heightScale	Height scale of this instance (compared to the prototype's size).
lightmapColor	Lightmap color calculated for this instance.
position	Position of the tree.
prototypeIndex	Index of this instance in the TerrainData.treePrototypes array.
rotation	Read-only.Rotation of the tree on X-Z plane (in radians).
widthScale	Width scale of this instance (compared to the prototype's size).













 */