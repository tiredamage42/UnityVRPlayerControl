using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleUI;
using InventorySystem;

namespace GameUI {


    /*
        stats page:
            
            button per each game value to keep track of
            
            text panel:
                TODO: game value description
                text panel shows value / max

        inventory page:

            already done

        perks panel:

            foreach perk in perk table (perks available in game for player)
                button flair if max perk level reached


            if (clicked button with perk) 
            {
                (make sure double check before spending perk points)
                
                actor.increasePerkLevel(buttons perk);
                perkhandler.perkpoint--;
            }

            text panel shows 
                perk level descriptions

                current level / max level

                current perk points amount
                

            perk as gameobject
                can have script fragments that query values

        quests panel

            full trade double window

            current quests ---------- completed quests

            text panel shows quest description

                current objective hint for current selected

            button per current active quests

            on clicked button with quest {
                quest handler. make current quest(the one showing)
            }

        map page:
            just show map....

                custom cursor in simple ui map 
                    (pos += selection axis * delta time speed)
                    check for pos within any map markers, then show (enlarge a bit, and show text)

                min max world grid cell, 
                tshow arrow with same rotation as player camera

                also show any map markers (quest objectives, map marker components in scene, if discovered)

                
                on click () {
                    // toggles marker active or not
                    custom map marker quest.SetMarker (calculate world pos, or world pos of map marker if selected) 
                }

                // have different visuals for objective marker and custom map marker....
                map marker component {
                    static list of all active markers


                    is active

                    draw in compass within distance

                    draw in game within distance



                    draw map marker () {
                        if (player positin within draw distance) {
                            enable visual canvas
                            make sure it stays the same size on screen
                        }
                        esle disable
                    }
                }

                compass ui:
                    reference looking transform = camera


                    show within angle = 90;


                    if (draw compass) {

                        for each world space NSEW :
                            if (dirspot angle with forward < show within angle){

                                bool to left = (marker angle with right > 90)
                                angle = marker angle with forward * (toleft ? -1 : 1)

                                int lerpinto compass space = Mathf.InverseLerp(-show within angle* .5, show within angle*.5, angle)
                            }
                    }



                    for all marker components {

                        marker.drawMapMarkerInGame(marker distance)

                        if (drawing compass) {
                            if (marker distance < marker.draw in compass eihtin distance ) {
                                if (marker angle with forward < show within angle){

                                    bool to left = (marker angle with right > 90)
                                    angle = marker angle with forward * (toleft ? -1 : 1)

                                    int lerpinto compass space = Mathf.InverseLerp(-45, 45, angle)
                                }
                            }
                        }
                    }







                custom map marker quest {

                    map_marker_component markerTransform; //build if null at runtime
                        show in game = true;



                    void SetMarker (Vector3 worldPos) {
                        if (markerActive) {
                            markeractive = false;
                        }
                        else {
                            markerActive = true

                            markertransform.position = worldPos
                        }
                    }

                       
                    override Vector3[] GetHintPositions () {
                        if (map_marker_component.markerActive)
                            return new Vector3[] { markerTransform.position }
                        else 
                            return null
                    }
                }

        radio panel:

            foreach radio station in actor all available stations

                if (button click with radio station):
                    if already on turn off
                        workingradio.SwitchStation(button station)

                player actor has radio ("pip boy")

            Radio {
                station 

                is on;

                void OnSwitchSong (clip newSong, RadioManager.StationManager stationManager) {
                    audioSource.stop();
                    audio source play (currentsong)
                }

                void SwitchStation(Station newStation) {
                    if (is on) {
                        RadioManager.UnsubscribeFromSTation(OnSwitchSong);
                    }
                    station = newStation
                    TurnOn()
                }

                void OnDisable () {
                    RadioManager.UnsubscribeFromSTations(OnSwitchSong);

                }
                void OnEnable () {
                    if (ison) {
                        TurnOn ();
                    }
                }

                void TurnOff () {

                    RadioManager.UnsubscribeFromSTations(OnSwitchSong);

                    effectSource.Play(radio stopEffect);
                    audioSource.Stop();
                }
                void TurnOn () {
                    effectSource.Play(radio startEffect);

                    radio start delay;

                    if (RadioManager.instance.RadioWithinStationRange(radioposition, station)) {

                        float currentsong timer = RadioManager.QueryPlayTime(station);
                        clip currentsong = RadioManager.QuerySong(station);
                        RadioManager.SubscribeToSTation(station, OnSwitchSong);


                        audioSource.stop();
                        audio source play (currentsong, currentsong timer)

                    }
                    else {
                        play static effect
                    }
                }
            }


            Radio Manager {
                update stations[]


                QueryStationTime (station) {
                    return update stations(station).currentsong timer
                }

                void Update () {
                    foreach station in update stations:


                        currentSongTimer += time.delta time;
                        if (currentSongTimer >= currentsongDuration) {
                            int choosenew song = choose from station
                            newCurrentSongDuration = station.songs[choosnewsong].duration;
                            currentSongTimer = 0;

                            on switch song(new song chosen)
                        }


                }
            }
                    


    */

    [System.Serializable] public class PerkHolder {
        public Perk perk;
        public int level;
        public PerkHolder (Perk perk, int level) {
            this.perk = perk;
            this.level = level;
        }
    }

        
    public class PerkHandler : MonoBehaviour {
        public int perkPoints; // maybe make this a game value so we can use it in condition checks ?
        public List<PerkHolder> allPerks = new List<PerkHolder>();

        public bool HasPerk (Perk perk, out int index) {
            for (int i = 0; i < allPerks.Count; i++) {
                if (allPerks[i].perk == perk) {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }
        public bool HasPerk (string perkDisplayName, int minLevel) {
            for (int i = 0; i < allPerks.Count; i++) {
                if (allPerks[i].perk.displayName == perkDisplayName) {
                    return allPerks[i].level >= minLevel;
                }
            }
            return false;
        }

        public void AddPerk (Perk perk) {
            if (HasPerk(perk, out _))
                return;
            
            allPerks.Add(new PerkHolder(perk, 1));
        }

        public void IncreasePerkLevel (Perk perk) {
            int index;
            if (HasPerk(perk, out index)) {
                allPerks[index].level++;
            }
            else {
                AddPerk(perk);
            }
        }
        public void DecreasePerkLevel (Perk perk) {
            int index;
            if (HasPerk(perk, out index)) {
                allPerks[index].level--;
                if (allPerks[index].level <= 0) {
                    allPerks.Remove(allPerks[index]);
                }
            }
        } 
        public void RemovePerk (Perk perk) {
            int index;
            if (HasPerk(perk, out index)) {
                allPerks.Remove(allPerks[index]);
            }
        }

        public object[] QueryPerksForValues ( string queryContext, object[] parameters, object[] originalValues) {
            for (int i = 0; i < allPerks.Count; i++) {
                originalValues = allPerks[i].perk.QueryPerkForValues(queryContext, allPerks[i].level, parameters, originalValues);
            }
            return originalValues;
        }



    }

    // TODO: come up with a system to keep track of query contexts for scripting ease
    
    // TODO: come up with a condition check for has perk level

    public abstract class PerkScript : MonoBehaviour {
        public abstract object[] QueryPerkForValues ( string queryContext, int perkLevel, object[] parameters, object[] originalValues);
    }


    //stored as prefab  
    public abstract class Perk : MonoBehaviour {
        public string displayName;
        [Header("Per Level")] [NeatArray] public NeatStringArray descriptions; // per level
        public int levels { get { return descriptions.list.Length; } }
        
        PerkScript[] scripts;
        public object[] QueryPerkForValues ( string queryContext, int perkLevel, object[] parameters, object[] originalValues) {
            if (scripts == null) scripts = GetComponents<PerkScript>();

            for (int i = 0; i < scripts.Length; i++) {
                originalValues = scripts[i].QueryPerkForValues ( queryContext, perkLevel, parameters, originalValues);
            }
            return originalValues;
        }
    }







    

    /*
        TODO: Add close ui on game pause
    */

    public abstract class InventoryManagementUIHandler : UISelectableElementHandler
    {

    #if UNITY_EDITOR
        public static string[] GetHandlerInputNames (string context) {
            InventoryManagementUIHandler handler = GetUIHandlerByContext(context);
            return handler != null ? handler.inputNames : null;
        }
    #endif
        public static InventoryManagementUIHandler GetUIHandlerByContext (string context) {
            InventoryManagementUIHandler[] allHandlers = GameObject.FindObjectsOfType<InventoryManagementUIHandler>();
            for (int i = 0; i < allHandlers.Length; i++) {
                if (allHandlers[i].context == context) {
                    return allHandlers[i];
                }
            }
            return null;
        }
        
        // public string[] inputNames;
        [NeatArray] public NeatStringArray inputNames;
        public int maxButtons = 8;
        public string context;
        public bool usesRadial;

        Inventory inventory;
        
        protected System.Func<Vector2Int> getUIInputs;
        public void SetUIInputCallback (System.Func<Vector2Int> callback) {
            getUIInputs = callback;
        }

        protected bool CheckForGetInputCallback () {
            if (getUIInputs == null) {
                Debug.LogError("cant open " + context + " UI, no getUIInputs callback supplied");
                return false;
            }
            return true;
        }


        protected abstract void OnUIInput (GameObject[] data, object[] customData, Vector2Int input);

        const int maxInventories = 2; // need 2 for trade, cant think of a situation for any more
        protected SelectableElement[][] inventoryButtonsPerInventory = new SelectableElement[maxInventories][];
        protected int[] currentPaginatedOffsets = new int[maxInventories];

        
        protected virtual void OnEnable () {
            inventory = GetComponent<Inventory>();
            inventory.onInventoryManagementInitiate += _OnInventoryManagementInitiate;
            inventory.onEndInventoryManagement += _OnEndInventoryManagement;
            //subscripbe to close when game paused
        }

        public void CloseUI () {
            _OnEndInventoryManagement(null, -1, null);
        }
    
        protected virtual void OnDisable () {
            inventory.onInventoryManagementInitiate -= _OnInventoryManagementInitiate;
            inventory.onEndInventoryManagement -= _OnEndInventoryManagement;
        }

        public System.Func<Inventory, int, Inventory, List<int>, bool> shouldOpenCheck;
        public System.Func<Inventory, int, bool> shouldCloseCheck;

        [HideInInspector] public int workingWithEquipID = -1;
        protected List<int> usingCategoryFilter;



        
        void _OnInventoryManagementInitiate (Inventory inventory, int usingEquipPoint, Inventory otherInventory, string context, List<int> categoryFilter) {

            if (this.context != context) return;
            if (!CheckForGetInputCallback()) return;
            if (UIObjectActive()) return;

            if (shouldOpenCheck != null && !shouldOpenCheck(inventory, usingEquipPoint, otherInventory, categoryFilter)) return;
            
            if (UIManager.AnyUIOpen()) return;
            
            usingCategoryFilter = categoryFilter;
            workingWithEquipID = usingEquipPoint;
            InitializeCallbacksForUIs();
            OnInventoryManagementInitiate(inventory, usingEquipPoint, otherInventory, categoryFilter);
            BroadcastUIOpen();
        }



        void InitializeCallbacksForUIs ( ) {
            //reset pagination offsets
            for (int i = 0; i < currentPaginatedOffsets.Length; i++) currentPaginatedOffsets[i] = 0;
            
            UIManager.ShowUI(uiObject, true, !usesRadial);

            uiObject.onBaseCancel = CloseUI;

            uiObject.runtimeSubmitHandler = getUIInputs;
            
            uiObject.SubscribeToSubmitEvent(OnUIInput);
            
            uiObject.SubscribeToSelectEvent(OnUISelect);
            
            if (!usesRadial) {
                uiObject.SubscribeToSelectEvent(OnPaginatedUISelect);
            }
        }

        protected void SetUpButtons (Inventory forInventory, Inventory linkedInventory, int uiIndex, int otherIndex, bool setSelection, UIElementHolder uiObject, List<int> categoryFilter){
            if (uiObject == null)
                uiObject = this.uiObject;

            if (inventoryButtonsPerInventory[uiIndex] == null) 
                inventoryButtonsPerInventory[uiIndex] = uiObject.GetAllSelectableElements(maxButtons);
            
            if (setSelection)
                UIManager.SetSelection(inventoryButtonsPerInventory[uiIndex][0].gameObject);


            UpdateUIButtons(forInventory, linkedInventory, uiIndex, otherIndex, categoryFilter);
        }

        protected abstract void OnInventoryManagementInitiate(Inventory inventory, int usingEquipPoint, Inventory otherInventory, List<int> categoryFilter);

        
        void _OnEndInventoryManagement(Inventory inventory, int usingEquipPoint, string context) {
            
            if (context != null && this.context != context) return;
            if (!UIObjectActive()) return;
            if (shouldCloseCheck != null && !shouldCloseCheck(inventory, usingEquipPoint)) return;
            
            UIManager.HideUI(uiObject);
            for (int i = 0; i < inventoryButtonsPerInventory.Length; i++) inventoryButtonsPerInventory[i] = null;
            for (int i = 0; i < currentPaginatedOffsets.Length; i++) currentPaginatedOffsets[i] = 0;
            BroadcastUIClose();
        
            workingWithEquipID = -1;
            usingCategoryFilter = null;
        }

        protected abstract void OnUISelect (GameObject[] data, object[] customData);
        

        // handle paginated scrolling
        void OnPaginatedUISelect (GameObject[] data, object[] customData) {
			if (customData != null) {
                string buttonSelectText = customData[0] as string;
                if (buttonSelectText != null) {

                    Inventory forInventory = (Inventory)customData[1];
                    
                    int uiIndex = (int)customData[3];

                    bool updateButtons = false;
                    SelectableElement newSelection = null;

                    // hovered over the page up button
                    if (buttonSelectText == "BACK") {
                        currentPaginatedOffsets[uiIndex]--;

                        if (currentPaginatedOffsets[uiIndex] != 0) {
                            newSelection = inventoryButtonsPerInventory[uiIndex][1];
                        }

                        updateButtons = true;
                    } 
                    
                    // hovered over the page down button
                    else if (buttonSelectText == "FWD") {
                        currentPaginatedOffsets[uiIndex]++;
                        bool isAtEnd = currentPaginatedOffsets[uiIndex] >= forInventory.allInventory.Count - maxButtons;
                        
                        if (!isAtEnd) {
                            newSelection = inventoryButtonsPerInventory[uiIndex][maxButtons - 2];
                        }
        
                        updateButtons = true;
                    }
                    if (updateButtons){
                        Inventory linkedInventory = (Inventory)customData[2];

                        UpdateUIButtons(forInventory, linkedInventory, uiIndex, (int)customData[4], usingCategoryFilter);
                        
                        if (newSelection != null) {
                            StartCoroutine(SetSelection(newSelection.gameObject));
                        }
                    }
                }   
            }
		}
        IEnumerator SetSelection(GameObject selection) {
            yield return new WaitForEndOfFrame();
            UIManager.SetSelection(selection);
        }

        protected void UnpackButtonData (object[] customData, out Inventory.InventorySlot slot, out Inventory shownInventory, out Inventory linkedInventory, out int uiIndex, out int otherUIIndex)
        {
            slot = customData[0] as Inventory.InventorySlot;
            shownInventory = customData[1] as Inventory;
            linkedInventory = customData[2] as Inventory;
            uiIndex = (int)customData[3];
            otherUIIndex = (int)customData[4];
        }
        
        protected abstract List<Inventory.InventorySlot> BuildInventorySlotsForDisplay (Inventory shownInventory, int uiIndex, List<int> categoryFilter) ;

        protected void UpdateUIButtons (Inventory shownInventory, Inventory linkedInventory, int uiIndex, int otherIndex, List<int> categoryFilter) 
        {
            bool paginate = !usesRadial;
            List<Inventory.InventorySlot> invSlots = BuildInventorySlotsForDisplay ( shownInventory, uiIndex, categoryFilter);
            int invCount = invSlots.Count;
            

            SelectableElement[] elements = inventoryButtonsPerInventory[uiIndex];
            
            int start = 0;
            int end = maxButtons;
            if (paginate) {

                bool isAtEnd = currentPaginatedOffsets[uiIndex] >= invCount - maxButtons;
                bool isAtBeginning = currentPaginatedOffsets[uiIndex] == 0;

                if (!isAtBeginning){
                    MakeButton(elements[0], " [ Page Up ] ", new object[]{ "BACK", shownInventory, linkedInventory, uiIndex, otherIndex });
                    start = 1;
                }
                if (!isAtEnd) {
                    MakeButton(elements[maxButtons-1], "[ Page Down ] ", new object[]{ "FWD", shownInventory, linkedInventory, uiIndex, otherIndex });
                    end = maxButtons - 1;
                }
            }
            
            for (int i = start ; i < end; i++) {
                int inventoryIndex = paginate ? (i-start) + currentPaginatedOffsets[uiIndex] : i;

                if (inventoryIndex < invCount) {
                    MakeButton( elements[i], invSlots[inventoryIndex].item.itemName + " ( x"+invSlots[inventoryIndex].count+" )", new object[] { invSlots[inventoryIndex], shownInventory, linkedInventory, uiIndex, otherIndex } );
                }
                else {
                    MakeButton( elements[i], "Empty", new object[] { null, shownInventory, linkedInventory, uiIndex, otherIndex } );
                }
            }
        }  
    }
}
