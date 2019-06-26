using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Valve.VR;


using SimpleUI;
using GameBase;

namespace GameUI {

    public class MainMenu : MonoBehaviour
    {
        public UIPage mainMenuBasePage;


        public void Quit()
        {
            GameManager.QuitApplication();
        }

        void OnEnable ()  {
            GameManager.onPauseRoutineEnd += OnPauseRoutineEnd;
            // gameManager.onShowGameMessage += OnShowGameMessage;
            mainMenuBasePage.onBaseCancel += OnCancelMainMenuPage;
        }
        void OnDisable ()  {
            GameManager.onPauseRoutineEnd -= OnPauseRoutineEnd;
            // gameManager.onShowGameMessage -= OnShowGameMessage;
            mainMenuBasePage.onBaseCancel -= OnCancelMainMenuPage;
            
        }

        void OnCancelMainMenuPage () {
            GameManager.TogglePause();
        }

        void OnPauseRoutineEnd(bool isPaused, float routineTime) {
            if (isPaused) {
                UIManager.ShowUI (mainMenuBasePage, true, true);
            }
            else {

                UIManager.HideUI (mainMenuBasePage);
            }
        }
    }

    
}
