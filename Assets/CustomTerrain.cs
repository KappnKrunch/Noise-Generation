using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]

public class CustomTerrain : MonoBehaviour {
    
    public Vector2 randomHeightRange = new Vector2(0,0.1f);
    public Texture2D heightMapImage;
    public Vector3 heightMapScale = new Vector3(1, 1, 1);

    public bool resetTerrain = true;

    //Perlin Noise-----------
    public float perlinXScale = 0.01f;
    public float perlinYScale = 0.01f;
    public int perlinOffsetX = 0;
    public int perlinOffsetY = 0;

    //Fractal Brownian Motion
    public int oct = 2;
    public float persistance = 0.5f;
    public float perlinHeightScale = 1;

    //Multiple perlin 
    [System.Serializable]
    public class PerlinParamaters {

        public float mPerlinXScale = 0.01f;
        public float mPerlinYScale = 0.01f;
        public int mPerlinOffsetX = 0;
        public int mPerlinOffsetY = 0;
        public int mPerlinOctaves = 3;
        public float mPersistance = 8;
        public float mPerlinHeightScale = 0.1f;
        public bool remove = false;
    }
    public List<PerlinParamaters> perlinParameters = new List<PerlinParamaters>() { new PerlinParamaters() };

    //Voronoi
    public float voronoiFallOff = 0.2f;
    public float voronoiDropOff = 0.5f;
    public int peakCount = 3;
    public float minMountainHeight = 0.000001f;
    public float maxMountainHeight = 1;
    public enum VoronoiType { Linear = 0, Combined = 1, Inverse = 2, Dumplings = 3}
    public VoronoiType voronoiType = VoronoiType.Combined;



    //midpoint
    public float MPDHeightMin = -1;
    public float MPDHeightMax = 1;
    public float roughness = 2.0f;
    public float heightDampenerPower = 2.0f;

    public int smoothAmount = 1;


    //SplatMAp
    [System.Serializable]
    public class SplatHeights {
        public bool remove = false;
        public Texture2D texture = null;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0;
        public float maxSlope = 1.5f;

        public Vector2 tileSize = new Vector2(50, 50);
        public Vector2 tileOffset = new Vector2(0,0);
        
        

    }
    public List<SplatHeights> splatHeights = new List<SplatHeights>(){
        new SplatHeights()
    };
    public float splatOffset = 0.01f;

    public float splatNoiseScale = 0.1f;
    public float splatNoiseXScale = 0.01f;
    public float splatNoiseYScale = 0.01f;

    //Vegetation
    [System.Serializable]
    public class VegetableHeights {
        public bool remove = false;
        public GameObject mesh;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0;
        public float maxSlope = 1.5f;

    }
    public int vegetationMaximumTrees = 10;
    public int vegetationTreeSpacing = 10;
    public List<VegetableHeights> vegetationHeights = new List<VegetableHeights>() {
        new VegetableHeights()
    };


    public Terrain terrain;
    public TerrainData terrainData;





    public void AddToVegetation() {
        vegetationHeights.Add(new VegetableHeights());


    }

    public void RemoveFromVegetation() {
        List<VegetableHeights> keptVegetationHeights = new List<VegetableHeights>();
        for (int i = 0; i < vegetationHeights.Count; i++) {
            if (!vegetationHeights[i].remove) {
                keptVegetationHeights.Add(vegetationHeights[i]);

            }
        }
        if (keptVegetationHeights.Count == 0) {
            keptVegetationHeights.Add(vegetationHeights[0]);
            print("Count Zero");
        }
        print(keptVegetationHeights);
        vegetationHeights = keptVegetationHeights;
    }








    public void AddToSplatMap() {
        splatHeights.Add(new SplatHeights());


    }

    public void RemoveFromSplatMap() {
        List<SplatHeights> keptSplatHeights = new List<SplatHeights>();
        for (int i = 0; i < splatHeights.Count; i++) {
            if (!splatHeights[i].remove) {
                keptSplatHeights.Add(splatHeights[i]);

            }
        }
        if (keptSplatHeights.Count == 0) {
            keptSplatHeights.Add(splatHeights[0]);
            print("Count Zero");
        }
        print(keptSplatHeights);
        splatHeights = keptSplatHeights;
    }

    float GetSteepness(float[,] heightMap, int x, int y, int width, int height) {

        float h = heightMap[x, y];
        int nx = x + 1;
        int ny = y + 1;

        //if on the upper edge of the map find gradient by going backward
        if (nx > width - 1) nx = x - 1;
        if (ny > height - 1) ny = y - 1;

        float dx = heightMap[nx, y] - h;
        float dy = heightMap[x, ny] - h;
        Vector2 gradient = new Vector2(dx, dy);

        float steep = gradient.magnitude;
        return steep;

    }


    public void SplatMaps() {
        TerrainLayer[] newSplatPrototypes;
        newSplatPrototypes = new TerrainLayer[splatHeights.Count];
        int spindex = 0;
        foreach (SplatHeights sh in splatHeights) {
            newSplatPrototypes[spindex] = new TerrainLayer();
            
            newSplatPrototypes[spindex].diffuseTexture = sh.texture;
            newSplatPrototypes[spindex].tileOffset = sh.tileOffset;
            newSplatPrototypes[spindex].tileSize = sh.tileSize;
            newSplatPrototypes[spindex].diffuseTexture.Apply(true);
            string path = "Assets/New Terrain Layer " + spindex + ".terrainLayer";
            AssetDatabase.CreateAsset(newSplatPrototypes[spindex], path);
            Selection.activeObject = this.gameObject;
            
            spindex++;            
        }
        terrainData.terrainLayers = newSplatPrototypes;

        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        float[,,] splatMapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        for (int x = 0; x < terrainData.alphamapHeight; x++) {
            for (int y = 0; y < terrainData.alphamapWidth; y++) {
                float[] splat = new float[terrainData.alphamapLayers];

                for (int i = 0; i < splatHeights.Count; i++) {

                    if (splatHeights[i].texture != null) {
                        float noise = Mathf.PerlinNoise(x * splatNoiseXScale, y * splatNoiseYScale) * splatNoiseScale;
                        float offset = splatOffset + noise;
                        float thisHeightStart = splatHeights[i].minHeight - offset;
                        float thisHeightStop = splatHeights[i].maxHeight + offset;

                        //float steepness = GetSteepness(heightMap, x, y, terrainData.heightmapWidth, terrainData.heightmapHeight);
                        float steepness = terrainData.GetSteepness(y / (float)terrainData.heightmapHeight, x / (float)terrainData.heightmapWidth);

                        if ((heightMap[x, y] >= thisHeightStart && heightMap[x, y] <= thisHeightStop) &&
                            steepness >= splatHeights[i].minSlope && steepness <= splatHeights[i].maxSlope) {
                            splat[i] = 1;
                        }
                    }
                }

                NormalizeVector(splat);
                for( int j = 0;j < splatHeights.Count; j++) {
                    splatMapData[x, y, j] = splat[j];
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, splatMapData);
    }

    void NormalizeVector(float[] v) {
        float total = 0;
        for (int i = 0; i < v.Length; i++) {
            total += v[i];
        }
        for (int i = 0; i < v.Length; i++) {
            v[i] /= total;
        }
    }


    List<Vector2> GenerateNeighbours(Vector2 pos, int width, int height) {
        List<Vector2> neighbours = new List<Vector2>();
        for (int y = -1; y < 2; y++) {
            for (int x = -1; x < 2; x++) {
                if (!(x == 0 && y == 0)) {
                    Vector2 nPos = new Vector2(Mathf.Clamp(pos.x + x, 0, width - 1),
                                                Mathf.Clamp(pos.y + y, 0, height - 1));
                    if (!neighbours.Contains(nPos))
                        neighbours.Add(nPos);
                }
            }
        }
        return neighbours;
    }

    public void Smooth() {
        float[,] heightMap = GetHeightMap();
        float smoothProgress = 0;
        EditorUtility.DisplayProgressBar("Smoothing Terrain",
                                 "Progress",
                                 smoothProgress);

        for (int s = 0; s < smoothAmount; s++) {
            for (int y = 0; y < terrainData.heightmapHeight; y++) {
                for (int x = 0; x < terrainData.heightmapWidth; x++) {
                    float avgHeight = heightMap[x, y];
                    List<Vector2> neighbours = GenerateNeighbours(new Vector2(x, y),
                                                                  terrainData.heightmapWidth,
                                                                  terrainData.heightmapHeight);
                    foreach (Vector2 n in neighbours) {
                        avgHeight += heightMap[(int)n.x, (int)n.y];
                    }

                    heightMap[x, y] = avgHeight / ((float)neighbours.Count + 1);
                }
            }
            smoothProgress++;
            EditorUtility.DisplayProgressBar("Smoothing Terrain",
                                             "Progress",
                                             smoothProgress / smoothAmount);

        }
        terrainData.SetHeights(0, 0, heightMap);
        EditorUtility.ClearProgressBar();
    }


    public void MidPointDisplacement() {
        float[,] heightMap = GetHeightMap();
        int width = terrainData.heightmapWidth - 1;
        int squareSize = width;
        float heightMin = MPDHeightMin;
        float heightMax = MPDHeightMax;
        float heightDampener = (float)Mathf.Pow(heightDampenerPower, -1 * roughness);


        int cornerX, cornerY;
        int midX, midY;
        int pmidXL, pmidXR, pmidYU, pmidYD;

        /* heightMap[0, 0] = UnityEngine.Random.Range(0f, 0.2f);
         heightMap[0, terrainData.heightmapHeight - 2] = UnityEngine.Random.Range(0f, 0.2f);
         heightMap[terrainData.heightmapWidth - 2, 0] = UnityEngine.Random.Range(0f, 0.2f);
         heightMap[terrainData.heightmapWidth - 2, terrainData.heightmapHeight - 2] = 
                                                     UnityEngine.Random.Range(0f, 0.2f);*/
        while (squareSize > 0) {
            for (int x = 0; x < width; x += squareSize) {
                for (int y = 0; y < width; y += squareSize) {
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    heightMap[midX, midY] = (float)((heightMap[x, y] +
                                                     heightMap[cornerX, y] +
                                                     heightMap[x, cornerY] +
                                                     heightMap[cornerX, cornerY]) / 4.0f +
                                                    UnityEngine.Random.Range(heightMin, heightMax));
                }
            }

            for (int x = 0; x < width; x += squareSize) {
                for (int y = 0; y < width; y += squareSize) {

                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    pmidXR = (int)(midX + squareSize);
                    pmidYU = (int)(midY + squareSize);
                    pmidXL = (int)(midX - squareSize);
                    pmidYD = (int)(midY - squareSize);

                    if (pmidXL <= 0 || pmidYD <= 0
                        || pmidXR >= width - 1 || pmidYU >= width - 1) continue;

                    //Calculate the square value for the bottom side  
                    heightMap[midX, y] = (float)((heightMap[midX, midY] +
                                                  heightMap[x, y] +
                                                  heightMap[midX, pmidYD] +
                                                  heightMap[cornerX, y]) / 4.0f +
                                                 UnityEngine.Random.Range(heightMin, heightMax));
                    //Calculate the square value for the top side   
                    heightMap[midX, cornerY] = (float)((heightMap[x, cornerY] +
                                                            heightMap[midX, midY] +
                                                            heightMap[cornerX, cornerY] +
                                                        heightMap[midX, pmidYU]) / 4.0f +
                                                       UnityEngine.Random.Range(heightMin, heightMax));

                    //Calculate the square value for the left side   
                    heightMap[x, midY] = (float)((heightMap[x, y] +
                                                            heightMap[pmidXL, midY] +
                                                            heightMap[x, cornerY] +
                                                  heightMap[midX, midY]) / 4.0f +
                                                 UnityEngine.Random.Range(heightMin, heightMax));
                    //Calculate the square value for the right side   
                    heightMap[cornerX, midY] = (float)((heightMap[midX, y] +
                                                            heightMap[midX, midY] +
                                                            heightMap[cornerX, cornerY] +
                                                            heightMap[pmidXR, midY]) / 4.0f +
                                                       UnityEngine.Random.Range(heightMin, heightMax));

                }
            }

            squareSize = (int)(squareSize / 2.0f);
            heightMin *= heightDampener;
            heightMax *= heightDampener;
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    float[,] GetHeightMap() {

        if(!resetTerrain) {
            return terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);

        }else {
            return new float[terrainData.heightmapWidth, terrainData.heightmapHeight];

        }

    }

    public void Voronoi() {
        float[,] heightMap = GetHeightMap();

        for (int i = 0; i < peakCount; i++) {

            int x = UnityEngine.Random.Range(0, terrainData.heightmapWidth);
            int y = UnityEngine.Random.Range(0, terrainData.heightmapHeight);

            float peakHeight = UnityEngine.Random.Range(minMountainHeight,maxMountainHeight);
            print(peakHeight);
            //float peakHeight = 0.2f;
            heightMap[x, y] += peakHeight;

            Vector2 peakLocation = new Vector2(x, y);
            float maxDistance = Vector2.Distance(new Vector2(0, 0), new Vector2(terrainData.heightmapWidth, terrainData.heightmapHeight));

            for (y = 0; y < terrainData.heightmapHeight; y++) {
                for (x = 0; x < terrainData.heightmapHeight; x++) {
                    if (!(y == peakLocation.y && x == peakLocation.x)) {

                        float distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x, y)) / maxDistance;

                        float h = 0;

                        if (voronoiType == VoronoiType.Linear) {
                            h = Mathf.Max(0, peakHeight - distanceToPeak) * voronoiFallOff;
                        } else if (voronoiType == VoronoiType.Combined) {
                            h = Mathf.Max(0, peakHeight - distanceToPeak * voronoiFallOff - Mathf.Pow(distanceToPeak, voronoiDropOff));
                        } else if (voronoiType == VoronoiType.Inverse) {

                        } else if (voronoiType == VoronoiType.Dumplings) {
                            h = Mathf.Max(0, peakHeight - Mathf.Pow(distanceToPeak*3,voronoiFallOff) - Mathf.Sin( distanceToPeak * 2 * Mathf.PI / voronoiDropOff) );
                        }



                        heightMap[x, y] += h;
                    }
                }
            }
        }


        terrainData.SetHeights(0, 0, heightMap);
    }

    public void Perlin() {
        float[,] heightMap = GetHeightMap();

        for ( int y = 0; y < terrainData.heightmapHeight; y++) {
            for (int x = 0; x < terrainData.heightmapWidth; x++) {
                heightMap[x, y] += Mathf.PerlinNoise((x + perlinOffsetX) * perlinXScale, (y + perlinOffsetY) * perlinYScale);

            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void MultiplePerlinTerrain() {
        float[,] heightMap = GetHeightMap();

        for (int y = 0; y < terrainData.heightmapHeight; y++) {
            for (int x = 0; x < terrainData.heightmapWidth; x++) {
                foreach (PerlinParamaters p in perlinParameters) {
                    if (!p.remove) {
                        heightMap[x, y] += Utils.FractalBrownianMotion((x + p.mPerlinOffsetX) * p.mPerlinXScale, (y + p.mPerlinOffsetY) * p.mPerlinYScale, p.mPerlinOctaves, p.mPersistance) * p.mPerlinHeightScale;
                    }
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);

    }

    public void AddNewPerlin() {
        perlinParameters.Add(new PerlinParamaters());


    }

    public void RemovePerlin() {
        List<PerlinParamaters> keptPerlinParameters = new List<PerlinParamaters>();
        for (int i = 0; i < perlinParameters.Count; i++) {
            if (!perlinParameters[i].remove) {
                keptPerlinParameters.Add(perlinParameters[i]);

            }
        }
        if (keptPerlinParameters.Count == 0) {
            keptPerlinParameters.Add(perlinParameters[0]);
            print("Count Zero");
        }
        print(keptPerlinParameters);
        perlinParameters = keptPerlinParameters;
    }

    public void FractalBrownianMotion() {
        float[,] heightMap = GetHeightMap();

        for (int y = 0; y < terrainData.heightmapHeight; y++) {
            for (int x = 0; x < terrainData.heightmapWidth; x++) {
                heightMap[x, y] += Utils.FractalBrownianMotion((x + perlinOffsetX) * perlinXScale, (y + perlinOffsetY) * perlinYScale, oct, persistance) * perlinHeightScale;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public float Truncate(float value, int digits) {
        double mult = Math.Pow(10.0, digits);
        double result = Math.Truncate(mult * value) / mult;
        return (float)result;
    }

    public void MakeJagged() {

        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapWidth; x++) {
            for (int y = 0; y < terrainData.heightmapHeight; y++) {
                heightMap[x, y] = Truncate(heightMap[x, y],2);
            }

        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void RandomTerrain(){
        float[,] heightMap = GetHeightMap();
        for (int x = 0; x < terrainData.heightmapWidth; x++){
            for (int z = 0; z < terrainData.heightmapHeight; z++){
                heightMap[x, z] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    
    }

    public void LoadTexture(){
        float[,] heightMap;
        heightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapWidth; x++){
            for (int z = 0; z < terrainData.heightmapHeight; z++){
                heightMap[x, z] += heightMapImage.GetPixel((int)(x * heightMapScale.x), 
                                                          (int)(z * heightMapScale.z)).grayscale 
                                                            * heightMapScale.y;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void ResetTerrain(){
        float[,] heightMap;
        heightMap = new float[terrainData.heightmapWidth,terrainData.heightmapHeight];
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int z = 0; z < terrainData.heightmapHeight; z++)
            {
                heightMap[x, z] = 0;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);

    }

    void OnEnable()
    {
        Debug.Log("Initialising Terrain Data");
        terrain = this.GetComponent<Terrain>();
        terrainData = Terrain.activeTerrain.terrainData;
    }

    void Awake()
    {
        SerializedObject tagManager = new SerializedObject(
                              AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        AddTag(tagsProp, "Terrain");
        AddTag(tagsProp, "Cloud");
        AddTag(tagsProp, "Shore");

        //apply tag changes to tag database
        tagManager.ApplyModifiedProperties();

        //take this object
        this.gameObject.tag = "Terrain";


    }

    void AddTag(SerializedProperty tagsProp, string newTag)
    {
        bool found = false;
        //ensure the tag doesn't already exist
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(newTag)) { found = true; break; }
        }
        //add your new tag
        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newTag;
        }
    }

    // Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        perlinOffsetX++;
        FractalBrownianMotion();
	}
} 
