using UnityEngine;
using RenderTricks;

namespace Valve.VR.InteractionSystem
{
	public class TagOnEquip : MonoBehaviour
	{
		Interactable interactable;
        void Awake()
		{
			interactable = GetComponentInParent<Interactable>();
		}
		void OnEnable()
		{
			interactable.onEquipped += AttachedToHand;
			interactable.onUnequipped += DetachedFromHand;
		}
		void OnDisable()
		{
			interactable.onEquipped -= AttachedToHand;
			interactable.onUnequipped -= DetachedFromHand;
		}
		private void AttachedToHand( Object hand )
		{
			ColorTag.TagObject(gameObject, Color.red);
		}
		private void DetachedFromHand( Object hand )
		{
			ColorTag.UntagObject(gameObject);
		}
	}
}