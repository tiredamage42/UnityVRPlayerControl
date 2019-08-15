using System.Collections.Generic;
using UnityEngine;

namespace RenderTricks {
    public class EmissionFlasher : MonoBehaviour
    {
        public string flasherName;
        public AnimationCurve flashAnimation = new AnimationCurve();
        public float duration = 1;
        public Color emissionColor = Color.white;
        // public float maxIntensity = 1;
        public Vector2 rimPowerRange = new Vector2(10, 1);


        // static readonly int _EmissionColor = Shader.PropertyToID("_EmissionColor");
        static readonly int _RimColor = Shader.PropertyToID("_RimColor");
        static readonly int _RimPower = Shader.PropertyToID("_RimPower");


        public static EmissionFlasher GetFlasherByName (string name) {

            EmissionFlasher[] allFlashers = GameObject.FindObjectsOfType<EmissionFlasher>();

            for (int i = 0; i < allFlashers.Length; i++) {
                if(allFlashers[i].flasherName == name) {
                    return allFlashers[i];
                }
            }
            Debug.LogWarning("Couldnt find flahser: " + name);
            return null;
        }

        List<Material> myMaterials = new List<Material>();

        public void AddMaterial (Material material) {
            if (!myMaterials.Contains(material)) {
                // material.EnableKeyword("_EMISSION");
                material.EnableKeyword("_RIM_LIGHTING");
                myMaterials.Add(material);
            }
        }
        
        public void RemoveMaterial (Material material) {
            myMaterials.Remove(material);
        }

        void SetEmissionValues () {
            int c = myMaterials.Count;

            if (c == 0)
                return;

            float t = flashAnimation.Evaluate((Time.time % duration) / duration);

            
            Color color = emissionColor;
            color.a = t;
            float rimPower = Mathf.Lerp(rimPowerRange.x, rimPowerRange.y, t);
            
            // Color color = emissionColor * (t * maxIntensity);
            for (int i = 0; i < c; i++) {
                //need set vector instead of color in case of hdr...
                // myMaterials[i].SetVector(_EmissionColor, color);
                myMaterials[i].SetVector(_RimColor, color);
                myMaterials[i].SetFloat(_RimPower, rimPower);
                
            }
        }

        void Update () {
            SetEmissionValues();
        }
    }
}