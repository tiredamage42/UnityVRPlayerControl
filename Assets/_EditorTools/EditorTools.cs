
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using UnityEditorInternal;
using UnityEditor.AnimatedValues;
using System;
using System.Linq;

public class AssetSelector<T> where T : UnityEngine.Object{
    T[] allAssets;
    string[] allAssetNames;
    System.Func<T, string> predicate;
    Func<T,int> orderPredicate;

    public AssetSelector (System.Func<T, string> predicate, System.Func<T, int> orderPredicate) {
        this.predicate = predicate;
        this.orderPredicate = orderPredicate;
        UpdateAssetReferences();
    }

    public void UpdateAssetReferences () {
        FindAllAssets();
    }


    void FindAllAssets () {
        allAssets = FindAssetsByType(orderPredicate).ToArray();
        allAssetNames = new string[allAssets.Length];
        for (int i = 0; i < allAssets.Length; i++) {

            if (predicate != null) {
                allAssetNames[i] = predicate(allAssets[i]);

            }
            else {
                allAssetNames[i] = allAssets[i].name;
            }
        }
    }

    public static IEnumerable<T> FindAssetsByType(System.Func<T, int> orderPredicate) //where T : UnityEngine.Object
    {
        List<T> assets = new List<T>();
        string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
        for( int i = 0; i < guids.Length; i++ )
        {
            string assetPath = AssetDatabase.GUIDToAssetPath( guids[i] );
            T asset = AssetDatabase.LoadAssetAtPath<T>( assetPath );
            if( asset != null )
            {
                assets.Add(asset);
            }
        }
        if (orderPredicate != null) {
            return assets.OrderBy(orderPredicate);
        }
        return assets;
    }

    public void Draw (SerializedProperty property, GUIContent gui) {
        property.objectReferenceValue = Draw((T) property.objectReferenceValue, gui);
    }

    public void Draw (Rect position, SerializedProperty property, GUIContent gui) {
        property.objectReferenceValue = Draw(position, (T) property.objectReferenceValue, gui);
    }

    // EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("item"), GUIContent.none);
    public T Draw (Rect position, T current, GUIContent gui) {

        int activeIndex = -1;

        for (int i =0 ; i < allAssets.Length; i++) {
            if (allAssets[i] == current) {
                activeIndex = i;
                break;
            }
        }
        
        int selected = EditorGUI.Popup (position, gui.text, activeIndex, allAssetNames);
        
        if (selected < 0)
            return null;
            
        return allAssets[selected];
    }


    public T Draw (T current, GUIContent gui) {

        int activeIndex = -1;

        for (int i =0 ; i < allAssets.Length; i++) {
            if (allAssets[i] == current) {
                activeIndex = i;
                break;
            }
        }

        int selected = EditorGUILayout.Popup (gui, activeIndex, allAssetNames);
        
        if (selected < 0) return null;
            
        return allAssets[selected];
    }
}

public static class EditorTools {
    const int buttonWidth = 16;
    public static float DrawIndent (int level, float startX) {
        return startX + buttonWidth * level;
    }           
}

// This is not an editor script. The property attribute class should be placed in a regular script file.
public class DisplayedArrayAttribute : PropertyAttribute { }
[CustomPropertyDrawer(typeof(DisplayedArrayAttribute))] public class DisplayedArrayAttributeDrawer : PropertyDrawer
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

        float labelWidth = EditorStyles.label.CalcSize(label).x;
        GUI.Label(new Rect(_x, y, labelWidth, singleLine), label);
        
        float buttonWidth = 12;
        GUI.backgroundColor = addColor;
        if (GUI.Button(new Rect(_x + labelWidth, y, buttonWidth, buttonWidth), "", EditorStyles.miniButton)) {
            property.InsertArrayElementAtIndex(property.arraySize);
        }
        GUI.backgroundColor = Color.white;

        
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
        for (int i = 0; i < arraySize; i++) 
            h += EditorGUI.GetPropertyHeight(prop.GetArrayElementAtIndex(i), true);
        return h;
    }
}
#endif

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