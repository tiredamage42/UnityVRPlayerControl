using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace EnvironmentTools {
	
	public class WorldGrid : GridHandler {
		public float gridSize = 20;
		
		// public float cellSize = 20;
		// public int cellActivateDistance = 2;

		// public static WorldGridPlayer playerCamera;
		// Vector2Int lastPlayerGrid;

		// public Vector2Int GetPlayerGrid () {
		// 	return GetGrid(playerCamera.transform.position, cellSize);
		// }

		// public event System.Action<Vector2Int, Vector3, float> onPlayerGridChange;

		// void Update () {
		// 	Vector2Int playerGrid = GetPlayerGrid();

		// 	if (lastPlayerGrid != playerGrid) {

		// 		if (onPlayerGridChange != null) {
		// 			onPlayerGridChange(playerGrid, playerCamera.transform.position, cellSize);
		// 		}
				
		// 		lastPlayerGrid = playerGrid;
		// 	}
		// }

		// public static int GetDistance (Vector3 a, Vector2Int gridB, float cellSize) {
		// 	return GetDistance(GetGrid(a, cellSize), gridB);
		// }

		// public static int GetDistance (Vector2Int gridA, Vector2Int gridB) {
		// 	int x = gridA.x - gridB.x;
		// 	if (x < 0) x = -x;
		// 	int y = gridA.y - gridB.y;
		// 	if (y < 0) y = -y;
		// 	return y > x ? y : x;
		// }

		// public static Vector3 GridStartPosition(Vector2Int grid, float cellSize) {
		// 	return new Vector3(grid.x * cellSize, 0, grid.y * cellSize);
		// }
		// public static Vector3 GridCenterPosition(Vector2Int grid, float cellSize) {
		// 	float halfCell = cellSize * .5f;
		// 	return new Vector3(grid.x * cellSize + halfCell, 0, grid.y * cellSize + halfCell);
		// }
		// public static Vector3 GridCenterPosition(Vector2Int grid) {
		// 	return GridCenterPosition(grid, instance.gridSize);
		// }


		// public static Vector2Int GetGrid (Vector3 location, float cellSize) {
		// 	float inv = 1f/cellSize;
			
		// 	float x = location.x * inv;
		// 	int xCell = (int)x;
		// 	if (x < 0) xCell -= 1;
			
		// 	float y = location.z * inv;
		// 	int yCell = (int)y;
		// 	if (y < 0) yCell -= 1;
		// 	return new Vector2Int (xCell, yCell);
		// }

		protected override float GetGridSize() {
			return gridSize;
		}


		public static Vector2Int GetGrid (Vector3 location) {
			return GetGrid(location, instance.gridSize);
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

		protected override void OnPlayerGridChange(Vector2Int playerGrid, Vector3 playerPosition, float cellSize) {

		}

	}


	
	public abstract class GridHandler : MonoBehaviour {
		protected abstract float GetGridSize();
		// public float gridSize = 20;
		// public int cellActivateDistance = 2;

		public static WorldGridPlayer playerCamera;
		
		static bool logged;
		public static Vector3 GetReferencePosition ( ) {
			if (playerCamera != null)
				return playerCamera.transform.position;
			
			if (!logged) {

				Debug.LogWarning("no player camera specified for grid handler :: getting first camera in scene");
				logged = true;
			}

			Camera cam = GameObject.FindObjectOfType<Camera>();
			if (cam != null)
				return cam.transform.position;

			Debug.LogError("cant find camera either ");
			return Vector3.zero;
		}




		Vector2Int lastPlayerGrid;

		public Vector2Int GetPlayerGrid (out Vector3 refPosition) {
			refPosition = GetReferencePosition();
			return GetGrid(refPosition, GetGridSize());
		}

		public event System.Action<Vector2Int, Vector3, float> onPlayerGridChange;

		protected abstract void OnPlayerGridChange(Vector2Int playerGrid, Vector3 playerPosition, float cellSize);

		protected virtual void Update () {
			Vector3 refPosition;
			Vector2Int playerGrid = GetPlayerGrid(out refPosition);

			if (lastPlayerGrid != playerGrid) {

				if (onPlayerGridChange != null) {
					onPlayerGridChange(playerGrid, refPosition, GetGridSize());
				}

				OnPlayerGridChange(playerGrid, refPosition, GetGridSize());

				lastPlayerGrid = playerGrid;
			}
		}

		public static int GetDistance (Vector3 a, Vector2Int gridB, float cellSize) {
			return GetDistance(GetGrid(a, cellSize), gridB);
		}

		public static int GetDistance (Vector2Int gridA, Vector2Int gridB) {
			int x = gridA.x - gridB.x;
			if (x < 0) x = -x;
			int y = gridA.y - gridB.y;
			if (y < 0) y = -y;
			return y > x ? y : x;
		}

		public Vector3 GridCenterPosition (Vector2Int grid) {
			return GridCenterPosition(grid, GetGridSize());
		}

		public static Vector3 GridStartPosition(Vector2Int grid, float cellSize) {
			return new Vector3(grid.x * cellSize, 0, grid.y * cellSize);
		}
		public static Vector3 GridCenterPosition(Vector2Int grid, float cellSize) {
			float halfCell = cellSize * .5f;
			return new Vector3(grid.x * cellSize + halfCell, 0, grid.y * cellSize + halfCell);
		}
		// public static Vector3 GridCenterPosition(Vector2Int grid) {
		// 	return GridCenterPosition(grid, instance.cellSize);
		// }


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


		// public static Vector2Int GetGrid (Vector3 location) {
		// 	return GetGrid(location, instance.cellSize);
		// }

		// static WorldGrid _instance;
		// public static WorldGrid instance {
		// 	get {
		// 		if (_instance == null) {
		// 			_instance = GameObject.FindObjectOfType<WorldGrid>();
		// 		}
		// 		return _instance;
		// 	}
		// }
	}
}