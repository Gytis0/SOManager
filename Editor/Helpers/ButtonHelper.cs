using Gytis0.SOManager.Runtime;
using System;
using UnityEditor;
using UnityEngine;

namespace Gytis0.SOManager.Editor.Helpers
{
	public static class ButtonHelper
	{
		private static float width = 0f;
		private static float height = 0f;

		private static void Init(int panelButtonSize)
		{
			width = panelButtonSize + (GUI.skin.button.margin.horizontal / 2);
			height = panelButtonSize + GUI.skin.button.margin.vertical;
		}

		public static bool DrawPanelButton(int buttonSize, GUIContent icon, GUIStyle style, string text = "")
		{
			if (width == 0f)
				Init(buttonSize);

			Rect rect = GUILayoutUtility.GetRect(width, height, GUILayout.Width(width), GUILayout.Height(height));

			Rect buttonRect = new Rect(rect.x + GUI.skin.button.margin.left, rect.y + GUI.skin.button.margin.top, buttonSize, buttonSize);

			bool clicked = GUI.Button(buttonRect, icon);
			GUI.Label(buttonRect, text, style);

			return clicked;
		}

		#region Types

		public static bool ListType(Type type, int count, bool selected, GUIStyle labelStyle, GUIStyle countStyle)
		{
			Rect rect = GUILayoutUtility.GetRect(0, 24, GUILayout.ExpandWidth(true));

			DrawType(rect, type, count, selected, labelStyle, countStyle);

			if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
			{
				Event.current.Use();
				return true;
			}

			return false;
		}

		private static void DrawType(Rect rect, Type type, int count, bool selected, GUIStyle labelStyle, GUIStyle countStyle)
		{
			if (selected)
				EditorGUI.DrawRect(rect, ColorPalette.Blue_Light);

			GUI.Box(rect, GUIContent.none);

			const float padding = 6f;
			const float countWidth = 40f;

			Rect nameRect = new(rect.x + padding, rect.y, rect.width - countWidth - (padding * 2), rect.height);
			Rect countRect = new(rect.xMax - countWidth - padding, rect.y, countWidth, rect.height);

			GUI.Label(nameRect, type.Name, labelStyle);
			GUI.Label(countRect, count.ToString(), countStyle);
		}

		#endregion

		#region Assets

		public static bool ListAsset(GameDataSO asset, bool selected, GUIStyle style)
		{
			Rect rect = GUILayoutUtility.GetRect(0, 24, GUILayout.ExpandWidth(true));

			ColorAsset(rect, asset, selected);

			DrawListAsset(rect, asset, style);

			if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
			{
				Event.current.Use();
				return true;
			}

			return false;
		}

		public static bool GridAsset(GameDataSO asset, bool selected, float size, GUIStyle style)
		{
			Rect rect = GUILayoutUtility.GetRect(size, size);

			ColorAsset(rect, asset, selected);

			DrawGridAsset(rect, asset, style);

			if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
			{
				Event.current.Use();
				return true;
			}

			return false;
		}

		private static void DrawListAsset(Rect rect, GameDataSO asset, GUIStyle style)
		{
			if (asset.Icon != null)
			{
				Rect icon = new(rect.x + 2, rect.y + 2, 20, 20);
				GUI.DrawTexture(icon, asset.Icon.texture, ScaleMode.ScaleToFit);
			}

			Rect label = new(rect.x + 26, rect.y, rect.width - 26, rect.height);
			GUI.Label(label, asset.EnumName, style);
		}

		private static void DrawGridAsset(Rect rect, GameDataSO asset, GUIStyle style)
		{
			if (asset.Icon != null)
			{
				Rect icon = new(rect.x + 4, rect.y + 4, rect.width - 8, rect.height - 22);
				GUI.DrawTexture(icon, asset.Icon.texture, ScaleMode.ScaleToFit);
			}

			Rect label = new(rect.x + 2, rect.yMax - 18, rect.width - 4, 16);
			GUI.Label(label, asset.EnumName, style);
		}

		private static void ColorAsset(Rect rect, GameDataSO asset, bool selected)
		{
			Color color = ColorPalette.UnityDefault;

			if (asset.IsDeleted)
			{
				if (selected) color = ColorPalette.Purple;
				else color = ColorPalette.Red;
			}
			else if (!asset.IsValid())
			{
				if (selected) color = ColorPalette.Orange;
				else color = ColorPalette.Yellow;
			}
			else if (selected) color = ColorPalette.Blue_Light;

			EditorGUI.DrawRect(rect, color);
			GUI.Box(rect, GUIContent.none);
		}

		#endregion
	}
}
