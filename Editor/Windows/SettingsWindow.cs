using Gytis0.SOManager.Editor.Enums;
using Gytis0.SOManager.Editor.Events;
using Gytis0.SOManager.Editor.Extensions;
using Gytis0.SOManager.Editor.Helpers;
using Gytis0.SOManager.Editor.Settings;
using Gytis0.SOManager.Runtime;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Gytis0.SOManager.Editor.Windows
{
	public class GameDataSettingsWindow : EditorWindow
	{
		private const int buttonWidth = 38;
		private SerializedObject serializedSettings;
		private GameDataSettings settings;

		[MenuItem("Tools/SOManager/Settings", priority = 11)]
		public static void Open()
		{
			GetWindow<GameDataSettingsWindow>("SO Manager Settings");
		}

		private void OnEnable()
		{
			settings = GameDataSettingsProvider.Settings;

			if (settings != null)
				serializedSettings = new SerializedObject(settings);

			ResourcesHelper.LoadIcons();
		}

		private void OnGUI()
		{
			ResourcesHelper.EnsureInit();

			EditorGUILayout.BeginVertical();

			if (settings == null)
			{
				EditorGUILayout.HelpBox("GameDataSettings asset could not be loaded.", MessageType.Error);
				return;
			}

			DrawHeader();

			serializedSettings.Update();

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(GameDataSettings.DataPath)));
			EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(GameDataSettings.EnumsFilesPath)));
			EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(GameDataSettings.ResourcesPath)));

			EditorGUILayout.Space();

			serializedSettings.ApplyModifiedProperties();

			EditorGUILayout.EndVertical();
		}

		private void DrawHeader()
		{
			EditorGUILayout.BeginHorizontal(ResourcesHelper.style_Header, GUILayout.Height(buttonWidth));

			if (ButtonHelper.DrawPanelButton(buttonWidth, ResourcesHelper.icon_Save, ResourcesHelper.style_Panel_Button))
			{
				serializedSettings.ApplyModifiedProperties();

				EditorUtility.SetDirty(settings);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}

			if (ButtonHelper.DrawPanelButton(buttonWidth, ResourcesHelper.icon_Prune, ResourcesHelper.style_Panel_Button))
				StartSoftDeletion();

			if (ButtonHelper.DrawPanelButton(buttonWidth, ResourcesHelper.icon_Migrate, ResourcesHelper.style_Panel_Button))
				StartGenericMigration();

			EditorGUILayout.EndHorizontal();
		}

		private void StartSoftDeletion()
		{
			List<GameDataSO> allAssets = new();

			foreach (Type type in TypeHelper.GetGameDataTypes())
				allAssets.AddRange(type.GetAssets(IncludeDeleted.Deleted));

			if (allAssets.Count > 0)
			{
				if (!EditorUtility.DisplayDialog("Delete Assets",
					string.Format("Permanently delete {0} soft deleted assets?", allAssets.Count),
					"Delete",
					"Cancel"))
					return;
			}
			else
			{
				EditorUtility.DisplayDialog("No Deleted Assets", "No soft deleted assets found.", "Ok");
				return;
			}

			int deletedCount = 0;

			foreach (var asset in allAssets)
			{
				string path = AssetDatabase.GetAssetPath(asset);

				if (AssetDatabase.DeleteAsset(path))
					deletedCount++;
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			EditorEvents.RaiseAssetsChanged();

			Debug.Log(string.Format("Deleted {0} soft deleted assets.", deletedCount));
		}

		private void StartGenericMigration()
		{
			if (!EditorUtility.DisplayDialog("Migrate to generics",
				"PLEASE, BACKUP YOUR PROJECT BEFORE PROCEEDING.\n\nThis will search for all classes that inherit the non-generic GameDataSO and it will convert them into generics.\n\nThe generic version provides more methods to work with and adds more compile-time safer methods.",
				"I've made a backup of my project. Proceed",
				"Cancel"))
				return;

			var results = SettingsHelper.ConvertNonGenericGameDataTypesToGeneric();

			StringBuilder sb = new();
			sb.AppendLine(string.Format("The migration converted {0} files:", results.converted));
			foreach(var fileName in results.convertedFiles)
				sb.AppendLine(fileName);
			
			sb.AppendLine();
			sb.AppendLine(string.Format("The migration skipped {0} files:", results.skipped));
			foreach (var fileName in results.skippedFiles)
				sb.AppendLine(fileName);

			EditorUtility.DisplayDialog("Migrate to generics", sb.ToString(), "Ok");
		}
	}
}