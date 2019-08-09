using UnityEngine;

// namespace CustomUtilities {

    [CreateAssetMenu()]
    public class TransformBehavior : ScriptableObject
    {
        [System.Serializable] public class TransformSetting {
            public Vector3 position, rotation, scale = Vector3.one;
        }
        public TransformSetting[] transformSettings;
        public bool useScale;


        public static void SetValues (TransformBehavior equipBehavior, int settingIndex, Transform transform)
        {
            
            if (equipBehavior == null) {
                Debug.LogWarning("Cant set values... transform behavior is null");
                return;
            }
            if (transform == null) {
                Debug.LogWarning("Cant set values on " + equipBehavior.name + " ... transform is null");
                return;
            }

            equipBehavior.SetValues(settingIndex, transform);

        }


        public static void GetValues (TransformBehavior equipBehavior, int settingIndex, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale)
        {
            localPosition = Vector3.zero;
            localRotation = Quaternion.identity;
            localScale = Vector3.one;

            if (equipBehavior == null) {
                Debug.LogWarning("Cant get values... transform behavior is null");
                return;
            }

            equipBehavior.GetValues(settingIndex, out localPosition, out localRotation, out localScale);

        }

        public static void AdjustTransform(Transform transform, Transform parent, TransformBehavior equipBehavior, int settingIndex) {
            AdjustTransform(transform, parent, equipBehavior, settingIndex, Vector3.one);
        }

        public static void AdjustTransform(Transform transform, Transform parent, TransformBehavior equipBehavior, int settingIndex, Vector3 multiplier) {
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

            equipBehavior.AdjustTransform(transform, settingIndex, multiplier);
        }

        public static void AdjustTransform(Transform transform, TransformBehavior equipBehavior, int settingIndex, Vector3 multiplier) {
            if (transform == null) {
                Debug.LogWarning("cant adjust, transform is null");
                return;
            }
            if (equipBehavior == null) {
                Debug.LogWarning("Cant move :: " + transform.name + " equip behavior is null");
                return;
            }
            equipBehavior.AdjustTransform(transform, settingIndex, multiplier);
        }
        public void AdjustTransform(Transform transform, int settingIndex, Vector3 multiplier) {
            if (settingIndex >= 0 && settingIndex < transformSettings.Length) {

                transform.localPosition = new Vector3(
                    transformSettings[settingIndex].position.x * multiplier.x,
                    transformSettings[settingIndex].position.y * multiplier.y,
                    transformSettings[settingIndex].position.z * multiplier.z
                );

                
                // transform.localPosition = transformSettings[settingIndex].position;
                
                transform.localRotation = Quaternion.Euler(transformSettings[settingIndex].rotation);
                if (useScale) {

                    transform.localScale = transformSettings[settingIndex].scale;
                    if (transform.localScale == Vector3.zero) {
                        Debug.LogWarning(name + " has scale of zero, setting to one");
                        transform.localScale = Vector3.one;
                    }
                }
                
            }
            else {
                Debug.LogWarning("Cant move :: " + transform.name + " equip setting index " + settingIndex + " is out of range");
            }
        }
        public void GetValues(int settingIndex, 
            out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale
        ) {
            
            localPosition = Vector3.zero;
            localRotation = Quaternion.identity;
            localScale = Vector3.one;

            if (settingIndex >= 0 && settingIndex < transformSettings.Length) {
                localPosition = transformSettings[settingIndex].position;
                localRotation = Quaternion.Euler(transformSettings[settingIndex].rotation);
                localScale = transformSettings[settingIndex].scale;

                if (localScale == Vector3.zero) {
                    Debug.LogWarning(name + " has scale of zero, setting to one");
                    localScale = Vector3.one;
                }
            }
            else {
                Debug.LogWarning("Cant get values transform setting index " + settingIndex + " is out of range");
            }
        }

         public void SetValues(int settingIndex, Transform transform) {
            
            if (settingIndex >= 0 && settingIndex < transformSettings.Length) {

                transformSettings[settingIndex].position = transform.localPosition;
                transformSettings[settingIndex].rotation = transform.localRotation.eulerAngles;
                transformSettings[settingIndex].scale = transform.localScale;
                
            }
            else {
                Debug.LogWarning("Cant set values transform setting index " + settingIndex + " is out of range");
            }
        }
    }
// }
