using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SOManager.EditorTools
{
	public class MainWindow : EditorWindow
	{
		private Type selectedType;
		private Type previousType;

		private GameDataSO selectedAsset;
		private GameDataSO previousAsset;
		private GameDataSO tempAsset;

		private Editor inspector;

		#region Search
		private string typeSearch = "";
		private string assetSearch = "";
		private SearchField searchField_Types;
		private SearchField searchField_Asset;
		#endregion

		#region Scroll
		private Vector2 typeScroll;
		private Vector2 assetScroll;
		private Vector2 inspectorScroll;
		#endregion

		#region Resizing
		private float typeWidth = 220;
		private float assetWidth = 220;

		private const float MinTypeWidth = 150;
		private const float MinAssetWidth = 200;
		private const float MinInspectorWidth = 250;

		private int activeResizeHandle = -1;
		private float startWidth;
		private Vector2 startMouse;
		#endregion

		#region Cache
		private List<Type> cachedTypes;
		private Dictionary<Type, List<GameDataSO>> cachedAssets = new();
		private GameDataSettings cachedSettings;
		#endregion

		#region Filters
		[SerializeField] private bool showFilters;
		[SerializeField] private bool showDeleted = false;
		[SerializeField] private bool showNonDeleted = true;
		#endregion

		#region Icons
		private GUIContent icon_FilterAssets;
		private GUIContent icon_ResetFilters;
		private GUIContent icon_AddType;
		private GUIContent icon_AddAsset;
		private GUIContent icon_DeleteAsset;
		private GUIContent icon_RestoreAsset;
		private GUIContent icon_Save;
		private GUIContent icon_Refresh;
		private GUIContent icon_Build;
		private GUIContent icon_SavePlus;
		private GUIContent icon_Cancel;
		private GUIContent icon_Grid;
		private GUIContent icon_List;
		private Sprite sprite_DefaultItemIcon;
		#endregion

		private static GUIStyle headerStyle;
		private static GUIStyle normalStyle;
		private static GUIStyle newItemStyle;
		private Color DeletedAssetColor = Color.softRed;
		private readonly int button1x1size = 36;

		#region State
		[SerializeField] private bool isCreatingAsset = false;
		[SerializeField] private bool isListView = true;
		[SerializeField] private string selectedTypeName;
		[SerializeField] private string selectedAssetGuid;
		#endregion

		#region Events

		private void EditorEvents_AssetsChanged()
		{
			Cache();
		}

		#endregion

		#region GUI

		[MenuItem("Tools/SOManager")]
		public static void Open()
		{
			GetWindow<MainWindow>("SO Manager");
		}

		private void OnEnable()
		{
			InitializeStyles();
			LoadIcons();
			searchField_Types = new();
			searchField_Asset = new();
			Cache();

			RestoreState();

			EditorEvents.AssetsChanged += EditorEvents_AssetsChanged;
		}

		private void OnDisable()
		{
			Build();

			EditorEvents.AssetsChanged -= EditorEvents_AssetsChanged;
		}

		private void RestoreState()
		{
			var tempAssetGuid = selectedAssetGuid;
			SelectType(cachedTypes.FirstOrDefault(x => x.FullName == selectedTypeName));

			if (!string.IsNullOrEmpty(tempAssetGuid))
			{
				string path = AssetDatabase.GUIDToAssetPath(tempAssetGuid);
				SelectAsset(AssetDatabase.LoadAssetAtPath<GameDataSO>(path));
			}
		}

		/// <summary>
		/// Fetches and caches <see cref="cachedSettings"/>, <see cref="cachedAssets"/>, <see cref="cachedTypes"/>.
		/// </summary>
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
			GUILayout.BeginVertical(normalStyle, GUILayout.Width(typeWidth));

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
					SelectType(type);
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
			GUILayout.BeginVertical(normalStyle, GUILayout.Width(assetWidth));

			DrawAssetPanelHeader(selectedType != null);

			assetSearch = DrawSearchField(searchField_Asset, assetSearch, "Search...");

			DrawFilters();

			assetScroll = EditorGUILayout.BeginScrollView(assetScroll);

			if (selectedType != null)
			{
				List<GameDataSO> assets = new();
				if (showNonDeleted && !showDeleted)
					assets = cachedAssets[selectedType].Where(x => !x.IsDeleted).ToList();
				else if (showNonDeleted && showDeleted)
					assets = cachedAssets[selectedType].ToList();
				else if (!showNonDeleted && showDeleted)
					assets = cachedAssets[selectedType].Where(x => x.IsDeleted).ToList();

				foreach (GameDataSO asset in assets)
				{
					if (!string.IsNullOrWhiteSpace(assetSearch) && !asset.EnumName.Contains(assetSearch, StringComparison.OrdinalIgnoreCase))
						continue;

					bool selected = selectedAsset == asset;
					if (asset.IsDeleted)
					{
						if (EditorHelper.Toggle_Colored(selected, asset.EnumName, DeletedAssetColor, "Button"))
							SelectAsset(asset);
					}
					else
					{
						if (GUILayout.Toggle(selected, asset.EnumName, "Button"))
							SelectAsset(asset);
					}
				}
			}

			EditorGUILayout.EndScrollView();

			GUILayout.EndVertical();
		}

		/// <summary>
		/// Handles inspector drawing logic.
		/// </summary>
		private void DrawInspector()
		{
			GUILayout.BeginVertical(normalStyle);

			if (inspector != null)
			{
				DrawInspectorPanelHeader();

				if (inspector != null)
				{
					inspectorScroll = EditorGUILayout.BeginScrollView(inspectorScroll);
					inspector.OnInspectorGUI();
					EditorGUILayout.EndScrollView();
				}
			}

			GUILayout.EndVertical();
		}

		private void DrawFilters()
		{
			if (showFilters)
			{
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);

				showNonDeleted = GUILayout.Toggle(showNonDeleted, "Show Non-Deleted");
				showDeleted = GUILayout.Toggle(showDeleted, "Show Deleted");

				EditorGUILayout.EndVertical();
			}
		}

		private void DrawResizeHandle(int id, ref float width)
		{
			Rect rect = GUILayoutUtility.GetRect(8, position.height, GUILayout.Width(8));

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
			EditorGUILayout.BeginHorizontal(headerStyle);
			if (GUILayout.Button(icon_Build, GUILayout.Width(button1x1size), GUILayout.Height(button1x1size)))
				Build();

			if (GUILayout.Button(icon_Refresh, GUILayout.Width(button1x1size), GUILayout.Height(button1x1size)))
				Cache();

			GUILayout.FlexibleSpace();

			if (selectedType != null)
			{
				if (GUILayout.Button(icon_DeleteAsset, GUILayout.Width(button1x1size), GUILayout.Height(button1x1size)))
					DeleteSelectedType();
			}
			else
			{
				GUI.enabled = false;
				if (GUILayout.Button(icon_DeleteAsset, GUILayout.Width(button1x1size), GUILayout.Height(button1x1size))) ;
				GUI.enabled = true;
			}

			if (GUILayout.Button(icon_AddType, GUILayout.Width(button1x1size), GUILayout.Height(button1x1size)))
				CreateType();

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(1);
		}

		private void DrawAssetPanelHeader(bool isAddEnabled)
		{
			EditorGUILayout.BeginHorizontal(headerStyle);

			if (GUILayout.Button(icon_FilterAssets, GUILayout.Width(button1x1size), GUILayout.Height(button1x1size)))
				showFilters = !showFilters;

			if (GUILayout.Button(icon_ResetFilters, GUILayout.Width(button1x1size), GUILayout.Height(button1x1size)))
				ResetFilters();

			GUILayout.FlexibleSpace();

			if (isListView)
			{
				if (GUILayout.Button(icon_Grid, GUILayout.Width(button1x1size), GUILayout.Height(button1x1size)))
					isListView = false;
			}
			else
			{
				if (GUILayout.Button(icon_List, GUILayout.Width(button1x1size), GUILayout.Height(button1x1size)))
					isListView = true;
			}

				GUI.enabled = isAddEnabled;
			if (GUILayout.Button(icon_AddAsset, GUILayout.Width(button1x1size), GUILayout.Height(button1x1size)))
				CreateAsset();
			GUI.enabled = true;

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(1);
		}

		private void DrawInspectorPanelHeader()
		{
			if (isCreatingAsset)
				EditorGUILayout.BeginHorizontal(newItemStyle);
			else
				EditorGUILayout.BeginHorizontal(headerStyle);

			if (isCreatingAsset) ;
			else if (!selectedAsset.IsDeleted)
			{
				if (GUILayout.Button(icon_DeleteAsset, GUILayout.Width(button1x1size), GUILayout.Height(button1x1size)))
					DeleteSelectedAsset(true);
			}
			else
			{
				if (GUILayout.Button(icon_RestoreAsset, GUILayout.Width(button1x1size), GUILayout.Height(button1x1size)))
					DeleteSelectedAsset(false);
			}

			if (isCreatingAsset)
			{
				if (GUILayout.Button(icon_SavePlus, GUILayout.Width(button1x1size), GUILayout.Height(button1x1size)))
					SaveNewAsset();

				if (GUILayout.Button(icon_Cancel, GUILayout.Width(button1x1size), GUILayout.Height(button1x1size)))
					CancelCreateAsset();
			}
			else
			{
				if (GUILayout.Button(icon_Save, GUILayout.Width(button1x1size), GUILayout.Height(button1x1size)))
					SaveAssetChangesIfNeeded();
			}

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(1);
		}

		#endregion

		#region Assets

		/// <summary>
		/// Selects asset and updates inspector based on that asset.
		/// </summary>
		/// <param name="asset">Asset to select.</param>
		private void SelectAsset(GameDataSO asset)
		{
			if (asset == selectedAsset) return;

			if (isCreatingAsset)
			{
				Debug.LogWarning("Finish creating the new asset first.");
				return;
			}

			previousAsset = selectedAsset;
			selectedAsset = asset;
			selectedAssetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));

			SaveAssetChangesIfNeeded();

			GUI.FocusControl(null);

			if (inspector != null)
				DestroyImmediate(inspector);

			if (selectedAsset == null) return;

			tempAsset = CreateInstance(selectedType) as GameDataSO;
			EditorUtility.CopySerialized(selectedAsset, tempAsset);

			inspector = Editor.CreateEditor(tempAsset);
		}

		private void SaveAssetChangesIfNeeded()
		{
			if (!DoChangesExist(tempAsset, previousAsset)) return;

			if (string.IsNullOrWhiteSpace(tempAsset.Name))
			{
				Debug.LogError(string.Format("Discarding changes, because an item \"{0}\" was renamed to an empty name.", previousAsset.Name));
				return;
			}

			if (!tempAsset.IsValid())
			{
				Debug.LogError(string.Format("Discarding chagnes, because the item \"{0}\" was not valid.", tempAsset.Name));
				return;
			}

			EditorUtility.CopySerialized(tempAsset, previousAsset);
		}

		/// <summary>
		/// Creates asset
		/// </summary>
		private void CreateAsset()
		{
			SelectAsset(null);

			tempAsset = CreateInstance(selectedType) as GameDataSO;
			tempAsset.GenerateGuid();
			tempAsset.Icon = sprite_DefaultItemIcon;

			isCreatingAsset = true;
			inspector = Editor.CreateEditor(tempAsset);
		}

		private void DeleteSelectedAsset(bool delete)
		{
			selectedAsset.SetSoftDeleted(delete);
		}

		private static bool DoChangesExist(UnityEngine.Object a, UnityEngine.Object b)
		{
			if (a == null || b == null)
				return false;

			SerializedObject soA = new(a);
			SerializedObject soB = new(b);

			SerializedProperty propA = soA.GetIterator();
			SerializedProperty propB = soB.GetIterator();

			bool enterChildren = true;

			while (propA.NextVisible(enterChildren))
			{
				if (!propB.NextVisible(enterChildren))
					return true;

				if (propA.propertyPath != propB.propertyPath)
					return true;

				if (!SerializedProperty.DataEquals(propA, propB))
					return true;

				enterChildren = false;
			}

			return propB.NextVisible(false);
		}

		#endregion

		#region Types

		private void SelectType(Type type)
		{
			if (type == selectedType) return;

			if (isCreatingAsset)
			{
				Debug.LogWarning("Finish creating the new asset first.");
				return;
			}

			selectedTypeName = type.FullName;
			previousType = selectedType;
			selectedType = type;

			SelectAsset(null);
		}

		private void CreateType()
		{
			throw new NotImplementedException();
		}

		private void DeleteSelectedType()
		{
			// TODO : Confirmation. No soft deletion here
		}

		#endregion

		private static void InitializeStyles()
		{
			Texture2D textureHeader = new(1, 1);
			textureHeader.SetPixel(0, 0, new Color(0.18f, 0.18f, 0.18f));
			textureHeader.Apply();

			headerStyle = new GUIStyle
			{
				normal = { background = textureHeader },
			};

			Texture2D textureNormal = new(1, 1);
			textureNormal.SetPixel(0, 0, new Color(0.22f, 0.22f, 0.22f));
			textureNormal.Apply();

			normalStyle = new GUIStyle
			{
				normal = { background = textureNormal },
			};

			Texture2D textureNewItem = new(1, 1);
			textureNewItem.SetPixel(0, 0, new Color(0.82f, 0.32f, 0f));
			textureNewItem.Apply();

			newItemStyle = new GUIStyle
			{
				normal = { background = textureNewItem },
			};
		}

		private void LoadIcons()
		{
			icon_FilterAssets = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/FilterAssets.png") as Texture2D, "Filter");
			icon_ResetFilters = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/funnel-x.png") as Texture2D, "Reset Filters");
			icon_AddType = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/package-plus.png") as Texture2D, "Add New Category");
			icon_AddAsset = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/AddAsset.png") as Texture2D, "Add New Item");
			icon_DeleteAsset = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/trash.png") as Texture2D, "Delete Item");
			icon_RestoreAsset = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/RestoreAsset.png") as Texture2D, "Restore Item");
			icon_Save = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/Save.png") as Texture2D, "Save");
			icon_Refresh = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/rotate-ccw.png") as Texture2D, "Refresh");
			icon_Build = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/hammer.png") as Texture2D, "Build");
			icon_SavePlus = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/save-plus.png") as Texture2D, "Save");
			icon_Cancel = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/ban.png") as Texture2D, "Cancel");
			icon_Grid = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/layout-grid.png") as Texture2D, "Grid View");
			icon_List = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/list.png") as Texture2D, "List View");
			sprite_DefaultItemIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.gytis0.somanager/Editor/Icons/box.png");
		}

		private void Build()
		{
			GenerateEnums();
			RegistryBuilder.Build(cachedSettings.ResourcesPath);
		}

		private void SaveNewAsset()
		{
			tempAsset.SetEnumName(tempAsset.Name.ToPascalCase());

			if (string.IsNullOrWhiteSpace(tempAsset.Name))
			{
				Debug.LogError("Name cannot be empty.");
				return;
			}

			if (!tempAsset.IsValid())
			{
				Debug.LogError(string.Format("The asset '{0}' is not valid.", tempAsset.Name));
				return;
			}

			string folderPath = Path.Combine(cachedSettings.DataPath, selectedType.Name).Replace('\\', '/');

			cachedSettings.DataPath.EnsureFolder();
			folderPath.EnsureFolder();

			string assetPath = string.Format("{0}/{1}.asset", folderPath, tempAsset.Name);

			if (AssetDatabase.LoadAssetAtPath<GameDataSO>(assetPath) != null)
			{
				Debug.LogError(string.Format("Asset '{0}' already exists.", tempAsset.Name));
				return;
			}

			GameDataSO asset = CreateInstance(selectedType) as GameDataSO;

			EditorUtility.CopySerialized(tempAsset, asset);

			AssetDatabase.CreateAsset(asset, assetPath);
			AssetDatabase.SaveAssets();

			Cache();
			GenerateEnums();

			isCreatingAsset = false;

			DestroyImmediate(inspector);
			DestroyImmediate(tempAsset);

			tempAsset = null;

			SelectAsset(asset);
		}

		private void CancelCreateAsset()
		{
			isCreatingAsset = false;

			if (inspector != null)
				DestroyImmediate(inspector);

			if (tempAsset != null)
				DestroyImmediate(tempAsset);

			inspector = null;
			SelectAsset(null);
		}

		private void ResetFilters()
		{
			showDeleted = false;
			showNonDeleted = true;
		}

		/// <summary>
		/// For each type in <see cref="cachedTypes"/>, generate an enum file with all enum values and refresh <see cref="AssetDatabase"/>.
		/// </summary>
		private void GenerateEnums()
		{
			foreach (Type type in cachedTypes)
				EnumGenerator.Generate(type, cachedSettings.GeneratedFilesPath);

			AssetDatabase.Refresh();
		}
	}
}