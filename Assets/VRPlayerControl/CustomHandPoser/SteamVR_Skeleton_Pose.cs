﻿
using System;
using UnityEngine;
using Valve.VR;

namespace Valve.VR
{
    public class SteamVR_Skeleton_Pose : ScriptableObject
    {
        public SteamVR_Skeleton_Pose_Hand leftHand = new SteamVR_Skeleton_Pose_Hand(SteamVR_Input_Sources.LeftHand);
        public SteamVR_Skeleton_Pose_Hand rightHand = new SteamVR_Skeleton_Pose_Hand(SteamVR_Input_Sources.RightHand);

        protected const int leftHandInputSource = (int)SteamVR_Input_Sources.LeftHand;
        protected const int rightHandInputSource = (int)SteamVR_Input_Sources.RightHand;

        public SteamVR_Skeleton_Pose_Hand GetHand(int hand)
        {
            if (hand == leftHandInputSource) return leftHand;
            else if (hand == rightHandInputSource) return rightHand;
            return null;
        }

        public SteamVR_Skeleton_Pose_Hand GetHand(SteamVR_Input_Sources hand)
        {
            if (hand == SteamVR_Input_Sources.LeftHand) return leftHand;
            else if (hand == SteamVR_Input_Sources.RightHand) return rightHand;
            return null;
        }
    }

    [Serializable]
    public class SteamVR_Skeleton_Pose_Hand
    {
        public SteamVR_Input_Sources inputSource;

        public SteamVR_Skeleton_FingerExtensionTypes thumbFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Static;
        public SteamVR_Skeleton_FingerExtensionTypes indexFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Static;
        public SteamVR_Skeleton_FingerExtensionTypes middleFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Static;
        public SteamVR_Skeleton_FingerExtensionTypes ringFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Static;
        public SteamVR_Skeleton_FingerExtensionTypes pinkyFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Static;

        public Vector3 position;
        public Quaternion rotation;

        public Vector3[] bonePositions;
        public Quaternion[] boneRotations;

        public SteamVR_Skeleton_Pose_Hand(SteamVR_Input_Sources source)
        {
            inputSource = source;
        }

        public SteamVR_Skeleton_FingerExtensionTypes GetMovementTypeForBone(int boneIndex)
        {
            int fingerIndex = SteamVR_Skeleton_JointIndexes.GetFingerForBone(boneIndex);

            switch (fingerIndex)
            {
                case SteamVR_Skeleton_FingerIndexes.thumb: return thumbFingerMovementType;
                case SteamVR_Skeleton_FingerIndexes.index: return indexFingerMovementType;
                case SteamVR_Skeleton_FingerIndexes.middle: return middleFingerMovementType;
                case SteamVR_Skeleton_FingerIndexes.ring: return ringFingerMovementType;
                case SteamVR_Skeleton_FingerIndexes.pinky: return pinkyFingerMovementType;
            }

            return SteamVR_Skeleton_FingerExtensionTypes.Static;
        }
    }

    public enum SteamVR_Skeleton_FingerExtensionTypes { 
        Static, Free, Extend, Contract,
    }
}