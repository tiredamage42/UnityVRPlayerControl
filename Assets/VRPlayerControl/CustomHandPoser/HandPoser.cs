
using System;
using System.Collections;
using UnityEngine;
using Valve.VR;
using System.Collections.Generic;
using System.Linq;


// using Skeleton = Valve.VR.SteamVR_Behaviour_Skeleton;
using Skeleton = VRPlayer.SteamVR_Behaviour_Skeleton;

using HandType = Valve.VR.SteamVR_Input_Sources;
// using Valve.VR;

namespace VRPlayer //Valve.VR
{

    public class HandPoser : MonoBehaviour
    {
        /*
            
            
        */
        
        
        // public SteamVR_Skeleton_Pose skeletonMainPose;
        // public List<SteamVR_Skeleton_Pose> skeletonAdditionalPoses = new List<SteamVR_Skeleton_Pose>();
        // public List<PoseBlendingBehaviour> blendingBehaviours = new List<PoseBlendingBehaviour>();

        // public HandPoserController behavior;
        
        // public int blendPoseCount { get { return blendPoses.Length; } }


        public SteamVR_Skeleton_PoseSnapshot blendedSnapshotL;
        public SteamVR_Skeleton_PoseSnapshot blendedSnapshotR;

        private SkeletonBlendablePose[] blendPoses;
        PoseBlendingBehaviour[] blendingBehaviours;


        // private int boneCount;
        
        private bool poseUpdatedThisFrame;

        // List<PoseBlendingBehaviour> blendingBehaviours = new List<PoseBlendingBehaviour>();

        [HideInInspector] public bool isPosing;

        public float smoothSpeed = 5;



        public void SetBehavior (){//HandPoserController behavior) {
            // this.behavior = behavior;

            
            // blendPoses = new SkeletonBlendablePose[behavior.skeletonAdditionalPoses.Count + 1];
            // blendPoses = new SkeletonBlendablePose[behavior.allPoses.Length];
            
            // for (int i = 0; i < blendPoseCount; i++)
            // {
            //     blendPoses[i] = new SkeletonBlendablePose(GetPoseByIndex(behavior, i));
            //     // blendPoses[i].PoseToSnapshots();
            // }
            
            // blendingBehaviours.Clear();
            // for (int i =0 ; i < behavior.blendingBehaviours.Count; i++) {
            //     blendingBehaviours.Add (new PoseBlendingBehaviour(behavior.blendingBehaviours[i]));
            // }

            // NOTE: Is there a better way to get the bone count? idk
            // boneCount = behavior.skeletonMainPose.leftHand.bonePositions.Length;
            // boneCount = behavior.allPoses[0].leftHand.bonePositions.Length;
            int boneCount = defaultPose.leftHand.bonePositions.Length;

            blendedSnapshotL = new SteamVR_Skeleton_PoseSnapshot(boneCount, HandType.LeftHand);
            blendedSnapshotR = new SteamVR_Skeleton_PoseSnapshot(boneCount, HandType.RightHand);
        }

        void Awake () {
            // SetBehavior ();//behavior);
        }
            
        

        // protected void Awake()
        public void EnablePosing () 
        // public void SetUpWithPoser (SteamVR_Skeleton_Poser originalPoser)
        {
            // blendPoses = new SkeletonBlendablePose[originalPoser.skeletonAdditionalPoses.Count + 1];
            // for (int i = 0; i < blendPoseCount; i++)
            // {
            //     blendPoses[i] = new SkeletonBlendablePose(GetPoseByIndex(originalPoser, i));
            //     blendPoses[i].PoseToSnapshots();
            // }


            // blendingBehaviours.Clear();
            // for (int i =0 ; i < originalPoser.blendingBehaviours.Count; i++) {
            //     blendingBehaviours.Add (new PoseBlendingBehaviour(originalPoser.blendingBehaviours[i]));
            // }

            // // NOTE: Is there a better way to get the bone count? idk
            // boneCount = originalPoser.skeletonMainPose.leftHand.bonePositions.Length;

            // blendedSnapshotL = new SteamVR_Skeleton_PoseSnapshot(boneCount, HandType.LeftHand);
            // blendedSnapshotR = new SteamVR_Skeleton_PoseSnapshot(boneCount, HandType.RightHand);
            
            isPosing = true;
        }
        public void DisablePosing ( )
        {
            isPosing = false;
        }
        public SteamVR_Skeleton_Pose GetPoseByIndex(HandPoserController originalPoser, int index)
        {
            return originalPoser.allPoses[index];
            // if (index == 0) { return originalPoser.skeletonMainPose; }
            // else { return originalPoser.skeletonAdditionalPoses[index - 1]; }
        }



        // SteamVR_Skeleton_Pose[] posesToBlend = new SteamVR_Skeleton_Pose[2];
        [HideInInspector] public SteamVR_Skeleton_Pose defaultPose;

        


        void OnEnable () {
            SetBehavior ();//behavior);

            // posesToBlend[0] = defualtPose;
            // posesToBlend[1] = defualtPose;
            // posesToBlend[2] = defualtPose;

            int c = 3; // default, blend0, blend1

            blendPoses = new SkeletonBlendablePose[c];
            
            for (int i = 0; i < c; i++)
            {
                blendPoses[i] = new SkeletonBlendablePose(defaultPose);   
            }
            blendPoses[0].SetPose(defaultPose);

            blendingBehaviours = new PoseBlendingBehaviour[c];
            for (int i = 0; i < c; i++)
            {
                blendingBehaviours[i] = new PoseBlendingBehaviour();
                blendingBehaviours[i].SetValue(0, true);
            }

            blendingBehaviours[0].SetValue(1, true);
            
        }



        // int useIndex = 0;


        /// <summary>
        /// Set the blending value of a blendingBehaviour. Works best on Manual type behaviours.
        /// </summary>
        // public bool EnablePose(string behaviourName, float influence)
        public bool EnablePose(SteamVR_Skeleton_Pose pose)//, float influence)
        
        {
            // blendingBehaviours[useIndex].SetValue(0);
            // useIndex = (useIndex+1) % 2;


            if (pose == null || pose == defaultPose) {
                for (int i =0 ; i < blendingBehaviours.Length; i++) {
                    blendingBehaviours[i].SetValue(i == 0 ? 1 : 0, false);
                }
                // pose = defualtPose;
            }
            return true;


            PoseBlendingBehaviour topPoseBlend = blendingBehaviours[2];
            PoseBlendingBehaviour lowerBlend = blendingBehaviours[1];


            SkeletonBlendablePose topPoseBlenda = blendPoses[2];
            SkeletonBlendablePose lowerBlenda = blendPoses[1];


            blendingBehaviours[1] = topPoseBlend;
            blendPoses[1] = topPoseBlenda;


            blendingBehaviours[2] = lowerBlend;
            blendPoses[2] = lowerBlenda;



            blendingBehaviours[1].SetValue(0, false);
            
            blendingBehaviours[2].SetValue(0, true);
            blendingBehaviours[2].SetValue(1, false);
            blendPoses[2].SetPose(pose);

            

            // blendPoses[useIndex].SetPose(pose);
            // blendingBehaviours[useIndex].SetValue(1);

            // bool found = false;
            
            // for (int i =0 ; i < blendingBehaviours.Count; i++) {

            //     // if (behaviourName == null) {
            //     if (pose == null) {
                
            //         // if (blendingBehaviours[i].targetBehaviour.type == HandPoserController.PoseBlendingBehaviour.BlenderTypes.Manual) {
            //             blendingBehaviours[i].SetValue(i == 0 ? 1 : 0);
            //         // }
            //         found = true;
            //         continue;
            //     }

            //     // if (blendingBehaviours[i].targetBehaviour.name == behaviourName) {
            //     // if ( blendingBehaviours[i].targetBehaviour.pose == pose) {
            //     if ( blendPoses[blendingBehaviours[i].targetBehaviour.pose].pose == pose ) {

                    
            //         // if (blendingBehaviours[i].targetBehaviour.type == HandPoserController.PoseBlendingBehaviour.BlenderTypes.Manual) {
            //             blendingBehaviours[i].SetValue(influence);
            //             found = true;
            //         // }
            //         // else {
            //         //     Debug.LogError("Cant manually enable disable non manual blend behaviors");
            //         // }
                    
            //     }
            //     else {
            //         // if (blendingBehaviours[i].targetBehaviour.type == HandPoserController.PoseBlendingBehaviour.BlenderTypes.Manual) {
            //             blendingBehaviours[i].SetValue(0);
            //         // }
                    
            //     }
            // }

            // if (!found) {
            //     // if (behaviourName != null) {
            //     if (pose != null) {
                
            //         // Debug.LogError("Couldnt find pose :: " + behaviourName);
            //         Debug.LogError("Couldnt find pose :: " + pose.name);
            //     }
            // }
            return true;

            
            // return found;



            // PoseBlendingBehaviour behaviour = blendingBehaviours.Find(b => b.targetBehaviour.name == behaviourName);
            // if(behaviour == null)
            // {
            //     Debug.LogError("[SteamVR] Blending Behaviour: " + behaviourName + " not found on Skeleton Poser: " + gameObject.name);
            //     return;
            // }
            // if(behaviour.targetBehaviour.type != SteamVR_Skeleton_PoserBehavior.PoseBlendingBehaviour.BlenderTypes.Manual)
            // {
            //     Debug.LogWarning("[SteamVR] Blending Behaviour: " + behaviourName + " is not a manual behaviour. Its value will likely be overriden.");
            // }
            // behaviour.value = value;
        }




        /// <summary>
        /// Set the blending value of a blendingBehaviour. Works best on Manual type behaviours.
        /// </summary>
        // public void SetBlendingBehaviourValue(string behaviourName, float value)
        // {
        //     PoseBlendingBehaviour behaviour = blendingBehaviours.Find(b => b.targetBehaviour.name == behaviourName);
        //     if(behaviour == null)
        //     {
        //         Debug.LogError("[SteamVR] Blending Behaviour: " + behaviourName + " not found on Skeleton Poser: " + gameObject.name);
        //         return;
        //     }
        //     if(behaviour.targetBehaviour.type != SteamVR_Skeleton_PoserBehavior.PoseBlendingBehaviour.BlenderTypes.Manual)
        //     {
        //         Debug.LogWarning("[SteamVR] Blending Behaviour: " + behaviourName + " is not a manual behaviour. Its value will likely be overriden.");
        //     }
        //     behaviour.value = value;
        // }
        // /// <summary>
        // /// Get the blending value of a blendingBehaviour.
        // /// </summary>
        // public float GetBlendingBehaviourValue(string behaviourName)
        // {
        //     PoseBlendingBehaviour behaviour = blendingBehaviours.Find(b => b.targetBehaviour.name == behaviourName);
        //     if (behaviour == null)
        //     {
        //         Debug.LogError("[SteamVR] Blending Behaviour: " + behaviourName + " not found on Skeleton Poser: " + gameObject.name);
        //         return 0;
        //     }
        //     return behaviour.GetValue();
        // }

        // /// <summary>
        // /// Enable or disable a blending behaviour.
        // /// </summary>
        // public void SetBlendingBehaviourEnabled(string behaviourName, bool value)
        // {
        //     PoseBlendingBehaviour behaviour = blendingBehaviours.Find(b => b.targetBehaviour.name == behaviourName);
        //     if (behaviour == null)
        //     {
        //         Debug.LogError("[SteamVR] Blending Behaviour: " + behaviourName + " not found on Skeleton Poser: " + gameObject.name);
        //         return;
        //     }
        //     if (behaviour.targetBehaviour.type == HandPoserController.PoseBlendingBehaviour.BlenderTypes.Manual) {
        //         return;
        //     }

        //     behaviour.enabled = value;
        // }
        
        // /// <summary>
        // /// Check if a blending behaviour is enabled.
        // /// </summary>
        // /// <param name="behaviourName"></param>
        // /// <returns></returns>
        // public bool GetBlendingBehaviourEnabled(string behaviourName)
        // {
        //     PoseBlendingBehaviour behaviour = blendingBehaviours.Find(b => b.targetBehaviour.name == behaviourName);
        //     if (behaviour == null)
        //     {
        //         Debug.LogError("[SteamVR] Blending Behaviour: " + behaviourName + " not found on Skeleton Poser: " + gameObject.name);
        //         return false;
        //     }
        //     if (behaviour.targetBehaviour.type == HandPoserController.PoseBlendingBehaviour.BlenderTypes.Manual) {
        //         return true;
        //     }

        //     return behaviour.enabled;
        // }
        
        // /// <summary>
        // /// Get a blending behaviour by name.
        // /// </summary>
        // public PoseBlendingBehaviour GetBlendingBehaviour(string behaviourName)
        // {
        //     PoseBlendingBehaviour behaviour = blendingBehaviours.Find(b => b.targetBehaviour.name == behaviourName);
        //     if (behaviour == null)
        //     {
        //         Debug.LogError("[SteamVR] Blending Behaviour: " + behaviourName + " not found on Skeleton Poser: " + gameObject.name);
        //         return null;
        //     }
        //     return behaviour;
        // }




        
        private SteamVR_Skeleton_PoseSnapshot GetHandSnapshot(HandType inputSource)
        {
            if (inputSource == HandType.LeftHand)
                return blendedSnapshotL;
            else
                return blendedSnapshotR;
        }

        /// <summary>
        /// Retrieve the final animated pose, to be applied to a hand skeleton
        /// </summary>
        /// <param name="forAction">The skeleton action you want to blend between</param>
        /// <param name="handType">If this is for the left or right hand</param>
        public SteamVR_Skeleton_PoseSnapshot GetBlendedPose(SteamVR_Action_Skeleton skeletonAction, HandType handType)
        {
            UpdatePose(skeletonAction, handType);
            return GetHandSnapshot(handType);
        }

        /// <summary>
        /// Retrieve the final animated pose, to be applied to a hand skeleton
        /// </summary>
        /// <param name="skeletonBehaviour">The skeleton behaviour you want to get the action/input source from to blend between</param>
        public SteamVR_Skeleton_PoseSnapshot GetBlendedPose(Skeleton skeletonBehaviour)
        {
            return GetBlendedPose(skeletonBehaviour.skeletonAction, skeletonBehaviour.inputSource);
        }


        /// <summary>
        /// Updates all pose animation and blending. Can be called from different places without performance concerns, as it will only let itself run once per frame.
        /// </summary>
        public void UpdatePose(SteamVR_Action_Skeleton skeletonAction, HandType inputSource)
        {
            // only allow this function to run once per frame
            if (poseUpdatedThisFrame) return;

            poseUpdatedThisFrame = true;

            // always do additive animation on main pose
            // blendPoses[0].UpdateAdditiveAnimation(skeletonAction, inputSource);

            //copy from main pose as a base
            SteamVR_Skeleton_PoseSnapshot snap = GetHandSnapshot(inputSource);
            // snap.CopyFrom(blendPoses[0].GetHandSnapshot(inputSource));

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
            // for (int behaviourIndex = 0; behaviourIndex < blendingBehaviours.Count; behaviourIndex++)
            
            {
                PoseBlendingBehaviour blending = blendingBehaviours[behaviourIndex];

                blending.Update(Time.deltaTime, inputSource, smoothSpeed);

                // if disabled or very low influence, skip for perf

                // float t = blending.targetBehaviour.influence * blending.GetValue();
                // float t = behaviourIndex == 0 ? 1 : blending.GetValue();
                float t = blending.GetValue();




                // bool isManual = blending.targetBehaviour.type == HandPoserController.PoseBlendingBehaviour.BlenderTypes.Manual;
                // if ((blending.enabled || isManual || behaviourIndex == 0) && t > 0.01f)
                
                if (t > 0.01f)
                
                {
                    // if (blending.targetBehaviour.pose != 0)
                    // {
                        // update additive animation only as needed
                        // blendPoses[blending.targetBehaviour.pose].UpdateAdditiveAnimation(skeletonAction, inputSource);
                        blendPoses[behaviourIndex].UpdateAdditiveAnimation(skeletonAction, inputSource);
                    // }

                    blending.ApplyBlending(snapshot, blendPoses[behaviourIndex], inputSource, t);
                }
            }

        }

        protected void LateUpdate()
        {
            // let the pose be updated again the next frame
            poseUpdatedThisFrame = false;
        }
        
        /// <summary>Weighted average of n vector3s</summary>
        // protected Vector3 BlendVectors(Vector3[] vectors, float[] weights)
        // {
        //     Vector3 blendedVector = Vector3.zero;
        //     for (int i = 0; i < vectors.Length; i++)
        //     {
        //         blendedVector += vectors[i] * weights[i];
        //     }
        //     return blendedVector;
        // }

        // /// <summary>Weighted average of n quaternions</summary>
        // protected Quaternion BlendQuaternions(Quaternion[] quaternions, float[] weights)
        // {
        //     Quaternion outquat = Quaternion.identity;
        //     for (int i = 0; i < quaternions.Length; i++)
        //     {
        //         outquat *= Quaternion.Slerp(Quaternion.identity, quaternions[i], weights[i]);
        //     }
        //     return outquat;
        // }

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
            public SteamVR_Skeleton_PoseSnapshot GetHandSnapshot(HandType inputSource)
            {
                if (inputSource == HandType.LeftHand)
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

















            /// <summary>
            /// Init based on an existing Skeleton_Pose
            /// </summary>
            public SkeletonBlendablePose(SteamVR_Skeleton_Pose p)
            {
                snapshotR = new SteamVR_Skeleton_PoseSnapshot(p.rightHand.bonePositions.Length, HandType.RightHand);
                snapshotL = new SteamVR_Skeleton_PoseSnapshot(p.leftHand.bonePositions.Length, HandType.LeftHand);
            

                // pose = p;
                // PoseToSnapshots();
            }

            public void SetPose (SteamVR_Skeleton_Pose p) {
                pose = p;
                PoseToSnapshots();
            }
            
            

            /// <summary>
            /// Copy the base pose into the snapshots.
            /// </summary>
            // public 
            void PoseToSnapshots()
            {
                // snapshotR.position = pose.rightHand.position;
                // snapshotR.rotation = pose.rightHand.rotation;
                pose.rightHand.bonePositions.CopyTo(snapshotR.bonePositions, 0);
                pose.rightHand.boneRotations.CopyTo(snapshotR.boneRotations, 0);

                // snapshotL.position = pose.leftHand.position;
                // snapshotL.rotation = pose.leftHand.rotation;
                pose.leftHand.bonePositions.CopyTo(snapshotL.bonePositions, 0);
                pose.leftHand.boneRotations.CopyTo(snapshotL.boneRotations, 0);
            }

            // public SkeletonBlendablePose() { }
        }

        /// <summary>
        /// A filter applied to the base pose. Blends to a secondary pose by a certain weight. Can be masked per-finger
        /// </summary>
        // [System.Serializable]
        public class PoseBlendingBehaviour
        {
            // public HandPoserController.PoseBlendingBehaviour targetBehaviour;

            public PoseBlendingBehaviour()//HandPoserController.PoseBlendingBehaviour targetBehaviour)
            {
                // this.targetBehaviour = targetBehaviour;
                // enabled = true;
            }

            // public bool enabled = true;

            float value = 0;
            public float GetValue () {
                return value;
            }
            public float targetValue;

            // SteamVR_Skeleton_Hand_Mask handMask;

            public void SetValue (float newValue, bool hardSet){//, SteamVR_Skeleton_Hand_Mask handMask) {
                targetValue = newValue;
                if (hardSet)
                    value = targetValue;

                // if (newValue != 0) {
                //     this.handMask = handMask;
                // }
            }
            
            /// <summary>
            /// Performs smoothing based on deltaTime parameter.
            /// </summary>
            public void Update(float deltaTime, HandType inputSource, float smoothSpeed)
            {
            //     if (targetBehaviour.type == HandPoserController.PoseBlendingBehaviour.BlenderTypes.AnalogAction)
            //     {
            //         targetValue = targetBehaviour.action_single.GetAxis(inputSource);

                    

            //         // if (targetBehaviour.smoothingSpeed == 0)
            //         //     value = targetBehaviour.action_single.GetAxis(inputSource);
            //         // else
            //         //     value = Mathf.Lerp(value, targetBehaviour.action_single.GetAxis(inputSource), deltaTime * targetBehaviour.smoothingSpeed);
            //     }
            //     if (targetBehaviour.type == HandPoserController.PoseBlendingBehaviour.BlenderTypes.BooleanAction)
            //     {
            //         targetValue = targetBehaviour.action_bool.GetState(inputSource) ? 1 : 0;

            //         // if (targetBehaviour.smoothingSpeed == 0)
            //         //     value = targetBehaviour.action_bool.GetState(inputSource) ? 1 : 0;
            //         // else
            //         //     value = Mathf.Lerp(value, targetBehaviour.action_bool.GetState(inputSource) ? 1 : 0, deltaTime * targetBehaviour.smoothingSpeed);
            //     }

                if (smoothSpeed == 0)
                    value = targetValue;
                else
                    value = Mathf.Lerp(value, targetValue, deltaTime * smoothSpeed);

            }

            // public void ApplyBlending(SteamVR_Skeleton_PoseSnapshot snapshot, SkeletonBlendablePose[] blendPoses, HandType inputSource, float t)
            public void ApplyBlending(SteamVR_Skeleton_PoseSnapshot snapshot, SkeletonBlendablePose blendPose, HandType inputSource, float t)

            {
                // targetBehaviour.ApplyBlending(snapshot, blendPoses, inputSource, value);


                // float t = targetBehaviour.influence * value;
                // if (t == 0) 
                //     return;

                
                SteamVR_Skeleton_PoseSnapshot targetSnapshot = blendPose.GetHandSnapshot(inputSource);
                // SteamVR_Skeleton_PoseSnapshot targetSnapshot = blendPoses[targetBehaviour.pose].GetHandSnapshot(inputSource);
                // SteamVR_Skeleton_PoseSnapshot targetSnapshot = targetBehaviour.pose.GetHandSnapshot(inputSource);
                
                // if (targetBehaviour.mask.GetFinger(0) || targetBehaviour.useMask == false)
                if (t >= 1)
                {
                    // snapshot.position = targetSnapshot.position;
                    // snapshot.rotation = targetSnapshot.rotation;


                }
                else {
                    // snapshot.position = Vector3.Lerp(snapshot.position, targetSnapshot.position, t);
                    // snapshot.rotation = Quaternion.Slerp(snapshot.rotation, targetSnapshot.rotation, t);

                }

                for (int boneIndex = 0; boneIndex < snapshot.bonePositions.Length; boneIndex++)
                {
                    // if ((boneIndex == 1) || (boneIndex == 0))
                    //     continue;
                    
                    // verify the current finger is enabled in the mask, or if no mask is used.
                    // if (targetBehaviour.mask.GetFinger(SteamVR_Skeleton_JointIndexes.GetFingerForBone(boneIndex) + 1) || targetBehaviour.useMask == false)
                    {
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
                
            }
        }
    }

    /// <summary>
    /// PoseSnapshots hold a skeleton pose for one hand, as well as storing which hand they contain. 
    /// They have several functions for combining BlendablePoses.
    /// </summary>
    public class SteamVR_Skeleton_PoseSnapshot
    {
        public HandType inputSource;

        // public Vector3 position;
        // public Quaternion rotation;

        public Vector3[] bonePositions;
        public Quaternion[] boneRotations;

        public SteamVR_Skeleton_PoseSnapshot(int boneCount, HandType source)
        {
            inputSource = source;
            bonePositions = new Vector3[boneCount];
            boneRotations = new Quaternion[boneCount];
            // position = Vector3.zero;
            // rotation = Quaternion.identity;
        }

        /// <summary>
        /// Perform a deep copy from one poseSnapshot to another.
        /// </summary>
        public void CopyFrom(SteamVR_Skeleton_PoseSnapshot source)
        {
            inputSource = source.inputSource;
            // position = source.position;
            // rotation = source.rotation;
            for (int i = 0; i < bonePositions.Length; i++)
            {
                bonePositions[i] = source.bonePositions[i];
                boneRotations[i] = source.boneRotations[i];
            }
        }
    }

    /// <summary>
    /// Simple mask for fingers
    /// </summary>
    // [System.Serializable]
    // public class SteamVR_Skeleton_HandMask
    // {
    //     // public bool palm;
    //     // public bool thumb;
    //     // public bool index;
    //     // public bool middle;
    //     // public bool ring;
    //     // public bool pinky;
    //     public bool[] values = new bool[6];

    //     public void SetFinger(int i, bool value)
    //     {
    //         values[i] = value;
    //         // Apply();
    //     }

    //     public bool GetFinger(int i)
    //     {
    //         return values[i];
    //     }

    //     public SteamVR_Skeleton_HandMask()
    //     {
    //         // values = new bool[6];
    //         Reset();
    //     }

    //     public SteamVR_Skeleton_HandMask(bool palm, bool thumb, bool index, bool middle, bool ring, bool pinky) 
    //     {
    //         values = new bool[6] {
    //             palm,
    //             thumb,
    //             index,
    //             middle,
    //             ring,
    //             pinky,
    //         };
                        
            
    //         // Reset();
    //     }

    //     /// <summary>
    //     /// All elements on
    //     /// </summary>
    //     public void Reset()
    //     {
    //         values = new bool[6];
    //         for (int i = 0; i < 6; i++)
    //         {
    //             values[i] = true;
    //         }
    //         // Apply();
    //     }

    //     // protected void Apply()
    //     // {
    //         // palm = values[0];
    //         // thumb = values[1];
    //         // index = values[2];
    //         // middle = values[3];
    //         // ring = values[4];
    //         // pinky = values[5];
    //     // }

    //     public static readonly SteamVR_Skeleton_HandMask fullMask = new SteamVR_Skeleton_HandMask();
    // };

}
