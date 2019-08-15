using System.Collections.Generic;
using UnityEngine;


namespace RenderTricks {

	public static class ColorTag {
		const int debugErrorAboveCount = 50;
		const float rimPower = 1.25f;

		static Dictionary<Color, Material> color2TagMaterial = new Dictionary<Color, Material>();

		static Shader _shader; 
		static Shader tagShader {
			get {
				if (_shader == null) _shader = Shader.Find("Custom/Tagged");
				return _shader;
			}
		}
		static Material GetNewMaterial(Color color) {
			Material _mat = new Material(tagShader);
			_mat.renderQueue = 2001;// Rendered after geometry
			_mat.SetFloat("_RimPower", rimPower);
			_mat.SetColor("_Color", color);
			_mat.enableInstancing = true;
			return _mat;
		}

		static Material GetMaterialForColor (Color color) {
			if (color2TagMaterial.ContainsKey(color)) {
				return color2TagMaterial[color];
			}
			Material newMat = GetNewMaterial(color);
			color2TagMaterial[color] = newMat;
			return newMat;
		}



		// [System.NonSerialized] static Material _mat;
		// static Material tagMaterial {
		// 	get {
		// 		if (_mat == null) {
		// 			_mat = new Material(Shader.Find("Custom/Tagged"));
		// 			_mat.renderQueue = 2001;// Rendered after geometry
		// 			_mat.SetFloat("_RimPower", rimPower);
		// 		}
		// 		return _mat;
		// 	}
		// }

		static Queue<int> availableIndicies = new Queue<int>();
		static List<TagFollower> allTagged = new List<TagFollower>();

		static TagFollower GetAvailableTagFollower () {

			if (availableIndicies.Count > 0) {
				return allTagged[availableIndicies.Dequeue()];
			}
			else {
				TagFollower newFollower = new TagFollower(allTagged.Count);
				allTagged.Add(newFollower);

				if (allTagged.Count > debugErrorAboveCount) Debug.LogError("Above 50 followers....");
				
				return newFollower;
			}
		}
		static void ReturnTagFollowerToPool(TagFollower follower) {
			availableIndicies.Enqueue(follower.key);
		}

		// static Dictionary<GameObject, List<TagFollower>> tagged2Followers = new Dictionary<GameObject, List<TagFollower>>();
		static Dictionary<GameObject, TagFollower> tagged2Followers = new Dictionary<GameObject, TagFollower>();

		public static void TagObject (GameObject gameObject, Color color) {
			Renderer[] allRenderers = gameObject.GetComponentsInChildren<Renderer>();

			if (allRenderers.Length == 0) {
				Debug.LogWarning( "trying to tag " + gameObject.name + " but it has no child renderers" );
				return;
			}

			// List<TagFollower> followersPerObject = new List<TagFollower>();

			TagFollower follower = GetAvailableTagFollower();
			follower.StartFollow(allRenderers, GetMaterialForColor(color));// allRenderers[i].GetComponent<MeshFilter>(), color);
				

			// for (int i = 0; i < allRenderers.Length; i++) {
			// 	TagFollower follower = GetAvailableTagFollower();
			// 	follower.StartFollow(allRenderers[i], GetMaterialForColor(color));// allRenderers[i].GetComponent<MeshFilter>(), color);
			// 	followersPerObject.Add(follower);
			// }

			// tagged2Followers.Add(gameObject, followersPerObject);
			tagged2Followers.Add(gameObject, follower);
		}
		public static void UntagObject (GameObject gameObject) {

			TagFollower follower;
			// List<TagFollower> objectFollowers;
			// if (tagged2Followers.TryGetValue(gameObject, out objectFollowers)) {
			
			if (tagged2Followers.TryGetValue(gameObject, out follower)) {
				follower.EndFollow();
				ReturnTagFollowerToPool(follower);
				// for (int i = 0; i< objectFollowers.Count; i++) {
				// 	objectFollowers[i].EndFollow();
				// 	ReturnTagFollowerToPool(objectFollowers[i]);
				// }
			}
		}

		static void UntagRenderer (Renderer r) {
			Material[] materials = r.sharedMaterials;
			int lastIndex = materials.Length - 1;
			if (materials[lastIndex].shader == tagShader) {
				System.Array.Resize(ref materials, lastIndex);
				r.sharedMaterials = materials;
			}
		}
		static void TagRenderer (Renderer r, Material tagMaterial) {
			Material[] materials = r.sharedMaterials;
			
			int lastIndex = materials.Length - 1;
			if (materials[lastIndex].shader != tagShader) {
				System.Array.Resize(ref materials, materials.Length+1);
				materials[materials.Length - 1] = tagMaterial;
				r.sharedMaterials = materials;
			}
		}


		class TagFollower {
			public int key;
			// GameObject rootObj;
			Renderer[] taggedRenderers;

			// SkinnedMeshRenderer skinnedRenderer;
			// MeshRenderer meshRenderer;
			// MeshFilter meshFilter;


			// bool isAvailable;

			// public bool isAvailable {
			// 	get {
			// 		return !rootObj.activeSelf && rootObj.transform.parent == null;
			// 	}
			// }
			
			public void EndFollow () {
				// isAvailable = true;
				// rootObj.transform.SetParent(null);
				// rootObj.SetActive(false);

				for (int i = 0; i < taggedRenderers.Length; i++) {
					UntagRenderer(taggedRenderers[i]);
				}
			}

			
			public void StartFollow(Renderer[] taggedRenderers, Material tagMaterial){//, MeshFilter followFilter, Color color) {
				// isAvailable = false;
				this.taggedRenderers = taggedRenderers;
				for (int i = 0; i < taggedRenderers.Length; i++) {
					TagRenderer(taggedRenderers[i], tagMaterial);
				}



				// rootObj.SetActive(true);
				// ResetTransform(rootObj.transform, followRenderer.transform);


				
				// Renderer usingRenderer = null;
				// SkinnedMeshRenderer sourceSkinnedMeshRenderer = followRenderer as SkinnedMeshRenderer;
				// if ( sourceSkinnedMeshRenderer != null )
				// {
				// 	skinnedRenderer.sharedMesh = sourceSkinnedMeshRenderer.sharedMesh;
				// 	skinnedRenderer.rootBone = sourceSkinnedMeshRenderer.rootBone;
				// 	skinnedRenderer.bones = sourceSkinnedMeshRenderer.bones;
				// 	skinnedRenderer.quality = sourceSkinnedMeshRenderer.quality;
				// 	skinnedRenderer.updateWhenOffscreen = sourceSkinnedMeshRenderer.updateWhenOffscreen;
				// 	skinnedRenderer.gameObject.SetActive(true);
				// 	meshFilter.gameObject.SetActive(false);
				// 	usingRenderer = skinnedRenderer;
				// }
				// else {
				// 	if (followFilter == null) {
				// 		EndFollow();
				// 		return;
				// 	}
				// 	skinnedRenderer.gameObject.SetActive(false);
				// 	meshFilter.sharedMesh = followFilter.sharedMesh;
				// 	meshFilter.gameObject.SetActive(true);
				
				// 	MeshRenderer sourceMeshRenderer = followRenderer as MeshRenderer;
				// 	meshRenderer.gameObject.SetActive(true);
				// 	usingRenderer = meshRenderer;
				// }

				// int materialCount = followRenderer.sharedMaterials.Length;
				// Material[] destRendererMaterials = new Material[materialCount];
				// for ( int i = 0; i < materialCount; i++ ) {
				// 	destRendererMaterials[i] = tagMaterial;
				// }
				// usingRenderer.sharedMaterials = destRendererMaterials;

				// Material[] copied = usingRenderer.materials;
				// for ( int i = 0; i < copied.Length; i++ ) {
				// 	copied[i].color = color;
				// }
					

				// Material[] followedCopied = followRenderer.materials;
				// for ( int i = 0; i < followedCopied.Length; i++ ) {
				// 	if ( followedCopied[i].renderQueue == 2000 )
				// 		followedCopied[i].renderQueue = 2002;
				// }
			
			}

			// void ResetTransform(Transform t, Transform parent) {
			// 	t.SetParent(parent);
			// 	t.localPosition = Vector3.zero;
			// 	t.localRotation = Quaternion.identity;
			// 	t.localScale = Vector3.one;
			// }

			public TagFollower (int key) {
				this.key = key;
				// rootObj = new GameObject( "tagFollower" );
				// GameObject sk = new GameObject("skinned");
				// GameObject mr = new GameObject("meshrend");

				// ResetTransform(sk.transform, rootObj.transform);
				// ResetTransform(mr.transform, rootObj.transform);

				// skinnedRenderer = sk.gameObject.AddComponent<SkinnedMeshRenderer>();
				// meshFilter = mr.gameObject.AddComponent<MeshFilter>();
				// meshRenderer = mr.gameObject.AddComponent<MeshRenderer>();
				
				// rootObj.SetActive(false);
			}
		}
	}
}