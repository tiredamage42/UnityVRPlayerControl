using UnityEngine;
using UnityEditor;
// using UnityEditorInternal;

namespace EnvironmentTools {

    [CustomEditor(typeof(WorldGrid))]
    public class WorldGridEditor : Editor
    {
    
        WorldGrid worldGrid;

        void OnEnable()
        {
            worldGrid = target as WorldGrid;
        }

        const int sceneViewBoxSize = 256;
        const float mosuePosOffset = 8;


        
        
        void OnSceneGUI()
        {
            Event e = Event.current;
            
            Handles.BeginGUI();
            Rect r = new Rect(e.mousePosition.x + mosuePosOffset, e.mousePosition.y + mosuePosOffset, sceneViewBoxSize, sceneViewBoxSize);
            float cellSize = worldGrid.cellSize;

            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Vector3 worldPos = ray.origin + ray.direction * 100;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                worldPos = hit.point;
            }

            Vector2Int mouseGrid = WorldGrid.GetGrid(worldPos, cellSize);
            GUI.Label(r, string.Format ("Grid x:{0}/y:{1}", mouseGrid.x, mouseGrid.y));
            Handles.EndGUI();
            
            Handles.color = Color.red;
            float size = cellSize * .1f;

            Handles.DrawWireDisc (new Vector3(mouseGrid.x * cellSize, 0, mouseGrid.y * cellSize), Vector3.up, size);
            Handles.DrawWireDisc (new Vector3((mouseGrid.x+1) * cellSize, 0, mouseGrid.y * cellSize), Vector3.up, size);
            Handles.DrawWireDisc (new Vector3(mouseGrid.x * cellSize, 0, (mouseGrid.y+1) * cellSize), Vector3.up, size);
            Handles.DrawWireDisc (new Vector3((mouseGrid.x+1) * cellSize, 0, (mouseGrid.y+1) * cellSize), Vector3.up, size);
            
            SceneView.RepaintAll();        
        }

        
    }

}

