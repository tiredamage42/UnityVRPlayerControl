

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Valve.VR;

namespace VRPlayer
{

    [CustomEditor(typeof(HandPoser))]
    public class HandPoserEditor : Editor
    {
        const string leftDefaultAssetName = "vr_glove_left_model_slim";
        const string rightDefaultAssetName = "vr_glove_right_model_slim";

        bool showLeftPreview, showRightPreview;
        
        GameObject previewInstanceL, previewInstanceR;
        
        void OnDisable () {
            if (previewInstanceL != null) {
                DestroyImmediate(previewInstanceL);    
            }
            if (previewInstanceR != null) {
                DestroyImmediate(previewInstanceR);    
            }
        }

        float poserScale;
        HandPoser poser;
        bool PoseChanged = false;

        Texture handTexL;
        Texture handTexR;

        // HandPoseSelector handPoseSelector;

        AssetSelector<SteamVR_Skeleton_Pose> handPoseSelector;


        
        protected void OnEnable()
        {            
            poser = (HandPoser)target;

            defaultPoseProp = serializedObject.FindProperty("defaultPose");

            handPoseSelector = new AssetSelector<SteamVR_Skeleton_Pose>(null, null);
        }

        protected void UpdatePreviewHand(ref GameObject preview, ref bool showPreview, string assetName, SteamVR_Skeleton_Pose_Hand handData, SteamVR_Skeleton_Pose sourcePose, bool forceUpdate)
        {
            if (showPreview)
            {
                if (forceUpdate && preview != null)
                {
                    DestroyImmediate(preview);
                }

                if (preview == null)
                {

                    GameObject prefabProperty = null;
                    string[] defPaths = AssetDatabase.FindAssets(string.Format("t:Prefab {0}", assetName));
                    if (defPaths != null && defPaths.Length > 0)
                    {
                        string guid = defPaths[0];
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        prefabProperty = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                        if (prefabProperty == null)
                            Debug.LogError("[SteamVR] Could not load prefab: " + assetName + ". Found path: " + path);
                    }
                    else {
                        Debug.LogError("[SteamVR] Could not load prefab: " + assetName);
                    }
                    
                    preview = GameObject.Instantiate(prefabProperty);
                    preview.transform.localScale = Vector3.one * poserScale;//.floatValue;
                    preview.transform.parent = poser.transform;
                    preview.transform.localPosition = Vector3.zero;
                    preview.transform.localRotation = Quaternion.identity;

                    SteamVR_Behaviour_Skeleton previewSkeleton = null;

                    if (preview != null)
                        previewSkeleton = preview.GetComponent<SteamVR_Behaviour_Skeleton>();

                    if (previewSkeleton != null)
                    {
                        if (handData.bonePositions == null || handData.bonePositions.Length == 0)
                        {
                            SteamVR_Skeleton_Pose poseResource = (SteamVR_Skeleton_Pose)Resources.Load("ReferencePose_OpenHand");
                            DeepCopyPose(poseResource, sourcePose);
                            EditorUtility.SetDirty(sourcePose);
                        }

                        preview.transform.localPosition = Vector3.zero;
                        preview.transform.localRotation = Quaternion.identity;
                        preview.transform.parent = null;
                        preview.transform.localScale = Vector3.one * poserScale;//.floatValue;
                        preview.transform.parent = poser.transform;

                        preview.transform.localRotation = handData.rotation;
                        preview.transform.localPosition = handData.position;


                        for (int boneIndex = 0; boneIndex < handData.bonePositions.Length; boneIndex++)
                        {
                            Transform bone = previewSkeleton.GetBone(boneIndex);
                            bone.localPosition = handData.bonePositions[boneIndex];
                            bone.localRotation = handData.boneRotations[boneIndex];
                        }
                    }
                    SceneView.RepaintAll();
                }
            }
            else
            {
                if (preview != null)
                {
                    DestroyImmediate(preview);
                    SceneView.RepaintAll();
                }
            }
        }

       

        protected EVRSkeletalReferencePose forceToReferencePose = EVRSkeletalReferencePose.OpenHand;

        protected void SaveHandData(SteamVR_Skeleton_Pose_Hand handData, SteamVR_Behaviour_Skeleton thisSkeleton)
        {
            handData.position = thisSkeleton.transform.localPosition;
            handData.rotation = thisSkeleton.transform.localRotation;
            
            handData.bonePositions = new Vector3[SteamVR_Action_Skeleton.numBones];
            handData.boneRotations = new Quaternion[SteamVR_Action_Skeleton.numBones];

            for (int boneIndex = 0; boneIndex < SteamVR_Action_Skeleton.numBones; boneIndex++)
            {
                Transform bone = thisSkeleton.GetBone(boneIndex);
                handData.bonePositions[boneIndex] = bone.localPosition;
                handData.boneRotations[boneIndex] = bone.localRotation;
            }

            EditorUtility.SetDirty(activePose);
        }

        

        protected void DrawPoseControlButtons()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            GameObject leftInstance = previewInstanceL;
            
            leftSkeleton = null;

            if (leftInstance != null)
                leftSkeleton = leftInstance.GetComponent<SteamVR_Behaviour_Skeleton>();

            GameObject rightInstance = previewInstanceR;
            
            rightSkeleton = null;

            if (rightInstance != null)
                rightSkeleton = rightInstance.GetComponent<SteamVR_Behaviour_Skeleton>();


            //only allow saving if a hand is opened for editing
            
            EditorGUI.BeginDisabledGroup(showRightPreview == false && showLeftPreview == false);
            GUI.color = new Color(0.9f, 1.0f, 0.9f);
            // save both hands at once, or whichever are being edited
            bool save = GUILayout.Button(string.Format("Save Pose"));
            if (save)
            {
                if (showRightPreview) SaveHandData(activePose.rightHand, rightSkeleton);
                if (showLeftPreview) SaveHandData(activePose.leftHand, leftSkeleton);
            }
            GUI.color = Color.white;
            EditorGUI.EndDisabledGroup();

            //MIRRORING
            //only allow mirroring if both hands are opened for editing
            EditorGUI.BeginDisabledGroup(showRightPreview == false || showLeftPreview == false);
            if (GUILayout.Button("Import Pose"))
                CopyPoseSelect();
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label("Reference Pose:");
            forceToReferencePose = (EVRSkeletalReferencePose)EditorGUILayout.EnumPopup(forceToReferencePose);
            GUI.color = new Color(1.0f, 0.73f, 0.7f);
            bool forcePose = GUILayout.Button("RESET TO REFERENCE POSE");
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
            
            if (forcePose)
            {
                bool confirm = EditorUtility.DisplayDialog("SteamVR", string.Format("This will overwrite your current skeleton data. (with data from the {0} reference pose)", forceToReferencePose.ToString()), "Overwrite", "Cancel");
                if (confirm)
                {
                    if (forceToReferencePose == EVRSkeletalReferencePose.GripLimit)
                    {
                        // grip limit is controller-specific, the rest use a baked pose
                        if (showLeftPreview) leftSkeleton.ForceToReferencePose(forceToReferencePose);
                        if (showRightPreview) rightSkeleton.ForceToReferencePose(forceToReferencePose);
                    }
                    else
                    {
                        SteamVR_Skeleton_Pose poseResource = null;
                        if (forceToReferencePose == EVRSkeletalReferencePose.OpenHand)
                            poseResource = (SteamVR_Skeleton_Pose)Resources.Load("ReferencePose_OpenHand");
                        if (forceToReferencePose == EVRSkeletalReferencePose.Fist)
                            poseResource = (SteamVR_Skeleton_Pose)Resources.Load("ReferencePose_Fist");
                        if (forceToReferencePose == EVRSkeletalReferencePose.BindPose)
                            poseResource = (SteamVR_Skeleton_Pose)Resources.Load("ReferencePose_BindPose");

                        DeepCopyPose(poseResource, activePose);
                    }
                }
            }


        }


        void CopyPoseSelect()
        {
            string selected = EditorUtility.OpenFilePanel("Open Skeleton Pose ScriptableObject", Application.dataPath, "asset");
            selected = selected.Replace(Application.dataPath, "Assets");

            if (selected == null) return;

            SteamVR_Skeleton_Pose newPose = (SteamVR_Skeleton_Pose)AssetDatabase.LoadAssetAtPath(selected, typeof(SteamVR_Skeleton_Pose));
            if (newPose == null)
            {
                EditorUtility.DisplayDialog("WARNING", "Asset could not be loaded. Is it not a SteamVR_Skeleton_Pose?", "ok");
                return;
            }
            DeepCopyPose(newPose, activePose);
        }


        void DeepCopyPose(SteamVR_Skeleton_Pose source, SteamVR_Skeleton_Pose dest)
        {
            int boneNum = SteamVR_Action_Skeleton.numBones;

            if (dest.rightHand.bonePositions == null) dest.rightHand.bonePositions = new Vector3[boneNum];
            if (dest.rightHand.boneRotations == null) dest.rightHand.boneRotations = new Quaternion[boneNum];

            if (dest.leftHand.bonePositions == null) dest.leftHand.bonePositions = new Vector3[boneNum];
            if (dest.leftHand.boneRotations == null) dest.leftHand.boneRotations = new Quaternion[boneNum];

            EditorUtility.SetDirty(dest);


            // RIGHT HAND COPY

            dest.rightHand.position = source.rightHand.position;
            dest.rightHand.rotation = source.rightHand.rotation;
            for (int boneIndex = 0; boneIndex < boneNum; boneIndex++)
            {
                dest.rightHand.bonePositions[boneIndex] = source.rightHand.bonePositions[boneIndex];
                dest.rightHand.boneRotations[boneIndex] = source.rightHand.boneRotations[boneIndex];
                EditorUtility.DisplayProgressBar("Copying...", "Copying right hand pose", (float)boneIndex / (float)boneNum / 2f);
            }
            dest.rightHand.thumbFingerMovementType = source.rightHand.thumbFingerMovementType;
            dest.rightHand.indexFingerMovementType = source.rightHand.indexFingerMovementType;
            dest.rightHand.middleFingerMovementType = source.rightHand.middleFingerMovementType;
            dest.rightHand.ringFingerMovementType = source.rightHand.ringFingerMovementType;
            dest.rightHand.pinkyFingerMovementType = source.rightHand.pinkyFingerMovementType;

            // LEFT HAND COPY

            dest.leftHand.position = source.leftHand.position;
            dest.leftHand.rotation = source.leftHand.rotation;
            for (int boneIndex = 0; boneIndex < boneNum; boneIndex++)
            {
                dest.leftHand.bonePositions[boneIndex] = source.leftHand.bonePositions[boneIndex];
                dest.leftHand.boneRotations[boneIndex] = source.leftHand.boneRotations[boneIndex];
                EditorUtility.DisplayProgressBar("Copying...", "Copying left hand pose", (float)boneIndex / (float)boneNum / 2f);
            }
            dest.leftHand.thumbFingerMovementType = source.leftHand.thumbFingerMovementType;
            dest.leftHand.indexFingerMovementType = source.leftHand.indexFingerMovementType;
            dest.leftHand.middleFingerMovementType = source.leftHand.middleFingerMovementType;
            dest.leftHand.ringFingerMovementType = source.leftHand.ringFingerMovementType;
            dest.leftHand.pinkyFingerMovementType = source.leftHand.pinkyFingerMovementType;

            EditorUtility.SetDirty(dest);

            forceUpdateHands = true;
            EditorUtility.ClearProgressBar();
        }

        SteamVR_Skeleton_Pose activePose, lastActivePose;

        bool forceUpdateHands = false;
        SerializedProperty defaultPoseProp;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            handPoseSelector.Draw(defaultPoseProp, new GUIContent("Default Pose"));

            DrawPoseEditorMenu();
   
            serializedObject.ApplyModifiedProperties();
        }

        bool getRightFromOpposite = false;
        bool getLeftFromOpposite = false;

        SteamVR_Behaviour_Skeleton leftSkeleton = null;
        SteamVR_Behaviour_Skeleton rightSkeleton = null;

        void DrawPoseEditorMenu()
        {
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Cannot modify pose while in play mode.");
                return;
            }

                //box containing all pose editing controls
                GUILayout.BeginVertical("box");

                //show selectable menu of all poses, highlighting the one that is selected
                EditorGUILayout.Space();
                EditorGUILayout.Space();



                activePose = handPoseSelector.Draw(activePose, new GUIContent("Preview And Edit Pose:"));

                    if (activePose != lastActivePose)
                    {
                        forceUpdateHands = true;
                        PoseChanged = true;
                        lastActivePose = activePose;    
                    }
                    if (PoseChanged)
                    {
                        PoseChanged = false;
                        forceUpdateHands = true;
                    }
                    
                    if (activePose == null)
                    {
                        if (previewInstanceL != null)
                            DestroyImmediate(previewInstanceL);
                        if (previewInstanceR != null)
                            DestroyImmediate(previewInstanceR);
                    }
                    else
                    {
                        
                        DrawPoseControlButtons();

                        UpdatePreviewHand(ref previewInstanceL, ref showLeftPreview, leftDefaultAssetName, activePose.leftHand, activePose, forceUpdateHands);
                        UpdatePreviewHand(ref previewInstanceR, ref showRightPreview, rightDefaultAssetName, activePose.rightHand, activePose, forceUpdateHands);

                        forceUpdateHands = false;

                        if (handTexL == null) handTexL = (Texture)EditorGUIUtility.Load("Assets/VRPlayerControl/CustomHandPoser/Editor/Resources/Icons/HandLeftIcon.png");
                        if (handTexR == null) handTexR = (Texture)EditorGUIUtility.Load("Assets/VRPlayerControl/CustomHandPoser/Editor/Resources/Icons/HandRightIcon.png");

                        GUILayout.BeginHorizontal();
                       
                        DrawHand(activePose, "Left Hand", handTexL, ref showLeftPreview, activePose.leftHand, activePose.rightHand, getLeftFromOpposite, ref getRightFromOpposite, "Copy Left pose to Right hand");
                        DrawHand(activePose, "Right Hand", handTexR, ref showRightPreview, activePose.rightHand, activePose.leftHand, getRightFromOpposite, ref getLeftFromOpposite, "Copy Right pose to Left hand");
                        
                        GUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();

                    DrawPoserPreviewScale();
        }
        protected void DrawHand(SteamVR_Skeleton_Pose poseDrawing, string title, Texture handTex, ref bool showPreview, SteamVR_Skeleton_Pose_Hand handData, SteamVR_Skeleton_Pose_Hand otherData, bool getFromOpposite, ref bool oppositeGetFromOpposite, string copyMessage)
        {
            EditorGUILayout.BeginVertical();
                        
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            
            GUI.color = new Color(1, 1, 1, showPreview ? 1 : 0.25f);
            if (GUILayout.Button(handTex, GUI.skin.label, GUILayout.Width(64), GUILayout.Height(64)))
            {
                showPreview = !showPreview;
                forceUpdateHands = true;
            }
            GUI.color = Color.white;

            EditorGUIUtility.labelWidth = 48;
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUIUtility.labelWidth = 0;

            EditorGUILayout.EndHorizontal();

            bool isLeft = handData.inputSource == SteamVR_Input_Sources.LeftHand;

            SteamVR_Behaviour_Skeleton thisSkeleton = isLeft ? leftSkeleton : rightSkeleton;
            SteamVR_Behaviour_Skeleton oppositeSkeleton = isLeft ? rightSkeleton : leftSkeleton;
            string thisSourceString = isLeft ? "Left Hand" : "Right Hand";
            string oppositeSourceString = !isLeft ? "Left Hand" : "Right Hand";
            EditorGUI.BeginChangeCheck();

            if (showPreview)
            {
                if (getFromOpposite)
                {
                    bool confirm = EditorUtility.DisplayDialog("SteamVR", string.Format("This will overwrite your current {0} skeleton data. (with data from the {1} skeleton)", thisSourceString, oppositeSourceString), "Overwrite", "Cancel");
                    if (confirm)
                    {
                        Vector3 reflectedPosition = new Vector3(-oppositeSkeleton.transform.localPosition.x, oppositeSkeleton.transform.localPosition.y, oppositeSkeleton.transform.localPosition.z);
                        thisSkeleton.transform.localPosition = reflectedPosition;

                        Quaternion oppositeRotation = oppositeSkeleton.transform.localRotation;
                        Quaternion reflectedRotation = new Quaternion(-oppositeRotation.x, oppositeRotation.y, oppositeRotation.z, -oppositeRotation.w);
                        thisSkeleton.transform.localRotation = reflectedRotation;

                        for (int boneIndex = 0; boneIndex < SteamVR_Action_Skeleton.numBones; boneIndex++)
                        {
                            Transform boneThis = thisSkeleton.GetBone(boneIndex);
                            Transform boneOpposite = oppositeSkeleton.GetBone(boneIndex);

                            boneThis.localPosition = boneOpposite.localPosition;
                            boneThis.localRotation = boneOpposite.localRotation;
                        }

                        handData.thumbFingerMovementType = otherData.thumbFingerMovementType;
                        handData.indexFingerMovementType = otherData.indexFingerMovementType;
                        handData.middleFingerMovementType = otherData.middleFingerMovementType;
                        handData.ringFingerMovementType = otherData.ringFingerMovementType;
                        handData.pinkyFingerMovementType = otherData.pinkyFingerMovementType;
                    }
                }
            }

            EditorGUIUtility.labelWidth = 120;
            handData.thumbFingerMovementType = (SteamVR_Skeleton_FingerExtensionTypes)EditorGUILayout.EnumPopup("Thumb movement", handData.thumbFingerMovementType);
            handData.indexFingerMovementType = (SteamVR_Skeleton_FingerExtensionTypes)EditorGUILayout.EnumPopup("Index movement", handData.indexFingerMovementType);
            handData.middleFingerMovementType = (SteamVR_Skeleton_FingerExtensionTypes)EditorGUILayout.EnumPopup("Middle movement", handData.middleFingerMovementType);
            handData.ringFingerMovementType = (SteamVR_Skeleton_FingerExtensionTypes)EditorGUILayout.EnumPopup("Ring movement", handData.ringFingerMovementType);
            handData.pinkyFingerMovementType = (SteamVR_Skeleton_FingerExtensionTypes)EditorGUILayout.EnumPopup("Pinky movement", handData.pinkyFingerMovementType);
            EditorGUIUtility.labelWidth = 0;

            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(poseDrawing);//poser.behavior);
            }

            EditorGUILayout.EndVertical();
            EditorGUI.BeginDisabledGroup((showLeftPreview && showRightPreview) == false);
            oppositeGetFromOpposite = GUILayout.Button(copyMessage);//"Copy Left pose to Right hand");
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();

        }
                


        void DrawPoserPreviewScale () {
            EditorGUI.BeginChangeCheck();
            EditorGUIUtility.labelWidth = 120;
            poserScale = EditorGUILayout.FloatField("Preview Pose Scale", poserScale);
            if (poserScale <= 0) poserScale = 1;                    
            EditorGUIUtility.labelWidth = 0;

            if (EditorGUI.EndChangeCheck())
            {
                forceUpdateHands = true;
            }
        }
    }

}
