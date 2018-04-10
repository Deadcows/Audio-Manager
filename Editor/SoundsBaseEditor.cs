using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SoundsBase))]
public class SoundsBaseEditor : Editor
{
	public override void OnInspectorGUI()
	{
		EditorGUILayout.LabelField("Use AudioManager to edit SoundsBase");

		if (GUILayout.Button("Find AudioManager", EditorStyles.toolbarButton))
		{
			var manager = FindObjectOfType<AudioManager>();
			if (manager != null) Selection.activeGameObject = manager.gameObject;
		}
	}
}
