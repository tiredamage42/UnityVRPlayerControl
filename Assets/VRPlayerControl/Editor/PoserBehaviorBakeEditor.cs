using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System;
using System.Text;
// using System.Linq;

using Valve.VR;
namespace VRPlayer
{

    [CustomEditor(typeof(PoserBehaviorBake))]
    public class PoserBehaviorBakeEditor : Editor
    {

        PoserBehaviorBake baker;


        protected void OnEnable()
        {

            baker = (PoserBehaviorBake)target;

        }



        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            DrawOptions();
        }


        void SetHandToPoserBehavior () {
            getWorkingHand.BlendToPoser(baker.GetComponent<SteamVR_Skeleton_Poser>(), 1);
        }





        string fileName = "New Pose";

        EVRSkeletalReferencePose referencePose;
        // SteamVR_Input_Sources handToUse = SteamVR_Input_Sources.LeftHand;


        SteamVR_Behaviour_Skeleton getWorkingHand {
            get {
                return Player.instance.GetHand(baker.handToUse).skeleton;
            }
        }
        SteamVR_Behaviour_Skeleton getOppositeHand {
            get {
                return Player.instance.GetHand(baker.handToUse == SteamVR_Input_Sources.LeftHand ? SteamVR_Input_Sources.RightHand : SteamVR_Input_Sources.LeftHand).skeleton;
            }
        }

        void DrawOptions () {
            // handToUse = (SteamVR_Input_Sources)EditorGUILayout.EnumPopup("Hand To Use", baker.handToUse);
            EditorGUILayout.Space();
            referencePose = (EVRSkeletalReferencePose)EditorGUILayout.EnumPopup("Reference Pose", referencePose);
            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button("Set To Pose")) {
                ResetToReferencePose();
            }
            EditorGUILayout.Space();
            GUI.enabled = true;
            fileName = EditorGUILayout.TextField("New Pose Name: ", fileName);
            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button("Make Pose")) {
                CopyHandsToPose();
            }
            GUI.enabled = true;

            
            EditorGUILayout.Space();
            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button("Set Hand To Poser Pose")) {
            
                SetHandToPoserBehavior();
            }
            GUI.enabled = true;

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

            EditorUtility.ClearProgressBar();
        }

        void CopyHand (SteamVR_Skeleton_Pose pose, SteamVR_Skeleton_Pose_Hand source, SteamVR_Skeleton_Pose_Hand dest) {
               int boneNum = SteamVR_Action_Skeleton.numBones;

            if (dest.bonePositions == null) dest.bonePositions = new Vector3[boneNum];
            if (dest.boneRotations == null) dest.boneRotations = new Quaternion[boneNum];

            
            dest.position = new Vector3(-source.position.x, source.position.y, source.position.z);
            dest.rotation = new Quaternion(-source.rotation.x, source.rotation.y, source.rotation.z, -source.rotation.w);
            
            for (int boneIndex = 0; boneIndex < SteamVR_Action_Skeleton.numBones; boneIndex++)
            {
                dest.bonePositions[boneIndex] = source.bonePositions[boneIndex];
                dest.boneRotations[boneIndex] = source.boneRotations[boneIndex];
                EditorUtility.DisplayProgressBar("Copying...", "Copying right hand pose", (float)boneIndex / (float)boneNum / 2f);
            }


            dest.thumbFingerMovementType = source.thumbFingerMovementType;
            dest.indexFingerMovementType = source.indexFingerMovementType;
            dest.middleFingerMovementType = source.middleFingerMovementType;
            dest.ringFingerMovementType = source.ringFingerMovementType;
            dest.pinkyFingerMovementType = source.pinkyFingerMovementType;

            //set ours dirty
            
            EditorUtility.SetDirty(pose);




            

        }


        /*
        
        SteamVR_Skeleton_Pose poseResource = null;
                        if (forceToReferencePose == EVRSkeletalReferencePose.OpenHand)
                            poseResource = (SteamVR_Skeleton_Pose)Resources.Load("ReferencePose_OpenHand");
                        if (forceToReferencePose == EVRSkeletalReferencePose.Fist)
                            poseResource = (SteamVR_Skeleton_Pose)Resources.Load("ReferencePose_Fist");
                        if (forceToReferencePose == EVRSkeletalReferencePose.BindPose)
                            poseResource = (SteamVR_Skeleton_Pose)Resources.Load("ReferencePose_BindPose");









if (getFromOpposite)
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

                        //set ours dirty
                        
                        // EditorUtility.SetDirty(poser.skeletonMainPose);
                    
                }






         */




        


        SteamVR_Skeleton_Pose MakeNewPose (string fileName) {
            SteamVR_Skeleton_Pose newPose = ScriptableObject.CreateInstance<SteamVR_Skeleton_Pose>();

            SteamVR_Skeleton_Pose poseResource = (SteamVR_Skeleton_Pose)Resources.Load("ReferencePose_OpenHand");
            DeepCopyPose(poseResource, newPose);
            AssetDatabase.CreateAsset(newPose, "Assets/" + fileName);
            
            AssetDatabase.SaveAssets();
            
            
            EditorUtility.SetDirty(newPose);
            return newPose;
        }
        

        void ResetToReferencePose ( ) {
            getWorkingHand.ForceToReferencePose(referencePose);

        }

        

        void CopyHandsToPose () {

                // SaveHandData(activePose.rightHand, rightSkeleton);
                // // if (showLeftPreviewProperty.boolValue)
                //     SaveHandData(activePose.leftHand, leftSkeleton);
            
            SteamVR_Skeleton_Pose pose = MakeNewPose(fileName);
            SaveHandData (pose.GetHand(baker.handToUse), getWorkingHand);
            CopyHand ( pose,  pose.GetHand(baker.handToUse),  pose.GetHand(getOppositeHand.inputSource));
            



            // for (int i =0 ; i < Player.instance.hands.Length; i++) {
            //     SteamVR_Behaviour_Skeleton hand = Player.instance.hands[i].GetComponentInChildren<SteamVR_Behaviour_Skeleton>();
            //     SaveHandData(pose.GetHand(hand.inputSource), hand);
            //     // CopyHandToPose( hand, pose);

                
            // }
        }

        protected void SaveHandData(SteamVR_Skeleton_Pose_Hand handData, SteamVR_Behaviour_Skeleton hand)
        {
            // handData.position = hand.transform.InverseTransformPoint(baker.transform.position);
            //handData.position = hand.transform.localPosition;
            
            // handData.rotation = Quaternion.Inverse(hand.transform.localRotation);

            handData.bonePositions = new Vector3[SteamVR_Action_Skeleton.numBones];
            handData.boneRotations = new Quaternion[SteamVR_Action_Skeleton.numBones];

            for (int i = 0; i < SteamVR_Action_Skeleton.numBones; i++)
            {
                Transform bone = hand.GetBone(i);
                handData.bonePositions[i] = bone.localPosition;
                handData.boneRotations[i] = bone.localRotation;
            }

            // EditorUtility.SetDirty(activePose);
        }


        // void CopyHandToPose(SteamVR_Behaviour_Skeleton hand, SteamVR_Skeleton_Pose pose)
        // {
        //     SteamVR_Skeleton_Pose_Hand handData = pose.GetHand(hand.inputSource);
        //     hand.CopyBonePositions(handData.bonePositions);
        //     hand.CopyBoneRotations(handData.boneRotations);
        // }




    }
}

