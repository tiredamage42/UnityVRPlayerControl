using UnityEngine;

namespace InventorySystem {

    [CreateAssetMenu()]
    public class EquipBehavior : ScriptableObject
    {
        [System.Serializable] public class EquipSetting {
            public Vector3 position, rotation;
        }
        public EquipSetting[] equipSettings;


            

        public static void AdjustTransform(Transform transform, Transform parent, EquipBehavior equipBehavior, int settingIndex) {
            if (transform == null) {
                Debug.LogWarning("cant adjust, transform is null");
                return;
            }


            if (equipBehavior == null) {
                Debug.LogWarning("Cant move :: " + transform.name + " equip behavior is null");
                return;
            }

            if (transform.parent != parent) {
                transform.SetParent(parent);
            }
        

            equipBehavior.AdjustTransform(transform, settingIndex);
        }




        public static void AdjustTransform(Transform transform, EquipBehavior equipBehavior, int settingIndex) {
            if (transform == null) {
                Debug.LogWarning("cant adjust, transform is null");
                return;
            }
            if (equipBehavior == null) {
                Debug.LogWarning("Cant move :: " + transform.name + " equip behavior is null");
                return;
            }
            equipBehavior.AdjustTransform(transform, settingIndex);
        }
        public void AdjustTransform(Transform transform, int settingIndex) {
            if (settingIndex >= 0 && settingIndex < equipSettings.Length) {
                transform.localPosition = equipSettings[settingIndex].position;
                transform.localRotation = Quaternion.Euler(equipSettings[settingIndex].rotation);
            }
            else {
                Debug.LogWarning("Cant move :: " + transform.name + " equip setting index " + settingIndex + " is out of range");
            }
        }
    }
}
