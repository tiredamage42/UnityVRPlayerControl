
// using GameUI;
// using UnityEditor;

// namespace VRPlayer.UI {

    // [CustomEditor(typeof(VRInventoryManagementUIInputHandler))]
    // public class VRInventoryManagementUIInputHandlerEditor : Editor {
    //     VRInventoryManagementUIInputHandler uiHandler;

    //     void OnEnable () {
    //         uiHandler = target as VRInventoryManagementUIInputHandler;
    //     }

    //     public override void OnInspectorGUI () {
    //         base.OnInspectorGUI();

    //         string[] actionNames = InventoryManagementUIHandler.GetHandlerInputNames(uiHandler.context);

    //         if (actionNames != null) {
    //             EditorGUILayout.HelpBox("Actions:\n" + string.Join(", ", actionNames) , MessageType.Info);
    //         }
    //         else {
    //             EditorGUILayout.HelpBox("No context found :\n" + uiHandler.context, MessageType.Error);
    //         }
    //     }
    // }
// }
