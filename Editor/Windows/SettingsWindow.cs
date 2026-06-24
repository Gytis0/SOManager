using UnityEditor;
using UnityEngine;

namespace SOManager.EditorTools
{
	public class GameDataSettingsWindow : EditorWindow
	{
		private SerializedObject serializedSettings;
		private GameDataSettings settings;

		[MenuItem("Tools/Game Data Manager/Settings")]
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

			EditorGUILayout.LabelField("SO Manager Settings", EditorStyles.boldLabel);

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(GameDataSettings.DataPath)));
			EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(GameDataSettings.GeneratedFilesPath)));
			EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(GameDataSettings.ResourcesPath)));

			EditorGUILayout.Space();

			if (GUILayout.Button("Save"))
			{
				serializedSettings.ApplyModifiedProperties();

				EditorUtility.SetDirty(settings);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}

			serializedSettings.ApplyModifiedProperties();
		}
	}
}