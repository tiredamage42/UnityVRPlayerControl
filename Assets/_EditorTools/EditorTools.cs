
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using UnityEditorInternal;
using UnityEditor.AnimatedValues;

public class AssetSelector<T> where T : UnityEngine.Object{
    T[] allAssets;
    string[] allAssetNames;

    public AssetSelector () {
        UpdateAssetReferences();
    }

    public void UpdateAssetReferences () {
        FindAllAssets();
    }


    void FindAllAssets () {
        allAssets = FindAssetsByType().ToArray();
        allAssetNames = new string[allAssets.Length];
        for (int i = 0; i < allAssets.Length; i++) {
            allAssetNames[i] = allAssets[i].name;
        }
    }

    public static List<T> FindAssetsByType() //where T : UnityEngine.Object
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
        
        if (selected < 0)
            return null;
            
        return allAssets[selected];

    }
}



// This is not an editor script. The property attribute class should be placed in a regular script file.
public class DisplayedArrayAttribute : PropertyAttribute
{
    public float[] rgba;
    public bool useSpacing;
    public DisplayedArrayAttribute(float[] rgba, bool useSpacing=false)
    {
        this.rgba = rgba;
        this.useSpacing = useSpacing;
    }
}




//Now that you have the attribute, you need to make a PropertyDrawer that draws properties that have that attribute. 
// The drawer must extend the PropertyDrawer class, and it must have a CustomPropertyDrawer attribute 
//to tell it which attribute it's a drawer for. Here's an example using IMGUI:

// The property drawer class should be placed in an editor script, inside a folder called Editor.

[CustomPropertyDrawer(typeof(DisplayedArrayAttribute))]

public class DisplayedArrayAttributeDrawer : PropertyDrawer
{

    static readonly Color32 deleteColor = new Color32(200,75,75,255);
    static readonly Color32 addColor = new Color32(75,200,75,255);


    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        float singleLine = EditorGUIUtility.singleLineHeight;

        DisplayedArrayAttribute att = attribute as DisplayedArrayAttribute;
        Color boxColor = new Color( att.rgba[0], att.rgba[1], att.rgba[2], att.rgba[3]);
        bool useSpacing = att.useSpacing;

        EditorGUI.BeginProperty(position, label, property);

        property = property.FindPropertyRelative("list");

        // float indenting = 32;

        float indentOffset = 0;// indenting * EditorGUI.indentLevel;

        GUI.backgroundColor = boxColor;
        GUI.Box( new Rect(position.x + (indentOffset-5), position.y, position.width, position.height),"");
        GUI.backgroundColor = Color.white;

        int arraySize = property.arraySize;
        // float y = position.y;
        float y = position.y + (arraySize == 0 ? 0 : singleLine * .25f);


        float labelWidth = GUI.skin.label.CalcSize(label).x;
        EditorGUI.LabelField(new Rect(position.x, y, labelWidth, singleLine), label, EditorStyles.miniButtonLeft);
        
        float buttonWidth = 32;
        GUI.backgroundColor = addColor;
        if (GUI.Button(new Rect(position.x + labelWidth, y, buttonWidth, singleLine), "+", EditorStyles.miniButtonRight)) {
            property.InsertArrayElementAtIndex(property.arraySize);
        }
        GUI.backgroundColor = Color.white;

        // EditorGUI.indentLevel++;
        

        // indentOffset = indenting * EditorGUI.indentLevel;


        buttonWidth = 12;
        float yOffset = arraySize == 0 || !useSpacing ? 0 : (singleLine * .5f);
        y += singleLine + yOffset;// (arraySize == 0 ? 0 : singleLine * .5f);

        int indexToDelete = -1;
        for (int i = 0; i < arraySize; i++) {
            
            GUI.backgroundColor = deleteColor;
            if (GUI.Button(new Rect(position.x + ((indentOffset)), y, buttonWidth, buttonWidth), "", EditorStyles.miniButton)) {
                indexToDelete = i;
            }   
            GUI.backgroundColor = Color.white;

            SerializedProperty p = property.GetArrayElementAtIndex(i);
            
            EditorGUI.PropertyField(new Rect(position.x + buttonWidth + 5, y, position.width, singleLine), p);
            y += EditorGUI.GetPropertyHeight(p, true) + yOffset;

        }

        // EditorGUI.indentLevel--;
        if (indexToDelete != -1) {
            property.DeleteArrayElementAtIndex(indexToDelete);
        }

        EditorGUI.EndProperty();
     }
     
     public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
     {
         float singleLine = EditorGUIUtility.singleLineHeight;
        property = property.FindPropertyRelative("list");

        bool useSpacing = (attribute as DisplayedArrayAttribute).useSpacing;

        int arraySize = property.arraySize;

        float yOffset = arraySize == 0 || !useSpacing ? 0 : (singleLine * .5f);

        float elementsHeight = 0;
        elementsHeight += singleLine + yOffset;
         for (int i = 0; i < property.arraySize; i++) {
             elementsHeight += EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(i), true) + yOffset;
         }
         return elementsHeight + (arraySize == 0 ? 0 : EditorGUIUtility.singleLineHeight * (useSpacing ? .25f : .5f));
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