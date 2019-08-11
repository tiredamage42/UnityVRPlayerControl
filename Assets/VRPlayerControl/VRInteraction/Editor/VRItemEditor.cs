using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

using UnityEditor;
namespace VRPlayer {

    [CustomEditor(typeof(VRItemAddon))][CanEditMultipleObjects]

    public class VRItemEditor : Editor
    {



        

        // SteamVR_Skeleton_Pose[] allPoses;
        // string[] allPoseNames;

        SerializedProperty usePoseProp;

        AssetSelector<SteamVR_Skeleton_Pose> handPoseSelector;

        


        void OnEnable () {
            // FindAllPoses();
            usePoseProp = serializedObject.FindProperty("poseToUse");
            handPoseSelector = new AssetSelector<SteamVR_Skeleton_Pose>(null, null);
        }
        public override void OnInspectorGUI () {
            base.OnInspectorGUI () ;

            handPoseSelector.Draw(usePoseProp, new GUIContent("Pose"));


            // usePoseProp.objectReferenceValue = DrawPoseSelect((SteamVR_Skeleton_Pose) usePoseProp.objectReferenceValue);

            serializedObject.ApplyModifiedProperties();


        }

        // public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
        // {
        //     List<T> assets = new List<T>();
        //     string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
        //     for( int i = 0; i < guids.Length; i++ )
        //     {
        //         string assetPath = AssetDatabase.GUIDToAssetPath( guids[i] );
        //         T asset = AssetDatabase.LoadAssetAtPath<T>( assetPath );
        //         if( asset != null )
        //         {
        //             assets.Add(asset);
        //         }
        //     }
        //     return assets;
        // }

        // void FindAllPoses () {
        //     allPoses = FindAssetsByType<SteamVR_Skeleton_Pose>().ToArray();
        //     allPoseNames = new string[allPoses.Length];

        //     for (int i = 0; i < allPoses.Length; i++) {
        //         allPoseNames[i] = allPoses[i].name;
        //     }
        // }

        // SteamVR_Skeleton_Pose DrawPoseSelect (SteamVR_Skeleton_Pose currentPose) {

        //     int activePoseIndex = -1;

        //     for (int i =0 ; i < allPoses.Length; i++) {
        //         if (allPoses[i] == currentPose) {
        //             activePoseIndex = i;
        //             break;
        //         }
        //     }

        //     // if (activePoseIndex == -1) {
        //         // return null;
        //     // }


        //     int poseSelected = EditorGUILayout.Popup ("Pose:", activePoseIndex, allPoseNames);
            
        //     if (poseSelected < 0)
        //         return null;
                
        //     return allPoses[poseSelected];

        // }
        
    }
}
