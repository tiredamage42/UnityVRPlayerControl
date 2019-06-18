using UnityEngine;
using RenderTricks;
using InventorySystem;
namespace Valve.VR.InteractionSystem
{
	public class TagOnEquip : MonoBehaviour, IInventoryItem
	{

		
		public void OnEquipped(Inventory inventory) {
			ColorTag.TagObject(gameObject, Color.red);

		}
        public void OnUnequipped(Inventory inventory){

			ColorTag.UntagObject(gameObject);

		}
        public void OnEquippedUpdate(Inventory inventory){

		}
        // Interactable interactable;
        // void Awake()
		// {
		// 	interactable = GetComponentInParent<Interactable>();
		// }
		// void OnEnable()
		// {
		// 	interactable.onEquipped += AttachedToHand;
		// 	interactable.onUnequipped += DetachedFromHand;
		// }
		// void OnDisable()
		// {
		// 	interactable.onEquipped -= AttachedToHand;
		// 	interactable.onUnequipped -= DetachedFromHand;
		// }
		// private void AttachedToHand( Object hand )
		// {
		// 	ColorTag.TagObject(gameObject, Color.red);
		// }
		// private void DetachedFromHand( Object hand )
		// {
		// 	ColorTag.UntagObject(gameObject);
		// }
	}
}