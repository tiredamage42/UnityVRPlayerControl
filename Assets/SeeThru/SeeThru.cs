//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Uses the see thru renderer while attached to hand
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Valve.VR.InteractionSystem
{

	//-------------------------------------------------------------------------
	public class SeeThru : MonoBehaviour
	{
		
		public Material seeThruMaterial;

		Interactable interactable;
		Renderer sourceRenderer, seeThruRenderer;
		

		//-------------------------------------------------
		void OnEnable()
		{
			interactable.onEquipped += AttachedToHand;
			interactable.onUnequipped += DetachedFromHand;
		}


		//-------------------------------------------------
		void OnDisable()
		{
			interactable.onEquipped -= AttachedToHand;
			interactable.onUnequipped -= DetachedFromHand;
		}


		//-------------------------------------------------
		private void AttachedToHand( Object hand )
		{
			seeThruRenderer.gameObject.SetActive( true );
		}


		//-------------------------------------------------
		private void DetachedFromHand( Object hand )
		{
			seeThruRenderer.gameObject.SetActive( false );
		}



		void Awake()
		{
			interactable = GetComponentInParent<Interactable>();

			//
			// Create child game object for see thru renderer
			//
			GameObject seeThru = new GameObject( "_see_thru" );
			seeThru.transform.parent = transform;
			seeThru.transform.localPosition = Vector3.zero;
			seeThru.transform.localRotation = Quaternion.identity;
			seeThru.transform.localScale = Vector3.one;

			//
			// Copy mesh filter
			//
			MeshFilter sourceMeshFilter = GetComponent<MeshFilter>();
			if ( sourceMeshFilter != null )
			{
				MeshFilter destMeshFilter = seeThru.AddComponent<MeshFilter>();
				destMeshFilter.sharedMesh = sourceMeshFilter.sharedMesh;
			}

			//
			// Copy mesh renderer
			//
			MeshRenderer sourceMeshRenderer = GetComponent<MeshRenderer>();
			if ( sourceMeshRenderer != null )
			{
				sourceRenderer = sourceMeshRenderer;
				seeThruRenderer = seeThru.AddComponent<MeshRenderer>();
			}

			//
			// Copy skinned mesh renderer
			//
			SkinnedMeshRenderer sourceSkinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
			if ( sourceSkinnedMeshRenderer != null )
			{
				SkinnedMeshRenderer destSkinnedMeshRenderer = seeThru.AddComponent<SkinnedMeshRenderer>();

				sourceRenderer = sourceSkinnedMeshRenderer;
				seeThruRenderer = destSkinnedMeshRenderer;

				destSkinnedMeshRenderer.sharedMesh = sourceSkinnedMeshRenderer.sharedMesh;
				destSkinnedMeshRenderer.rootBone = sourceSkinnedMeshRenderer.rootBone;
				destSkinnedMeshRenderer.bones = sourceSkinnedMeshRenderer.bones;
				destSkinnedMeshRenderer.quality = sourceSkinnedMeshRenderer.quality;
				destSkinnedMeshRenderer.updateWhenOffscreen = sourceSkinnedMeshRenderer.updateWhenOffscreen;
			}

			//
			// Create see thru materials
			//
			if ( sourceRenderer != null && seeThruRenderer != null )
			{
				int materialCount = sourceRenderer.sharedMaterials.Length;
				Material[] destRendererMaterials = new Material[materialCount];
				for ( int i = 0; i < materialCount; i++ )
				{
					destRendererMaterials[i] = seeThruMaterial;
				}
				seeThruRenderer.sharedMaterials = destRendererMaterials;

				for ( int i = 0; i < seeThruRenderer.materials.Length; i++ )
				{
					seeThruRenderer.materials[i].renderQueue = 2001; // Rendered after geometry
				}

				for ( int i = 0; i < sourceRenderer.materials.Length; i++ )
				{
					if ( sourceRenderer.materials[i].renderQueue == 2000 )
					{
						sourceRenderer.materials[i].renderQueue = 2002;
					}
				}
			}

			seeThru.gameObject.SetActive( false );
		}


		


		//-------------------------------------------------
		void Update()
		{
			Debug.Log("USING SEE THRU");
			if ( seeThruRenderer.gameObject.activeInHierarchy )
			{
				Material[] seeThruMaterials = seeThruRenderer.materials;
				Material[] origMaterials = sourceRenderer.materials;
				int materialCount = Mathf.Min( origMaterials.Length, seeThruMaterials.Length );
				for ( int i = 0; i < materialCount; i++ )
				{
					seeThruMaterials[i].mainTexture = origMaterials[i].mainTexture;
					seeThruMaterials[i].color = seeThruMaterials[i].color * origMaterials[i].color;
				}
			}
		}
	}
}
