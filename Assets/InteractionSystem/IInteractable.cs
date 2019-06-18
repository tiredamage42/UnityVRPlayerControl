using UnityEngine;
namespace InteractionSystem {
    public interface IInteractable {
        void OnInspectStart(Interactor interactor);
        void OnInspectEnd(Interactor interactor);
        void OnInspectUpdate(Interactor interactor);
        void OnUseStart(Interactor interactor, int useIndex);
        void OnUseEnd(Interactor interactor, int useIndex);
        void OnUseUpdate(Interactor interactor, int useIndex);
    }
}
