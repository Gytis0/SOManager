using System;
using UnityEditor;
using UnityEngine;

public class InputWindow : EditorWindow
{
	private string value;
	private Action<string> onConfirm;

	public static void Show(string title, string defaultValue, Action<string> onConfirm)
	{
		InputWindow window = CreateInstance<InputWindow>();

		window.titleContent = new GUIContent(title);
		window.value = defaultValue;
		window.onConfirm = onConfirm;

		window.position = new Rect(Screen.width / 2f, Screen.height / 2f, 300, 80);
		window.ShowUtility();
	}

	private void OnGUI()
	{
		EditorGUILayout.LabelField("Name");

		GUI.SetNextControlName("Input");
		value = EditorGUILayout.TextField(value);

		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button("Cancel"))
			Close();

		if (GUILayout.Button("OK"))
		{
			onConfirm?.Invoke(value);
			Close();
		}

		EditorGUILayout.EndHorizontal();

		EditorGUI.FocusTextInControl("Input");
	}
}