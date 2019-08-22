using UnityEngine;
using SimpleUI;
using GameBase;

namespace Game.UI {

    // TODO: main menu is destroyed... needs a ton of work
    // maybe a ui handler that takes in a list of main menu items... some of them re update with 
    // menu item.children

    public class MainMenu : MonoBehaviour
    {
        public UIPage mainMenuBasePage;

        public void Quit()
        {
            GameManager.QuitApplication();
        }

        void OnEnable ()  {
            GameManager.onPauseRoutineEnd += OnPauseRoutineEnd;


            // mainMenuBasePage.onBaseCancel = OnCancelMainMenuPage;
            // mainMenuBasePage.onBaseCancel += OnCancelMainMenuPage;
        }
        void OnDisable ()  {
            GameManager.onPauseRoutineEnd -= OnPauseRoutineEnd;
            // mainMenuBasePage.onBaseCancel -= OnCancelMainMenuPage;
            
        }

        void OnCancelMainMenuPage () {
            GameManager.TogglePause();
        }

        void OnPauseRoutineEnd(bool isPaused, float routineTime) {
            if (isPaused) {
                UIManager.ShowUI (mainMenuBasePage);//, true);//, true);
            }
            else {
                UIManager.HideUI (mainMenuBasePage);
            }
        }
    }

    
}
