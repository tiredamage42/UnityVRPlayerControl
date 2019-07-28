

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CanEditMultipleObjects]
[CustomEditor(typeof(PrefabPainter))]
public class PrefabPainterEditor : Editor {

    public static T[] GetAllInstances<T>() where T : ScriptableObject
    {
        //FindAssets uses tags check documentation for more info
        string[] guids = AssetDatabase.FindAssets("t:"+ typeof(T).Name);  
        T[] a = new T[guids.Length];
        
        //probably could get optimized 
        for(int i =0;i<guids.Length;i++)         
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
        }
        return a;
    }

    // static GUILayoutOption randomAxisWidth, randomAxisLabelWidth, smallButtonWidth;
    static GUILayoutOption[] smallButtonWidth;

    static Color32 redColor = new Color32(225, 75, 75, 255);

    void InitializeGUI () {
        // randomAxisWidth = GUILayout.Width(32);
        // randomAxisLabelWidth = GUILayout.Width(200);
        smallButtonWidth = new GUILayoutOption[]{ GUILayout.Height(10), GUILayout.Width(10) };

    }


    static void DrawPainterElement(PrefabPainterElement element) {
        if (element == null) 
        {
            return;
        }

        EditorGUI.indentLevel++;

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.Space();
        
        GameObject newVariation = (GameObject)EditorGUILayout.ObjectField("New Variation:", null, typeof(GameObject), false);
        if (newVariation != null) {
            element.variations.Add(newVariation);
            // System.Array.Resize(ref element.variations, element.variations.Count + 1);
            // element.variations[element.variations.Count -1] = newVariation;
        }

        int indexToDelete = -1;
        for (int i = 0; i < element.variations.Count; i++) {
            EditorGUILayout.BeginHorizontal();
            element.variations[i] = (GameObject)EditorGUILayout.ObjectField(element.variations[i], typeof(GameObject), false);
            GUI.backgroundColor = redColor;
            if (GUILayout.Button("", smallButtonWidth)) {
                indexToDelete = i;
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }
        if (indexToDelete != -1) {
            element.variations.Remove(element.variations[indexToDelete]);
        }

        
        // EditorGUILayout.BeginHorizontal();

        // EditorGUILayout.LabelField("Randomize Rotations: (X, Y, Z)", randomAxisLabelWidth);
        
        // element.randomX = EditorGUILayout.Toggle(element.randomX, randomAxisWidth);
        // element.randomY = EditorGUILayout.Toggle(element.randomY, randomAxisWidth);
        // element.randomZ = EditorGUILayout.Toggle(element.randomZ, randomAxisWidth);
        
        // EditorGUILayout.EndHorizontal();

        // element.yOffset = EditorGUILayout.Vector3Field("Local Pos Offset", element.localPositionOffset);
        
        // element.localPositionOffset = EditorGUILayout.Vector3Field("Local Pos Offset", element.localPositionOffset);
        // element.localRotationOffset = EditorGUILayout.Vector3Field("Local Rot Offset", element.localRotationOffset);

        EditorGUILayout.Space();
        
        element.posOffsetMin = EditorGUILayout.Vector3Field("Pos Offset Min", element.posOffsetMin);
        element.posOffsetMax = EditorGUILayout.Vector3Field("Pos Offset Max", element.posOffsetMax);
        
        EditorGUILayout.Space();
        element.rotOffsetMin = EditorGUILayout.Vector3Field("Ros Offset Min", element.rotOffsetMin);
        element.rotOffsetMax = EditorGUILayout.Vector3Field("Rot Offset Max", element.rotOffsetMax);


EditorGUILayout.Space();
        
        element.sizeMultiplierRange = EditorGUILayout.Vector2Field("Size Multiplier Range", element.sizeMultiplierRange);
        // element.randomizeSize = EditorGUILayout.Toggle("Random Size", element.randomizeSize);
        // if (element.randomizeSize) {
        // }

        EditorGUILayout.Space();
        
        element.alignNormal = EditorGUILayout.Toggle("Align To Normal", element.alignNormal);
        EditorGUILayout.EndVertical();
        EditorGUI.indentLevel--;
        
        EditorUtility.SetDirty(element);
    }
    PrefabPainterElement[] allElements;




    int currentElementIndex=-1;
    bool validElement { get { return currentElementIndex >= 0 && currentElementIndex < allElements.Length; } }
    PrefabPainterElement currentElement { get { return validElement ? allElements[currentElementIndex] : null; } }
    PrefabPainter painter;
    string[] elementNames;


    PrefabPainterElement CreateNewElement (GameObject variation) {
        PrefabPainterElement newElement = ScriptableObject.CreateInstance<PrefabPainterElement>();

        string newElementName = "NewPainterElement";
        if (variation != null) {
            newElement.variations = new List<GameObject>() { variation };
            newElementName = variation.name + "_PainterElement";
        }
        AssetDatabase.CreateAsset(newElement, AssetDatabase.GenerateUniqueAssetPath("Assets/"+newElementName+".asset"));
        AssetDatabase.SaveAssets();

        return newElement;

    }
    void CreateNewPainterElement (GameObject variation) {
        PrefabPainterElement newElement = CreateNewElement(variation);


        System.Array.Resize(ref allElements, allElements.Length+1);
        System.Array.Resize(ref elementNames, elementNames.Length+1);
        

        allElements[allElements.Length-1] = newElement;
        elementNames[elementNames.Length-1] = newElement.name;
        currentElementIndex = allElements.Length-1;

        
    }


    void OnEnable()
    {
        painter = target as PrefabPainter;
        
        allElements = GetAllInstances<PrefabPainterElement>();

        Debug.Log(allElements.Length);

        elementNames = new string[allElements.Length];

        for (int i =0 ; i< allElements.Length; i++) {
            elementNames[i] = allElements[i].name;
        }

        helperTransform = new GameObject("PREFAB_PAINTER_HELPER").transform;
        helperTransform.SetParent(painter.transform);

        InitializeGUI();
    }

    Transform helperTransform;
        

        
     
    void OnDisable () {
        DestroyImmediate(gameObjectEditor);
        DestroyImmediate(helperTransform.gameObject);
    }

    LayerMask paintLayerMask = -1;
    float radius = 1;
    int amount = 1;
    bool drawMode = true;
    bool useRadius;


    int variationIndex=-2;    
    GameObject currentPreview;
    Quaternion chosenPreviewRotation;
    Vector4 chosenPreviewPosition;
    
    int previewIndex;




    void UpdatePreviewRandom () {
        chosenPreviewRotation = Quaternion.Euler (
            Random.Range(currentElement.rotOffsetMin.x, currentElement.rotOffsetMax.x),
            Random.Range(currentElement.rotOffsetMin.y, currentElement.rotOffsetMax.y),
            Random.Range(currentElement.rotOffsetMin.z, currentElement.rotOffsetMax.z)
        );
        chosenPreviewPosition = new Vector4 (
            Random.Range(currentElement.posOffsetMin.x, currentElement.posOffsetMax.x),
            Random.Range(currentElement.posOffsetMin.y, currentElement.posOffsetMax.y),
            Random.Range(currentElement.posOffsetMin.z, currentElement.posOffsetMax.z),
            Random.Range(currentElement.sizeMultiplierRange.x, currentElement.sizeMultiplierRange.y)

        );


        // chosenPreviewRotation.x = Random.Range(0,360);
        // chosenPreviewRotation.y = Random.Range(0,360);
        // chosenPreviewRotation.z = Random.Range(0,360);
        // chosenPreviewRotation.w = Random.Range(currentElement.sizeRange.x, currentElement.sizeRange.y);
    }

    void DestoryCurrentPreview () {
        if (currentPreview != null) {
            DestroyImmediate(currentPreview);
        }
    }



    GameObject previewOBJ;

    void UpdatePreviewObj () {
        if (previewOBJ) {

            gameObjectEditor = Editor.CreateEditor(previewOBJ);
            previewOBJ = null;
        }

    }


    void OnPreviewIndexChange () {
        if (!validElement)
            return;
        // if (useRadius) 
        //     return;

        DestoryCurrentPreview();
        
        // GameObject obj = variationIndex != -1 ? currentElement.variations[variationIndex] : null;
        
        int index = variationIndex == -1 ? Random.Range(0, currentElement.variations.Count) : variationIndex;
        

        previewIndex = variationIndex == -1 ? index : -1;
        if (gameObjectEditor != null) {
            DestroyImmediate(gameObjectEditor);
        }
        previewOBJ = null;
        if (index != -1 && index < currentElement.variations.Count) {
            previewOBJ = currentElement.variations[index];
            // gameObjectEditor = Editor.CreateEditor(currentElement.variations[index]);
        }

        if (previewOBJ == null) 
            return;
        
        currentPreview = PrefabUtility.InstantiatePrefab(previewOBJ) as GameObject;
        originalPreviewScale = currentPreview.transform.localScale;


        Collider[] cols = currentPreview.GetComponentsInChildren<Collider>();
        for (int i = 0; i < cols.Length; i++) cols[i].enabled = false;
        
        currentPreview.transform.SetParent(helperTransform);

        currentPreview.transform.localPosition = chosenPreviewPosition;
        currentPreview.transform.localRotation = chosenPreviewRotation;
        

        currentPreview.transform.localScale = originalPreviewScale * chosenPreviewPosition.w;// new Vector3 (chosenPreviewPosition.w, chosenPreviewPosition.w, chosenPreviewPosition.w);

        UpdatePreviewRandom();
    }

    void OnCurrentElementChange () {
        variationIndex = -1;
        OnPreviewIndexChange();
    }

    Vector3 lastHitPos, lastHitNormal, originalPreviewScale;

    void HandlePreview (bool isValid, Vector3 hitPos, Vector3 hitNormal, bool shiftHeld) {
        
        if (useRadius || !drawMode || !validElement) {
            // previewLocked = false;
            DestoryCurrentPreview();
            return;
        }

        if ((!isValid||shiftHeld)){// && !previewLocked) {
            if (currentPreview != null) {
                currentPreview.SetActive(false);
            }
            return;
        }
        if (currentPreview == null) {
            OnPreviewIndexChange();
        }
        else {
            currentPreview.SetActive(true);
        }

        // if (previewLocked) {
        //     hitPos = lastHitPos;
        //     hitNormal = lastHitNormal;
        // }
        // else {
            lastHitPos = hitPos;
            lastHitNormal = hitNormal;
        // }

        helperTransform.position = hitPos;
        helperTransform.rotation = Quaternion.identity;
            
        if (currentElement.alignNormal)
            helperTransform.rotation = Quaternion.FromToRotation(Vector3.up, hitNormal);

        // if (currentElement.randomX) helperTransform.Rotate(chosenPreviewRotation.x, 0, 0);
        // if (currentElement.randomY) helperTransform.Rotate(0, chosenPreviewRotation.y, 0);
        // if (currentElement.randomZ) helperTransform.Rotate(0, 0, chosenPreviewRotation.z);
        
        // if (currentElement.randomizeSize) {
            // currentPreview.transform.localScale = originalPreviewScale * chosenPreviewPosition.w;// new Vector3 (chosenPreviewPosition.w, chosenPreviewPosition.w, chosenPreviewPosition.w);
        // }


        // currentPreview.transform.localPosition = chosenPreviewPosition;
        // currentPreview.transform.localRotation = chosenPreviewRotation;
        
        // currentPreview.transform.localPosition = currentElement.localPositionOffset;
        // currentPreview.transform.localRotation = Quaternion.Euler(currentElement.localRotationOffset);


        // if (previewLocked) {
        //     GUI.enabled = false;
        //     Handles.PositionHandle (currentPreview.transform.position, currentPreview.transform.rotation);
        //     GUI.enabled = true;
        // }
    }

    // bool previewLocked;
    

    public override void OnInspectorGUI()
    {
        // DrawModeButton();

        UpdatePreviewObj();
        // base.OnInspectorGUI();

        paintLayerMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(EditorGUILayout.MaskField("Paint Mask", InternalEditorUtility.LayerMaskToConcatenatedLayersMask(paintLayerMask), InternalEditorUtility.layers));

        useRadius = EditorGUILayout.Toggle("Use Radius", useRadius);
        if (useRadius) {
            EditorGUI.indentLevel++;
            radius = EditorGUILayout.FloatField("Radius", radius);
            amount = EditorGUILayout.IntField("Amount", amount);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // EditorGUILayout.BeginHorizontal();
        

        if (GUILayout.Button("Create New Element")){//}, GUILayout.Width(150))) {
            CreateNewPainterElement (null);
        }

        // float old = EditorGUIUtility.labelWidth; 
        // EditorGUIUtility.labelWidth = 0;



        GameObject newVariation = (GameObject)EditorGUILayout.ObjectField("From Prefab", null, typeof(GameObject), false);//, GUILayout.Width(200));
        // GameObject newVariation = (GameObject)EditorGUILayout.ObjectField("From Prefab", null, typeof(GameObject), false, GUILayout.Width(128));
        // EditorGUIUtility.labelWidth = old;
        
        if (newVariation != null) {
            CreateNewPainterElement(newVariation);
        }
        // EditorGUILayout.EndHorizontal();
            

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        float old = EditorGUIUtility.labelWidth; 
        EditorGUIUtility.labelWidth = 64;

        int oldElementIndex = currentElementIndex;
        if (currentElementIndex == -1) {
            currentElementIndex = 0;
        }
        currentElementIndex = EditorGUILayout.Popup ("Painting:", currentElementIndex, elementNames);

        if (currentElementIndex != oldElementIndex) {
            OnCurrentElementChange();
        }
        EditorGUIUtility.labelWidth = old;


        if (validElement){
            
            GUI.enabled = false;
            EditorGUILayout.ObjectField("", currentElement, typeof(PrefabPainterElement), false);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        
            int oldIndex = variationIndex;

            
            // GUI.enabled = !randomizeVariation;
            variationIndex = EditorGUILayout.IntSlider("Paint Variation", variationIndex, currentElement.variations.Count == 1 ? 0 : -1, currentElement.variations.Count -1);
            // GUI.enabled = true;
            
            if (oldIndex != variationIndex) {
                OnPreviewIndexChange();
            }

            DrawPainterElement(currentElement);
        }
        else {

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        
DrawModeButton();
        
    }

    void DrawModeButton () {
        if (drawMode) {
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Exit Draw Mode")) {
                drawMode = false;
            }
            GUI.backgroundColor = Color.white;   
        }
        else {
            if (GUILayout.Button("Draw Mode")) {
                drawMode = true;
            }
        }
    }
     
    public override bool HasPreviewGUI() { return variationIndex != -1; }
    Editor gameObjectEditor;
    
    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        if (gameObjectEditor)
        {
            gameObjectEditor.OnPreviewGUI(r, background);
        } 
    }

    

    const int sceneViewBoxSize = 256;
    const float mosuePosOffset = 8;
    
    private void OnSceneGUI()
    {
        if (!drawMode) {
            return;
        }
        if (!validElement)
            return;


        Event e = Event.current;
        Handles.BeginGUI();
        Rect r = new Rect(e.mousePosition.x + mosuePosOffset, e.mousePosition.y + mosuePosOffset, sceneViewBoxSize, sceneViewBoxSize);
        
        string helpString = "";
        if (useRadius) {
            helpString = string.Format("Radius: {0:0.00}\nA: Switch To Single Draw\nD: Exit draw mode.\nF: Focus area\nShift+Scroll: Adjust radius", radius);
        }
        else {
            // helpString = string.Format("Preview Lock: {0}\nA: Switch To Radius Draw\nD: Exit draw mode.\nF: Focus area\nS: Lock preview", previewLocked);
            helpString = "A: Switch To Radius Draw\nD: Exit draw mode.\nF: Focus area";
        }
        
        GUI.Label(r, helpString);
        
        Handles.EndGUI();

        bool focus = false;
        
        if (SceneView.sceneViews.Count > 0)
        {
            EditorWindow sceneView = (EditorWindow)SceneView.sceneViews[0];
            if (EditorWindow.focusedWindow != sceneView) {
                if (EditorWindow.mouseOverWindow && EditorWindow.mouseOverWindow == sceneView) {
                    sceneView.Focus();
                }                
            }
        }
        

        if (allElements.Length == 0)
            return;

        Event current = Event.current;
        if((EventType.KeyUp == current.type ||  EventType.KeyDown == current.type ) && KeyCode.F == current.keyCode)
        {
            focus = EventType.KeyUp == current.type;
            current.Use();
        }
        if (EventType.KeyUp == current.type) {

        if((EventType.KeyUp == current.type ) && KeyCode.D == current.keyCode)
        {
            current.Use();
            drawMode = false;
            // return;
        }
        // if((EventType.KeyUp == current.type ) && KeyCode.S == current.keyCode)
        // {
        //     current.Use();
        //     previewLocked = !previewLocked;
        //     return;
        // }
        if((EventType.KeyUp == current.type ) && KeyCode.A == current.keyCode)
        {
            current.Use();
            useRadius = !useRadius;
            return;
        }
        }
        

        if (EventType.ScrollWheel == current.type && current.shift) {
            radius += current.delta.y;
            current.Use();
        }
        

        int controlId = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
        
        bool isValid = false;
        Vector3 hitPos, hitNormal;
        RaycastHit hit;
        if (CalculateMousePosition(out hit, out hitPos, out hitNormal)) {
            isValid = true;

            if (useRadius) {
                Handles.color = Color.blue;
                Handles.DrawWireDisc(hitPos, hitNormal, radius);

            }

            if (focus) {
                SceneView.lastActiveSceneView.Frame(new Bounds(hitPos, Vector3.one * radius), false);
            }

            if ((current.type == EventType.MouseDown) && !current.alt && allElements != null)
            {
                if (current.button == 0 && !current.shift)
                {
                    if (useRadius) {

                        for (int i = 0; i < amount; i++)
                        {
                            PrefabInstantiate(hitPos, hitNormal);
                        }
                    }
                    else {
                            PrefabInstantiate(hitPos, hitNormal);
                    }
                } 
                else if (current.button == 0 && current.shift)
                {
                    PrefabRemove(hitPos, hit);
                }
            }
        }
        
        HandlePreview(isValid, hitPos, hitNormal, current.shift);
        
        if (Event.current.type == EventType.Layout)
            HandleUtility.AddDefaultControl(controlId);

        SceneView.RepaintAll();
        
    }

    

    public bool CalculateMousePosition (out RaycastHit hit, out Vector3 hitPos, out Vector3 hitNormal)
    {
        hitPos = Vector3.zero;
        hitNormal = Vector3.up;

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        
        if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, paintLayerMask))
        {
            hitPos = hit.point;
            hitNormal = hit.normal;

            return true;
        }
        return false;
    }


    public void PrefabInstantiate (Vector3 hitPos, Vector3 hitNormal)
    {
        // if (previewLocked) {
        //     Debug.LogWarning("Cant instantiate when preview locked");
        //     return;
        // }
        if (!validElement || currentElement.variations == null || currentElement.variations.Count == 0)
            return;
        
        // Transform helperTransform = new GameObject("PREFAB_PAINTER_HELPER").transform;
        helperTransform.position = hitPos;
        helperTransform.rotation = Quaternion.identity;

        if (currentElement.alignNormal)
            helperTransform.rotation = Quaternion.FromToRotation(Vector3.up, hitNormal);

        
        // helperTransform.position = helperTransform.position + helperTransform.up * currentElement.yOffset; 
        
        // if (amount > 1)
        if (useRadius)
        {
            Vector3 radiusAdjust = Random.insideUnitSphere * radius;
            helperTransform.Translate(radiusAdjust.x, 0, radiusAdjust.y);

            RaycastHit hit;
            if (Physics.Raycast(helperTransform.position + helperTransform.up * 50, -helperTransform.up, out hit, Mathf.Infinity, paintLayerMask))
            {
                
                helperTransform.position = hit.point;
                if (currentElement.alignNormal)
                    helperTransform.up = hit.normal;
                
                // helperTransform.position = helperTransform.position + helperTransform.up * currentElement.yOffset; 
        
            }
            else
            {
                return;
            }

            // if (currentElement.randomX) helperTransform.Rotate(Random.Range(0, 360), 0, 0);
            // if (currentElement.randomY) helperTransform.Rotate(0, Random.Range(0, 360), 0);
            // if (currentElement.randomZ) helperTransform.Rotate(0, 0, Random.Range(0, 360));

        }
        else {
            // if (currentElement.randomX) helperTransform.Rotate(chosenPreviewRotation.x, 0, 0);
            // if (currentElement.randomY) helperTransform.Rotate(0, chosenPreviewRotation.y, 0);
            // if (currentElement.randomZ) helperTransform.Rotate(0, 0, chosenPreviewRotation.z);
        }

        int index = variationIndex == -1 ? previewIndex : variationIndex;
        if (useRadius) {
            index = variationIndex == -1 ? Random.Range(0, currentElement.variations.Count) : variationIndex;
        }

        GameObject instance = PrefabUtility.InstantiatePrefab(currentElement.variations[index]) as GameObject;
        
        instance.transform.SetParent(helperTransform);

        if (useRadius) {
            instance.transform.localPosition = new Vector3 (
            Random.Range(currentElement.posOffsetMin.x, currentElement.posOffsetMax.x),
            Random.Range(currentElement.posOffsetMin.y, currentElement.posOffsetMax.y),
            Random.Range(currentElement.posOffsetMin.z, currentElement.posOffsetMax.z)
            

        );
            instance.transform.localRotation = Quaternion.Euler (
            Random.Range(currentElement.rotOffsetMin.x, currentElement.rotOffsetMax.x),
            Random.Range(currentElement.rotOffsetMin.y, currentElement.rotOffsetMax.y),
            Random.Range(currentElement.rotOffsetMin.z, currentElement.rotOffsetMax.z)
        );
        
        }
        else {
            instance.transform.localPosition = chosenPreviewPosition;
            instance.transform.localRotation = chosenPreviewRotation;
        }

        // instance.transform.localPosition = currentElement.localPositionOffset;
        // instance.transform.localRotation = Quaternion.Euler(currentElement.localRotationOffset);
        
        // if (currentElement.randomizeSize) {
            float size = chosenPreviewPosition.w;
            if (useRadius) {
                size = Random.Range(currentElement.sizeMultiplierRange.x, currentElement.sizeMultiplierRange.y);
            }
            instance.transform.localScale = instance.transform.localScale * size;// new Vector3 (size, size, size);
        // }
        
        instance.transform.SetParent(null);
        
        instance.AddComponent<PrefabPainted>();

        Undo.RegisterCreatedObjectUndo(instance, "Instantiate");

        if (!useRadius) {
            OnPreviewIndexChange();
            // UpdatePreviewRandom();
        }
    }

    void PrefabRemove (Vector3 hitPos, RaycastHit hit)
    {
        if (useRadius) {
            PrefabPainted[] prefabsInRadius;
            prefabsInRadius = GameObject.FindObjectsOfType<PrefabPainted> ();
            float thresh = radius * radius;
            foreach (PrefabPainted p in prefabsInRadius)
            {
                float dist = Vector3.SqrMagnitude(hitPos - p.transform.position);
                if (dist <= thresh)
                    Undo.DestroyObjectImmediate(p.gameObject);
            }
        }
        else {
            PrefabPainted p = hit.collider.GetComponent<PrefabPainted>();
            if (p != null) {
                Undo.DestroyObjectImmediate(p.gameObject);
            }
        }
    }
}