using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
namespace InventorySystem {
    
    public class EquipPoint : MonoBehaviour {
        public Inventory baseInventory;
        public int equipSlotOnBase;


        void Awake () {
            baseInventory.SetEquipPoint(equipSlotOnBase, this);
        }



        protected const float MaxVelocityChange = 10f;
        protected const float VelocityMagic = 6000f;
        protected const float AngularVelocityMagic = 50f;
        protected const float MaxAngularVelocityChange = 20f;


        // bool ThisSubInventoryHasCurrentEquipped (out Inventory.InventorySlot slot) {
        //     return baseInventory.EquipSlotIsOccupied(equipSlotOnBase, out slot);
        // }




        protected virtual void FixedUpdate()
        {


            if (equipSlotOnBase < 0 || equipSlotOnBase >= baseInventory.equippedSlots.Length) {
            Debug.LogError("Equip slot " + equipSlotOnBase + " is out of range on inventory " + baseInventory);
            return;
        }

            Inventory.InventorySlot slot = baseInventory.equippedSlots[equipSlotOnBase]; 
            if (slot != null) {
                ItemBehavior item = slot.item;

                if (item.equipType == Inventory.EquipType.Physics) {        
                    UpdateAttachedVelocity(slot);
                }
                else if (item.equipType == Inventory.EquipType.Normal) {

                    // Vector3 localPosition;
                    // Quaternion localRotation;

                    // TransformBehavior.GetValues(item.equipTransform, equipSlotOnBase, out localPosition, out localRotation);

                    // slot.sceneItem.transform.localPosition = localPosition;//attachedInfo.targetLocalPos;
                    // slot.sceneItem.transform.localRotation = localRotation;//attachedInfo.initialRotationalOffset;


                    TransformBehavior.AdjustTransform(slot.sceneItem.transform, transform, item.equipTransform, equipSlotOnBase);

                    
                }
   
            }
        }


        void UpdateAttachedVelocity(Inventory.InventorySlot equippedSlot)
        {
            Vector3 velocityTarget, angularTarget;
            bool success = GetUpdatedEquippedVelocities(equippedSlot, out velocityTarget, out angularTarget);
            if (success)
            {
                float scale = SteamVR_Utils.GetLossyScale(transform);// equipPoint);
                
                float maxAngularVelocityChange = MaxAngularVelocityChange * scale;
                float maxVelocityChange = MaxVelocityChange * scale;
                Rigidbody attachedRigidbody = equippedSlot.sceneItem.rigidbody;
                attachedRigidbody.velocity = Vector3.MoveTowards(attachedRigidbody.velocity, velocityTarget, maxVelocityChange);
                attachedRigidbody.angularVelocity = Vector3.MoveTowards(attachedRigidbody.angularVelocity, angularTarget, maxAngularVelocityChange);
            }
        }

        public bool GetUpdatedEquippedVelocities(Inventory.InventorySlot equippedSlot, out Vector3 velocityTarget, out Vector3 angularTarget)
        {
            bool realNumbers = false;


            Vector3 localPosition;
            Quaternion localRotation;

            TransformBehavior.GetValues(equippedSlot.item.equipTransform, equipSlotOnBase, out localPosition, out localRotation);

            // GetLocalEquippedPositionTargets (equippedItem.item, out localPosition, out localRotation);


            Vector3 targetItemPosition = transform.TransformPoint(localPosition);//equippedItem.targetLocalPos);
            // Vector3 targetItemPosition = TargetEquippedItemWorldPosition();//equippedItem);




            Vector3 positionDelta = (targetItemPosition - equippedSlot.sceneItem.rigidbody.position);
            velocityTarget = (positionDelta * VelocityMagic * Time.deltaTime);

            if (float.IsNaN(velocityTarget.x) == false && float.IsInfinity(velocityTarget.x) == false)
            {
                realNumbers = true;
            }
            else
                velocityTarget = Vector3.zero;


            Quaternion targetItemRotation = transform.rotation * localRotation;
            
            Quaternion rotationDelta = targetItemRotation * Quaternion.Inverse(equippedSlot.sceneItem.transform.rotation);


            float angle;
            Vector3 axis;
            rotationDelta.ToAngleAxis(out angle, out axis);

            if (angle > 180)
                angle -= 360;

            if (angle != 0 && float.IsNaN(axis.x) == false && float.IsInfinity(axis.x) == false)
            {
                angularTarget = angle * axis * AngularVelocityMagic * Time.deltaTime;

                realNumbers &= true;
            }
            else
                angularTarget = Vector3.zero;

            return realNumbers;
        }
    }
}
