using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Game.UI;
using SimpleUI;

using InteractionSystem;
using Game.InventorySystem.CraftingSystem;
namespace Game.InventorySystem.WorkshopSystem {
    
    public class WorkshopMode : MonoBehaviour {
        public int highlightGroup = 0;
        public float interactRayDistance = 10;
        public int submitAction, cancelAction = 3;
        public float gridSnap = .1f;
        public float angleSnap = 15f;
        public float turnSpeed = 1;
        public float moveSpeed = 1;

        Actor actor;
        public LayerMask groundMask;

        Vector2 axisDeltas;
        float yAngle, zDistance;
        


        public void ProvideAxisDeltas (Vector2 axisDeltas) {
            this.axisDeltas = axisDeltas;
        }

        Vector3 environmentHitPoint;
        bool hittingPoint;

        void FindEnvironmentPoint () {

            InteractionPoint point = actor.interactor.mainInteractor;
            Transform refT = point.referenceTransform;

            RaycastHit hit;
            if (Physics.Raycast(new Ray(refT.position, refT.forward), out hit, zDistance, groundMask, QueryTriggerInteraction.Ignore)) {
                environmentHitPoint = hit.point;
                hittingPoint = true;
            }
            else {
                environmentHitPoint = refT.position + refT.forward * zDistance;
                hittingPoint = false;
            }

            // setting interaction point values for VR show....
            point.rayHitPoint = environmentHitPoint;
            point.usingRay = true;

        }

        static float SnapValue(float raw, float snap, float current, ref bool changed) {
            int grid = (int)(raw / snap);
            float snapped = (((raw % snap) / snap) < .5f ? grid : grid + 1) * snap;
            if (current != snapped) {
                changed = true;
            }
            return snapped;
        }

        void Update () {
            if (inBuildMode || isMovingObject) {

                yAngle += axisDeltas.x * turnSpeed * Time.deltaTime;
                zDistance += axisDeltas.y * moveSpeed * Time.deltaTime;

                FindEnvironmentPoint();

                Transform referenceTransform = actor.interactor.mainInteractor.referenceTransform;
                
                // bool isPointingUp = Vector3.Angle(Vector3.up, referenceTransform.forward) <= 90;

                

                // bool stickToGround = !isPointingUp;
                Vector3 rawSpace = environmentHitPoint;// referenceTransform.position + referenceTransform.forward * zDistance;
                // rawSpace.y = referenceTransform.position.y;


                Vector3 currentPreviewPos = currentPreviewTransform.position;
                float currentYAngle = currentPreviewTransform.rotation.eulerAngles.y;
                bool changedSnap = false;

                // int xGrid = (int)(rawSpace.x / gridSnap);
                // float xSnap = ((rawSpace.x % gridSnap) / gridSnap) < .5f ? xGrid : xGrid + 1;
                // xSnap *= gridSnap;
                // if (currentPreviewPos.x != xSnap) {
                //     changedSnap = true;
                // }
                float xSnap = SnapValue(rawSpace.x, gridSnap, currentPreviewPos.x, ref changedSnap);


                // int zGrid = (int)(rawSpace.z / gridSnap);
                // float zSnap = ((rawSpace.z % gridSnap) / gridSnap) < .5f ? zGrid : zGrid + 1;
                // zSnap *= gridSnap;

                float zSnap = SnapValue(rawSpace.z, gridSnap, currentPreviewPos.z, ref changedSnap);


                float ySnap  = rawSpace.y;
                // if (stickToGround) {
                if (hittingPoint) {
                    // RaycastHit hit;
                    // if (Physics.Raycast(new Vector3(xSnap, rawSpace.y, zSnap), Vector3.down, out hit, 100, groundMask, QueryTriggerInteraction.Ignore)) {
                    //     ySnap = hit.point.y;
                    // }
                }
                else {

                    ySnap = SnapValue(rawSpace.y, gridSnap, currentPreviewPos.y, ref changedSnap);

                    // int yGrid = (int)(rawSpace.y / gridSnap);
                    // ySnap = ((rawSpace.y % gridSnap) / gridSnap) < .5f ? yGrid : yGrid + 1;
                    // ySnap *= gridSnap;
                }

                Vector3 snappedPosition = new Vector3(xSnap, ySnap, zSnap);

                // int angleGrid = (int)(yAngle / angleSnap);
                // float angleSnapped = ((yAngle % angleSnap) / angleSnap) < .5f ? angleGrid : angleGrid + 1;
                // angleSnapped *= angleSnap;
                float angleSnapped = SnapValue(yAngle, angleSnap, currentYAngle, ref changedSnap);

                Quaternion rotation = Quaternion.Euler(0,angleSnapped,0);

                currentPreviewTransform.position = snappedPosition;
                currentPreviewTransform.rotation = rotation;

                if (changedSnap) {
                    OnSnapChange();
                }
            }
        }


        void OnSnapChange () {
            // just for haptic pulses in VR...
            WorkshopUIHandler.instance.uiObject.BroadcastSelectEvent(null, null, null);
        }

        void Awake () {
            actor = GetComponent<Actor>();
        }   

        void OnEnable () {   
            WorkshopUIHandler.instance.onUIClose += OnCloseUI;
            WorkshopUIHandler.instance.onUIOpen += OnOpenUI;
        }
        void OnDisable () {   
            WorkshopUIHandler.instance.onUIClose -= OnCloseUI;
            WorkshopUIHandler.instance.onUIOpen -= OnOpenUI;
        }

        void OnOpenUI (GameObject uiObject, int interactorID) {
            actor.inventory.equipper.UnequipMainItemsTemp();
            actor.interactor.SetInteractionMode(1, interactRayDistance);
            
            actor.onActionStart += OnActionStart;
            
            WorkshopUIHandler.instance.uiObject.SubscribeToSelectEvent(OnUISelect);

            GameUI.ShowInGameMessage ("Entered Workshop Mode", false, UIColorScheme.Normal);
        }

        void OnCloseUI (GameObject uiObject) {
            if (inBuildMode) {
                DestroyCurrentPreview();
            }
            else {
                if (isMovingObject) {
                    StopMovingHoveredObject(false);
                }
            }



            actor.inventory.equipper.EquipMainItemsTemp();
            actor.interactor.SetInteractionMode(0, -1);
            actor.interactor.mainInteractor.findInteractables = true;
            
            actor.onActionStart -= OnActionStart;
            
            GameUI.ShowInGameMessage ("Exited Workshop Mode", false, UIColorScheme.Normal);

        }

        void OnActionStart (int controllerIndex, int action) {
            if (UIManager.popupOpen) return;
            if (controllerIndex != 0) return;

            if (action == submitAction) {
                OnWorkshopSubmitAction();
            }
            else if (action == cancelAction) {
                OnWorkshopCancelAction();
            }
        }

        SceneItem currentHoveredInScene;

        void OnScrapStoreConfirmation(bool used, int value) {
            if (used) {
                // scrap
                if (value == 0) {       
                    // use the inventory to scrap the selected scene item (or jsut store it if it's already base components)
                    if (currentHoveredInScene.itemBehavior.composedOf.list.Length == 0) {
                        actor.inventory.StashItem(currentHoveredInScene, -1, true);
                    }
                    else {
                        actor.inventory.ScrapItem(currentHoveredInScene.itemBehavior, currentHoveredInScene.itemCount, sendMessage:true, actor, actor);
                        currentHoveredInScene.gameObject.SetActive(false);
                    }
                }
                // store
                else if (value == 1) {
                    // stash the scene item into the inventory (show message, equip slot -1 (no manual))
                    actor.inventory.StashItem(currentHoveredInScene, -1, true);
                }
            }
        }
        // when pointint at something and hitting cancel
        void AttemptScrapStore () {
            GameUI.ShowSelectionPopup(true, "\n\n"+currentHoveredInScene.itemBehavior.itemName+":", new string[] {"Scrap", "Store"}, OnScrapStoreConfirmation);
        }

        void StopMovingHoveredObject (bool successMove) {
            movedObject.SetActive(true);
            if (successMove) TransformCopyPreview(movedObject.transform);
            DestroyCurrentPreview();
            movedObject = null;
        }

        bool FindSceneItemOnCurrentHoveredInteractable () {
            currentHoveredInScene = null;
            if (actor.interactor.mainInteractor.hoveringInteractable != null) {
                currentHoveredInScene = actor.interactor.mainInteractor.hoveringInteractable.GetComponent<SceneItem>();
            }
            return currentHoveredInScene != null;
        }

        void OnWorkshopCancelAction () {
            if (inBuildMode) {
                // exti build mode, start scrap mode
                DestroyCurrentPreview();
            }
            else {
                if (isMovingObject) {
                    StopMovingHoveredObject(false);
                }
                else {
                    if (FindSceneItemOnCurrentHoveredInteractable()) {
                        AttemptScrapStore();
                    }
                }
            }
        }

        void OnBuildConfirmation(bool used, int value) {
            if (used) {
                // build the selected workshop recipe
                if (value == 0) {       
                    
                    // give buffs, take away crafting items etc...
                    currentSelectedRecipeItem.OnConsume(actor.inventory, 1, 0);

                    // get a scene item representation of the item our recipe gives us
                    SceneItem newCraftedItem = SceneItem.GetSceneItem(currentSelectedWorkshopRecipe.returnItem); 
                    
                    // set it to our preview transform
                    TransformCopyPreview(newCraftedItem.transform);
                }
            }
        }
        

        ItemBehavior currentSelectedRecipeItem;

        void AttemptBuild () {

            //check workshop recipe for item given
            int slotIndex;
            if (actor.inventory.ContainsItem(currentSelectedWorkshopRecipe.returnItem, out slotIndex)) {
                SceneItem droppedSceneItem = actor.inventory.DropItem (currentSelectedWorkshopRecipe.returnItem, 1, getScene:true, slotIndex, true, true);
                TransformCopyPreview(droppedSceneItem.transform);
            }
            else {
                if (actor.inventory.ItemCompositionAvailableInInventoryAfterAutoScrap (currentSelectedWorkshopRecipe.requires, actor, actor)) {
                    GameUI.ShowSelectionPopup(true, CraftingUIHandler.BuildConfirmationText(currentSelectedWorkshopRecipe.requires, "Build", currentSelectedRecipeItem.itemName, actor), new string[] {"Yes", "No"}, OnBuildConfirmation);
                }
                else {
                    GameUI.ShowInGameMessage ("Not enough components to build...", false, UIColorScheme.Normal);
                }
            }
        }

        void StartMovingHoveredObject () {
            movedObject = currentHoveredInScene.gameObject; 
            movedObject.SetActive(false);
            SetNewPreviewTransform(currentHoveredInScene.itemBehavior.previewTransform);
        }

        void OnWorkshopSubmitAction() {
            if (inBuildMode) {
                AttemptBuild();
            }
            else {
                if (isMovingObject) {
                    StopMovingHoveredObject(true);
                }
                else {
                    // start moving scene object if pointing at one
                    if (FindSceneItemOnCurrentHoveredInteractable()) {
                        StartMovingHoveredObject();
                    }
                    // enter build mode
                    else {
                        if (currentSelectedRecipeItem != null) {
                            if (currentSelectedWorkshopRecipe == null) {
                                    Debug.LogError("null recipe item for" + currentSelectedRecipeItem.name);

                            }
                            if (currentSelectedWorkshopRecipe.returnItem == null) {
                                Debug.LogError("null return item for" + currentSelectedWorkshopRecipe.name);
                            }
                            
                            SetNewPreviewTransform(currentSelectedWorkshopRecipe.returnItem.previewTransform);
                        }
                    }
                }
            }
        }

        WorkshopRecipe currentSelectedWorkshopRecipe;

        // if we're not moving an object and we're currently in build mode
        void OnUISelect (GameObject selectedObject, GameObject[] data, object[] customData) {
            currentSelectedRecipeItem = null;
            currentSelectedWorkshopRecipe = null;

            InventorySlot slot = (customData[0] as InventorySlot);
            if (slot != null) {
                currentSelectedRecipeItem = slot.item;
            }
            
            // if (inBuildMode) {
                if (currentSelectedRecipeItem == null) {
                 if (inBuildMode) {
            
                    DestroyCurrentPreview();
                 }
                    return;
                }
                currentSelectedWorkshopRecipe = currentSelectedRecipeItem.FindItemBehavior<WorkshopRecipe>();
                if (currentSelectedWorkshopRecipe == null) {
                    currentSelectedRecipeItem = null;

                    if (inBuildMode) {
            
                    DestroyCurrentPreview();
                    }
                    Debug.LogError("workShopRecipe == null");
                    return;
                }
                if (inBuildMode) {
            
                Transform newPreviewPrefab = currentSelectedWorkshopRecipe.returnItem.previewTransform;
                int id = newPreviewPrefab.GetInstanceID();
                if (id != currentPreviewID) {
                    Destroy(currentPreviewTransform.gameObject);
                    currentPreviewTransform = GameObject.Instantiate(newPreviewPrefab);
                    currentPreviewID = id;
                }
                }
            // }
        }

    
        Transform currentPreviewTransform;
        int currentPreviewID;
        GameObject movedObject;
        
        void TransformCopyPreview (Transform t) {
            t.position = currentPreviewTransform.position;
            t.rotation = currentPreviewTransform.rotation;
        }

        bool isMovingObject { get { return movedObject != null; } }
        bool inBuildMode { get { return currentPreviewTransform != null && !isMovingObject; } }



        Renderer[] currentPreviewRenderers;

        void DestroyCurrentPreview () {




            






            RenderTricks.ObjectOutlines.UnHighlight_Renderers( currentPreviewRenderers );
            currentPreviewRenderers = null;

            Destroy(currentPreviewTransform.gameObject);
            currentPreviewTransform = null;        
            currentPreviewID = -1;
            // actor.interactor.mainInteractor.findInteractables = true;
            // actor.interactor.mainInteractor.StopFindingEnvironmentPoint();
            actor.interactor.mainInteractor.findInteractables = true;


            
       
        





        }
        void SetNewPreviewTransform(Transform previewPrefab) {
            currentPreviewTransform = GameObject.Instantiate(previewPrefab);

            currentPreviewRenderers = currentPreviewTransform.GetComponentsInChildren<Renderer>();
            RenderTricks.ObjectOutlines.Highlight_Renderers( currentPreviewRenderers, highlightGroup );
            

            currentPreviewID = previewPrefab.GetInstanceID();
            // disable our interactor
            actor.interactor.mainInteractor.findInteractables = false;
            // Actor.interactor.mainInteractor.FindEnvironmentPointWithRay(environmentMask);

        }
    }
}
