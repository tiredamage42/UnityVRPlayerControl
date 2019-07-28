using System.Collections;
using System.Collections.Generic;
using UnityEngine;



using System;
using System.Linq;
using System.Text;

using UnityEditor;
using Valve.VR;

namespace VRPlayer// Valve.VR
{

    [CustomEditor(typeof(HandPoserController))]
    public class HandPoserControllerEditor : Editor
    {
        private SerializedProperty allPosesProperty;
        int previewPoseSelection;
        private SerializedProperty blendingBehaviourArray;


        private HandPoserController poserBehavior;
        private bool PoseChanged = false;



        GUILayoutOption blendbuttonWidth;
        GUIStyle blendButtonStyle;


        protected void OnEnable()
        {
            poserBehavior = (HandPoserController)target;            
            allPosesProperty = serializedObject.FindProperty("allPoses");   
            blendingBehaviourArray = serializedObject.FindProperty("blendingBehaviours");
            blendbuttonWidth = GUILayout.Width(28);
            blendButtonStyle = EditorStyles.miniButton;
        }

        protected void DrawHand( SteamVR_Skeleton_Pose_Hand handData, SteamVR_Skeleton_Pose_Hand otherData)//, bool getFromOpposite)
        {
            EditorGUIUtility.labelWidth = 120;
            SteamVR_Skeleton_FingerExtensionTypes newThumb = (SteamVR_Skeleton_FingerExtensionTypes)EditorGUILayout.EnumPopup("Thumb", handData.thumbFingerMovementType);
            SteamVR_Skeleton_FingerExtensionTypes newIndex = (SteamVR_Skeleton_FingerExtensionTypes)EditorGUILayout.EnumPopup("Index", handData.indexFingerMovementType);
            SteamVR_Skeleton_FingerExtensionTypes newMiddle = (SteamVR_Skeleton_FingerExtensionTypes)EditorGUILayout.EnumPopup("Middle", handData.middleFingerMovementType);
            SteamVR_Skeleton_FingerExtensionTypes newRing = (SteamVR_Skeleton_FingerExtensionTypes)EditorGUILayout.EnumPopup("Ring", handData.ringFingerMovementType);
            SteamVR_Skeleton_FingerExtensionTypes newPinky = (SteamVR_Skeleton_FingerExtensionTypes)EditorGUILayout.EnumPopup("Pinky", handData.pinkyFingerMovementType);
            EditorGUIUtility.labelWidth = 0;

            if (newThumb != handData.thumbFingerMovementType || newIndex != handData.indexFingerMovementType ||
                    newMiddle != handData.middleFingerMovementType || newRing != handData.ringFingerMovementType ||
                    newPinky != handData.pinkyFingerMovementType)
            {
         
                handData.thumbFingerMovementType = newThumb;
                handData.indexFingerMovementType = newIndex;
                handData.middleFingerMovementType = newMiddle;
                handData.ringFingerMovementType = newRing;
                handData.pinkyFingerMovementType = newPinky;

                EditorUtility.SetDirty(poserBehavior);//.skeletonMainPose);
            }
        }
      


        int activePoseIndex = 0;
        SerializedProperty activePoseProp;
        SteamVR_Skeleton_Pose activePose;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            DrawPoseEditorMenu();

            DrawBlendingBehaviourMenu();
            
            if (GUILayout.Button("Create Blending Behaviors From All Poses")){
                CreateBlendingBehaviorsFromAllPoses();
            }

            serializedObject.ApplyModifiedProperties();

        }


        string[] poseNames;


        void DrawPoseEditorMenu()
        {
                bool createNew = false;

                activePoseIndex = previewPoseSelection;//.intValue;
            
                if (activePoseIndex >= 0 && activePoseIndex < allPosesProperty.arraySize) {
                    activePoseProp = allPosesProperty.GetArrayElementAtIndex(activePoseIndex);
                }


                //box containing all pose editing controls
                GUILayout.BeginVertical("box");

                poseNames = new string[allPosesProperty.arraySize];
                
                for (int i = 0; i < allPosesProperty.arraySize; i++)
                {
                    poseNames[i] = allPosesProperty.GetArrayElementAtIndex(i).objectReferenceValue == null ? "[not set]" : allPosesProperty.GetArrayElementAtIndex(i).objectReferenceValue.name;
                }
                
                    //show selectable menu of all poses, highlighting the one that is selected
                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();

                    int poseSelected = EditorGUILayout.Popup ("Current Pose:", activePoseIndex, poseNames);
                    if (poseSelected != activePoseIndex)
                    {
                        activePoseIndex = poseSelected;
                        PoseChanged = true;
                        previewPoseSelection = activePoseIndex;
                    }

                    // EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(32));
                    if (GUILayout.Button("+", GUILayout.MaxWidth(32)))
                    {
                        allPosesProperty.InsertArrayElementAtIndex(allPosesProperty.arraySize);
                        allPosesProperty.GetArrayElementAtIndex(allPosesProperty.arraySize - 1).objectReferenceValue = null;                        
                    }
                    //only allow deletion of additional poses
                    EditorGUI.BeginDisabledGroup(allPosesProperty.arraySize == 0 || activePoseIndex == 0);
                    if (GUILayout.Button("-", GUILayout.MaxWidth(32)) && allPosesProperty.arraySize > 0)
                    {
                        allPosesProperty.DeleteArrayElementAtIndex(activePoseIndex);
                        allPosesProperty.DeleteArrayElementAtIndex(activePoseIndex);
                        if (activePoseIndex >= allPosesProperty.arraySize)
                        {
                            activePoseIndex = allPosesProperty.arraySize-1;
                            previewPoseSelection = activePoseIndex;
                            return;
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                    
                    EditorGUILayout.EndHorizontal();

                    GUILayout.BeginVertical(GUILayout.MaxWidth(200));

                    
                    if (PoseChanged)
                    {
                        PoseChanged = false;
                        
                        if (activePoseIndex >= 0 && activePoseIndex < allPosesProperty.arraySize) {

                            activePoseProp = allPosesProperty.GetArrayElementAtIndex(activePoseIndex);
                            activePose = (SteamVR_Skeleton_Pose)activePoseProp.objectReferenceValue;
                        }
                    }

                    if (activePoseProp != null) {

                    
                    
                    activePose = (SteamVR_Skeleton_Pose)activePoseProp.objectReferenceValue;
                    if (activePoseProp.objectReferenceValue == null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        activePoseProp.objectReferenceValue = EditorGUILayout.ObjectField(activePoseProp.objectReferenceValue, typeof(SteamVR_Skeleton_Pose), false);
                        if (GUILayout.Button("Create")) 
                            createNew = true;
                        EditorGUILayout.EndHorizontal();
                        if (createNew)
                        {
                            string fullPath = EditorUtility.SaveFilePanelInProject("Create New Skeleton Pose", "newPose", "asset", "Save file");
                            if (string.IsNullOrEmpty(fullPath) == false)
                            {
                                SteamVR_Skeleton_Pose newPose = ScriptableObject.CreateInstance<SteamVR_Skeleton_Pose>();
                                AssetDatabase.CreateAsset(newPose, fullPath);
                                AssetDatabase.SaveAssets();

                                activePoseProp.objectReferenceValue = newPose;
                                serializedObject.ApplyModifiedProperties();
                            }
                        }
                    }
                    else
                    {
                        activePoseProp.objectReferenceValue = EditorGUILayout.ObjectField(activePoseProp.objectReferenceValue, typeof(SteamVR_Skeleton_Pose), false);
                        GUILayout.BeginHorizontal();
                        
                        EditorGUILayout.BeginVertical("box");
                        GUI.color = Color.white;
                        EditorGUIUtility.labelWidth = 24;
                        EditorGUILayout.LabelField("Left Hand", EditorStyles.boldLabel);
                        EditorGUIUtility.labelWidth = 0;
                        DrawHand(activePose.leftHand, activePose.rightHand);//, getLeftFromOpposite);
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical("box");
                        EditorGUIUtility.labelWidth = 24;
                        EditorGUILayout.LabelField("Right Hand", EditorStyles.boldLabel);
                        EditorGUIUtility.labelWidth = 0;
                        DrawHand(activePose.rightHand, activePose.leftHand);//, getRightFromOpposite);
                        EditorGUILayout.EndVertical();
                        
                        GUILayout.EndHorizontal();
                    }
                    }

                    EditorGUILayout.EndVertical();
                
                GUILayout.EndVertical();
            
        }


        string[] blendNames;
        
        int blendPoseIndex;
        void DrawBlendingBehaviourMenu()
        {
            GUILayout.BeginVertical("box");

            
                blendNames = new string[blendingBehaviourArray.arraySize];

                for (int i = 0; i < blendingBehaviourArray.arraySize; i++)
                {
                    // additional poses from array
                    blendNames[i] =( i == 0 ? "[MAIM] " : "") + blendingBehaviourArray.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;// == null ? "[not set]" : skeletonAdditionalPosesProperty.GetArrayElementAtIndex(i).objectReferenceValue.name;
                }
                
                blendPoseIndex = EditorGUILayout.Popup ("BlendedPose:", blendPoseIndex, blendNames);

                    float bright = 0.6f;
                    if (EditorGUIUtility.isProSkin) 
                        bright = 1;
                    if (blendPoseIndex >= 0 && blendPoseIndex < blendingBehaviourArray.arraySize) 
                    {

                    
                    SerializedProperty blender = blendingBehaviourArray.GetArrayElementAtIndex(blendPoseIndex);
                    // SerializedProperty blenderName = blender.FindPropertyRelative("name");
                    SerializedProperty blenderPose = blender.FindPropertyRelative("pose");
                    // SerializedProperty blenderType = blender.FindPropertyRelative("type");
                    
                    // GUILayout.Space(10);
                    

                    GUI.color = new Color(bright, bright, bright);
                    GUILayout.BeginVertical("box");
                    GUI.color = Color.white;

                    EditorGUIUtility.labelWidth = 64;
                    // EditorGUILayout.PropertyField(blenderName);

                    blenderPose.intValue = EditorGUILayout.Popup("Pose", blenderPose.intValue, poseNames);
                        
                    // EditorGUILayout.PropertyField(blenderType);
                    // if (blenderType.intValue == (int)HandPoserController.PoseBlendingBehaviour.BlenderTypes.AnalogAction)
                    // {
                    //     EditorGUI.indentLevel++;
                    //     SerializedProperty blenderAction = blender.FindPropertyRelative("action_single");
                    //     EditorGUILayout.PropertyField(blenderAction);
                    //     EditorGUI.indentLevel--;
                    // }
                    // if (blenderType.intValue == (int)HandPoserController.PoseBlendingBehaviour.BlenderTypes.BooleanAction)
                    // {
                    //     EditorGUI.indentLevel++;
                    //     SerializedProperty blenderAction = blender.FindPropertyRelative("action_bool");
                    //     EditorGUILayout.PropertyField(blenderAction);
                    //     EditorGUI.indentLevel--;
                    // }

                    EditorGUIUtility.labelWidth = 0;

                    GUILayout.EndVertical();
                }


                GUI.color = new Color(bright, bright, bright);
                GUILayout.BeginVertical("box");
                GUI.color = Color.white;
                EditorGUIUtility.labelWidth = 0;


                for (int i = 0; i < blendingBehaviourArray.arraySize; i++)
                {

                    // SerializedProperty blenderName = blendingBehaviourArray.GetArrayElementAtIndex(i).FindPropertyRelative("name");
                    
                    EditorGUILayout.BeginHorizontal();

                    EditorGUIUtility.labelWidth = 0;
                    // EditorGUILayout.PropertyField(blenderName, GUIContent.none);
                    
                    EditorGUI.BeginDisabledGroup(i == 0);
                    if (GUILayout.Button("^", blendButtonStyle, blendbuttonWidth))
                    {
                        blendingBehaviourArray.MoveArrayElement(i, i - 1);
                    }
                    EditorGUI.EndDisabledGroup();

                    EditorGUI.BeginDisabledGroup(i == blendingBehaviourArray.arraySize - 1);
                    if (GUILayout.Button("v", blendButtonStyle, blendbuttonWidth))
                    {
                        blendingBehaviourArray.MoveArrayElement(i, i + 1);
                    }
                    EditorGUI.EndDisabledGroup();

                    GUILayout.Space(6);
                    GUI.color = new Color(0.9f, 0.8f, 0.78f);
                    if (GUILayout.Button("x", blendButtonStyle, blendbuttonWidth))
                    {
                        // if (EditorUtility.DisplayDialog("", "Do you really want to delete this Blend Behaviour?", "Yes", "Cancel"))
                        // {
                            blendingBehaviourArray.DeleteArrayElementAtIndex(i);
                            return;
                        // }
                    }
                    GUI.color = Color.white;
                    EditorGUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();

                if (GUILayout.Button("+", blendButtonStyle, blendbuttonWidth))
                {
                    int i = blendingBehaviourArray.arraySize;
                    blendingBehaviourArray.InsertArrayElementAtIndex(i);
                    blendingBehaviourArray.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue = "New Behaviour";
                    serializedObject.ApplyModifiedProperties();
                 
                }
            // }
            GUILayout.EndVertical();

        }


        void CreateBlendingBehaviorsFromAllPoses () {

            int count = allPosesProperty.arraySize;
            for (int i = 0; i < count; i++) {

                SerializedProperty p = allPosesProperty.GetArrayElementAtIndex(i);
                SteamVR_Skeleton_Pose behavior = (SteamVR_Skeleton_Pose)p.objectReferenceValue;

                int c = blendingBehaviourArray.arraySize;
                blendingBehaviourArray.InsertArrayElementAtIndex(c);
                blendingBehaviourArray.GetArrayElementAtIndex(c).FindPropertyRelative("name").stringValue = behavior.name;
                blendingBehaviourArray.GetArrayElementAtIndex(c).FindPropertyRelative("pose").intValue = i;//+1;
                blendingBehaviourArray.GetArrayElementAtIndex(c).FindPropertyRelative("type").enumValueIndex = 0;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(poserBehavior);
            }
        }

    }
}

