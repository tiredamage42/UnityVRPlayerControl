using System.Collections.Generic;
using UnityEngine;


using SimpleUI;
using Game.PerkSystem;
namespace Game.GameUI {

    // TODO: order game value modifiers by :
        // set, add, add percentage, multiply
        // when adding percentage : add percentage of base value so things stack correctly
    // TODO: add game modifier message for ui showing



    /*
        TODO: make quest that initializes perk table available to player into player perk holder
    */

    public class UIPerksTable : UISelectableElementHandler
    {
        // TODO: move to game settings
        [NeatArray] public NeatStringArray specialNames;
        GameValueModifier addPerkPointModifier;
        protected void OnEnable () {
            addPerkPointModifier = new GameValueModifier (GameValue.GameValueComponent.BaseValue, GameValueModifier.ModifyBehavior.Add, 1.0f);
        }


        protected override int ParamsLength () { return 2; }
        protected override bool CheckParameters (object[] parameters) {
            if ((parameters[0] as Actor) == null) {
                Debug.LogError("Cant open " + GetType().ToString() + " ui handler, actorToShow null");
                return false;
            }
            if ((parameters[1] as PerkHandler) == null) {
                Debug.LogError("Cant open " + GetType().ToString() + " ui handler, perkHandlerController null");
                return false;
            }   
            return true;
        }

        protected override void OnOpenUI(object[] parameters) { 
            Actor actorToShow = parameters[0] as Actor;
            BuildButtons(actorToShow.actorName + " Perks", true, 0, parameters);
            BuildButtons(actorToShow.actorName + " Special", false, 1, parameters);
        }



        

        // int clickAttempts;
        protected override void OnUISelect (GameObject[] data, object[] customData) { 
            // clickAttempts = 0;
            if (customData != null) {

                int panelIndex = (int)customData[1];
                object[] updateParams = customData[2] as object[];
                
                Actor actorToShow = updateParams[0] as Actor;
                PerkHandler perkHandlerController = updateParams[1] as PerkHandler;
            
                //showing perks
                if (panelIndex == 0) {
                    PerkHolder perkHolder = customData[0] as PerkHolder;  
                    
                    string textToShow = perkHolder.perk.displayName + "\n\n" + perkHolder.perk.description;
                    
                    for (int i = 0; i < perkHolder.perk.descriptions.list.Length; i++) {
                        textToShow += "Level " + (i+1) + ": " + perkHolder.perk.descriptions.list[i] + "\n";
                    }

                    textToShow += "Current Level: " + perkHolder.level + " / " + (perkHolder.perk.levels+1);

                    if (perkHandlerController.perkPoints > 0 && perkHolder.level < perkHolder.perk.levels+1) {
                        textToShow += "\n\nClick To Add Perk Point";
                    }
                    
                    textToShow += "\n\nCurrent Perk Points: " + perkHandlerController.perkPoints;

                    (uiObject as SimpleUI.ElementHolderCollection).textPanel.SetText(textToShow);
                }
                // showing special
                else {
                    GameValue specialValue = customData[0] as GameValue;   
                    string textToShow = specialValue.description + "\n\n" + specialValue.baseValue + " / " + specialValue.baseMinMax.y;
                    
                    if (perkHandlerController.perkPoints > 0 && specialValue.baseValue < specialValue.baseMinMax.y) {
                        textToShow += "\n\nClick To Add Perk Point";
                    }
                    textToShow += "\n\nCurrent Perk Points: " + perkHandlerController.perkPoints;

                    // TODO: add modifiers to show
                    (uiObject as SimpleUI.ElementHolderCollection).textPanel.SetText(textToShow);

                }
            }
        }
        
        protected override object[] GetDefaultColdOpenParams() { 
            return new object[] {  myActor, myActor.perkHandler }; 
        }

        protected override List<object> BuildButtonObjectsListForDisplay (int panelIndex, object[] updateButtonsParams) { 
            
            Actor actorToShow = updateButtonsParams[0] as Actor;
            
            List<object> r = new List<object>();

            //build perks list
            if (panelIndex == 0) {
                PerkHandler perkHandler = actorToShow.perkHandler;
                for (int i = 0; i < perkHandler.allPerks.Count; i++) {
                    if (perkHandler.allPerks[i].perk.playerEdit || perkHandler.allPerks[i].level > 0) {
                        r.Add(perkHandler.allPerks[i]);
                    }
                }
            }
            else {

                for (int i =0; i < specialNames.list.Length; i++) {
                    GameValue v = actorToShow.GetGameValue(specialNames.list[i]);
                    if (v != null) {
                        r.Add(v);
                    }
                }
            }
            return r;
        }

        protected override string GetDisplayForButtonObject(object obj) { 
            PerkHolder holder = obj as PerkHolder;
            if (holder != null) {
                return holder.perk.displayName;
            }
            else {
                return (obj as GameValue).name;
            }
        }

        const int attemptChangeAction = 0;


        object[] currentUpdateParams;
        int currentPanelIndex;
        PerkHolder currentPerkHolder;
        GameValue currentGameValue;
        void OnConfirmationSelection(bool used, int selectedOption) {
            if (used && selectedOption == 0) {

                PerkHandler perkHandlerController = currentUpdateParams[1] as PerkHandler;
                perkHandlerController.perkPoints--;
                if (currentPanelIndex == 0) {
                    currentPerkHolder.SetLevel(currentPerkHolder.level+1, currentUpdateParams[0] as Actor); 
                }
                else {
                    currentGameValue.AddModifier (addPerkPointModifier, 1, Vector3Int.zero);
                }
                UpdateUIButtons( 0, currentUpdateParams );
                UpdateUIButtons( 1, currentUpdateParams );
            }
        }

        void ShowConfirmationForPerkPoint(string msg, int currentPanelIndex, GameValue currentGameValue, PerkHolder currentPerkHolder, object[] currentUpdateParams) {
            this.currentPanelIndex = currentPanelIndex;
            this.currentUpdateParams = currentUpdateParams;
            this.currentPerkHolder = currentPerkHolder;
            this.currentGameValue = currentGameValue;

            UIManager.ShowSelectionPopup(msg, new string[] {"Yes", "No"}, OnConfirmationSelection);
        }

        

        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input, int actionOffset) {
            if (customData != null) {

                if (input.x == attemptChangeAction+actionOffset) {

                    object[] updateParams = customData[2] as object[];
                    PerkHandler perkHandlerController = updateParams[1] as PerkHandler;
                    
                    if (perkHandlerController.perkPoints > 0) {

                        Actor actorToShow = updateParams[0] as Actor;
                        int panelIndex = (int)customData[1];
                        
                        //showing perks
                        if (panelIndex == 0) {
                            PerkHolder perkHolder = customData[0] as PerkHolder;   

                            int currentLevel = perkHolder.level;

                            if (currentLevel < perkHolder.perk.levels+1) {

                                ShowConfirmationForPerkPoint(
                                    "\n\nAre you sure you want to add a perk point into " + perkHolder.perk.displayName + " ?\n"
                                    , panelIndex, null, perkHolder, updateParams);


                                // if (clickAttempts == 1) {
                                //     clickAttempts = 0;
                                //     perkHandlerController.perkPoints--;
                                //     perkHolder.SetLevel(currentLevel+1, actorToShow); 
                                //     UpdateUIButtons( 0, updateParams );
                                //     UpdateUIButtons( 1, updateParams );
                         
                                // }
                                // else {
                                //     uiObject.textPanel.SetText("\n\nAre you sure you want to add a perk point into " + perkHolder.perk.displayName + " ?\n\nClick again to confirm.\nSelect another Perk to cancel...");
                                //     clickAttempts = 1;
                                // }
                            }
                        }
                        // showing special
                        else {
                            GameValue specialValue = customData[0] as GameValue;   
                            if (specialValue.baseValue < specialValue.baseMinMax.y) {

                                ShowConfirmationForPerkPoint(
                                    "\n\nAre you sure you want to add a perk point into " + specialValue.name + " ?\n"
                                    , panelIndex, specialValue, null, updateParams);


                                // if (clickAttempts == 1) {
                                //     clickAttempts = 0;
                                //     perkHandlerController.perkPoints--;
                                //     specialValue.AddModifier (addPerkPointModifier, 1, Vector3Int.zero);

                                //     UpdateUIButtons( 0, updateParams );
                                //     UpdateUIButtons( 1, updateParams );
                                // }
                                // else {
                                //     uiObject.textPanel.SetText("\n\nAre you sure you want to add a perk point into " + specialValue.name + " ?\n\nClick again to confirm.\nSelect another Perk to cancel...");
                                //     clickAttempts = 1;
                                // }
                            }
                        }
                    }
                }
            }
		}
    }    
}    