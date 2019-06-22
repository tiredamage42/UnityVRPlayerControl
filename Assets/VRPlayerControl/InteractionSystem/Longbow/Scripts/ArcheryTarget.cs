using UnityEngine;
using UnityEngine.Events;

namespace VRPlayerDemo
{
	public class ArcheryTarget : MonoBehaviour
	{
		public UnityEvent onTakeDamage;

		private void ApplyDamage()
		{
			onTakeDamage.Invoke();
		}
		private void FireExposure()
		{
			onTakeDamage.Invoke();	
		}
		private void OnDamageTaken()
		{
			onTakeDamage.Invoke();			
		}
	}
}
