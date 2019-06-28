

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


    void DrawPainterElement(PrefabPainterElement element) {
        EditorGUI.indentLevel++;

        EditorGUILayout.BeginVertical("box");
        
        GameObject newVariation = (GameObject)EditorGUILayout.ObjectField("Add New Variation", null, typeof(GameObject), false);
        if (newVariation != null) {
            System.Array.Resize(ref element.variations, element.variations.Length + 1);
            element.variations[element.variations.Length -1] = newVariation;
        }
        EditorGUILayout.Space();

        for (int i = 0; i < element.variations.Length; i++) {
            element.variations[i] = (GameObject)EditorGUILayout.ObjectField(element.variations[i], typeof(GameObject), false);
        }
        
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Randomize Rotations: (X, Y, Z)", GUILayout.Width(200));
        
        element.randomX = EditorGUILayout.Toggle(element.randomX, GUILayout.Width(32));
        element.randomY = EditorGUILayout.Toggle(element.randomY, GUILayout.Width(32));
        element.randomZ = EditorGUILayout.Toggle(element.randomZ, GUILayout.Width(32));
        
        EditorGUILayout.EndHorizontal();

        element.localPositionOffset = EditorGUILayout.Vector3Field("Local Pos Offset", element.localPositionOffset);
        element.localRotationOffset = EditorGUILayout.Vector3Field("Local Rot Offset", element.localRotationOffset);

        element.randomizeSize = EditorGUILayout.Toggle("Random Size", element.randomizeSize);
        if (element.randomizeSize) {
            element.sizeRange = EditorGUILayout.Vector2Field("Size Range", element.sizeRange);
        }
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
            newElement.variations = new GameObject[1] { variation };
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
        // painter.gameObject.name = "Prefab Painter";

        allElements = GetAllInstances<PrefabPainterElement>();

        elementNames = new string[allElements.Length];

        for (int i =0 ; i< allElements.Length; i++) {
            elementNames[i] = allElements[i].name;
        }

        helperTransform = new GameObject("PREFAB_PAINTER_HELPER").transform;
    }

    Transform helperTransform;
        

        
     
    void OnDisable () {
        DestroyImmediate(helperTransform.gameObject);
    }

    LayerMask paintLayerMask = -1;
    float radius = 1;
    int amount = 1;
    bool drawMode = true;
    bool useRadius;
    int variationIndex=-2;    
    GameObject currentPreview;
    Vector4 chosenPreviewRotation;


    void UpdatePreviewRandom () {
        chosenPreviewRotation.x = Random.Range(0,360);
        chosenPreviewRotation.y = Random.Range(0,360);
        chosenPreviewRotation.z = Random.Range(0,360);
        chosenPreviewRotation.w = Random.Range(currentElement.sizeRange.x, currentElement.sizeRange.y);
    }

    void DestoryCurrentPreview () {
        if (currentPreview != null) {
            DestroyImmediate(currentPreview);
        }
    }


    void OnPreviewIndexChange () {
        if (!validElement)
            return;
        if (useRadius) 
            return;

        DestoryCurrentPreview();
        
        int index = variationIndex == -1 ? Random.Range(0, currentElement.variations.Length) : variationIndex;
        currentPreview = PrefabUtility.InstantiatePrefab(currentElement.variations[index]) as GameObject;
        
        Collider[] cols = currentPreview.GetComponentsInChildren<Collider>();
        for (int i = 0; i < cols.Length; i++) cols[i].enabled = false;
        
        currentPreview.transform.SetParent(helperTransform);
        // currentPreview.transform.localPosition = currentElement.localPositionOffset;
        // currentPreview.transform.localRotation = Quaternion.Euler(currentElement.localRotationOffset);
        
        UpdatePreviewRandom();
    }

    void OnCurrentElementChange () {
        variationIndex = -1;
        OnPreviewIndexChange();
    }

    Vector3 lastHitPos, lastHitNormal;

    void HandlePreview (bool isValid, Vector3 hitPos, Vector3 hitNormal, bool shiftHeld) {
        
        if (useRadius || !drawMode || !validElement) {
            previewLocked = false;
            DestoryCurrentPreview();
            return;
        }

        if ((!isValid||shiftHeld) && !previewLocked) {
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

        if (previewLocked) {
            hitPos = lastHitPos;
            hitNormal = lastHitNormal;
        }
        else {
            lastHitPos = hitPos;
            lastHitNormal = hitNormal;
        }

        helperTransform.position = hitPos;
        helperTransform.rotation = Quaternion.identity;
            
        if (currentElement.alignNormal)
            helperTransform.rotation = Quaternion.FromToRotation(Vector3.up, hitNormal);

        if (currentElement.randomX) helperTransform.Rotate(chosenPreviewRotation.x, 0, 0);
        if (currentElement.randomY) helperTransform.Rotate(0, chosenPreviewRotation.y, 0);
        if (currentElement.randomZ) helperTransform.Rotate(0, 0, chosenPreviewRotation.z);
        
        if (currentElement.randomizeSize) {
            currentPreview.transform.localScale = new Vector3 (chosenPreviewRotation.w, chosenPreviewRotation.w, chosenPreviewRotation.w);
        }
    
        currentPreview.transform.localPosition = currentElement.localPositionOffset;
        currentPreview.transform.localRotation = Quaternion.Euler(currentElement.localRotationOffset);


        if (previewLocked) {
            GUI.enabled = false;
            Handles.PositionHandle (currentPreview.transform.position, currentPreview.transform.rotation);
            GUI.enabled = true;
        }
    }

    bool previewLocked;
    

    public override void OnInspectorGUI()
    {
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

        if (GUILayout.Button("Create New Element")) {
            CreateNewPainterElement (null);
        }
        GameObject newVariation = (GameObject)EditorGUILayout.ObjectField("New From Prefab", null, typeof(GameObject), false);
        if (newVariation != null) {
            CreateNewPainterElement(newVariation);
        }

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
            variationIndex = EditorGUILayout.IntSlider("Paint Variation (-1: Random)", variationIndex, -1, currentElement.variations.Length-1);
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
            helpString = string.Format("Preview Lock: {0}\nA: Switch To Radius Draw\nD: Exit draw mode.\nF: Focus area\nS: Lock preview", previewLocked);
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
        if((EventType.KeyUp == current.type ) && KeyCode.D == current.keyCode)
        {
            current.Use();
            drawMode = false;
            // return;
        }
        if((EventType.KeyUp == current.type ) && KeyCode.S == current.keyCode)
        {
            current.Use();
            previewLocked = !previewLocked;
            return;
        }
        if((EventType.KeyUp == current.type ) && KeyCode.A == current.keyCode)
        {
            current.Use();
            useRadius = !useRadius;
            return;
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
        if (previewLocked) {
            Debug.LogWarning("Cant instantiate when preview locked");
            return;
        }
        if (!validElement || currentElement.variations == null || currentElement.variations.Length == 0)
            return;
        
        // Transform helperTransform = new GameObject("PREFAB_PAINTER_HELPER").transform;
        helperTransform.position = hitPos;
        helperTransform.rotation = Quaternion.identity;

        if (currentElement.alignNormal)
            helperTransform.rotation = Quaternion.FromToRotation(Vector3.up, hitNormal);
        
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
            }
            else
            {
                return;
            }

            if (currentElement.randomX) helperTransform.Rotate(Random.Range(0, 360), 0, 0);
            if (currentElement.randomY) helperTransform.Rotate(0, Random.Range(0, 360), 0);
            if (currentElement.randomZ) helperTransform.Rotate(0, 0, Random.Range(0, 360));

        }
        else {
            if (currentElement.randomX) helperTransform.Rotate(chosenPreviewRotation.x, 0, 0);
            if (currentElement.randomY) helperTransform.Rotate(0, chosenPreviewRotation.y, 0);
            if (currentElement.randomZ) helperTransform.Rotate(0, 0, chosenPreviewRotation.z);

        }


        int index = variationIndex == -1 ? Random.Range(0, currentElement.variations.Length) : variationIndex;
        GameObject instance = PrefabUtility.InstantiatePrefab(currentElement.variations[index]) as GameObject;
        
        instance.transform.SetParent(helperTransform);
        instance.transform.localPosition = currentElement.localPositionOffset;
        instance.transform.localRotation = Quaternion.Euler(currentElement.localRotationOffset);
        

        if (currentElement.randomizeSize) {
            float size = chosenPreviewRotation.w;
            if (useRadius) {
                size = Random.Range(currentElement.sizeRange.x, currentElement.sizeRange.y);
            }
            instance.transform.localScale = new Vector3 (size, size, size);
        }
        
        instance.transform.SetParent(null);
        
        instance.AddComponent<PrefabPainted>();

        Undo.RegisterCreatedObjectUndo(instance, "Instantiate");

        if (!useRadius) {
            UpdatePreviewRandom();
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