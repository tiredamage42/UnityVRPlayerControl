
using UnityEngine;
using Valve.VR;

using Skeleton = VRPlayer.SteamVR_Behaviour_Skeleton;
using HandType = Valve.VR.SteamVR_Input_Sources;

namespace VRPlayer 
{

    public class HandPoser : MonoBehaviour
    {
       
        public SteamVR_Skeleton_PoseSnapshot blendedSnapshotL;
        public SteamVR_Skeleton_PoseSnapshot blendedSnapshotR;

        PoseBlendingBehaviour[] blendingBehaviours;
        
        bool poseUpdatedThisFrame;
        [HideInInspector] public bool isPosing;

        public float smoothSpeed = 5;

        public void SetBehavior () {
            int boneCount = defaultPose.leftHand.bonePositions.Length;

            blendedSnapshotL = new SteamVR_Skeleton_PoseSnapshot(boneCount, HandType.LeftHand);
            blendedSnapshotR = new SteamVR_Skeleton_PoseSnapshot(boneCount, HandType.RightHand);
        }

        public void EnablePosing () 
        {
            isPosing = true;
        }
        public void DisablePosing ( )
        {
            isPosing = false;
        }

        [HideInInspector] public SteamVR_Skeleton_Pose defaultPose;
        

        void OnEnable () {
            SetBehavior ();

            int c = 3; // default, blend0, blend1

            blendingBehaviours = new PoseBlendingBehaviour[c];
            for (int i = 0; i < c; i++)
            {
                blendingBehaviours[i] = new PoseBlendingBehaviour(defaultPose);

                blendingBehaviours[i].SetValue(0, true);
            }

            blendingBehaviours[0].SetValue(1, true);   
            blendingBehaviours[0].SetPose(defaultPose);

        }

        public bool EnablePose(SteamVR_Skeleton_Pose pose)
        {
            
            if (pose == null || pose == defaultPose) {
                for (int i =0 ; i < blendingBehaviours.Length; i++) {
                    blendingBehaviours[i].SetValue(i == 0 ? 1 : 0, false);
                }
                return true;
            }
            

            PoseBlendingBehaviour t = blendingBehaviours[1];
            blendingBehaviours[1] = blendingBehaviours[2];
            blendingBehaviours[2] = t;

            blendingBehaviours[1].SetValue(0, false);
            blendingBehaviours[2].SetValue(0, true);

            blendingBehaviours[2].SetValue(1, false);
            blendingBehaviours[2].SetPose(pose);

            return true;
        }
        
        private SteamVR_Skeleton_PoseSnapshot GetHandSnapshot(HandType inputSource)
        {
            return inputSource == HandType.LeftHand ? blendedSnapshotL : blendedSnapshotR;
        }

        public SteamVR_Skeleton_PoseSnapshot GetBlendedPose(SteamVR_Action_Skeleton skeletonAction, HandType handType)
        {
            UpdatePose(skeletonAction, handType);
            return GetHandSnapshot(handType);
        }

        public SteamVR_Skeleton_PoseSnapshot GetBlendedPose(Skeleton skeletonBehaviour)
        {
            return GetBlendedPose(skeletonBehaviour.skeletonAction, skeletonBehaviour.inputSource);
        }

        public void UpdatePose(SteamVR_Action_Skeleton skeletonAction, HandType inputSource)
        {
            // only allow this function to run once per frame
            if (poseUpdatedThisFrame) return;

            poseUpdatedThisFrame = true;

            // always do additive animation on main pose
            
            //copy from main pose as a base
            SteamVR_Skeleton_PoseSnapshot snap = GetHandSnapshot(inputSource);
            
            ApplyBlenderBehaviours(skeletonAction, inputSource, snap);

            if (inputSource == HandType.RightHand)
                blendedSnapshotR = snap;
            else if (inputSource == HandType.LeftHand)
                blendedSnapshotL = snap;
        }

        protected void ApplyBlenderBehaviours(SteamVR_Action_Skeleton skeletonAction, HandType inputSource, SteamVR_Skeleton_PoseSnapshot snapshot)
        {

            // apply blending for each behaviour
            for (int behaviourIndex = 0; behaviourIndex < blendingBehaviours.Length; behaviourIndex++)
            {
                PoseBlendingBehaviour blending = blendingBehaviours[behaviourIndex];

                blending.Update(Time.deltaTime, inputSource, smoothSpeed);

                float t = behaviourIndex == 0 ? 1 : blending.GetValue();

                // if disabled or very low influence, skip for perf
                if (t > 0.01f)
                {
                    blending.UpdateAdditiveAnimation(skeletonAction, inputSource);
                    blending.ApplyBlending(snapshot, inputSource, t);
                }
            }
        }

        protected void LateUpdate()
        {
            // let the pose be updated again the next frame
            poseUpdatedThisFrame = false;
        }
       
        public class PoseBlendingBehaviour
        {
            float value = 0;
            public float GetValue () {
                return value;
            }
            public float targetValue;

            public void SetValue (float newValue, bool hardSet) {
                targetValue = newValue;
                if (hardSet)
                    value = targetValue;

            }
            
            public void Update(float deltaTime, HandType inputSource, float smoothSpeed)
            {
                if (smoothSpeed == 0)
                    value = targetValue;
                else
                    value = Mathf.Lerp(value, targetValue, deltaTime * smoothSpeed);
            }

            // public void ApplyBlending(SteamVR_Skeleton_PoseSnapshot snapshot, SkeletonBlendablePose blendPose, HandType inputSource, float t)
            public void ApplyBlending(SteamVR_Skeleton_PoseSnapshot snapshot, HandType inputSource, float t)
            
            {
                // float t = targetBehaviour.influence * value;
                // if (t == 0) 
                //     return;

                
                SteamVR_Skeleton_PoseSnapshot targetSnapshot = GetHandSnapshot(inputSource);
                
                for (int boneIndex = 0; boneIndex < snapshot.bonePositions.Length; boneIndex++)
                {
                    // if ((boneIndex == 1) || (boneIndex == 0))
                    //     continue;
                    
                    if (t >= 1) {
                        snapshot.bonePositions[boneIndex] = targetSnapshot.bonePositions[boneIndex];
                        snapshot.boneRotations[boneIndex] = targetSnapshot.boneRotations[boneIndex];
                    }
                    else {

                        snapshot.bonePositions[boneIndex] = Vector3.Lerp(snapshot.bonePositions[boneIndex], targetSnapshot.bonePositions[boneIndex], t);
                        snapshot.boneRotations[boneIndex] = Quaternion.Slerp(snapshot.boneRotations[boneIndex], targetSnapshot.boneRotations[boneIndex], t);
                    }
                }
                
            }





            public SteamVR_Skeleton_Pose pose;
            public SteamVR_Skeleton_PoseSnapshot snapshotR, snapshotL;
            
            public SteamVR_Skeleton_PoseSnapshot GetHandSnapshot(HandType inputSource)
            {
                return inputSource == HandType.LeftHand ? snapshotL : snapshotR;
            }

            Vector3[] additivePositionBuffer;
            Quaternion[] additiveRotationBuffer;

            public void UpdateAdditiveAnimation(SteamVR_Action_Skeleton skeletonAction, HandType inputSource)
            {
                SteamVR_Skeleton_PoseSnapshot snapshot = GetHandSnapshot(inputSource);
                SteamVR_Skeleton_Pose_Hand poseHand = pose.GetHand(inputSource);

                //setup mirrored pose buffers
                if (additivePositionBuffer == null) additivePositionBuffer = new Vector3[skeletonAction.boneCount];
                if (additiveRotationBuffer == null) additiveRotationBuffer = new Quaternion[skeletonAction.boneCount];


                for (int boneIndex = 0; boneIndex < snapshotL.bonePositions.Length; boneIndex++)
                {
                    // if ((boneIndex == 1) || (boneIndex == 0))
                    //     continue;

                    int fingerIndex = SteamVR_Skeleton_JointIndexes.GetFingerForBone(boneIndex);
                    SteamVR_Skeleton_FingerExtensionTypes extensionType = poseHand.GetMovementTypeForBone(boneIndex);

                    //do target pose mirroring on left hand
                    if(inputSource == HandType.LeftHand)
                    {
                        SteamVR_Behaviour_Skeleton.MirrorBonePosition(ref skeletonAction.bonePositions[boneIndex], ref additivePositionBuffer[boneIndex], boneIndex);
                        SteamVR_Behaviour_Skeleton.MirrorBoneRotation(ref skeletonAction.boneRotations[boneIndex], ref additiveRotationBuffer[boneIndex], boneIndex);
                    }
                    else
                    {
                        additivePositionBuffer[boneIndex] = skeletonAction.bonePositions[boneIndex];
                        additiveRotationBuffer[boneIndex] = skeletonAction.boneRotations[boneIndex];
                    }

                    if (extensionType == SteamVR_Skeleton_FingerExtensionTypes.Free)
                    {
                        snapshot.bonePositions[boneIndex] = additivePositionBuffer[boneIndex];
                        snapshot.boneRotations[boneIndex] = additiveRotationBuffer[boneIndex];
                    }
                    else if (extensionType == SteamVR_Skeleton_FingerExtensionTypes.Extend)
                    {
                        // lerp to open pose by fingercurl
                        snapshot.bonePositions[boneIndex] = Vector3.Lerp(poseHand.bonePositions[boneIndex], additivePositionBuffer[boneIndex], 1 - skeletonAction.fingerCurls[fingerIndex]);
                        snapshot.boneRotations[boneIndex] = Quaternion.Lerp(poseHand.boneRotations[boneIndex], additiveRotationBuffer[boneIndex], 1 - skeletonAction.fingerCurls[fingerIndex]);
                    }
                    else if (extensionType == SteamVR_Skeleton_FingerExtensionTypes.Contract)
                    {
                        // lerp to closed pose by fingercurl
                        snapshot.bonePositions[boneIndex] = Vector3.Lerp(poseHand.bonePositions[boneIndex], additivePositionBuffer[boneIndex], skeletonAction.fingerCurls[fingerIndex]);
                        snapshot.boneRotations[boneIndex] = Quaternion.Lerp(poseHand.boneRotations[boneIndex], additiveRotationBuffer[boneIndex], skeletonAction.fingerCurls[fingerIndex]);
                    }
                }
            }



            public PoseBlendingBehaviour(SteamVR_Skeleton_Pose p) 
            // public SkeletonBlendablePose(SteamVR_Skeleton_Pose p)
            {
                snapshotR = new SteamVR_Skeleton_PoseSnapshot(p.rightHand.bonePositions.Length, HandType.RightHand);
                snapshotL = new SteamVR_Skeleton_PoseSnapshot(p.leftHand.bonePositions.Length, HandType.LeftHand);
            }

            public void SetPose (SteamVR_Skeleton_Pose p) {
                pose = p;
                PoseToSnapshots();
            }
            
            
            void PoseToSnapshots()
            {
                pose.rightHand.bonePositions.CopyTo(snapshotR.bonePositions, 0);
                pose.rightHand.boneRotations.CopyTo(snapshotR.boneRotations, 0);

                pose.leftHand.bonePositions.CopyTo(snapshotL.bonePositions, 0);
                pose.leftHand.boneRotations.CopyTo(snapshotL.boneRotations, 0);
            }
        }
    }

    public class SteamVR_Skeleton_PoseSnapshot
    {
        public HandType inputSource;
        public Vector3[] bonePositions;
        public Quaternion[] boneRotations;

        public SteamVR_Skeleton_PoseSnapshot(int boneCount, HandType source)
        {
            inputSource = source;
            bonePositions = new Vector3[boneCount];
            boneRotations = new Quaternion[boneCount];
        }

        public void CopyFrom(SteamVR_Skeleton_PoseSnapshot source)
        {
            inputSource = source.inputSource;
            for (int i = 0; i < bonePositions.Length; i++)
            {
                bonePositions[i] = source.bonePositions[i];
                boneRotations[i] = source.boneRotations[i];
            }
        }
    }
}
