using UnityEngine;
namespace InteractionSystem {
    public interface IInteractable {
        void OnInspectedStart(Interactor interactor);
        void OnInspectedEnd(Interactor interactor);
        void OnInspectedUpdate(Interactor interactor);
        void OnUsedStart(Interactor interactor, int useIndex);
        void OnUsedEnd(Interactor interactor, int useIndex);
        void OnUsedUpdate(Interactor interactor, int useIndex);
    }


    // public interface IInteractableInspectHandler {
    //     void OnInspectedStart(Interactor interactor);
    //     void OnInspectedEnd(Interactor interactor);
    //     void OnInspectedUpdate(Interactor interactor);
    // }
    // public interface IInteractableUseHandler {
    //     void OnUsedStart(Interactor interactor, int useIndex);
    //     void OnUsedEnd(Interactor interactor, int useIndex);
    //     void OnUsedUpdate(Interactor interactor, int useIndex);
    // }
}
