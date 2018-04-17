using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

// TODO: relate on ResourceEngine, and not on custom editor? But what about sound bundles?


[CustomEditor(typeof(AudioManager))]
public class AudioManagerEditor : Editor
{


	private static AudioManager Manager => _manager ?? (_manager = FindObjectOfType<AudioManager>());
	private static AudioManager _manager;

	private void OnEnable()
	{
		_manager = target as AudioManager;
		Debug.Assert(Manager != null, "Manager != null");

		Manager.Base.Sounds = Manager.Base.Sounds.OrderBy(s => s.Type).ToList();
	}

	private string _unfoldedGroup = string.Empty;
	private string _unfoldedSound = string.Empty;
	public override void OnInspectorGUI()
	{
		var groups = Manager.Base.Sounds.Where(s => s.Type.Contains('/')).GroupBy(e => e.Type.Split('/').First()).Select(e => e.Key);

		serializedObject.Update();

		var audioBase = serializedObject.FindProperty("Base");
		var audioBaseSO = new SerializedObject(audioBase.objectReferenceValue);
		audioBaseSO.Update();
		var sounds = audioBaseSO.FindProperty("Sounds");

		EditorGUILayout.PropertyField(audioBase);


		EditorGUILayout.Space();
		if (GUILayout.Toggle(_unfoldedGroup == string.Empty, "All", EditorStyles.toolbarButton))
			_unfoldedGroup = string.Empty;

		foreach (var g in groups)
		{
			if (GUILayout.Toggle(_unfoldedGroup == g, g, EditorStyles.toolbarButton) && _unfoldedGroup != g)
			{
				_unfoldedGroup = g;
				_unfoldedSound = string.Empty;
			}
		}
		EditorGUILayout.Space();

		int toReplace = -1;
		var soundsArray = AudioManagerUtils.AsArray(sounds);
		for (int index = 0; index < soundsArray.Length; index++)
		{
			var sound = soundsArray[index];

			var soundType = sound.FindPropertyRelative("Type");
			var clips = sound.FindPropertyRelative("Clips");
			var clipsArray = AudioManagerUtils.AsArray(clips);

			bool toDraw = !soundType.stringValue.Contains('/') || soundType.stringValue.StartsWith(_unfoldedGroup);
			if (!toDraw) continue;

			bool unfolded = soundType.stringValue == _unfoldedSound;


			AudioManagerUtils.DrawLine(AudioManagerUtils.Blue);

			if (clipsArray.Length == 0 || clipsArray.Any(c => c.objectReferenceValue == null))
				AudioManagerUtils.DrawBackgroundBox(AudioManagerUtils.Red, 28, -4);
			if (clipsArray.Length > 1)
				AudioManagerUtils.DrawBackgroundBox(AudioManagerUtils.Yellow, 28, -4);

			using (new GUILayout.HorizontalScope())
			{
				if (GUILayout.Button(unfolded ? AudioManagerUtils.ArrowUp : AudioManagerUtils.ArrowDown, EditorStyles.toolbarButton, GUILayout.Width(20)))
					_unfoldedSound = unfolded ? string.Empty : soundType.stringValue;

				var newType = EditorGUILayout.DelayedTextField(soundType.stringValue);
				if (soundType.stringValue != newType)
				{
					soundType.stringValue = newType;
					if (newType.Contains('/'))
						_unfoldedGroup = newType.Split('/')[0];
				}

				// Replace clips of Sound with new ones
				var newClipsToReplace = AudioManagerUtils.DropArea<AudioClip>("replace", 20);
				if (newClipsToReplace != null)
					AudioManagerUtils.ReplaceArray(clips, newClipsToReplace.ToArray<Object>());

				if (unfolded)
				{
					if (GUILayout.Button("x", EditorStyles.toolbarButton, GUILayout.Width(20)))
					{
						toReplace = index;
					}
				}
			}


			// Draw Clips of Sound if unfolded
			if (_unfoldedSound != soundType.stringValue) continue;

			EditorGUI.indentLevel++;
			for (int y = 0; y < clipsArray.Length; y++)
			{
				clipsArray[y].objectReferenceValue = (AudioClip)EditorGUILayout.ObjectField(clipsArray[y].objectReferenceValue, typeof(AudioClip), false);
			}
			EditorGUI.indentLevel--;
		}

		EditorGUILayout.Space();
		var newClips = AudioManagerUtils.DropArea<AudioClip>("Drop to add sound", 50);
		if (newClips != null)
		{
			var newSound = AudioManagerUtils.NewElement(sounds);
			var newSoundType = newSound.FindPropertyRelative("Type");
			var newSoundId = newSound.FindPropertyRelative("Id");
			var newSoundClips = newSound.FindPropertyRelative("Clips");

			newSoundType.stringValue = string.IsNullOrEmpty(_unfoldedGroup) ? "Custom" : _unfoldedGroup;
			newSoundId.stringValue = Guid.NewGuid().ToString();
			AudioManagerUtils.ReplaceArray(newSoundClips, newClips.ToArray<Object>());
		}
		EditorGUILayout.Space();

		if (toReplace >= 0)
		{
			sounds.DeleteArrayElementAtIndex(toReplace);
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty(Manager);
			serializedObject.ApplyModifiedProperties();
			audioBaseSO.ApplyModifiedProperties();
		}
	}



	public static void DrawMenu(GenericMenu.MenuFunction2 callback)
	{
		GenericMenu toolsMenu = new GenericMenu();

		toolsMenu.AddItem(new GUIContent("Custom"), false, callback, "Custom");

		foreach (Sound entry in Manager.Base.Sounds)
		{
			if (entry == null) continue;
			toolsMenu.AddItem(new GUIContent(entry.Type.Trim('/')), false, callback, entry.Type);
		}

		toolsMenu.ShowAsContext();
	}

}
