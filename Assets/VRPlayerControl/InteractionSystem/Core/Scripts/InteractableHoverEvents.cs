//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Sends UnityEvents for basic hand interactions
//
//=============================================================================

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( Interactable ) )]
	public class InteractableHoverEvents : MonoBehaviour
	{
		[UnityEditor.MenuItem("PINGS/Ping %.")]
        static void Ping()
        {
            Object obj = GameObject.FindObjectOfType<InteractableHoverEvents>();
            UnityEditor.EditorGUIUtility.PingObject(obj);
            UnityEditor.Selection.activeObject = obj;
            Debug.Log(obj);
            // MyScript[] allFoundScripts = Resources.FindObjectsOfTypeAll<MyScript>();
        }


		public UnityEvent onHandHoverBegin;
		public UnityEvent onHandHoverEnd;
		public UnityEvent onAttachedToHand;
		public UnityEvent onDetachedFromHand;

		Interactable interactable;
		void Awake () {
			interactable = GetComponent<Interactable>();
		}


		void OnEnable () {
			interactable.onEquipped += OnEquipped;
			interactable.onUnequipped += OnUnequipped;
		}
		void OnDisable () {
			interactable.onEquipped -= OnEquipped;
			interactable.onUnequipped -= OnUnequipped;
		}


		//-------------------------------------------------
		private void OnHandHoverBegin()
		{
			onHandHoverBegin.Invoke();
		}


		//-------------------------------------------------
		private void OnHandHoverEnd()
		{
			onHandHoverEnd.Invoke();
		}


		//-------------------------------------------------
		private void OnEquipped( Object hand )
		{
			onAttachedToHand.Invoke();
		}


		//-------------------------------------------------
		private void OnUnequipped( Object hand )
		{
			onDetachedFromHand.Invoke();
		}
	}
}
