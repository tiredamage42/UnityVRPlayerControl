using UnityEngine;

using GameUI;
using SimpleUI;
// using Valve.VR;

namespace VRPlayer.UI {

    public class VRUIInventoryBuilders : MonoBehaviour {
        public GameObject inventoryUIsObject;

        public TransformBehavior pageWithPanelTransform, fullTradeTransform, subTitlesTransform;
        
        void Awake () {
            if (inventoryUIsObject == null)
            {
                Debug.LogError("no inventory uis object specified");
                return;
            }

            float textScale = .0025f;
            float width = 3f;

            UIPageParameters pageParams = new UIPageParameters("Fix Page Title", .25f, width, TextAnchor.MiddleLeft, .05f);

            UIElementHolder pageWPanelUI = VRUI.MakeButtonsPage("PageWPanel", null, pageParams,
                new TextPanelParameters(new Vector2(width, 4), new Vector2(.1f, .1f), textScale, TextAnchor.UpperLeft, 64),
                true, pageWithPanelTransform, textScale
            );

            CraftingUIHandler[] craftingUIs = inventoryUIsObject.GetComponents<CraftingUIHandler>();
            for (int i = 0; i < craftingUIs.Length; i++) craftingUIs[i].SetUIObject(pageWPanelUI);
            
            // ScrappingUIHandler scrappingUI = inventoryUIsObject.GetComponent<ScrappingUIHandler>();
            // scrappingUI.SetUIObject(pageWPanelUI);

            FullInventoryUIHandler fullInventoryUI = inventoryUIsObject.GetComponent<FullInventoryUIHandler>();
            fullInventoryUI.SetUIObject(pageWPanelUI);

            FullTradeUIHandler fullTradeUI = inventoryUIsObject.GetComponent<FullTradeUIHandler>();
            fullTradeUI.SetUIObject(VRUI.MakeFullTradeUI("FullTrade", pageParams, fullTradeTransform, textScale));

            QuickInventoryUIHandler quickInventoryUI = inventoryUIsObject.GetComponent<QuickInventoryUIHandler>();
            quickInventoryUI.SetUIObject(VRUI.MakeButtonsPage("QuickInvRadial", new UIRadialParameters(.8f, .1f), null, null, false, null, .0035f));
            
            QuickTradeUIHandler quickTradeUI = inventoryUIsObject.GetComponent<QuickTradeUIHandler>();
            quickTradeUI.SetUIObject(VRUI.MakeButtonsPage("QuickTrade", null, new UIPageParameters("Fix Page Title", .25f, width, TextAnchor.MiddleCenter, 0), null, false, null, .0025f));

            DialoguePlayerUIHandler dialogueHandler = inventoryUIsObject.GetComponent<DialoguePlayerUIHandler>();
            dialogueHandler.SetUIObject(VRUI.MakeButtonsPage("Dialogue", null, new UIPageParameters("", .25f, 4, TextAnchor.MiddleLeft, 0.05f), null, false, null, .0025f));

            VRUI.MakeSubtitles("Subtitles", new UISubtitlesParameters(.005f, .4f, 3, 64, 9), subTitlesTransform);
        }
    }
}