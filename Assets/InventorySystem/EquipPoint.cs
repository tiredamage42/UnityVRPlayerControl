// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace Game.InventorySystem {
    
    public class EquipPoint : MonoBehaviour {
        public InventoryEquipper equipper;
        public int equipID;

        void Start () {
            equipper.SetEquipPoint(equipID, this);
        }

        const float MaxVelocityChange = 10f;
        const float VelocityMagic = 6000f;
        const float AngularVelocityMagic = 50f;
        const float MaxAngularVelocityChange = 20f;

        void FixedUpdate()
        {
            if (equipID < 0 || equipID >= equipper.equippedSlots.Length) {
                Debug.LogError("Equip slot " + equipID + " is out of range on equipper " + equipper);
                return;
            }

            InventoryEquipper.EquipSlot slot = equipper.equippedSlots[equipID]; 
            if (slot != null) {
                ItemBehavior item = slot.sceneItem.itemBehavior;

                if (item.equipType == InventoryEquipper.EquipType.Physics) {        
                    UpdateAttachedVelocity(slot);
                }
                else if (item.equipType == InventoryEquipper.EquipType.Normal) {
                    TransformBehavior.AdjustTransform(slot.sceneItem.transform, transform, item.equipTransform, equipID);
                }
            }
        }

        void UpdateAttachedVelocity(InventoryEquipper.EquipSlot equippedSlot)
        {
            Vector3 velocityTarget, angularTarget;
            if (GetUpdatedEquippedVelocities(equippedSlot, out velocityTarget, out angularTarget))
            {
                float scale = Valve.VR.SteamVR_Utils.GetLossyScale(transform);// equipPoint);
                
                float maxAngularVelocityChange = MaxAngularVelocityChange * scale;
                float maxVelocityChange = MaxVelocityChange * scale;
                Rigidbody attachedRigidbody = equippedSlot.sceneItem.rigidbody;
                attachedRigidbody.velocity = Vector3.MoveTowards(attachedRigidbody.velocity, velocityTarget, maxVelocityChange);
                attachedRigidbody.angularVelocity = Vector3.MoveTowards(attachedRigidbody.angularVelocity, angularTarget, maxAngularVelocityChange);
            }
        }

        bool GetUpdatedEquippedVelocities(InventoryEquipper.EquipSlot equippedSlot, out Vector3 velocityTarget, out Vector3 angularTarget)
        {
            bool realNumbers = false;

            Vector3 localPosition;
            Quaternion localRotation;
            Vector3 localScale;

            TransformBehavior.GetValues(equippedSlot.sceneItem.itemBehavior.equipTransform, equipID, out localPosition, out localRotation, out localScale);

            Vector3 targetItemPosition = transform.TransformPoint(localPosition);
            Vector3 positionDelta = (targetItemPosition - equippedSlot.sceneItem.rigidbody.position);
            velocityTarget = (positionDelta * VelocityMagic * Time.deltaTime);

            if (float.IsNaN(velocityTarget.x) == false && float.IsInfinity(velocityTarget.x) == false)
                realNumbers = true;
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
