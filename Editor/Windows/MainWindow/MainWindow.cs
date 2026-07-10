using Gytis0.SOManager.Editor.CodeGeneration;
using Gytis0.SOManager.Editor.Helpers;
using System;
using System.Collections.Generic;
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
			GetWindow<MainWindow>("SO Manager");
		}

		private void OnEnable()
		{
			LoadEditorPrefs();
			ResourcesHelper.LoadIcons();
			Cache();
			RestoreState();

			EditorEvents.OnAssetsChanged += EditorEvents_AssetsChanged;
			AssetController.OnDeleteChanged += AssetController_OnDeleteChanged;

			position = new Rect(position.x, position.y, WindowWidth, WindowHeight);
		}

		private void OnDisable()
		{
			SaveEditorPrefs();

			SavePendingChanges();

			Build();

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

		private void LoadEditorPrefs()
		{
			TypePanelRatio = EditorPrefs.GetFloat(TypeRatio_Key, TypePanelRatio_Default);
			AssetPanelRatio = EditorPrefs.GetFloat(AssetRatio_Key, AssetPanelRatio_Default);
			InspectorPanelRatio = EditorPrefs.GetFloat(InspectorRatio_Key, InspectorPanelRatio_Default);
			WindowHeight = EditorPrefs.GetFloat(WindowWidth_Key, WindowWidth_Default);
			WindowHeight = EditorPrefs.GetFloat(WindowHeight_Key, WindowHeight_Default);
		}

		private void OnGUI()
		{
			if (!ResourcesHelper.IsInit)
				ResourcesHelper.Init();

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

		private void Build()
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
	}
}