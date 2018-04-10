using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System;

public static class AudioManagerUtils 
{
	public static Color Gray => new Color(.3f, .3f, .3f);
	public static Color Green => new Color(.4f, .6f, .4f, .2f);
	public static Color Red => new Color(.8f, .6f, .6f);
	public static Color Yellow => new Color(.8f, .8f, .2f, .6f);
	public static Color Blue => new Color(.6f, .6f, .8f);


	public static string ArrowUp => "▲";
	public static string ArrowDown => "▼";


	public static void DrawLine(Color color)
	{
		EditorGUILayout.Space();

		var defaultBackgroundColor = GUI.backgroundColor;
		GUI.backgroundColor = color;
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
		GUI.backgroundColor = defaultBackgroundColor;

		EditorGUILayout.Space();
	}

	public static SerializedProperty[] AsArray(SerializedProperty property)
	{
		List<SerializedProperty> items = new List<SerializedProperty>();
		for (int i = 0; i < property.arraySize; i++)
			items.Add(property.GetArrayElementAtIndex(i));
		return items.ToArray();
	}

	public static void ReplaceArray(SerializedProperty property, UnityEngine.Object[] newElements)
	{
		property.arraySize = 0;
		property.serializedObject.ApplyModifiedProperties();
		property.arraySize = newElements.Length;
		for (var i = 0; i < newElements.Length; i++)
		{
			property.GetArrayElementAtIndex(i).objectReferenceValue = newElements[i];
		}
		property.serializedObject.ApplyModifiedProperties();
	}

	public static SerializedProperty NewElement(SerializedProperty property)
	{
		int newElementIndex = property.arraySize;
		property.InsertArrayElementAtIndex(newElementIndex);
		return property.GetArrayElementAtIndex(newElementIndex);
	}

	public static void DrawBackgroundBox(Color color, int height, int yOffset = 0)
	{
		var defColor = GUI.color;
		GUI.color = color;
		var rect = GUILayoutUtility.GetLastRect();
		rect.center = new Vector2(rect.center.x, rect.center.y + 6 + yOffset);
		rect.height = height;
		GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
		GUI.color = defColor;
	}

	public static void DrawColouredRect(Rect rect, Color color)
	{
		var defaultBackgroundColor = GUI.backgroundColor;
		GUI.backgroundColor = color;
		GUI.Box(rect, "");
		GUI.backgroundColor = defaultBackgroundColor;
	}


	public static T[] DropArea<T>(string areaText, float height, bool allowExternal = false,
		string externalImportFolder = null) where T : UnityEngine.Object
	{
		Event currentEvent = Event.current;
		Rect drop_area = GUILayoutUtility.GetRect(0.0f, height, GUILayout.ExpandWidth(true));
		var style = new GUIStyle(GUI.skin.box);
		style.alignment = TextAnchor.MiddleCenter;
		GUI.Box(drop_area, areaText, style);

		switch (currentEvent.type)
		{
		case EventType.DragUpdated:
		case EventType.DragPerform:
			if (!drop_area.Contains(currentEvent.mousePosition))
				return null;

			DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
			if (currentEvent.type == EventType.DragPerform)
			{
				DragAndDrop.AcceptDrag();
				Event.current.Use();

				List<T> result = new List<T>();

				if (allowExternal && DragAndDrop.paths.Length > 0 && DragAndDrop.paths.Length > DragAndDrop.objectReferences.Length)
				{
					var folderToLoad = "/";
					if (!string.IsNullOrEmpty(externalImportFolder))
					{
						folderToLoad = "/" + externalImportFolder.Replace("Assets/", "").Trim('/', '\\') + "/";
					}
					List<string> importedFiles = new List<string>();

					foreach (string externalPath in DragAndDrop.paths)
					{
						if (externalPath.Length == 0) continue;
						try
						{
							var filename = Path.GetFileName(externalPath);
							var relativePath = folderToLoad + filename;
							Directory.CreateDirectory(Application.dataPath + folderToLoad);
							FileUtil.CopyFileOrDirectory(externalPath, Application.dataPath + relativePath);
							importedFiles.Add("Assets" + relativePath);
						}
						catch (Exception ex)
						{
							Debug.LogException(ex);
						}
					}
					AssetDatabase.Refresh();

					foreach (var importedFile in importedFiles)
					{
						var asset = AssetDatabase.LoadAssetAtPath<T>(importedFile);
						if (asset != null)
						{
							result.Add(asset);
							Debug.Log("Asset imported at path: " + importedFile);
						}
						else AssetDatabase.DeleteAsset(importedFile);
					}
				}
				else
				{
					foreach (UnityEngine.Object dragged in DragAndDrop.objectReferences)
					{
						var validObject = dragged as T ?? AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GetAssetPath(dragged));

						if (validObject != null) result.Add(validObject);
					}
				}

				if (result.Count > 0) return result.OrderBy(o => o.name).ToArray();
				return null;
			}
			break;
		}
		return null;
	}
}
