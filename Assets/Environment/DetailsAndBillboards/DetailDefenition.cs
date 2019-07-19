﻿using UnityEngine;

namespace RenderTools {
	[CreateAssetMenu()] public class DetailDefenition : ScriptableObject {
		[System.Serializable] public class LODInfo {
			public Mesh mesh;
			public Material[] materials;
			public int gridDistance;
			public BillboardAsset billboardAsset;
            public bool isBillboard, castShadows=true, receiveShadows=true;
		}
		public LODInfo[] lods;
	}
}