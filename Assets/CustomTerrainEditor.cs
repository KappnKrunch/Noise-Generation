using UnityEngine;
using UnityEditor;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]

public class CustomTerrainEditor : Editor {

    //properties -----------
    SerializedProperty randomHeightRange;
    SerializedProperty heightMapScale;
    SerializedProperty heightMapImage;
    SerializedProperty perlinXScale;
    SerializedProperty perlinYScale;
    SerializedProperty perlinOffsetX;
    SerializedProperty perlinOffsetY;
    SerializedProperty oct;
    SerializedProperty persistance;
    SerializedProperty perlinHeightScale;
    SerializedProperty resetTerrain;

    GUITableState perlinParameterTable;
    SerializedProperty perlinParameters;
    SerializedProperty voronoiDropOff;
    SerializedProperty voronoiFallOff;
    SerializedProperty voronoipeakCount;
    SerializedProperty voronoiMinMountainHeight;
    SerializedProperty voronoiMaxMountainHeight;
    SerializedProperty voronoiType;

    SerializedProperty midPointHeightMin;
    SerializedProperty midPointHeightMax;
    SerializedProperty midPointRoughness;
    SerializedProperty midPointHeightDampening;
    SerializedProperty smoothAmount;

    GUITableState splatHeightsTable;
    SerializedProperty splatHeights;
    SerializedProperty splatOffset;
    SerializedProperty splatNoiseXScale;
    SerializedProperty splatNoiseYScale;
    SerializedProperty splatNoiseScale;

    GUITableState vegeatationHeightsTable;
    SerializedProperty vegetationHeights;
    SerializedProperty vegetationMaximumTrees;
    SerializedProperty vegetationTreeSpacing;




    //fold outs ------------
    bool showRandom = false;
    bool showLoadHeights = false;
    bool showPerlin = false;
    bool showFBM = false;
    bool showMultiplePerlin = false;
    bool showVoronoi = false;
    bool showMidPointDisplacement = false;
    bool showSmooth = false;
    bool showSplatMap = false;
    bool showVegetation = false;

    void OnEnable()
    {
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        heightMapImage = serializedObject.FindProperty("heightMapImage");
        perlinXScale = serializedObject.FindProperty("perlinXScale");
        perlinYScale = serializedObject.FindProperty("perlinYScale");
        perlinOffsetX = serializedObject.FindProperty("perlinOffsetX");
        perlinOffsetY = serializedObject.FindProperty("perlinOffsetY");
        oct = serializedObject.FindProperty("oct");
        persistance = serializedObject.FindProperty("persistance");
        perlinHeightScale = serializedObject.FindProperty("perlinHeightScale");
        resetTerrain = serializedObject.FindProperty("resetTerrain");
        perlinParameterTable = new GUITableState("perlinParameterTable");
        perlinParameters = serializedObject.FindProperty("perlinParameters");
        voronoiDropOff = serializedObject.FindProperty("voronoiDropOff");
        voronoiFallOff = serializedObject.FindProperty("voronoiFallOff");
        voronoipeakCount = serializedObject.FindProperty("peakCount");
        voronoiMinMountainHeight = serializedObject.FindProperty("minMountainHeight");
        voronoiMaxMountainHeight = serializedObject.FindProperty("maxMountainHeight");
        voronoiType = serializedObject.FindProperty("voronoiType");
        midPointHeightMin = serializedObject.FindProperty("MPDHeightMin");
        midPointHeightMax = serializedObject.FindProperty("MPDHeightMax");
        midPointRoughness = serializedObject.FindProperty("roughness");
        midPointHeightDampening = serializedObject.FindProperty("heightDampenerPower");
        smoothAmount = serializedObject.FindProperty("smoothAmount");

        splatHeightsTable = new GUITableState("splatHeightsTable");
        splatHeights = serializedObject.FindProperty("splatHeights");
        splatNoiseScale = serializedObject.FindProperty("splatNoiseScale");
        splatNoiseXScale = serializedObject.FindProperty("splatNoiseXScale");
        splatNoiseYScale = serializedObject.FindProperty("splatNoiseYScale");
        splatOffset = serializedObject.FindProperty("splatOffset");

        vegeatationHeightsTable = new GUITableState("vegeatationHeightsTable");
        vegetationHeights = serializedObject.FindProperty("vegetationHeights");
        vegetationMaximumTrees = serializedObject.FindProperty("vegetationMaximumTrees");
        vegetationTreeSpacing = serializedObject.FindProperty("vegetationTreeSpacing");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain) target;

        showRandom = EditorGUILayout.Foldout(showRandom, "Random");
        if (showRandom)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Heights Between Random Values", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(randomHeightRange);
            if (GUILayout.Button("Random Heights"))
            {
                terrain.RandomTerrain();
            }
        }

        showLoadHeights = EditorGUILayout.Foldout(showLoadHeights, "Load Heights");
        if (showLoadHeights)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Load Heights From Texture", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(heightMapImage);
            EditorGUILayout.PropertyField(heightMapScale);
            if (GUILayout.Button("Load Texture"))
            {
                terrain.LoadTexture();
            }
        }

        showPerlin = EditorGUILayout.Foldout(showPerlin, "Perlin");
        if (showPerlin) {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("X and Y Scale", EditorStyles.boldLabel);
            EditorGUILayout.IntSlider(perlinOffsetX, 0, 10000000, new GUIContent("X Offset"));
            EditorGUILayout.IntSlider(perlinOffsetY, 0, 10000000, new GUIContent("Y Offset"));
            EditorGUILayout.Slider(perlinXScale, 0, .01f, new GUIContent("X Scale"));
            EditorGUILayout.Slider(perlinYScale, 0, .01f, new GUIContent("Y Scale"));
            if (GUILayout.Button("Draw")) {
                terrain.Perlin();
            }
        }

        showFBM = EditorGUILayout.Foldout(showFBM, "Fractal Brownian Motion");
        if (showFBM) {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Config", EditorStyles.boldLabel);
            
            EditorGUILayout.IntSlider(perlinOffsetX, 0, 10000000, new GUIContent("X Offset"));
            EditorGUILayout.IntSlider(perlinOffsetY, 0, 10000000, new GUIContent("Y Offset"));
            EditorGUILayout.IntSlider(oct, 2, 15, new GUIContent("Oct"));
            EditorGUILayout.Slider(persistance, 0, 10, new GUIContent("Persistance"));
            EditorGUILayout.Slider(perlinHeightScale, 0, 2, new GUIContent("Height Scale"));
            EditorGUILayout.Slider(perlinXScale, 0, .01f, new GUIContent("X Scale"));
            EditorGUILayout.Slider(perlinYScale, 0, .01f, new GUIContent("Y Scale"));
            if (GUILayout.Button("Draw")) {
                terrain.FractalBrownianMotion();
            }
        }

        showMultiplePerlin = EditorGUILayout.Foldout(showMultiplePerlin, "Multiple perlin");
        if (showMultiplePerlin) {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Config", EditorStyles.boldLabel);
            
            perlinParameterTable = GUITableLayout.DrawTable(perlinParameterTable, serializedObject.FindProperty("perlinParameters"));
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+")) {
                terrain.AddNewPerlin();
            }
            if(GUILayout.Button("Remove selected")) {
                terrain.RemovePerlin();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Draw")) {
                terrain.MultiplePerlinTerrain();
            }
        }



        showVoronoi = EditorGUILayout.Foldout(showVoronoi, "Voronoi");
        if (showVoronoi) {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("mountains", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(voronoiType);
            EditorGUILayout.IntSlider(voronoipeakCount, 1, 10, new GUIContent("peak count"));
            EditorGUILayout.Slider(voronoiMinMountainHeight, 0.0001f, 1.9999f);
            EditorGUILayout.Slider(voronoiMaxMountainHeight, 0.0002f, 2);
            EditorGUILayout.Slider(voronoiDropOff, 0, 10, new GUIContent("drop off"));
            EditorGUILayout.Slider(voronoiFallOff, 0, 10, new GUIContent("fall off"));
            if (GUILayout.Button("Draw")) {
                terrain.Voronoi();
            }
        }

        showMidPointDisplacement = EditorGUILayout.Foldout(showMidPointDisplacement,"Mid-Point Displacement");
        if(showMidPointDisplacement) {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("mid-point displacement", EditorStyles.boldLabel);
            EditorGUILayout.Slider(midPointHeightMax, 0, 10);
            EditorGUILayout.Slider(midPointHeightMin, -10, 0);
            EditorGUILayout.Slider(midPointRoughness, 1,10, new GUIContent("smoothness"));
            EditorGUILayout.Slider(midPointHeightDampening, 0, 4, new GUIContent("peak dampening"));
            if (GUILayout.Button("MPD")) {
                terrain.MidPointDisplacement();
            }
        }

        showSmooth = EditorGUILayout.Foldout(showSmooth, "Smooth");
        if (showSmooth) {
            EditorGUILayout.IntSlider(smoothAmount, 1, 10, new GUIContent("smooth x amount of times"));
            if(GUILayout.Button("Smooth")) {
                terrain.Smooth();
            }
        }

        showSplatMap = EditorGUILayout.Foldout(showSplatMap, "Splat Map");
        if(showSplatMap) {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Colors", EditorStyles.boldLabel);

            EditorGUILayout.Slider(splatOffset, 0, 0.1f);
            EditorGUILayout.Slider(splatNoiseXScale, 0.001f, 1);
            EditorGUILayout.Slider(splatNoiseYScale, 0.001f, 1);
            EditorGUILayout.Slider(splatNoiseScale, 0, 1);
            splatHeightsTable = GUITableLayout.DrawTable(splatHeightsTable, serializedObject.FindProperty("splatHeights"));
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+")) {
                terrain.AddToSplatMap();
            }
            if (GUILayout.Button("Remove selected")) {
                terrain.RemoveFromSplatMap();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Paint")) {
                terrain.SplatMaps();
            }
        }

        showVegetation = EditorGUILayout.Foldout(showVegetation, "vegetation");
        if (showVegetation) {

            EditorGUILayout.IntSlider(vegetationMaximumTrees, 1, 1000);
            EditorGUILayout.IntSlider(vegetationTreeSpacing, 1, 1000);

            vegeatationHeightsTable = GUITableLayout.DrawTable(vegeatationHeightsTable, serializedObject.FindProperty("vegetationHeights"));
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+")) {
                terrain.AddToVegetation();
            }

            if (GUILayout.Button("Plant")) {
                terrain.RemoveFromVegetation();
            }
        }


        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.PropertyField(resetTerrain, new GUIContent("Reset terrain after update"));
        if (GUILayout.Button("Make Jagged")) {
            terrain.MakeJagged();
        }
        if (GUILayout.Button("Reset Terrain"))
        {
            terrain.ResetTerrain();
        }

        serializedObject.ApplyModifiedProperties();
    }

    // Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
