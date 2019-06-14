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
	public static class ColorTag {
		const int debugErrorAboveCount = 50;
		static Material _mat;
		static Material tagMaterial {
			get {
				if (_mat == null) {
					_mat = new Material(Shader.Find("Valve/VR/SeeThru"));
					_mat.renderQueue = 2001;// Rendered after geometry
				}
				return _mat;
			}
		}

		static Queue<int> availableIndicies = new Queue<int>();
		static List<TagFollower> allTagged = new List<TagFollower>();

		static TagFollower GetAvailableTagFollower () {

			if (availableIndicies.Count > 0) {
				return allTagged[availableIndicies.Dequeue()];
			}
			else {
				TagFollower newFollower = new TagFollower(allTagged.Count);
				allTagged.Add(newFollower);
				if (allTagged.Count > debugErrorAboveCount) {
					Debug.LogError("Above 50 followers....");
				}
				return newFollower;
			}
		}
		static void ReturnTagFollowerToPool(TagFollower follower) {
			availableIndicies.Enqueue(follower.key);
		}


		static Dictionary<GameObject, List<TagFollower>> tagged2Followers = new Dictionary<GameObject, List<TagFollower>>();

		public static void TagObject (GameObject gameObject, Color color) {
			Renderer[] allRenderers = gameObject.GetComponentsInChildren<Renderer>();

			if (allRenderers.Length == 0) {
				Debug.LogWarning( "trying to tag " + gameObject.name + " but it has no child renderers" );
				return;
			}

			List<TagFollower> followersPerObject = new List<TagFollower>();

			for (int i = 0; i < allRenderers.Length; i++) {
				TagFollower follower = GetAvailableTagFollower();
				follower.StartFollow(allRenderers[i], allRenderers[i].GetComponent<MeshFilter>(), color);
				followersPerObject.Add(follower);
			}

			tagged2Followers.Add(gameObject, followersPerObject);
		}
		public static void UntagObject (GameObject gameObject) {
			List<TagFollower> objectFollowers;
			if (tagged2Followers.TryGetValue(gameObject, out objectFollowers)) {
				for (int i = 0; i< objectFollowers.Count; i++) {
					objectFollowers[i].EndFollow();
					ReturnTagFollowerToPool(objectFollowers[i]);
				}
			}
		}


		class TagFollower {
			public int key;
			GameObject rootObj;
			SkinnedMeshRenderer skinnedRenderer;
			MeshRenderer meshRenderer;
			MeshFilter meshFilter;

			public bool isAvailable {
				get {
					return !rootObj.activeSelf && rootObj.transform.parent == null;
				}
			}
			
			public void EndFollow () {
				rootObj.transform.SetParent(null);
				rootObj.SetActive(false);
			}

			
			public void StartFollow(Renderer followRenderer, MeshFilter followFilter, Color color) {
				if (followRenderer == null) {
					return;
				}
				rootObj.SetActive(true);
				ResetTransform(rootObj.transform, followRenderer.transform);
				Renderer myRenderer = null;
				
				SkinnedMeshRenderer sourceSkinnedMeshRenderer = followRenderer as SkinnedMeshRenderer;
				if ( sourceSkinnedMeshRenderer != null )
				{
					skinnedRenderer.sharedMesh = sourceSkinnedMeshRenderer.sharedMesh;
					skinnedRenderer.rootBone = sourceSkinnedMeshRenderer.rootBone;
					skinnedRenderer.bones = sourceSkinnedMeshRenderer.bones;
					skinnedRenderer.quality = sourceSkinnedMeshRenderer.quality;
					skinnedRenderer.updateWhenOffscreen = sourceSkinnedMeshRenderer.updateWhenOffscreen;
					skinnedRenderer.gameObject.SetActive(true);
					meshFilter.gameObject.SetActive(false);

					myRenderer = skinnedRenderer;
				}
				else {
					if (followFilter == null) {
						EndFollow();
						return;
					}
					skinnedRenderer.gameObject.SetActive(false);

					meshFilter.sharedMesh = followFilter.sharedMesh;
					meshFilter.gameObject.SetActive(true);
				
					MeshRenderer sourceMeshRenderer = followRenderer as MeshRenderer;
					meshRenderer.gameObject.SetActive(true);

					myRenderer = meshRenderer;
				}

				int materialCount = followRenderer.sharedMaterials.Length;
				Material[] destRendererMaterials = new Material[materialCount];
				for ( int i = 0; i < materialCount; i++ )
				{
					destRendererMaterials[i] = tagMaterial;
				}
				myRenderer.sharedMaterials = destRendererMaterials;

				Material[] copied = myRenderer.materials;
				for ( int i = 0; i < copied.Length; i++ )
				{
					copied[i].color = Color.green;
				
					// copied[i].renderQueue = 2001; // Rendered after geometry
				}

				Material[] followedCopied = followRenderer.materials;
				for ( int i = 0; i < followedCopied.Length; i++ )
				{
					if ( followedCopied[i].renderQueue == 2000 )
					{
						followedCopied[i].renderQueue = 2002;
					}
				}
			
			}

			void ResetTransform(Transform t, Transform parent) {
				t.SetParent(parent);
				t.localPosition = Vector3.zero;
				t.localRotation = Quaternion.identity;
				t.localScale = Vector3.one;
			}

			public TagFollower (int key) {
				this.key = key;
				rootObj = new GameObject( "tagFollower" );
				GameObject sk = new GameObject("skinned");
				GameObject mr = new GameObject("meshrend");

				ResetTransform(sk.transform, rootObj.transform);
				ResetTransform(mr.transform, rootObj.transform);

				skinnedRenderer = sk.gameObject.AddComponent<SkinnedMeshRenderer>();

				meshFilter = mr.gameObject.AddComponent<MeshFilter>();
				meshRenderer = mr.gameObject.AddComponent<MeshRenderer>();
				
				rootObj.SetActive(false);
			}




		}
	}


	// used on longbow (so we can see what we're holding through other objects)
	//-------------------------------------------------------------------------
	public class SeeThru : MonoBehaviour
	{
		// static Material _mat;
		// static Material seeThruMaterial {
		// 	get {
		// 		if (_mat == null) {
		// 			_mat = new Material(Shader.Find("Valve/VR/SeeThru"));
		// 		}
		// 		return _mat;
		// 	}
		// }
		Interactable interactable;
		// Renderer sourceRenderer, seeThruRenderer;
		

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
			ColorTag.TagObject(gameObject, Color.red);
			// seeThruRenderer.gameObject.SetActive( true );
		}


		//-------------------------------------------------
		private void DetachedFromHand( Object hand )
		{
			ColorTag.UntagObject(gameObject);
			// seeThruRenderer.gameObject.SetActive( false );
		}


		void Awake()
		{
			interactable = GetComponentInParent<Interactable>();

			// //
			// // Create child game object for see thru renderer
			// //
			// GameObject seeThru = new GameObject( "_see_thru" );
			// seeThru.transform.parent = transform;
			// seeThru.transform.localPosition = Vector3.zero;
			// seeThru.transform.localRotation = Quaternion.identity;
			// seeThru.transform.localScale = Vector3.one;

			// //
			// // Copy mesh filter
			// //
			// MeshFilter sourceMeshFilter = GetComponent<MeshFilter>();
			// if ( sourceMeshFilter != null )
			// {
			// 	MeshFilter destMeshFilter = seeThru.AddComponent<MeshFilter>();
			// 	destMeshFilter.sharedMesh = sourceMeshFilter.sharedMesh;
			// }

			// //
			// // Copy mesh renderer
			// //
			// MeshRenderer sourceMeshRenderer = GetComponent<MeshRenderer>();
			// if ( sourceMeshRenderer != null )
			// {
			// 	sourceRenderer = sourceMeshRenderer;
			// 	seeThruRenderer = seeThru.AddComponent<MeshRenderer>();
			// }

			// //
			// // Copy skinned mesh renderer
			// //
			// SkinnedMeshRenderer sourceSkinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
			// if ( sourceSkinnedMeshRenderer != null )
			// {
			// 	SkinnedMeshRenderer destSkinnedMeshRenderer = seeThru.AddComponent<SkinnedMeshRenderer>();

			// 	sourceRenderer = sourceSkinnedMeshRenderer;
			// 	seeThruRenderer = destSkinnedMeshRenderer;

			// 	destSkinnedMeshRenderer.sharedMesh = sourceSkinnedMeshRenderer.sharedMesh;
			// 	destSkinnedMeshRenderer.rootBone = sourceSkinnedMeshRenderer.rootBone;
			// 	destSkinnedMeshRenderer.bones = sourceSkinnedMeshRenderer.bones;
			// 	destSkinnedMeshRenderer.quality = sourceSkinnedMeshRenderer.quality;
			// 	destSkinnedMeshRenderer.updateWhenOffscreen = sourceSkinnedMeshRenderer.updateWhenOffscreen;
			// }

			// //
			// // Create see thru materials
			// //
			// if ( sourceRenderer != null && seeThruRenderer != null )
			// {
			// 	int materialCount = sourceRenderer.sharedMaterials.Length;
			// 	Material[] destRendererMaterials = new Material[materialCount];
			// 	for ( int i = 0; i < materialCount; i++ )
			// 	{
			// 		destRendererMaterials[i] = seeThruMaterial;
			// 	}
			// 	seeThruRenderer.sharedMaterials = destRendererMaterials;

			// 	for ( int i = 0; i < seeThruRenderer.materials.Length; i++ )
			// 	{
			// 		seeThruRenderer.materials[i].renderQueue = 2001; // Rendered after geometry
			// 	}

			// 	for ( int i = 0; i < sourceRenderer.materials.Length; i++ )
			// 	{
			// 		if ( sourceRenderer.materials[i].renderQueue == 2000 )
			// 		{
			// 			sourceRenderer.materials[i].renderQueue = 2002;
			// 		}
			// 	}
			// }

			// seeThru.gameObject.SetActive( false );
		}

		//-------------------------------------------------
		void Update()
		{
			// if ( seeThruRenderer.gameObject.activeInHierarchy )
			// {
			// 	Material[] seeThruMaterials = seeThruRenderer.materials;
			// 	Material[] origMaterials = sourceRenderer.materials;
			// 	int materialCount = Mathf.Min( origMaterials.Length, seeThruMaterials.Length );
			// 	for ( int i = 0; i < materialCount; i++ )
			// 	{
			// 		seeThruMaterials[i].color = Color.green;
			// 	}
			// }
		}
	}
}
