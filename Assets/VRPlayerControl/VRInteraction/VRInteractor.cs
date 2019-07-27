using UnityEngine;

using InteractionSystem;

namespace VRPlayer {
    
    /*
        modify the interaction reference transform
        and display the interaction ray
    */
    public class VRInteractor : MonoBehaviour
    {
        public TransformBehavior interactorReferenceBehavior;
        InteractionPoint[] interactionPoints;
        public Color interactRayColor = Color.green, interactRayNullColor = new Color(.5f, .5f, .5f, .25f);
        public float interactRayWidth = .1f;
        LineRenderer[] interactRays;

        void Awake () {
            BuildLineRenderers();
        }
        void Start () {
            GetInteractionPoints();
            UpdateInteractorReferenceTransform();
        }

        void GetInteractionPoints () {
            if (Player.instance != null) {
                if (interactionPoints == null || interactionPoints.Length != 2) {
                    interactionPoints = new InteractionPoint[2];
                }
                for (int i = 0; i < 2; i++) {
                    if (interactionPoints[i] == null) {
                        interactionPoints[i] = Player.instance.GetHand(VRManager.Int2Hand(i)).GetComponent<InteractionPoint>();
                    }
                }
            }
        }

        void UpdateInteractorReferenceTransform () {
            for (int i = 0; i < interactionPoints.Length; i++) {
                if (interactionPoints[i] != null) {
                    if (interactionPoints[i].referenceTransform == null) {
                        interactionPoints[i].referenceTransform = new GameObject(interactionPoints[i].name + " interactor helper transform").transform;
                    }
                    TransformBehavior.AdjustTransform(interactionPoints[i].referenceTransform, interactionPoints[i].transform, interactorReferenceBehavior, 0);
                }
            }
        }

#if UNITY_EDITOR
        void Update()
        {
            UpdateInteractorReferenceTransform();
        }
#endif

        void BuildLineRenderers () {
            Material interactionShowMaterial = new Material(Shader.Find("Mobile/Particles/Additive"));
            interactionShowMaterial.hideFlags = HideFlags.HideAndDontSave;

            interactRays = new LineRenderer[2];
            for (int i = 0; i < interactionPoints.Length; i++) {
                interactRays[i] = gameObject.AddComponent<LineRenderer>();
                interactRays[i].sharedMaterial = interactionShowMaterial;
            }
        }

        void OnEnable () {
            GetInteractionPoints();
            for (int i = 0; i < interactionPoints.Length; i++) {
                if (interactionPoints[i] != null) interactionPoints[i].onRayCheckUpdate += OnInteractionRayUpdate;
            }
        }
        void OnDisable () {
            for (int i = 0; i < interactionPoints.Length; i++) {
                if (interactionPoints[i] != null) interactionPoints[i].onRayCheckUpdate -= OnInteractionRayUpdate;
            }   
        }

        void OnInteractionRayUpdate (bool rayEnabled, Vector3 origin, Vector3 end, bool isHitting, int interactionPointID) {
             interactRays[interactionPointID].enabled = rayEnabled;
                
             if (rayEnabled) {       
                
                interactRays[interactionPointID].startWidth = interactRayWidth;
                interactRays[interactionPointID].endWidth = interactRayWidth;

                Color color = isHitting ? interactRayColor : interactRayNullColor;
                interactRays[interactionPointID].startColor = color;
                interactRays[interactionPointID].endColor = color;

                interactRays[interactionPointID].SetPosition(0, origin);
                interactRays[interactionPointID].SetPosition(1, end);
            }
        }
    }
}
