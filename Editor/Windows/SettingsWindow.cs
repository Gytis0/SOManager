using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SOManager.EditorTools
{
	public class GameDataSettingsWindow : EditorWindow
	{
		private SerializedObject serializedSettings;
		private GameDataSettings settings;
		private int buttonWidth = 150;

		[MenuItem("Tools/SOManager Settings")]
		public static void Open()
		{
			GetWindow<GameDataSettingsWindow>("SO Manager Settings");
		}

		private void OnEnable()
		{
			settings = GameDataSettingsProvider.Settings;

			if (settings != null)
				serializedSettings = new SerializedObject(settings);
		}

		private void OnGUI()
		{
			if (settings == null)
			{
				EditorGUILayout.HelpBox("GameDataSettings asset could not be loaded.", MessageType.Error);
				return;
			}

			serializedSettings.Update();

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(GameDataSettings.DataPath)));
			EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(GameDataSettings.GeneratedFilesPath)));
			EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(GameDataSettings.ResourcesPath)));

			EditorGUILayout.Space();

			if (GUILayout.Button("Save", GUILayout.Width(buttonWidth)))
			{
				serializedSettings.ApplyModifiedProperties();

				EditorUtility.SetDirty(settings);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}

			if (GUILayout.Button("Prune Soft Deleted", GUILayout.Width(buttonWidth)))
				DeleteAllSoftDeletedAssets();

			serializedSettings.ApplyModifiedProperties();
		}

		private void DeleteAllSoftDeletedAssets()
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
	}
}