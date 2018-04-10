using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Sound))]
public class SoundDrawer : PropertyDrawer
{

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		if (_manager == null)
		{
			return 20 + EditorGUI.GetPropertyHeight(property, label);
		}
		return 35;
	}

	private static AudioManager _manager;


	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		if (_manager == null) _manager = Object.FindObjectOfType<AudioManager>();
		if (_manager == null)
		{
			EditorGUI.LabelField(position, label.text + ": AudioManager required on scene to draw gui", EditorStyles.boldLabel);
			EditorGUI.PropertyField(position, property, label, true);
			return;
		}

		SerializedProperty type = property.FindPropertyRelative("Type");
		SerializedProperty id = property.FindPropertyRelative("Id");
		SerializedProperty clips = property.FindPropertyRelative("Clips");

		var labelWidth = EditorGUIUtility.labelWidth;
		var width = position.width;

		bool custom = type.stringValue == "Custom";
		if (id.stringValue == null) id.stringValue = string.Empty;

		EditorGUI.BeginProperty(position, label, property);

		bool customValid = custom && clips.arraySize > 0;
		if (customValid)
		{
			if (clips.arraySize == 0)
				customValid = false;
			else
			{
				var firstClipProp = clips.GetArrayElementAtIndex(0);
				var firstClip = firstClipProp.objectReferenceValue as AudioClip;
				customValid = firstClip != null;
			}
		}
		bool soundValid = false;
		if (!custom)
		{
			if (clips.arraySize > 0)
			{
				clips.arraySize = 0;
				clips.serializedObject.ApplyModifiedProperties();
			}

			var soundInBase = !string.IsNullOrEmpty(id.stringValue) ? _manager.Base.Sounds.SingleOrDefault(s => s.Id == id.stringValue) : null;
			if (soundInBase == null) soundInBase = _manager.Base.Sounds.SingleOrDefault(s => s.Type == type.stringValue);

			if (soundInBase != null)
			{
				if (soundInBase.Id != id.stringValue)
				{
					if (!string.IsNullOrEmpty(id.stringValue))
						Debug.LogWarning("Sound Id changed from " + id.stringValue + " to " + soundInBase.Id, property.serializedObject.targetObject);
					id.stringValue = soundInBase.Id;
					id.serializedObject.ApplyModifiedProperties();
				}
				if (soundInBase.Type != type.stringValue)
				{
					Debug.LogWarning("Sound Type changed from " + type.stringValue + " to " + soundInBase.Type, property.serializedObject.targetObject);
					type.stringValue = soundInBase.Type;
					type.serializedObject.ApplyModifiedProperties();
				}

				soundValid = true;
			}
		}
		bool valid = (custom && customValid) || soundValid;

		var colored = position;
		colored.y += 5;
		colored.height -= 8;
		AudioManagerUtils.DrawColouredRect(colored, valid ? AudioManagerUtils.Blue : AudioManagerUtils.Red);


		position.y += 10;
		position.height = 20;
		position.width = labelWidth - 5;
		EditorGUI.LabelField(position, label);
		position.x += labelWidth;


		if (custom)
			position.width = 20;
		else
			position.width = width - labelWidth - 5;

		if (GUI.Button(position, custom ? "~" : type.stringValue, EditorStyles.toolbarButton))
		{
			AudioManagerEditor.DrawMenu(data =>
			{
				type.stringValue = (string)data;
				if (type.stringValue != "Custom")
					id.stringValue = _manager.Base.Sounds.Single(s => s.Type == type.stringValue).Id;

				property.serializedObject.ApplyModifiedProperties();
			});
		}

		if (custom)
		{
			position.width = width - (labelWidth + 25);
			position.x = labelWidth + 40;
			position.height = 16;
			position.y += 1;

			Object clip = null;
			if (clips.arraySize > 0)
			{
				clip = clips.GetArrayElementAtIndex(0).objectReferenceValue;
				if (clip == null)
				{
					clips.arraySize = 0;

					clips.serializedObject.ApplyModifiedProperties();
				}
			}

			var newClip = EditorGUI.ObjectField(position, clip, typeof(AudioClip), false);
			if (clip != newClip)
			{
				if (clips.arraySize < 1)
				{
					clips.arraySize = 1;

					clips.serializedObject.ApplyModifiedProperties();
				}
				clips.GetArrayElementAtIndex(0).objectReferenceValue = newClip;
			}
		}

		EditorGUI.EndProperty();

		if (property.depth == 0)
		{
			Sound sound = fieldInfo.GetValue(property.serializedObject.targetObject) as Sound;
			if (sound != null && clips.arraySize == 0 && sound.Clips != null)
			{
				sound.Clips = null;
				fieldInfo.SetValue(property.serializedObject.targetObject, sound);
				EditorUtility.SetDirty(property.serializedObject.targetObject);
			}
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty(property.serializedObject.targetObject);
			property.serializedObject.ApplyModifiedProperties();
		}
	}

}
