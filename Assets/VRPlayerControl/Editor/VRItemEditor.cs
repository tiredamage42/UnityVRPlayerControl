using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRPlayer {

[CustomEditor(typeof(VRItemAddon))]
public class VRItemEditor : Editor
{
    

    void OnEnable () {

    }

    public override void OnInspectorGUI () {
        base.OnInspectorGUI();

        // EditorGUILayout.Label("Pose Mask:");
        // DrawMaskEnabled();
    }


        Color maskColorEnabled = new Color(0.3f, 1.0f, 0.3f, 1.0f);
        Color maskColorDisabled = new Color(0, 0, 0, 0.5f);
        Color maskColorUnused = new Color(0.3f, 0.7f, 0.3f, 0.3f);


    void DrawMaskEnabled()
    {
        SerializedProperty blenderMask = serializedObject.FindProperty("poseMask").FindPropertyRelative("values");
        // if (blenderMask.arraySize != maskSize) {

        //     for (int c =0 ; c < maskSize; c++) {
        //         blenderMask.InsertArrayElementAtIndex(c);
        //         blenderMask.GetArrayElementAtIndex(c).boolValue = true;
        //     }
        // }

                    



        GUILayout.Label("", GUILayout.Height(50), GUILayout.Width(50));
        Rect maskRect = GUILayoutUtility.GetLastRect();
        for (int i = 0; i < 6; i++)
        {
            // GUI.color = blenderMask.GetArrayElementAtIndex(i).boolValue ? maskColorEnabled : maskColorDisabled;
            // GUI.Label(maskRect, handMaskTextures[i]);
            // GUI.color = new Color(0, 0, 0, 0.0f);
            // if (GUI.Button(GetFingerAreaRect(maskRect, i), ""))
            // {
            //     blenderMask.GetArrayElementAtIndex(i).boolValue = !blenderMask.GetArrayElementAtIndex(i).boolValue;
            // }
            // GUI.color = Color.white;
            //maskVal
        }
    }
}
}
