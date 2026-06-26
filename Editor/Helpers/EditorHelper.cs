using UnityEditor;
using UnityEngine;

public static class EditorHelper
{
	public static void DrawHorizontalLine(float thickness = 1f, float padding = 4f)
	{
		Rect rect = EditorGUILayout.GetControlRect(false, thickness + padding * 2f);
		rect.height = thickness;
		rect.y += padding;

		EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f));
	}

	public static void DrawVerticalLine(float height, float thickness = 1f)
	{
		Rect rect = GUILayoutUtility.GetRect(thickness, height, GUILayout.Width(thickness), GUILayout.Height(height));

		EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f));
	}

	public static bool Button_Colored(GUIContent content, Color color, params GUILayoutOption[] options)
	{
		Color old = GUI.backgroundColor;
		GUI.backgroundColor = color;
		bool clicked = GUILayout.Button(content, options);
		GUI.backgroundColor = old;
		return clicked;
	}

	public static bool Toggle_Colored(bool clicked, string text, Color color, GUIStyle style, params GUILayoutOption[] options)
	{
		Color old = GUI.backgroundColor;
		GUI.backgroundColor = color;
		clicked = GUILayout.Toggle(clicked, text, style, options);
		GUI.backgroundColor = old;
		return clicked;
	}
}
