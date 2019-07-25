// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine;
using UnityEditor;
using System;
// using System.Collections;
using System.IO;
using System.Text;
 

namespace CustomEnvironment {

    public class TerrainOBJExporter : EditorWindow
    {
        // enum SaveResolution { Full=0, Half, Quarter, Eighth, Sixteenth } 
        // SaveResolution saveResolution = SaveResolution.Eighth;
        
        // static TerrainData terrain;
        
        // int tCount;
        // int counter;
        // int totalCount;
        const int progressUpdateInterval = 10000;
        
        [MenuItem("GameObject/Terrain/Export To Obj...")]
        static void Init()
        {
            // terrain = null;
            // Terrain terrainObject = Selection.activeObject as Terrain;
            // if (!terrainObject)
            // {
            //     terrainObject = Terrain.activeTerrain;
            // }
            // if (terrainObject)
            // {
            //     terrain = terrainObject.terrainData;
            // }
            EditorWindow.GetWindow<TerrainOBJExporter>().Show();
        }
        
        void OnGUI()
        {
            // if (!terrain)
            // {
            //     GUILayout.Label("No terrain found");
            //     if (GUILayout.Button("Cancel"))
            //     {
            //         EditorWindow.GetWindow<TerrainOBJExporter>().Close();
            //     }
            //     return;
            // }
            // saveResolution = (SaveResolution) EditorGUILayout.EnumPopup("Resolution", saveResolution);
        
            if (GUILayout.Button("Export"))
            {
                Export();
            }
        }

        static void ExportSplatMap (TerrainData terrain, string directory) {
            string assetDir = directory.Substring(Application.dataPath.Length-6);
            string splatTexturePath = assetDir + "SplatTexture.asset";

            // Debug.Log(fileName);
            // Debug.Log(splatTexturePath);

            // return;

            float[,,] alphaMaps = terrain.GetAlphamaps(0,0, terrain.alphamapWidth, terrain.alphamapHeight);
            int alphamapLayers = terrain.alphamapLayers;

            Texture2D splatTexture = new Texture2D(terrain.alphamapWidth, terrain.alphamapHeight);
            Color[] splatColors = new Color[terrain.alphamapWidth * terrain.alphamapHeight];
            // Debug.Log(splatColors.Length);
            // Debug.Log(terrain.alphamapWidth);
            // Debug.Log(terrain.alphamapHeight);
            
            for (int y = 0; y < terrain.alphamapHeight; y++) {

            for (int x = 0; x < terrain.alphamapWidth; x++) {
                
                // float[] alphaValues = new float[]{
                //     alphaMaps[x,y,0],
                //     alphaMaps[x,y,1],
                //     alphaMaps[x,y,2],
                //     alphaMaps[x,y,3],
                // } ;

                splatColors[y * terrain.alphamapWidth + x] = new Color(
                    alphamapLayers <= 0 ? 0 : alphaMaps[x,y,0],
                    alphamapLayers <= 1 ? 0 : alphaMaps[x,y,1],
                    alphamapLayers <= 2 ? 0 : alphaMaps[x,y,2],
                    alphamapLayers <= 3 ? 0 : alphaMaps[x,y,3]

                    // alphamapLayers <= 0 ? (byte)0 : (byte)(alphaMaps[x,y,0]/255),
                    // alphamapLayers <= 1 ? (byte)0 : (byte)(alphaMaps[x,y,1]/255),
                    // alphamapLayers <= 2 ? (byte)0 : (byte)(alphaMaps[x,y,2]/255),
                    // alphamapLayers <= 3 ? (byte)0 : (byte)(alphaMaps[x,y,3]/255)
                );
            }   
            }


            splatTexture.SetPixels(splatColors);
            splatTexture.Apply();

            AssetDatabase.CreateAsset(splatTexture, splatTexturePath);
        }


        static void ExportTerrainChunk(string directory, TerrainChunk chunk, Vector3 chunkSize, int chunkRes, int chunkIndex, int gridResolution) {
            
            bool isEven = gridResolution % 2 == 0;

            int lod = 0;
            if (isEven) {
                int halfMap = gridResolution / 2;
                
                
                int coord = Mathf.Max(
                    Mathf.Abs(chunk.chunk.x - halfMap), 
                    Mathf.Abs(chunk.chunk.y - halfMap)
                );


                int thresh = halfMap / 2;

                if (coord > 2){//thresh) {
                    lod = 4;
                }
                else {
                    lod = 0;
                }
                



            }
            else {
                int halfMap = (gridResolution / 2) + 1;
                Vector2Int center = new Vector2Int(halfMap, halfMap);
                
                int dist = EnvironmentTools.GridHandler.GetDistance(center, chunk.chunk);
                
                int thresh = (halfMap / 2);

                if (dist >= thresh) {
                    lod = 4;
                }
                else {
                    lod = 0;
                }
                

            }
            Debug.Log("LOD" + lod);


            // lod = 0;
            
            
            int w = chunkRes;//terrain.heightmapWidth;
            int h = chunkRes;//terrain.heightmapHeight;
            
            string fileName = directory + "TerrainObj" + chunkIndex + ".obj";

            
            Vector3 meshScale = chunkSize;
            // meshScale.y *=10;

            int tRes = (int)Mathf.Pow(2, lod);//(int)saveResolution );
            
            meshScale = new Vector3(meshScale.x / (w - 1) * tRes, meshScale.y, meshScale.z / (h - 1) * tRes);
            Vector2 uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));
            float[,] tData = chunk.heights;
        
            w = (w - 1) / tRes + 1;
            h = (h - 1) / tRes + 1;
            Vector3[] tVertices = new Vector3[w * h];
            Vector2[] tUV = new Vector2[w * h];
        
            int[] tPolys;
            tPolys = new int[(w - 1) * (h - 1) * 6];
        
        
            // Build vertices and UVs
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {

                    // float height = tData[x * tRes, y * tRes];
                    float height = tData[y * tRes, x * tRes];

                    // tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(y, tData[x * tRes, y * tRes], x)) + chunk.localPosition;
                    tVertices[y * w + x] = Quaternion.Euler(0,-90,0) * (Vector3.Scale(meshScale, new Vector3(y, height, x)) + Vector3.Scale(chunk.localPosition, new Vector3(1,1,1)));
                    
                    tUV[y * w + x] = Vector2.Scale( new Vector2(x * tRes, y * tRes), uvScale);
                }
            }
        
            int  index = 0;
            // Build triangle indices: 3 indices into vertex array for each triangle
            for (int y = 0; y < h - 1; y++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                // For each grid cell output two triangles
                // tPolys[index++] = (y * w) + x;
                // tPolys[index++] = ((y + 1) * w) + x;
                // tPolys[index++] = (y * w) + x + 1;
    
                // tPolys[index++] = ((y + 1) * w) + x;
                // tPolys[index++] = ((y + 1) * w) + x + 1;
                // tPolys[index++] = (y * w) + x + 1;


                tPolys[index++] = (y * w) + x + 1;
                tPolys[index++] = ((y + 1) * w) + x;
                tPolys[index++] = (y * w) + x;
    
                tPolys[index++] = (y * w) + x + 1;
                tPolys[index++] = ((y + 1) * w) + x + 1;
                tPolys[index++] = ((y + 1) * w) + x;
                }
            }
            

            // Export to .obj
            StreamWriter sw = new StreamWriter(fileName);
            try
            {
        
                sw.WriteLine("# Unity terrain OBJ File");
        
                // Write vertices
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                int counter = 0; 
                int tCount = 0;
                
                int totalCount = (tVertices.Length * 2 + (tPolys.Length / 3)) / progressUpdateInterval;
                for (int i = 0; i < tVertices.Length; i++)
                {
                    
                    UpdateProgress(ref counter, ref tCount, totalCount);
                    StringBuilder sb = new StringBuilder("v ", 20);
                    // StringBuilder stuff is done this way because it's faster than using the "{0} {1} {2}"etc. format
                    // Which is important when you're exporting huge terrains.
                    sb.Append(tVertices[i].x.ToString()).Append(" ").
                    Append(tVertices[i].y.ToString()).Append(" ").
                    Append(tVertices[i].z.ToString());
                    sw.WriteLine(sb);
                }
                // Write UVs
                for (int i = 0; i < tUV.Length; i++)
                {
                    UpdateProgress(ref counter, ref tCount, totalCount);
                    StringBuilder sb = new StringBuilder("vt ", 22);
                    sb.Append(tUV[i].x.ToString()).Append(" ").
                    Append(tUV[i].y.ToString());
                    sw.WriteLine(sb);
                }
                // Write triangles
                for (int i = 0; i < tPolys.Length; i += 3)
                {
                UpdateProgress(ref counter, ref tCount, totalCount);
                StringBuilder sb = new StringBuilder("f ", 43);
                sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
                    Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
                    Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1);
                sw.WriteLine(sb);
                }
                
            }
            catch(Exception err)
            {
                Debug.Log("Error saving file: " + err.Message);
            }
            sw.Close();
        
            EditorUtility.DisplayProgressBar("Saving file to disc.", "This might take a while...", 1f);
            EditorUtility.ClearProgressBar();

        }
        
        void Export()
        {
            TerrainData terrain = null;
            Terrain terrainObject = Selection.activeObject as Terrain;
            if (!terrainObject)
            {
                terrainObject = Terrain.activeTerrain;
            }
            if (terrainObject)
            {
                terrain = terrainObject.terrainData;
            }
            if (!terrain) {
                Debug.LogWarning("no terrain found...");
                return;
            }
            


            // string fileName = EditorUtility.SaveFilePanel("Export .obj file", "", "Terrain", "obj");
            string directory = EditorUtility.SaveFolderPanel("Export .obj file", "", "");//, "Terrain", "obj");
            if (string.IsNullOrEmpty(directory ) || string.IsNullOrWhiteSpace(directory))
                return;
            
            if (!directory.EndsWith("/"))
                directory += "/";



            // ExportSplatMap ( terrain,  directory);


            // string fileName = directory + "TerrainObj.obj";
            // string assetDir = directory.Substring(Application.dataPath.Length-6);
            // string splatTexturePath = assetDir + "SplatTexture.asset";

            // // Debug.Log(fileName);
            // // Debug.Log(splatTexturePath);

            // // return;

            // float[,,] alphaMaps = terrain.GetAlphamaps(0,0, terrain.alphamapWidth, terrain.alphamapHeight);
            // int alphamapLayers = terrain.alphamapLayers;

            // Texture2D splatTexture = new Texture2D(terrain.alphamapWidth, terrain.alphamapHeight);
            // Color32[] splatColors = new Color32[terrain.alphamapWidth * terrain.alphamapHeight];
            // // Debug.Log(splatColors.Length);
            // // Debug.Log(terrain.alphamapWidth);
            // // Debug.Log(terrain.alphamapHeight);
            
            // for (int y = 0; y < terrain.alphamapHeight; y++) {

            // for (int x = 0; x < terrain.alphamapWidth; x++) {
                
            //     // float[] alphaValues = new float[]{
            //     //     alphaMaps[x,y,0],
            //     //     alphaMaps[x,y,1],
            //     //     alphaMaps[x,y,2],
            //     //     alphaMaps[x,y,3],
            //     // } ;

            //     splatColors[y * terrain.alphamapWidth + x] = new Color32(
            //         alphamapLayers <= 0 ? (byte)0 : (byte)(alphaMaps[x,y,0]/255),
            //         alphamapLayers <= 1 ? (byte)0 : (byte)(alphaMaps[x,y,1]/255),
            //         alphamapLayers <= 2 ? (byte)0 : (byte)(alphaMaps[x,y,2]/255),
            //         alphamapLayers <= 3 ? (byte)0 : (byte)(alphaMaps[x,y,3]/255)
            //     );
            // }   
            // }


            // splatTexture.SetPixels32(splatColors);

            // AssetDatabase.CreateAsset(splatTexture, splatTexturePath);

            Vector3 chunkSize;
            int chunkRes;
            int gridRes = 8;
            List<TerrainChunk> chunks = SplitTerrain(terrainObject, gridRes, out chunkSize, out chunkRes);
            if (chunks == null)
            {
                return;
            }
            for (int i = 0; i < chunks.Count; i++) {
                ExportTerrainChunk(directory, chunks[i], chunkSize, chunkRes, i, gridRes);// saveResolution);
            }

            
            ExportSplatMap ( terrain,  directory);



            // int w = terrain.heightmapWidth;
            // int h = terrain.heightmapHeight;
            // Vector3 meshScale = terrain.size;
            // int tRes = (int)Mathf.Pow(2, (int)saveResolution );
            // meshScale = new Vector3(meshScale.x / (w - 1) * tRes, meshScale.y, meshScale.z / (h - 1) * tRes);
            // Vector2 uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));
            // float[,] tData = terrain.GetHeights(0, 0, w, h);
        
            // w = (w - 1) / tRes + 1;
            // h = (h - 1) / tRes + 1;
            // Vector3[] tVertices = new Vector3[w * h];
            // Vector2[] tUV = new Vector2[w * h];
        
            // int[] tPolys;
            // tPolys = new int[(w - 1) * (h - 1) * 6];
        
        
            // // Build vertices and UVs
            // for (int y = 0; y < h; y++)
            // {
            //     for (int x = 0; x < w; x++)
            //     {
            //         tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(-y, tData[x * tRes, y * tRes], x));
            //         tUV[y * w + x] = Vector2.Scale( new Vector2(x * tRes, y * tRes), uvScale);
            //     }
            // }
        
            // int  index = 0;
            // // Build triangle indices: 3 indices into vertex array for each triangle
            // for (int y = 0; y < h - 1; y++)
            // {
            //     for (int x = 0; x < w - 1; x++)
            //     {
            //     // For each grid cell output two triangles
            //     tPolys[index++] = (y * w) + x;
            //     tPolys[index++] = ((y + 1) * w) + x;
            //     tPolys[index++] = (y * w) + x + 1;
    
            //     tPolys[index++] = ((y + 1) * w) + x;
            //     tPolys[index++] = ((y + 1) * w) + x + 1;
            //     tPolys[index++] = (y * w) + x + 1;
            //     }
            // }
            
        
            // // Export to .obj
            // StreamWriter sw = new StreamWriter(fileName);
            // try
            // {
        
            //     sw.WriteLine("# Unity terrain OBJ File");
        
            //     // Write vertices
            //     System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            //     counter = tCount = 0;
            //     totalCount = (tVertices.Length * 2 + (tPolys.Length / 3)) / progressUpdateInterval;
            //     for (int i = 0; i < tVertices.Length; i++)
            //     {
            //         UpdateProgress();
            //         StringBuilder sb = new StringBuilder("v ", 20);
            //         // StringBuilder stuff is done this way because it's faster than using the "{0} {1} {2}"etc. format
            //         // Which is important when you're exporting huge terrains.
            //         sb.Append(tVertices[i].x.ToString()).Append(" ").
            //         Append(tVertices[i].y.ToString()).Append(" ").
            //         Append(tVertices[i].z.ToString());
            //         sw.WriteLine(sb);
            //     }
            //     // Write UVs
            //     for (int i = 0; i < tUV.Length; i++)
            //     {
            //         UpdateProgress();
            //         StringBuilder sb = new StringBuilder("vt ", 22);
            //         sb.Append(tUV[i].x.ToString()).Append(" ").
            //         Append(tUV[i].y.ToString());
            //         sw.WriteLine(sb);
            //     }
            //     // Write triangles
            //     for (int i = 0; i < tPolys.Length; i += 3)
            //     {
            //     UpdateProgress();
            //     StringBuilder sb = new StringBuilder("f ", 43);
            //     sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
            //         Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
            //         Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1);
            //     sw.WriteLine(sb);
            //     }
                
            // }
            // catch(Exception err)
            // {
            //     Debug.Log("Error saving file: " + err.Message);
            // }
            // sw.Close();
        
            // terrain = null;
            // EditorUtility.DisplayProgressBar("Saving file to disc.", "This might take a while...", 1f);
            // EditorUtility.ClearProgressBar();


            EditorWindow.GetWindow<TerrainOBJExporter>().Close();      
            AssetDatabase.SaveAssets();
            // Debug.Log(fileName);
            // Debug.Log(splatTexturePath);

        }
        
        static void UpdateProgress(ref int counter, ref int tCount, int totalCount)
        {
            if (counter++ == progressUpdateInterval)
            {
                counter = 0;
                EditorUtility.DisplayProgressBar("Saving...", "", Mathf.InverseLerp(0, totalCount, ++tCount));
            }
        }


    class TerrainChunk {

        public Vector2Int chunk;
        public Vector3 localPosition;
        public float[,] heights;
        // public Vector3 size;

        public TerrainChunk(int chunkRes, Vector3 localPosition, Vector2Int chunk){//, Vector3 size) {
            heights = new float[ chunkRes, chunkRes ];
            this.localPosition = localPosition;
            // this.size = size;
            this.chunk = chunk;
			
        }

    }	
        
	List<TerrainChunk> SplitTerrain(Terrain terrain, int gridResolution, out Vector3 chunkSize, out int chunkRes)
	{

        List<TerrainChunk> chunks = new List<TerrainChunk>();
        Vector3 parentPosition = terrain.GetPosition();
        Vector3 parentSize = terrain.terrainData.size;
        int heightMapRes = terrain.terrainData.heightmapResolution;
        
        // int gridResolution = (int) Mathf.Sqrt( terrainsCount );

        //Keep y same
        chunkSize = new Vector3( parentSize.x / gridResolution, parentSize.y, parentSize.z / gridResolution );
			
			
        // Shift calc
        int heightShift = ((heightMapRes - 1) / gridResolution);// + 1;	
        // int heightShift = (heightMapRes) / gridResolution;	
        
        chunkRes = (heightShift) + 1;

        // Debug.Log("heightmapRes" + heightMapRes);
        // if (heightMapRes % gridResolution != 0) {
        //     Debug.LogError("Cant split " + heightMapRes + " into " + gridResolution + " grid res evenly");
        //     return null;
        // }							
        // else {
        //     return null;
        // }
        

        float[,] parentHeight = terrain.terrainData.GetHeights(0,0, heightMapRes, heightMapRes );

        float spaceShiftX = parentSize.x / gridResolution;
        float spaceShiftY = parentSize.z / gridResolution;
        
        int i = 0;

        int terrainsCount = gridResolution * gridResolution;
        for (int y = 0; y < gridResolution; y++) {
            for (int x = 0; x < gridResolution; x++) {
        //     }
        // }
		//Split terrain 
		// for ( int i=0; i< terrainsCount; i++)
		// {										
			
			EditorUtility.DisplayProgressBar("Split terrain","Process " + i, (float) i / terrainsCount );
							
			//Start processing it			
						
			// Translate peace to position
			// float xWShift = ( i % gridResolution ) * spaceShiftX;
			// float zWShift = ( i / gridResolution ) * spaceShiftY;

            // Vector3 chunkPosition = new Vector3(zWShift + parentPosition.x, parentPosition.y, xWShift + parentPosition.z); 	
			Vector3 chunkPosition = new Vector3(x * spaceShiftX, 0, y * spaceShiftY);
			
			// Split height
			#region split height
			
			//Copy heightmap											
			// int new_heightmapResolution = heightMapRes /  gridResolution;	


            TerrainChunk chunk = new TerrainChunk(chunkRes, chunkPosition, new Vector2Int(x, y));//, chunkSize);						
			
			
			
			// float[,] peaceHeight = new float[ heightMapRes / terraPeaces + 1, heightMapRes / terraPeaces + 1 ];
			
            int startX = x * heightShift;
            int startY = y * heightShift;
            
			// int startX = 0;
			// int startY = 0;
			
			// int endX = 0;
			// int endY = 0;
			int endX = startX + chunkRes;
			int endY = startY + chunkRes;
			
            // startX = startY = 0;

			// if ( i==0 )
			// {
			// 	endX = endY = chunkRes;
			// }
			
			// if ( i==1 )
			// {
			// 	endX = heightMapRes / terraPeaces + 1;
			// 	endY = heightMapRes / terraPeaces + 1;
			// }
			
			// if ( i==2 )
			// {
			// 	endX = heightMapRes / terraPeaces + 1;
			// 	endY = heightMapRes / terraPeaces + 1;
			// }
			
			// if ( i==3 )
			// {
			// 	endX = heightMapRes / terraPeaces + 1;
			// 	endY = heightMapRes / terraPeaces + 1;
			// }
            // }}
            
			int xL = 0;				
			// iterate
			for ( int x2=startX;x2< endX;x2++)
			{	
				
				EditorUtility.DisplayProgressBar("Split terrain","Split height", (float) x2 / ( endX - startX ));  
				
                int yL = 0;
				for ( int y2=startY;y2< endY;y2++)
				{
				
					// int xShift=0; 
					// int yShift=0;
					
					// //
					// if ( i==0 )
					// {
					// 	xShift = 0;
					// 	yShift = 0;						
					// }
					
					// //
					// if ( i==1 )
					// {						
					// 	xShift = heightShift;
					// 	yShift = 0;						
					// }
					
					// //
					// if ( i==2 )
					// {
					// 	xShift = 0;
					// 	yShift = heightShift;	
					// }
					
					// if ( i==3 )
					// {
					// 	xShift = heightShift;
					// 	yShift = heightShift;	
					// }
					
					chunk.heights[xL, yL] = parentHeight[ x2, y2 ];

                    yL++;	
				}
                xL++;								
			}

            chunks.Add(chunk);
			
			EditorUtility.ClearProgressBar();
            i++;
			#endregion
		}

    
    }






		EditorUtility.ClearProgressBar();

        return chunks;
	}
	





    }
}