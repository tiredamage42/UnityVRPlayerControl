using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleUI;
using Game.InventorySystem;

namespace Game.GameUI {


    /*
        
    
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


    /*
    
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
*/

    /*
        TODO: Add close ui on game pause
    */

    public abstract class InventoryManagementUIHandler : UISelectableElementHandler//<Inventory.InventorySlot>
    {
        protected override void OnOpenUI(object[] parameters) {
            OnOpenInventoryUI(parameters[0] as Inventory, (int)parameters[1], parameters[2] as Inventory, parameters[3] as List<int>);
        }

        protected override object[] GetDefaultColdOpenParams () { return new object[] { myActor.inventory, -1, null, null }; }
        protected override int ParamsLength() { return 4; }

        // TODO: limit cold open for things that need these extra parameters
        protected override bool CheckParameters (object[] parameters) {
            if ((parameters[0] as Inventory) == null) {
                Debug.LogError("Cant open " + GetType().ToString() + " ui handler, suppliedInventory null");
                return false;
            }
            return true;
        }

        protected abstract void OnOpenInventoryUI(Inventory inventory, int usingEquipPoint, Inventory otherInventory, List<int> categoryFilter);
        
        protected abstract List<InventorySlot> BuildInventorySlotsForDisplay (int uiIndex, Inventory shownInventory, List<int> categoryFilter);
        
        protected override List<object> BuildButtonObjectsListForDisplay(int panelIndex, object[] updateButtonsParams) {
            List<InventorySlot> s = BuildInventorySlotsForDisplay(panelIndex, (Inventory)updateButtonsParams[0], (List<int>)updateButtonsParams[2]);
            List<object> r = new List<object>();
            for (int i = 0; i < s.Count; i++) {
                r.Add(s[i]);
            }
            return r;
        }

        protected override string GetDisplayForButtonObject(object buttonObject) {
            InventorySlot slot = buttonObject as InventorySlot;
            return slot.item.itemName + " ( x"+slot.count+" )";
        }
    }
}
