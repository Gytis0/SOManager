using Gytis0.SOManager.Editor.CodeGeneration;
using Gytis0.SOManager.Editor.Events;
using Gytis0.SOManager.Editor.Helpers;
using Gytis0.SOManager.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Gytis0.SOManager.Editor.Windows
{
	public partial class MainWindow : EditorWindow
	{
		private Type selectedType;

		private readonly List<GameDataSO> assetsToDisplay = new();
		private GameDataSO createdAsset;
		private readonly Dictionary<GameDataSO, GameDataSO> tempAssets = new();

		[SerializeField] private bool isCreatingAsset = false;
		[SerializeField] private bool isListView = true;
		[SerializeField] private string selectedTypeName;
		[SerializeField] private string selectedAssetGuid;

		[MenuItem("Tools/SOManager/Manager", priority = 0)]
		public static void Open()
		{
			LoadEditorPrefs();

			var window = GetWindow<MainWindow>("SO Manager");
			window.position = new Rect(window.position.x, window.position.y, WindowWidth, WindowHeight);
			window.minSize = new Vector2(MinTypeWidth + MinAssetWidth + MinInspectorWidth, 300);
		}

		private void OnEnable()
		{
			EnsureAssembly();
			LoadEditorPrefs();
			ResourcesHelper.LoadIcons();
			Cache();
			GenerateEnums();
			RestoreState();

			EditorEvents.OnAssetsChanged += EditorEvents_AssetsChanged;
			AssetController.OnDeleteChanged += AssetController_OnDeleteChanged;
		}

		private void OnDisable()
		{
			SaveEditorPrefs();

			SavePendingChanges();

			UpdateEnumsAndBuild();

			EditorEvents.OnAssetsChanged -= EditorEvents_AssetsChanged;
			AssetController.OnDeleteChanged -= AssetController_OnDeleteChanged;

			ResourcesHelper.Deinit();
		}

		private void SaveEditorPrefs()
		{
			EditorPrefs.SetFloat(TypeRatio_Key, TypePanelRatio);
			EditorPrefs.SetFloat(AssetRatio_Key, AssetPanelRatio);
			EditorPrefs.SetFloat(InspectorRatio_Key, InspectorPanelRatio);
			EditorPrefs.SetFloat(WindowWidth_Key, WindowWidth);
			EditorPrefs.SetFloat(WindowHeight_Key, WindowHeight);
		}

		private static void LoadEditorPrefs()
		{
			TypePanelRatio = EditorPrefs.GetFloat(TypeRatio_Key, TypePanelRatio_Default);
			AssetPanelRatio = EditorPrefs.GetFloat(AssetRatio_Key, AssetPanelRatio_Default);
			InspectorPanelRatio = EditorPrefs.GetFloat(InspectorRatio_Key, InspectorPanelRatio_Default);
			WindowHeight = EditorPrefs.GetFloat(WindowWidth_Key, WindowWidth_Default);
			WindowHeight = EditorPrefs.GetFloat(WindowHeight_Key, WindowHeight_Default);
		}

		private void OnGUI()
		{
			ResourcesHelper.EnsureInit();

			EditorGUILayout.BeginHorizontal();

			HandleKeyboard();

			CalculateWidths();

			DrawTypesPanel();

			DrawResizeHandle(0);

			DrawAssetsPanel();

			DrawResizeHandle(1);
			
			DrawInspectorPanel();

			EditorGUILayout.EndHorizontal();
		}

		private void UpdateEnumsAndBuild()
		{
			GenerateEnums();
			RegistryBuilder.Build(cachedSettings.ResourcesPath);
		}

		/// <summary>
		/// For each type in <see cref="cachedTypes"/>, generate an enum file with all enum values and refresh <see cref="AssetDatabase"/>.
		/// </summary>
		private void GenerateEnums()
		{
			foreach (Type type in cachedTypes)
				EnumGenerator.Generate(type, cachedSettings.EnumsFilesPath);

			AssetDatabase.Refresh();
		}

		private void EnsureAssembly()
		{
			string folder = Path.Combine(Application.dataPath, "SOManager/Enums");
			Directory.CreateDirectory(folder);

			string asmdefPath = Path.Combine(folder, "Gytis0.SOManager.Enums.asmdef");

			string json = string.Format(
			@"{{
			  ""name"": ""{0}"",
			  ""references"": [
				""{1}""
			  ]
			}}",
			"Gytis0.SOManager.Enums",
			"Gytis0.SOManager.Runtime");

			File.WriteAllText(asmdefPath, json);

			AssetDatabase.Refresh();
		}
	}
}