using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SOManager.EditorTools
{
	public class MainWindow : EditorWindow
	{
		private Type selectedType;
		private Type oldType;
		private GameDataSO selectedAsset;
		private GameDataSO oldAsset;

		private SearchField searchField_Types;
		private SearchField searchField_Asset;
		private Editor inspector;

		private Vector2 typeScroll;
		private Vector2 assetScroll;
		private Vector2 inspectorScroll;

		private string typeSearch = "";
		private string assetSearch = "";

		private float typeWidth = 220;
		private float assetWidth = 300;

		private const float MinTypeWidth = 150;
		private const float MinAssetWidth = 200;
		private const float MinInspectorWidth = 250;

		private int activeResizeHandle = -1;
		private float startWidth;
		private Vector2 startMouse;

		private List<Type> cachedTypes;
		private Dictionary<Type, List<GameDataSO>> cachedAssets = new();
		private GameDataSettings cachedSettings;

		private bool showFilters;
		private bool showDeleted = true;

		#region GUI

		[MenuItem("Tools/Game Data Manager")]
		public static void Open()
		{
			GetWindow<MainWindow>("Game Data");
		}

		private void OnEnable()
		{
			searchField_Types = new();
			searchField_Asset = new();
			Cache();
		}

		private void OnDisable()
		{
			Save();
		}

		private void Cache()
		{
			cachedSettings = GameDataSettingsProvider.Settings;

			cachedAssets.Clear();

			cachedTypes = TypeHelper.GetGameDataTypes();
			foreach (Type type in cachedTypes)
				cachedAssets[type] = type.GetAssets(IncludeDeleted.Both);
		}

		private void OnGUI()
		{
			EditorGUILayout.Space(2);

			EditorGUILayout.BeginHorizontal();

			DrawTypes();

			DrawResizeHandle(0, ref typeWidth);

			DrawAssets();

			DrawResizeHandle(1, ref assetWidth);

			DrawInspector();

			EditorGUILayout.EndHorizontal();
		}

		/// <summary>
		/// Draws all found types that inherit from <see cref="GameDataSO"/>.
		/// </summary>
		private void DrawTypes()
		{
			GUILayout.BeginVertical(GUILayout.Width(typeWidth));

			DrawTypePanelHeader();

			typeSearch = DrawSearchField(searchField_Types, typeSearch, "Search...");

			typeScroll = EditorGUILayout.BeginScrollView(typeScroll);

			foreach (Type type in cachedTypes)
			{
				if (!string.IsNullOrWhiteSpace(typeSearch) && !type.Name.Contains(typeSearch, StringComparison.OrdinalIgnoreCase))
					continue;

				bool selected = selectedType == type;

				if (GUILayout.Toggle(selected, type.Name, "Button"))
				{
					oldType = selectedType;
					selectedType = type;
					if (oldType != selectedType)
						selectedAsset = null;
				}
			}

			EditorGUILayout.EndScrollView();

			GUILayout.EndVertical();
		}

		/// <summary>
		/// Draws all assets from the <see cref="selectedType"/>.
		/// </summary>
		private void DrawAssets()
		{
			GUILayout.BeginVertical(GUILayout.Width(assetWidth));

			DrawAssetPanelHeader(selectedType != null);

			assetSearch = DrawSearchField(searchField_Asset, assetSearch, "Search...");

			DrawFilters();

			assetScroll = EditorGUILayout.BeginScrollView(assetScroll);

			if (selectedType != null)
			{
				List<GameDataSO> assets;
				if (showDeleted)
					assets = cachedAssets[selectedType].Where(x => x.IsDeleted).ToList();
				else
					assets = cachedAssets[selectedType].Where(x => !x.IsDeleted).ToList();

				foreach (GameDataSO asset in assets)
				{
					if (!string.IsNullOrWhiteSpace(assetSearch) && !asset.EnumName.Contains(assetSearch, StringComparison.OrdinalIgnoreCase))
						continue;

					bool selected = selectedAsset == asset;

					if (GUILayout.Toggle(selected, asset.EnumName, "Button"))
						SelectAsset(asset);
				}
			}

			EditorGUILayout.EndScrollView();

			GUILayout.EndVertical();
		}

		private void DrawFilters()
		{
			if (showFilters)
			{
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);

				showDeleted = GUILayout.Toggle(showDeleted, "Show Deleted");

				EditorGUILayout.EndVertical();
			}
		}

		/// <summary>
		/// Handles inspector drawing logic.
		/// </summary>
		private void DrawInspector()
		{
			GUILayout.BeginVertical();

			if (selectedAsset != null)
			{
				inspectorScroll = EditorGUILayout.BeginScrollView(inspectorScroll);

				inspector?.OnInspectorGUI();

				EditorGUILayout.EndScrollView();
			}

			GUILayout.EndVertical();
		}

		private void DrawResizeHandle(int id, ref float width)
		{
			Rect rect = GUILayoutUtility.GetRect(4, position.height, GUILayout.Width(4));

			EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);

			Event e = Event.current;

			if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
			{
				activeResizeHandle = id;
				startWidth = width;
				startMouse = e.mousePosition;
				e.Use();
			}

			if (activeResizeHandle == id)
			{
				if (e.type == EventType.MouseDrag)
				{
					float delta = e.mousePosition.x - startMouse.x;
					float newWidth = startWidth + delta;

					float maxWidth;

					if (id == 0)
					{
						maxWidth = position.width - assetWidth - MinInspectorWidth - 8;
						width = Mathf.Clamp(newWidth, MinTypeWidth, maxWidth);
					}
					else
					{
						maxWidth = position.width - typeWidth - MinInspectorWidth - 8;
						width = Mathf.Clamp(newWidth, MinAssetWidth, maxWidth);
					}

					Repaint();
					e.Use();
				}

				if (e.type == EventType.MouseUp)
				{
					activeResizeHandle = -1;
					e.Use();
				}
			}

			EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f));
		}

		private static string DrawSearchField(SearchField field, string value, string hint)
		{
			Rect rect = GUILayoutUtility.GetRect(1, 20, GUILayout.ExpandWidth(true));

			value = field.OnGUI(rect, value);

			if (string.IsNullOrEmpty(value) && GUI.GetNameOfFocusedControl() != hint)
			{
				Color old = GUI.color;
				GUI.color = Color.gray;

				Rect hintRect = rect;
				hintRect.x += 18;
				hintRect.yMin += -3;

				GUI.Label(hintRect, hint);

				GUI.color = old;
			}

			return value;
		}

		private void DrawTypePanelHeader()
		{
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(EditorGUIUtility.IconContent("d_SaveAs"), GUILayout.Width(28)))
				Save();

			GUILayout.FlexibleSpace();

			if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"), GUILayout.Width(28)))
				CreateType();

			EditorGUILayout.EndHorizontal();
		}

		private void DrawAssetPanelHeader(bool isAddEnabled)
		{
			EditorGUILayout.BeginHorizontal();

			if (selectedAsset != null)
			{
				if (!selectedAsset.IsDeleted)
				{
					if (GUILayout.Button(EditorGUIUtility.IconContent("P4_DeletedLocal"), GUILayout.Width(28)))
						selectedAsset.SetSoftDeleted(true);
				}
				else
				{
					if (GUILayout.Button(EditorGUIUtility.IconContent("P4_CheckOutRemote"), GUILayout.Width(28)))
						selectedAsset.SetSoftDeleted(false);
				}
			}

			GUILayout.FlexibleSpace();

			if (GUILayout.Button(EditorGUIUtility.IconContent("d_Preset.Context"), GUILayout.Width(28)))
				showFilters = !showFilters;

			var old = GUI.enabled;
			GUI.enabled = isAddEnabled;

			if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"), GUILayout.Width(28)))
				CreateAsset();

			GUI.enabled = old;

			EditorGUILayout.EndHorizontal();
		}

		#endregion

		/// <summary>
		/// Selects asset and updates inspector based on that asset.
		/// </summary>
		/// <param name="asset">Asset to select.</param>
		private void SelectAsset(GameDataSO asset)
		{
			oldAsset = selectedAsset;
			selectedAsset = asset;

			if (oldAsset == selectedAsset)
				return;

			GUI.FocusControl(null);

			if (inspector != null)
				DestroyImmediate(inspector);

			if (selectedAsset != null)
				inspector = Editor.CreateEditor(selectedAsset);
		}

		/// <summary>
		/// Creates asset
		/// </summary>
		private void CreateAsset()
		{
			InputWindow.Show(string.Format("Create new {0}", selectedType.Name), string.Format("New {0}", selectedType.Name), OnConfirm);
		}

		private void CreateType()
		{
			throw new NotImplementedException();
		}

		private void OnConfirm(string name)
		{
			name = name.ToPascalCase();
			string folderPath = Path.Combine(cachedSettings.DataPath, selectedType.Name).Replace('\\', '/');

			cachedSettings.DataPath.EnsureFolder();
			folderPath.EnsureFolder();

			string assetPath = string.Format("{0}/{1}.asset", folderPath, name);

			if (AssetDatabase.LoadAssetAtPath<GameDataSO>(assetPath) != null)
			{
				EditorUtility.DisplayDialog("Error", string.Format("Asset '{0}' already exists.", name), "OK");
				return;
			}

			GameDataSO asset = CreateInstance(selectedType) as GameDataSO;

			asset.GenerateGuid();
			asset.SetEnumName(name);

			AssetDatabase.CreateAsset(asset, assetPath);
			AssetDatabase.SaveAssets();

			Cache();
			GenerateEnums();

			SelectAsset(asset);
		}

		private void Save()
		{
			Cache();
			GenerateEnums();
			RegistryBuilder.Build(cachedSettings.ResourcesPath);
		}

		private void GenerateEnums()
		{
			foreach (Type type in cachedTypes)
				EnumGenerator.Generate(type, cachedSettings.GeneratedFilesPath);

			AssetDatabase.Refresh();
		}
	}
}