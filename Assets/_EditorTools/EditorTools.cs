using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Linq;

public class AssetSelector<T> where T : UnityEngine.Object{
    List<T> assets;
    string[] allNames;
    Func<T, string> namePredicate;
    Func<T,int> orderPredicate;
    GUIContent resetButtonContent;
    public AssetSelector (Func<T, string> namePredicate, Func<T, int> orderPredicate) {
        this.namePredicate = namePredicate;
        this.orderPredicate = orderPredicate;
        resetButtonContent = new GUIContent("", "Update Asset References");
        UpdateAssetReferences();
    }

    public void UpdateAssetReferences () {
        assets = FindAssetsByType(orderPredicate);
        assets.Insert(0, null);
        
        allNames = new string[assets.Count];
        allNames[0] = " [ Null ] ";
        for (int i = 1; i < assets.Count; i++) 
            allNames[i] = namePredicate != null ? namePredicate(assets[i]) : assets[i].name;
    }

    public static List<T> FindAssetsByType(System.Func<T, int> orderPredicate) //where T : UnityEngine.Object
    {
        Debug.Log("finding assets");
        List<T> assets = new List<T>();

        string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T).FullName));

        Debug.Log("found " + guids.Length + " guids");
        for( int i = 0; i < guids.Length; i++ )
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>( AssetDatabase.GUIDToAssetPath( guids[i] ) );
            if ( asset != null ) assets.Add(asset);
        }
        Debug.Log("found assets: " + assets.Count);

        T[] resourcesFound = Resources.FindObjectsOfTypeAll<T>();

        for (int i = 0; i < resourcesFound.Length; i++) {
            if (!assets.Contains(resourcesFound[i])) {
                assets.Add(resourcesFound[i]);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(resourcesFound[i]));
            }
        }

                Debug.Log("resources found: " + resourcesFound.Length);

        if (orderPredicate != null) 
            return assets.OrderBy(orderPredicate).ToList();

        return assets;
    }

    public void Draw (SerializedProperty property, GUIContent gui) {
        property.objectReferenceValue = Draw((T) property.objectReferenceValue, gui);
    }

    public void Draw (Rect position, SerializedProperty property, GUIContent gui) {
        property.objectReferenceValue = Draw(position, (T) property.objectReferenceValue, gui);
    }
    
    int GetActiveIndex (T current) {
        for (int i =0 ; i < assets.Count; i++) {
            if (assets[i] == current) return i;
        }
        return -1;
    }

    const float buttonWidth = 12;
    const float buffer = 12;
    static readonly Color32 resetButtonColor = new Color32( 100, 100, 255, 255 );

    public T Draw (Rect position, T current, GUIContent gui) {
        int selected = EditorGUI.Popup (new Rect(position.x, position.y, (position.width - buttonWidth) - buffer, position.height), gui.text, GetActiveIndex(current), allNames);
        GUI.backgroundColor = resetButtonColor;
        if (GUI.Button(new Rect(position.x + ((position.width - buttonWidth) - buffer), position.y, buttonWidth, buttonWidth), resetButtonContent, EditorStyles.miniButton)) {
            UpdateAssetReferences();
        }
        GUI.backgroundColor = Color.white;
        return selected < 0 ? null : assets[selected];
    }
    public T Draw (T current, GUIContent gui) {
        EditorGUILayout.BeginHorizontal();
        int selected = EditorGUILayout.Popup (gui, GetActiveIndex(current), allNames);

        GUI.backgroundColor = resetButtonColor;
        if (GUILayout.Button(resetButtonContent, EditorStyles.miniButton)) {
            UpdateAssetReferences();
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        return selected < 0 ? null : assets[selected];
    }
}

public static class EditorTools {
    const int buttonWidth = 20;
    public static float DrawIndent (int level, float startX) {
        return startX + buttonWidth * level;
    }           
}

[CustomPropertyDrawer(typeof(NeatArrayAttribute))] public class NeatArrayAttributeDrawer : PropertyDrawer
{
    static readonly Color32 deleteColor = new Color32(200,75,75,255);
    static readonly Color32 addColor = new Color32(75,200,75,255);

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        float singleLine = EditorGUIUtility.singleLineHeight;
        property = property.FindPropertyRelative("list");
        
        float _x = EditorTools.DrawIndent(EditorGUI.indentLevel, position.x);
        float y = position.y;

        float buttonWidth = 12;
        GUI.backgroundColor = addColor;
        if (GUI.Button(new Rect(_x, y, buttonWidth, buttonWidth), "", EditorStyles.miniButton)) {
            property.InsertArrayElementAtIndex(property.arraySize);
        }
        GUI.backgroundColor = Color.white;
        
        float labelWidth = EditorStyles.label.CalcSize(label).x;
        GUI.Label(new Rect(_x + buttonWidth, y, labelWidth, singleLine), label);
        
        int arraySize = property.arraySize;
        float h = EditorGUIUtility.singleLineHeight * (arraySize == 0 ? 1 : 1.25f);
        for (int i = 0; i < arraySize; i++) h += EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(i), true);
        
        GUI.backgroundColor = new Color(0,0,0,.1f);
        GUI.Box( new Rect(_x, y, position.width, h ),"");
        GUI.backgroundColor = Color.white;
        
        y += singleLine;

        int indexToDelete = -1;

        EditorGUI.indentLevel ++;
        for (int i = 0; i < arraySize; i++) {

            GUI.backgroundColor = deleteColor;
            if (GUI.Button(new Rect(EditorTools.DrawIndent(EditorGUI.indentLevel, position.x), y, buttonWidth, buttonWidth), "", EditorStyles.miniButton)) indexToDelete = i;
            GUI.backgroundColor = Color.white;

            SerializedProperty p = property.GetArrayElementAtIndex(i);
            
            EditorGUI.PropertyField(new Rect(position.x + buttonWidth + 5, y, position.width, singleLine), p);
            y += EditorGUI.GetPropertyHeight(p, true);
        }
        EditorGUI.indentLevel--;

        if (indexToDelete != -1) property.DeleteArrayElementAtIndex(indexToDelete);
        
        EditorGUI.EndProperty();
     }
     
    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        prop = prop.FindPropertyRelative("list");
        int arraySize = prop.arraySize;
        float h = EditorGUIUtility.singleLineHeight * (arraySize == 0 ? 1 : 1.5f);
        for (int i = 0; i < arraySize; i++) h += EditorGUI.GetPropertyHeight(prop.GetArrayElementAtIndex(i), true);
        return h;
    }
}
#endif

    public class NeatArrayAttribute : PropertyAttribute { }
    [System.Serializable] public class NeatIntList : NeatListWrapper<int> {}
    [System.Serializable] public class NeatStringArray : NeatArrayWrapper<string> {}
    [System.Serializable] public class NeatFloatArray : NeatArrayWrapper<float> {}

    
    
    public class NeatArrayWrapper<T> {
        public T[] list;
        public static implicit operator T[](NeatArrayWrapper<T> c) { return c.list; }
        public static implicit operator NeatArrayWrapper<T>(T[] l) { return new NeatArrayWrapper<T>(){ list = l }; }
    }
    public class NeatListWrapper<T> {
        public List<T> list;
        public static implicit operator List<T>(NeatListWrapper<T> c) { return c.list; }
        public static implicit operator NeatListWrapper<T>(List<T> l) { return new NeatListWrapper<T>(){ list = l }; }
    }


    
    // // Create a layer at the next available index. Returns silently if layer already exists.
    // public static void CreateLayer(string name)
    // {
    //     if (string.IsNullOrEmpty(name))
    //         throw new System.ArgumentNullException("name", "New layer name string is either null or empty.");

    //     var tagManager = new UnityEditor.SerializedObject(UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
    //     var layerProps = tagManager.FindProperty("layers");
    //     var propCount = layerProps.arraySize;

    //     UnityEditor.SerializedProperty firstEmptyProp = null;

    //     for (var i = 0; i < propCount; i++)
    //     {
    //         var layerProp = layerProps.GetArrayElementAtIndex(i);
    //         var stringValue = layerProp.stringValue;

    //         if (stringValue == name) return;

    //         if (i < 8 || stringValue != string.Empty) 
    //             continue;

    //         if (firstEmptyProp == null) {
    //             firstEmptyProp = layerProp;
    //             break;
    //         }
    //     }

    //     if (firstEmptyProp == null)
    //     {
    //         Debug.LogError("Maximum limit of " + propCount + " layers exceeded. Layer \"" + name + "\" not created.");
    //         return;
    //     }

    //     firstEmptyProp.stringValue = name;
    //     tagManager.ApplyModifiedProperties();
    // }