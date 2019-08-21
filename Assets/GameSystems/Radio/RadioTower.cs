using System.Collections;
using System.Collections.Generic;
using UnityEngine;



using InteractionSystem;

using Game.UI;

namespace Game.RadioSystem {

   


    // TODO: make destructible, give health etc....
    public class RadioTower : MonoBehaviour, IInteractable {
        

        public int GetInteractionMode() { return 0; }

    public void OnInteractableAvailabilityChange(bool available) { }
    public void OnInteractableInspectedStart (InteractionPoint interactor) {
        
    }
    public void OnInteractableInspectedEnd (InteractionPoint interactor) {
        
    }

    public void OnInteractableInspectedUpdate (InteractionPoint interactor) { }
    public void OnInteractableUsedStart (InteractionPoint interactor, int useAction) { 

        if (useAction == enableAction) {
            Debug.LogError("toggling state");
            ToggleState();
        }
        else  if (useAction == optionsAction) {
            Debug.LogError("options");
            BringUpOptionsMenu();
        }
    }



    public int enableAction = 0;
    public int optionsAction = 1;

        
    
    public void OnInteractableUsedEnd (InteractionPoint interactor, int useAction) { }
    public void OnInteractableUsedUpdate (InteractionPoint interactor, int useAction) { }


        void OnRadioStationRemoveChosen (bool used, int index) {
            if (used) {
                RadioStation removed = stationsToBroadcast[index];
                OnRemoveStation(removed);
                GameUI.ShowInGameMessage ("Removed "+ removed.displayName, false, UIColorScheme.Normal);
            }
        }
        string[] BuildStationRemoveNames () {
            string[] r = new string[stationsToBroadcast.Count];
            for (int i = 0; i< stationsToBroadcast.Count; i++) {
                r[i] = stationsToBroadcast[i].displayName;
            }
            return r;
        }
        public void BringUpStationRemoveUI () {
            Game.UI.GameUI.ShowSelectionPopup(true, "Remove Station:", BuildStationRemoveNames(), OnRadioStationRemoveChosen);
        }

        void OnRadioStationAddChosen (bool used, int index) {
            Debug.LogError("chose station maybe used");
                
            if (used) {
                Debug.LogError("chose station");
                RadioStation added = RadioManager.instance.allAvailableRadioStations[index].station;
                if (!stationsToBroadcast.Contains(added)) {
                    OnAddStation(added);
                    GameUI.ShowInGameMessage ("Added "+ added.displayName, false, UIColorScheme.Normal);
                }
            }
        }
        string[] BuildStationAddNames () {
            List<RuntimeRadioStation> allAvailableRadioStations = RadioManager.instance.allAvailableRadioStations;
            string[] r = new string[allAvailableRadioStations.Count];
            for (int i = 0; i< allAvailableRadioStations.Count; i++) {
                RadioStation station = allAvailableRadioStations[i].station;
                if (stationsToBroadcast.Contains(station)) {
                    r[i] = station.displayName + " - [Already Added]";
                }
                else {
                    r[i] = station.displayName;
                }
            }
            return r;
        }
        public void BringUpStationAddUI () {

            Game.UI.GameUI.ShowSelectionPopup(true, "Add Station:", BuildStationAddNames(), OnRadioStationAddChosen);
        }

        void OnOptionsMenuChoose (bool used, int index) {
            if (used) {
                if (index == 0) {
                    BringUpStationAddUI();
                }
                else if (index == 1) {
                    BringUpStationRemoveUI();
                }
                // cancel
                else if (index == 2) {

                }
            }
        }
        
        void BringUpOptionsMenu () {
            Game.UI.GameUI.ShowSelectionPopup(true, "Radio Tower Options:", new string[] { "Add Station", "Remove Station", "Cancel" }, OnOptionsMenuChoose);
        }
        










        void ToggleState() {
            isOn = !isOn;
        }

        public List<RadioStation> stationsToBroadcast = new List<RadioStation>();
        
        public float range = 100;

        public bool isOn;

        void OnAddStation (RadioStation station) {
            Debug.LogError("addign station " + station.name);
            stationsToBroadcast.Add(station);
        }
        void OnRemoveStation (RadioStation station) {
            stationsToBroadcast.Remove(station);
        }


        void OnEnable () {
            RadioManager.AddTower(this);
        }
        void OnDisable () {
            RadioManager.RemoveTower(this);
        }
    
    }
}