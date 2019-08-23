using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractionSystem;


namespace Game.RadioSystem {

    // TODO: emission turn on for radio material...
    public class Radio : MonoBehaviour, IInteractable {

    public int GetInteractionMode() { return 0; }

    public void OnInteractableAvailabilityChange(bool available) { }
    public void OnInteractableInspectedStart (InteractionPoint interactor) {
        
    }
    public void OnInteractableInspectedEnd (InteractionPoint interactor) {
        
    }

    public void OnInteractableInspectedUpdate (InteractionPoint interactor) { }
    public void OnInteractableUsedStart (InteractionPoint interactor, int useAction) { 

        if (useAction == enableAction) {
            ToggleState();
        }
        else  if (useAction == switchAction) {
            RadioManager.BringUpStationChooseUI (SwitchStation);
        }
    }
    public void OnInteractableUsedEnd (InteractionPoint interactor, int useAction) { }
    public void OnInteractableUsedUpdate (InteractionPoint interactor, int useAction) { }




        public int enableAction = 0;
        public int switchAction = 1;

        public RadioStation currentStation;
        public bool isOn;

        public AudioClip turnOnClip, turnOffClip, staticEffect;
        public AudioSource fxSource, songSource;

        void PlaySongClip (AudioClip songClip, float offset = 0) {
            songSource.loop = false;
            songSource.Stop();
            songSource.clip = songClip;
            songSource.time = offset;
            songSource.Play();
        }



        // called by radio manager
        void OnSwitchSong (AudioClip newSong) {
            if (isOn) {
                PlaySongClip(newSong);
            }
            else {
                RadioManager.UnSubscribeStationSwitch(OnSwitchSong);
            }

        }

        public void SwitchStation(RadioStation newStation) {
            if (isOn) RadioManager.UnSubscribeStationSwitch(OnSwitchSong);
            
            currentStation = newStation;
            TurnOn();
        }

        void OnDisable () {
            RadioManager.UnSubscribeStationSwitch(OnSwitchSong);
        }

        void OnEnable () {
            if (isOn) {
                TurnOn ();
            }
        }

        void ToggleState () {
            if (isOn) TurnOff();
            else TurnOn();
        }
        public void TurnOff () {
            isOn = false;
            RadioManager.UnSubscribeStationSwitch(OnSwitchSong);
            fxSource.PlayOneShot(turnOffClip);
            songSource.Stop();
        }

        void CatchUpWithStation () {
            float currentSongTimer;
            AudioClip currentsong = RadioManager.QuerySong(currentStation, out currentSongTimer);
            RadioManager.SubscribeToStationSwitch(currentStation, OnSwitchSong);
            PlaySongClip(currentsong, currentSongTimer);
        }


        public void TurnOn () {
            isOn = true;
            fxSource.PlayOneShot(turnOnClip);
            // TODO: warm up volume effect
            withinStation = CheckDistance(currentStation, out withinAnyRadioTower);
            PlayStationStartOrStatic();
            
        }

        void PlayStationStartOrStatic () {
            if (withinStation) {
                CatchUpWithStation();
            }
            else {
                PlaySongClip(staticEffect);
                songSource.loop = true;

            }
        }

        const float checkRadioTime = 1f;
        float radioCheckTimer;

        bool withinAnyRadioTower, withinStation;

        bool CheckDistance (RadioStation station, out bool withinAnyRadioTower) {
            return RadioManager.RadioWithinStationRange(transform.position, station, out withinAnyRadioTower);
        }

        public bool StationAvailable (RadioStation station) {
            return StationAvailable(station, out _);
        }
        public bool StationAvailable (RadioStation station, out bool withinAnyRadioTower) {
            withinAnyRadioTower = this.withinAnyRadioTower;
            if (isOn && station == currentStation) {
                return withinStation;
            }
            return CheckDistance(station, out withinAnyRadioTower);
        }
        public bool CurrentStationAvailable () {
            if (isOn) {
                return withinStation;
            }
            else {
                return StationAvailable(currentStation);
            }
        }
        public bool WithinAnyRadioTower () {
            if (isOn) {
                return withinAnyRadioTower;
            }
            return RadioManager.RadioWithinAnyTowerRange(transform.position);
        }
        void Update(){
            UpdateRadioCheck();
        }

        void UpdateRadioCheck () {
            if (isOn) {

                radioCheckTimer+= Time.deltaTime;

                if (radioCheckTimer >= checkRadioTime) {


                    bool wasWithinStation = withinStation;
                    withinStation = CheckDistance(currentStation, out withinAnyRadioTower);
                    if (withinStation != wasWithinStation) {
                        PlayStationStartOrStatic();
                        
                    }
                    radioCheckTimer = 0;
                }
            }
        }
    }
}