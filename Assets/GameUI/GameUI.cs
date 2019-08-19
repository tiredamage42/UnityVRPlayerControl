// using System.Collections;

using System.Collections.Generic;
using UnityEngine;


using SimpleUI;
using Game.PerkSystem;
using Game.DialogueSystem;
using Game.InventorySystem;

namespace Game.UI {


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
        TODO: Add close ui on game pause
    */
    
    public class GameUI
    {
        public static UIPerksTable perksUI { get { return UIPerksTable.instance; } }
        public static UIGameValuePage gameValuesUI { get { return UIGameValuePage.instance; } }
        public static DialoguePlayerUIHandler dialogueResponseUI { get { return DialoguePlayerUIHandler.instance; } }
        public static QuickTradeUIHandler quickTradeUI { get { return QuickTradeUIHandler.instance; } }
        public static QuickInventoryUIHandler quickInventoryUI { get { return QuickInventoryUIHandler.instance; } }
        public static FullTradeUIHandler tradeUI { get { return FullTradeUIHandler.instance; } }
        public static FullInventoryUIHandler inventoryManagementUI { get { return FullInventoryUIHandler.instance; } }        
        public static CraftingUIHandler craftingUI { get { return CraftingUIHandler.instance; } }
        public static UIQuestsPage questsUI { get { return UIQuestsPage.instance; } }

        // IN GAME MESSAGES
        public static void SetUIMessageCenterInstance(UIMessageCenter messagesElement) {
            UIManager.SetUIMessageCenterInstance ( messagesElement );
        }
        public static void ShowInGameMessage(string msg, bool immediate, UIColorScheme schemeType) {
            UIManager.ShowInGameMessage( msg, immediate, schemeType);
        }

        // SUBTITLES
        public static void SetUISubtitlesInstance(UISubtitles subtitlesElement) {
            UIManager.SetUISubtitlesInstance ( subtitlesElement );
        }
        public static void ShowSubtitles(string speaker, string msg) {
            UIManager.ShowSubtitles( speaker,  msg);
        }

        // POPUPS SELECTION
        public static void SetUISelectionPopupInstance(UISelectionPopup selectionPopupElement) {
            UIManager.SetUISelectionPopupInstance ( selectionPopupElement );
        }
        public static void ShowSelectionPopup(string msg, string[] options, System.Action<bool, int> returnValue) {
            UIManager.ShowSelectionPopup( msg,  options,  returnValue);
        }

        // POPUPS SLIDER
        public static void SetUISliderPopupInstance(UISliderPopup sliderElement) {
            UIManager.SetUISliderPopupInstance( sliderElement );
        }
        public static void ShowIntSliderPopup(string title, int minValue, int maxValue, System.Action<bool, int> returnValue) {
            UIManager.ShowIntSliderPopup( title,  minValue,  maxValue, returnValue);
        }

        











    }
}
