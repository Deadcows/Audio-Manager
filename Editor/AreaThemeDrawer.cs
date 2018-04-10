using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(AreaTheme))]
public class AreaThemeDrawer : Editor
{
	public override void OnInspectorGUI()
	{
		var themeAmbient = serializedObject.FindProperty("_ambient");
		var themeRandoms = serializedObject.FindProperty("_randomSounds");
		var themeVolume = serializedObject.FindProperty("_baseVolume");
		
		AudioManagerUtils.DrawLine(AudioManagerUtils.Gray);
		
		EditorGUILayout.PropertyField(themeAmbient);

		bool addRandom = false;
		int removeRandom = -1;
		if (GUILayout.Button("Add Random Sound", EditorStyles.toolbarButton)) addRandom = true;
		
		var randoms = AudioManagerUtils.AsArray(themeRandoms);
		for (int i = 0; i < randoms.Length; i++)
		{
			var random = randoms[i];
			var sound = random.FindPropertyRelative("Sound");
			var minDelay = random.FindPropertyRelative("MinSecondsDelay");
			var maxDelay = random.FindPropertyRelative("MaxSecondsDelay");
			
			EditorGUILayout.PropertyField(sound);

			using (new GUILayout.HorizontalScope())
			{
				if (GUILayout.Button("-", EditorStyles.toolbarButton, GUILayout.Width(25))) removeRandom = i;
				EditorGUILayout.Space();
				EditorGUILayout.PropertyField(minDelay, GUIContent.none, GUILayout.Width(60));
				EditorGUILayout.PropertyField(maxDelay, GUIContent.none, GUILayout.Width(60));
				EditorGUILayout.Space();
			}

			if (minDelay.floatValue < 10) minDelay.floatValue = 10;
			if (maxDelay.floatValue < minDelay.floatValue) maxDelay.floatValue = minDelay.floatValue + 5;
			
			AudioManagerUtils.DrawLine(AudioManagerUtils.Green);
		}
		
		EditorGUILayout.PropertyField(themeVolume);

		AudioManagerUtils.DrawLine(AudioManagerUtils.Gray);

		if (addRandom) AudioManagerUtils.NewElement(themeRandoms);
		if (removeRandom >= 0) themeRandoms.DeleteArrayElementAtIndex(removeRandom);

		if (GUI.changed) serializedObject.ApplyModifiedProperties();
	}

}
