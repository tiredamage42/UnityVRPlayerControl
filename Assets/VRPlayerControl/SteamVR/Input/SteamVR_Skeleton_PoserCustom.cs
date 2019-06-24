
using System;
using System.Collections;
using UnityEngine;
using Valve.VR;
using System.Collections.Generic;
using System.Linq;

namespace Valve.VR
{
    public class SteamVR_Skeleton_PoserCustom : MonoBehaviour
    {
        // public SteamVR_Skeleton_Pose skeletonMainPose;
        // public List<SteamVR_Skeleton_Pose> skeletonAdditionalPoses = new List<SteamVR_Skeleton_Pose>();
        // public List<PoseBlendingBehaviour> blendingBehaviours = new List<PoseBlendingBehaviour>();

        
        public int blendPoseCount { get { return blendPoses.Length; } }


        public SteamVR_Skeleton_PoseSnapshot blendedSnapshotL;
        public SteamVR_Skeleton_PoseSnapshot blendedSnapshotR;

        private SkeletonBlendablePose[] blendPoses;

        private int boneCount;
        
        private bool poseUpdatedThisFrame;

        List<PoseBlendingBehaviour> blendingBehaviours = new List<PoseBlendingBehaviour>();

        public bool isPosing;

        // protected void Awake()
        public void SetUpWithPoser (SteamVR_Skeleton_Poser originalPoser)
        {
            blendPoses = new SkeletonBlendablePose[originalPoser.skeletonAdditionalPoses.Count + 1];
            for (int i = 0; i < blendPoseCount; i++)
            {
                blendPoses[i] = new SkeletonBlendablePose(GetPoseByIndex(originalPoser, i));
                blendPoses[i].PoseToSnapshots();
            }


            blendingBehaviours.Clear();
            for (int i =0 ; i < originalPoser.blendingBehaviours.Count; i++) {
                blendingBehaviours.Add (new PoseBlendingBehaviour(originalPoser.blendingBehaviours[i]));
            }

            // NOTE: Is there a better way to get the bone count? idk
            boneCount = originalPoser.skeletonMainPose.leftHand.bonePositions.Length;

            blendedSnapshotL = new SteamVR_Skeleton_PoseSnapshot(boneCount, SteamVR_Input_Sources.LeftHand);
            blendedSnapshotR = new SteamVR_Skeleton_PoseSnapshot(boneCount, SteamVR_Input_Sources.RightHand);
            
            isPosing = true;
        }
        public void DoneWithPoser ( )
        {
            isPosing = false;
        }
        public SteamVR_Skeleton_Pose GetPoseByIndex(SteamVR_Skeleton_Poser originalPoser, int index)
        {
            if (index == 0) { return originalPoser.skeletonMainPose; }
            else { return originalPoser.skeletonAdditionalPoses[index - 1]; }
        }




        /// <summary>
        /// Set the blending value of a blendingBehaviour. Works best on Manual type behaviours.
        /// </summary>
        public void SetBlendingBehaviourValue(string behaviourName, float value)
        {
            PoseBlendingBehaviour behaviour = blendingBehaviours.Find(b => b.targetBehaviour.name == behaviourName);
            if(behaviour == null)
            {
                Debug.LogError("[SteamVR] Blending Behaviour: " + behaviourName + " not found on Skeleton Poser: " + gameObject.name);
                return;
            }
            if(behaviour.targetBehaviour.type != SteamVR_Skeleton_Poser.PoseBlendingBehaviour.BlenderTypes.Manual)
            {
                Debug.LogWarning("[SteamVR] Blending Behaviour: " + behaviourName + " is not a manual behaviour. Its value will likely be overriden.");
            }
            behaviour.value = value;
        }
        /// <summary>
        /// Get the blending value of a blendingBehaviour.
        /// </summary>
        public float GetBlendingBehaviourValue(string behaviourName)
        {
            PoseBlendingBehaviour behaviour = blendingBehaviours.Find(b => b.targetBehaviour.name == behaviourName);
            if (behaviour == null)
            {
                Debug.LogError("[SteamVR] Blending Behaviour: " + behaviourName + " not found on Skeleton Poser: " + gameObject.name);
                return 0;
            }
            return behaviour.value;
        }

        /// <summary>
        /// Enable or disable a blending behaviour.
        /// </summary>
        public void SetBlendingBehaviourEnabled(string behaviourName, bool value)
        {
            PoseBlendingBehaviour behaviour = blendingBehaviours.Find(b => b.targetBehaviour.name == behaviourName);
            if (behaviour == null)
            {
                Debug.LogError("[SteamVR] Blending Behaviour: " + behaviourName + " not found on Skeleton Poser: " + gameObject.name);
                return;
            }
            behaviour.enabled = value;
        }
        /// <summary>
        /// Check if a blending behaviour is enabled.
        /// </summary>
        /// <param name="behaviourName"></param>
        /// <returns></returns>
        public bool GetBlendingBehaviourEnabled(string behaviourName)
        {
            PoseBlendingBehaviour behaviour = blendingBehaviours.Find(b => b.targetBehaviour.name == behaviourName);
            if (behaviour == null)
            {
                Debug.LogError("[SteamVR] Blending Behaviour: " + behaviourName + " not found on Skeleton Poser: " + gameObject.name);
                return false;
            }
            return behaviour.enabled;
        }
        /// <summary>
        /// Get a blending behaviour by name.
        /// </summary>
        public PoseBlendingBehaviour GetBlendingBehaviour(string behaviourName)
        {
            PoseBlendingBehaviour behaviour = blendingBehaviours.Find(b => b.targetBehaviour.name == behaviourName);
            if (behaviour == null)
            {
                Debug.LogError("[SteamVR] Blending Behaviour: " + behaviourName + " not found on Skeleton Poser: " + gameObject.name);
                return null;
            }
            return behaviour;
        }




        
        private SteamVR_Skeleton_PoseSnapshot GetHandSnapshot(SteamVR_Input_Sources inputSource)
        {
            if (inputSource == SteamVR_Input_Sources.LeftHand)
                return blendedSnapshotL;
            else
                return blendedSnapshotR;
        }

        /// <summary>
        /// Retrieve the final animated pose, to be applied to a hand skeleton
        /// </summary>
        /// <param name="forAction">The skeleton action you want to blend between</param>
        /// <param name="handType">If this is for the left or right hand</param>
        public SteamVR_Skeleton_PoseSnapshot GetBlendedPose(SteamVR_Action_Skeleton skeletonAction, SteamVR_Input_Sources handType)
        {
            UpdatePose(skeletonAction, handType);
            return GetHandSnapshot(handType);
        }

        /// <summary>
        /// Retrieve the final animated pose, to be applied to a hand skeleton
        /// </summary>
        /// <param name="skeletonBehaviour">The skeleton behaviour you want to get the action/input source from to blend between</param>
        public SteamVR_Skeleton_PoseSnapshot GetBlendedPose(SteamVR_Behaviour_Skeleton skeletonBehaviour)
        {
            return GetBlendedPose(skeletonBehaviour.skeletonAction, skeletonBehaviour.inputSource);
        }


        /// <summary>
        /// Updates all pose animation and blending. Can be called from different places without performance concerns, as it will only let itself run once per frame.
        /// </summary>
        public void UpdatePose(SteamVR_Action_Skeleton skeletonAction, SteamVR_Input_Sources inputSource)
        {
            // only allow this function to run once per frame
            if (poseUpdatedThisFrame) return;

            poseUpdatedThisFrame = true;

            // always do additive animation on main pose
            blendPoses[0].UpdateAdditiveAnimation(skeletonAction, inputSource);

            //copy from main pose as a base
            SteamVR_Skeleton_PoseSnapshot snap = GetHandSnapshot(inputSource);
            snap.CopyFrom(blendPoses[0].GetHandSnapshot(inputSource));

            ApplyBlenderBehaviours(skeletonAction, inputSource, snap);


            if (inputSource == SteamVR_Input_Sources.RightHand)
                blendedSnapshotR = snap;
            else if (inputSource == SteamVR_Input_Sources.LeftHand)
                blendedSnapshotL = snap;
        }

        protected void ApplyBlenderBehaviours(SteamVR_Action_Skeleton skeletonAction, SteamVR_Input_Sources inputSource, SteamVR_Skeleton_PoseSnapshot snapshot)
        {

            // apply blending for each behaviour
            for (int behaviourIndex = 0; behaviourIndex < blendingBehaviours.Count; behaviourIndex++)
            {
                blendingBehaviours[behaviourIndex].Update(Time.deltaTime, inputSource);
                // if disabled or very low influence, skip for perf
                if (blendingBehaviours[behaviourIndex].enabled && blendingBehaviours[behaviourIndex].targetBehaviour.influence * blendingBehaviours[behaviourIndex].value > 0.01f)
                {
                    if (blendingBehaviours[behaviourIndex].targetBehaviour.pose != 0)
                    {
                        // update additive animation only as needed
                        blendPoses[blendingBehaviours[behaviourIndex].targetBehaviour.pose].UpdateAdditiveAnimation(skeletonAction, inputSource);
                    }

                    blendingBehaviours[behaviourIndex].ApplyBlending(snapshot, blendPoses, inputSource);
                }
            }

        }

        protected void LateUpdate()
        {
            // let the pose be updated again the next frame
            poseUpdatedThisFrame = false;
        }
        
        /// <summary>Weighted average of n vector3s</summary>
        protected Vector3 BlendVectors(Vector3[] vectors, float[] weights)
        {
            Vector3 blendedVector = Vector3.zero;
            for (int i = 0; i < vectors.Length; i++)
            {
                blendedVector += vectors[i] * weights[i];
            }
            return blendedVector;
        }

        /// <summary>Weighted average of n quaternions</summary>
        protected Quaternion BlendQuaternions(Quaternion[] quaternions, float[] weights)
        {
            Quaternion outquat = Quaternion.identity;
            for (int i = 0; i < quaternions.Length; i++)
            {
                outquat *= Quaternion.Slerp(Quaternion.identity, quaternions[i], weights[i]);
            }
            return outquat;
        }

        /// <summary>
        /// A SkeletonBlendablePose holds a reference to a Skeleton_Pose scriptableObject, and also contains some helper functions. 
        /// Also handles pose-specific animation like additive finger motion.
        /// </summary>
        public class SkeletonBlendablePose
        {
            public SteamVR_Skeleton_Pose pose;
            public SteamVR_Skeleton_PoseSnapshot snapshotR;
            public SteamVR_Skeleton_PoseSnapshot snapshotL;

            /// <summary>
            /// Get the snapshot of this pose with effects such as additive finger animation applied.
            /// </summary>
            public SteamVR_Skeleton_PoseSnapshot GetHandSnapshot(SteamVR_Input_Sources inputSource)
            {
                if (inputSource == SteamVR_Input_Sources.LeftHand)
                {
                    return snapshotL;
                }
                else
                {
                    return snapshotR;
                }
            }

            //buffers for mirrored poses
            private Vector3[] additivePositionBuffer;
            private Quaternion[] additiveRotationBuffer;

            public void UpdateAdditiveAnimation(SteamVR_Action_Skeleton skeletonAction, SteamVR_Input_Sources inputSource)
            {
                SteamVR_Skeleton_PoseSnapshot snapshot = GetHandSnapshot(inputSource);
                SteamVR_Skeleton_Pose_Hand poseHand = pose.GetHand(inputSource);

                //setup mirrored pose buffers
                if (additivePositionBuffer == null) additivePositionBuffer = new Vector3[skeletonAction.boneCount];
                if (additiveRotationBuffer == null) additiveRotationBuffer = new Quaternion[skeletonAction.boneCount];


                for (int boneIndex = 0; boneIndex < snapshotL.bonePositions.Length; boneIndex++)
                {
                    int fingerIndex = SteamVR_Skeleton_JointIndexes.GetFingerForBone(boneIndex);
                    SteamVR_Skeleton_FingerExtensionTypes extensionType = poseHand.GetMovementTypeForBone(boneIndex);

                    //do target pose mirroring on left hand
                    if(inputSource == SteamVR_Input_Sources.LeftHand)
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

















            /// <summary>
            /// Init based on an existing Skeleton_Pose
            /// </summary>
            public SkeletonBlendablePose(SteamVR_Skeleton_Pose p)
            {
                pose = p;
                snapshotR = new SteamVR_Skeleton_PoseSnapshot(p.rightHand.bonePositions.Length, SteamVR_Input_Sources.RightHand);
                snapshotL = new SteamVR_Skeleton_PoseSnapshot(p.leftHand.bonePositions.Length, SteamVR_Input_Sources.LeftHand);
            }

            /// <summary>
            /// Copy the base pose into the snapshots.
            /// </summary>
            public void PoseToSnapshots()
            {
                snapshotR.position = pose.rightHand.position;
                snapshotR.rotation = pose.rightHand.rotation;
                pose.rightHand.bonePositions.CopyTo(snapshotR.bonePositions, 0);
                pose.rightHand.boneRotations.CopyTo(snapshotR.boneRotations, 0);

                snapshotL.position = pose.leftHand.position;
                snapshotL.rotation = pose.leftHand.rotation;
                pose.leftHand.bonePositions.CopyTo(snapshotL.bonePositions, 0);
                pose.leftHand.boneRotations.CopyTo(snapshotL.boneRotations, 0);
            }

            public SkeletonBlendablePose() { }
        }

        /// <summary>
        /// A filter applied to the base pose. Blends to a secondary pose by a certain weight. Can be masked per-finger
        /// </summary>
        [System.Serializable]
        public class PoseBlendingBehaviour
        {
            public SteamVR_Skeleton_Poser.PoseBlendingBehaviour targetBehaviour;

            public PoseBlendingBehaviour(SteamVR_Skeleton_Poser.PoseBlendingBehaviour targetBehaviour)
            {
                this.targetBehaviour = targetBehaviour;
                enabled = true;
            }

            public bool enabled = true;
            public float value = 0;
            
            /// <summary>
            /// Performs smoothing based on deltaTime parameter.
            /// </summary>
            public void Update(float deltaTime, SteamVR_Input_Sources inputSource)
            {
                if (targetBehaviour.type == SteamVR_Skeleton_Poser.PoseBlendingBehaviour.BlenderTypes.AnalogAction)
                {
                    if (targetBehaviour.smoothingSpeed == 0)
                        value = targetBehaviour.action_single.GetAxis(inputSource);
                    else
                        value = Mathf.Lerp(value, targetBehaviour.action_single.GetAxis(inputSource), deltaTime * targetBehaviour.smoothingSpeed);
                }
                if (targetBehaviour.type == SteamVR_Skeleton_Poser.PoseBlendingBehaviour.BlenderTypes.BooleanAction)
                {
                    if (targetBehaviour.smoothingSpeed == 0)
                        value = targetBehaviour.action_bool.GetState(inputSource) ? 1 : 0;
                    else
                        value = Mathf.Lerp(value, targetBehaviour.action_bool.GetState(inputSource) ? 1 : 0, deltaTime * targetBehaviour.smoothingSpeed);
                }
            }

            public void ApplyBlending(SteamVR_Skeleton_PoseSnapshot snapshot, SkeletonBlendablePose[] blendPoses, SteamVR_Input_Sources inputSource)
            {
                // targetBehaviour.ApplyBlending(snapshot, blendPoses, inputSource, value);

                SteamVR_Skeleton_PoseSnapshot targetSnapshot = blendPoses[targetBehaviour.pose].GetHandSnapshot(inputSource);
                if (targetBehaviour.mask.GetFinger(0) || targetBehaviour.useMask == false)
                {
                    snapshot.position = Vector3.Lerp(snapshot.position, targetSnapshot.position, targetBehaviour.influence * value);
                    snapshot.rotation = Quaternion.Slerp(snapshot.rotation, targetSnapshot.rotation, targetBehaviour.influence * value);
                }

                for (int boneIndex = 0; boneIndex < snapshot.bonePositions.Length; boneIndex++)
                {
                    // verify the current finger is enabled in the mask, or if no mask is used.
                    if (targetBehaviour.mask.GetFinger(SteamVR_Skeleton_JointIndexes.GetFingerForBone(boneIndex) + 1) || targetBehaviour.useMask == false)
                    {
                        snapshot.bonePositions[boneIndex] = Vector3.Lerp(snapshot.bonePositions[boneIndex], targetSnapshot.bonePositions[boneIndex], targetBehaviour.influence * value);
                        snapshot.boneRotations[boneIndex] = Quaternion.Slerp(snapshot.boneRotations[boneIndex], targetSnapshot.boneRotations[boneIndex], targetBehaviour.influence * value);
                    }
                }
                
            }

            
            
        }
    }

    /// <summary>
    /// PoseSnapshots hold a skeleton pose for one hand, as well as storing which hand they contain. 
    /// They have several functions for combining BlendablePoses.
    /// </summary>
    public class SteamVR_Skeleton_PoseSnapshot
    {
        public SteamVR_Input_Sources inputSource;

        public Vector3 position;
        public Quaternion rotation;

        public Vector3[] bonePositions;
        public Quaternion[] boneRotations;

        public SteamVR_Skeleton_PoseSnapshot(int boneCount, SteamVR_Input_Sources source)
        {
            inputSource = source;
            bonePositions = new Vector3[boneCount];
            boneRotations = new Quaternion[boneCount];
            position = Vector3.zero;
            rotation = Quaternion.identity;
        }

        /// <summary>
        /// Perform a deep copy from one poseSnapshot to another.
        /// </summary>
        public void CopyFrom(SteamVR_Skeleton_PoseSnapshot source)
        {
            inputSource = source.inputSource;
            position = source.position;
            rotation = source.rotation;
            for (int i = 0; i < bonePositions.Length; i++)
            {
                bonePositions[i] = source.bonePositions[i];
                boneRotations[i] = source.boneRotations[i];
            }
        }
    }

}
