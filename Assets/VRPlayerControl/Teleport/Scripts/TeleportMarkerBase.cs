
using UnityEngine;
namespace Valve.VR.InteractionSystem
{
	public abstract class TeleportMarkerBase : MonoBehaviour
	{
		public bool locked = false;
		public bool markerActive = true;
		public void SetLocked( bool locked )
		{
			this.locked = locked;
			UpdateVisuals();
		}
		public abstract void UpdateVisuals();
		public abstract void Highlight( bool highlight );
		public abstract void SetAlpha( float tintAlpha, float alphaPercent );
		public abstract bool ShouldActivate( Vector3 playerPosition );
	}
}
