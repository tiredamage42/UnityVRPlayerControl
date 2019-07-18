using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

public class SkyEditor 
{

    public static bool ShurikenFoldoutHeader(string text, GUIStyle texStyle, GUILayoutOption headerHeight, ref bool foldout)
	{
		EditorGUILayout.BeginHorizontal(texStyle, headerHeight);
		{
			StartIndent();
			foldout = EditorGUILayout.Foldout(foldout, text);//, texStyle);
			EndIndent();
		}
		EditorGUILayout.EndHorizontal();
		return foldout;
	}
    public static int SeparatorHeight{ get{ return 2; } }

		static GUILayoutOption _indentWidth;
		static GUILayoutOption indentWidth {
			get {
				if (_indentWidth == null) {
					_indentWidth = GUILayout.Width(16);
				}
				return _indentWidth;
			}
		}
		
		public static void StartIndent () {
			EditorGUILayout.BeginHorizontal();
			GUI.backgroundColor = Color.clear;
			GUILayout.Button("", indentWidth);
			GUI.backgroundColor = Color.white;
			EditorGUILayout.BeginVertical();
		}
		public static void EndIndent () {
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}

        public static void Separator(int height)
		{
			GUILayout.Box("", new GUILayoutOption[] {GUILayout.ExpandWidth(true), GUILayout.Height(height)});			
		}
        

}
