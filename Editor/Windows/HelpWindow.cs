using Gytis0.SOManager.Editor.Helpers;
using UnityEditor;
using UnityEngine;

namespace Gytis0.SOManager.Editor.Windows
{
	public class HelpWindow : EditorWindow
	{
		private bool isInit = false;
		private Vector2 scroll;

		private static  GUIStyle wrapStyle;

		[MenuItem("Tools/SOManager/Help", priority = 12)]
		public static void Open()
		{
			GetWindow<HelpWindow>("SOManager Help");
		}

		private void OnGUI()
		{
			if (!isInit)
			{
				isInit = true;
				wrapStyle = new GUIStyle(EditorStyles.label)
				{
					wordWrap = true
				};
			}

			scroll = EditorGUILayout.BeginScrollView(scroll);

			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			DrawFeaturesHelp();
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			DrawButtonHelp();
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			DrawShortcuts();
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			EditorGUILayout.EndScrollView();
		}

		private void DrawShortcuts()
		{
			DrawShortcut("Arrows", "Iterate through items");
			DrawShortcut("CTRL + Arrows", "Change panel focus to the right or left");
			DrawShortcut("Del", "Delete / Restore selected assets");
			DrawShortcut("End", "Scroll and select last item");
			DrawShortcut("Home", "Scroll and select first item");
			DrawShortcut("CTRL + Mouse", "Select multiple items");
			DrawShortcut("SHIFT + Mouse", "Select multiple items");
			DrawShortcut("SHIFT + Arrows", "Select multiple items");
		}

		private void DrawButtonHelp()
		{
			DrawHelpIcon(ResourcesHelper.icon_Build, "Builds registry, making the data ready to use in scripts.");
			DrawHelpIcon(ResourcesHelper.icon_Refresh, "Refreshes the categories list.");
			DrawHelpIcon(ResourcesHelper.icon_ActiveItems_On, "Filters, whether to show active items or not.");
			DrawHelpIcon(ResourcesHelper.icon_DeletedItems_On, "Filters, whether to show deleted items or not.");
			DrawHelpIcon(ResourcesHelper.icon_List, "Change to list view.");
			DrawHelpIcon(ResourcesHelper.icon_Grid, "Change to grid view.");
			DrawHelpIcon(ResourcesHelper.icon_AddAsset, "Start the creation of a new item.");
			DrawHelpIcon(ResourcesHelper.icon_Delete, "Soft-deletes active selected items.");
			DrawHelpIcon(ResourcesHelper.icon_RestoreAsset, "Restores selected soft-deleted items.");
			DrawHelpIcon(ResourcesHelper.icon_Save, "Saves selected item.");
			DrawHelpIcon(ResourcesHelper.icon_SavePlus, "Creates new item.");
			DrawHelpIcon(ResourcesHelper.icon_Cancel, "Cancels the creation of a new item.");
		}

		private void DrawFeaturesHelp()
		{
			DrawBullet("You can select multiple items for bulk editing.");
			DrawBullet("Soft-deleted items are not available in scripts and can be restored later.");
			DrawBullet("If the SO implements GameDataSO.IsValid() method, validity checks will be run on item create or update.");
			DrawBullet("SOs are saved automatically immediately after a successful validation.");
			DrawBullet("Registry is built either on SOManager close or \"Build\" button click.");
			DrawBullet("GameData is initiliazed before first scene load using the Registry.");
		}

		private static void DrawBullet(string text)
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("•", GUILayout.Width(12));
			GUILayout.Label(text, wrapStyle, GUILayout.ExpandWidth(true));
			EditorGUILayout.EndHorizontal();
		}

		private static void DrawShortcut(string shortcut, string description)
		{
			EditorGUILayout.BeginHorizontal();

			GUILayout.Label(shortcut, EditorStyles.boldLabel, GUILayout.Width(120));
			GUILayout.Label(description, wrapStyle);

			EditorGUILayout.EndHorizontal();
		}

		private static void DrawHelpIcon(GUIContent icon, string description)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Height(24));

			GUILayout.Label(icon.image, GUILayout.Width(20), GUILayout.Height(20));
			GUILayout.Space(6);
			GUILayout.Label(description, wrapStyle);

			EditorGUILayout.EndHorizontal();
		}
	}
}