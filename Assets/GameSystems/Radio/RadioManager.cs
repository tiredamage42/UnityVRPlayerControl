using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.RadioSystem {

    [System.Serializable] public class RuntimeRadioStation {
        
        public RadioStation station;

        public RuntimeRadioStation (RadioStation station) {
            this.station = station;
        }

        public event System.Action<AudioClip> onSwitchSong;
        float songTimer;
        float currentSongDuration;
        int currentSongIndex;

        public AudioClip currentSong { get { return station.songs[currentSongIndex]; } }
        
        public AudioClip GetCurrentSong (out float songTimer) {
            songTimer = this.songTimer;
            return currentSong;
        }

        void PickNextSong () {
            if (station.randomizeSongs) {
                currentSongIndex = Random.Range(0, station.songs.Length);
            }
            else {
                currentSongIndex++;
                if (currentSongIndex >= station.songs.Length) {
                    currentSongIndex = 0;
                }
            }
        }

        public void UpdateStation (float deltaTime) {
            songTimer += deltaTime;
            if (songTimer >= currentSongDuration) {
                PickNextSong();
                currentSongDuration = currentSong.length;
                songTimer = 0;
                if (onSwitchSong != null) {
                    onSwitchSong(currentSong);
                }
            }
        }
    }
    public class RadioManager : MonoBehaviour
    {
        static RadioManager _instance;
        public static RadioManager instance {
            get {
                if (_instance == null) _instance = GameObject.FindObjectOfType<RadioManager>();
                return _instance;
            }
        }
        static bool NullCheck () {
            if (instance == null) {
                Debug.LogError("No Radio Manager Instance in the scene!!!");
                return true;
            }
            return false;
        }

        public List<RuntimeRadioStation> allAvailableRadioStations = new List<RuntimeRadioStation>();
        public List<RadioTower> allTowers = new List<RadioTower>();



        static System.Action<RadioStation> onStationChosen;
        static void OnRadioStationUIChosen (bool used, int index) {
            if (used) {
                onStationChosen(instance.allAvailableRadioStations[index].station);
            }
        }
        static string[] BuildStationNames () {
            string[] r = new string[instance.allAvailableRadioStations.Count];
            for (int i = 0; i< instance.allAvailableRadioStations.Count; i++) {
                r[i] = instance.allAvailableRadioStations[i].station.displayName;
            }
            return r;
        }
        public static void BringUpStationChooseUI (System.Action<RadioStation> onStationChosen) {
            RadioManager.onStationChosen = onStationChosen;
            Game.UI.GameUI.ShowSelectionPopup(true, "Radio Stations:", BuildStationNames(), OnRadioStationUIChosen);
        }
        


        void Update () {
            float deltaTime = Time.deltaTime;
            for (int i = 0; i< allAvailableRadioStations.Count; i++) {
                allAvailableRadioStations[i].UpdateStation (deltaTime);
            }
        }
        public static bool RadioWithinStationRange (Vector3 radioPosition, RadioStation station, out bool withinAnyRadioTower) {
            withinAnyRadioTower = false;
            if (NullCheck()) return false;
            return instance._RadioWithinStationRange ( radioPosition,  station, out withinAnyRadioTower) ;
        }
        public static bool RadioWithinAnyTowerRange (Vector3 radioPosition) {
            if (NullCheck()) return false;
            return instance._RadioWithinAnyTowerRange ( radioPosition) ;
        }
            
        public static void AddTower (RadioTower tower) {
            if (NullCheck()) return;
            instance._AddTower ( tower) ;
        }
        public static void RemoveTower(RadioTower tower) {
            if (NullCheck()) return;
            instance._RemoveTower( tower) ;
        }

        public static void AddRadioStation (RadioStation station) {
            if (NullCheck()) return;
            instance._AddRadioStation ( station) ;
        }
        public static void RemoveRadioStation (RadioStation station) {
            if (NullCheck()) return;
            instance._RemoveRadioStation ( station) ;
        }

        public static void SubscribeToStationSwitch(RadioStation station, System.Action<AudioClip> onSwitch) {
            if (NullCheck()) return;
            instance._SubscribeToStationSwitch( station,  onSwitch) ;
        }
        public static void UnSubscribeStationSwitch(System.Action<AudioClip> onSwitch) {
            if (NullCheck()) return;
            instance._UnSubscribeStationSwitch( onSwitch) ;
        }

        public static AudioClip QuerySong (RadioStation station, out float songTimer) {
            songTimer = 0;
            if (NullCheck()) return null;
            return instance._QuerySong(station, out songTimer);
        }

        AudioClip _QuerySong (RadioStation station, out float songTimer) {
            songTimer = 0;
            RuntimeRadioStation found = GetRuntimeStation(station);
            if (found == null) return null;
            return found.GetCurrentSong(out songTimer);
        }


        bool _RadioWithinAnyTowerRange (Vector3 radioPosition) {
            for (int i = 0; i< allTowers.Count; i++) {
                if (allTowers[i].isOn) {
                    if (Vector3.SqrMagnitude(radioPosition - allTowers[i].transform.position) <= allTowers[i].range * allTowers[i].range) {
                        return true;
                    }
                }
            }
            return false;
        }

        bool _RadioWithinStationRange (Vector3 radioPosition, RadioStation station, out bool withinAnyRadioTower) {
            withinAnyRadioTower = false;
            for (int i = 0; i< allTowers.Count; i++) {
                if (allTowers[i].isOn) {
                    if (Vector3.SqrMagnitude(radioPosition - allTowers[i].transform.position) <= allTowers[i].range * allTowers[i].range) {
                        withinAnyRadioTower = true;
                        
                        if (allTowers[i].stationsToBroadcast.Contains(station)) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
            
         void _AddTower (RadioTower tower) {
            if (!allTowers.Contains(tower)) {
                allTowers.Add(tower);
            }
        }
         void _RemoveTower(RadioTower tower) {
            allTowers.Remove(tower);
        }

        RuntimeRadioStation GetRuntimeStation (RadioStation station) {
            for (int i =0 ; i < allAvailableRadioStations.Count; i++) {
                if (allAvailableRadioStations[i].station == station) {
                    return allAvailableRadioStations[i];
                }
            }
            return null;
        }
         void _AddRadioStation (RadioStation station) {
            if (GetRuntimeStation(station) != null) return;
            allAvailableRadioStations.Add(new RuntimeRadioStation(station));
        }
         void _RemoveRadioStation (RadioStation station) {
            RuntimeRadioStation found = GetRuntimeStation(station);
            if (found == null) return;
            allAvailableRadioStations.Remove(found);
        }

         void _UnSubscribeStationSwitch(System.Action<AudioClip> onSwitch) {
            for (int i =0 ; i < allAvailableRadioStations.Count; i++) {
                allAvailableRadioStations[i].onSwitchSong -= onSwitch;
            }
        }
        void _SubscribeToStationSwitch(RadioStation station, System.Action<AudioClip> onSwitch) {
            RuntimeRadioStation found = GetRuntimeStation(station);
            if (found == null) return;
            found.onSwitchSong += onSwitch;

        }
    }
}
