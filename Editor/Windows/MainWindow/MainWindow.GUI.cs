using Gytis0.SOManager.Editor.Helpers;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

#pragma warning disable CS0642

namespace Gytis0.SOManager.Editor.Windows
{
	public partial class MainWindow : EditorWindow
	{
		private const int panelButtonSize = 38;
		private float gridItemSize = 80;
		private int gridColumns = -1;

		private string typeSearch = "";
		private string assetSearch = "";
		private SearchField searchField_Types;
		private SearchField searchField_Asset;

		private Vector2 typeScroll;
		private Vector2 assetScroll;
		private Vector2 inspectorScroll;

		private bool isAutoScrollNeeded_Asset = false;
		private bool isAutoScrollNeeded_Type = false;

		private float typeWidth;
		private float assetWidth;

		private const float MinTypeWidth = (panelButtonSize + 4f) * 3;
		private const float MinAssetWidth = (panelButtonSize + 4f) * 6;
		private const float MinInspectorWidth = 200f;

		private const float TypePanelRatio_Default = 0.20f;
		private const float AssetPanelRatio_Default = 0.40f;
		private const float InspectorPanelRatio_Default = 0.40f;
		private const float WindowWidth_Default = 900f;
		private const float WindowHeight_Default = 600f;

		[SerializeField] private float TypePanelRatio;
		[SerializeField] private float AssetPanelRatio;
		[SerializeField] private float InspectorPanelRatio;
		[SerializeField] private float WindowWidth;
		[SerializeField] private float WindowHeight;

		private const string TypeRatio_Key = "Gytis0.SOManager.TypePanelRatio";
		private const string AssetRatio_Key = "Gytis0.SOManager.AssetPanelRatio";
		private const string InspectorRatio_Key = "Gytis0.SOManager.InspectorPanelRatio";
		private const string WindowWidth_Key = "Gytis0.SOManager.WindowWidth";
		private const string WindowHeight_Key = "Gytis0.SOManager.WindowHeight";

		private const float HandleWidth = 8f;
		private int activeResizeHandle = -1;
		private float startWidth;
		private Vector2 startMouse;
		private const float sliderOffset = panelButtonSize / 2 - 9f;

		/// <summary>
		/// Draws all found types that inherit from <see cref="GameDataSO"/>.
		/// </summary>
		private void DrawTypesPanel()
		{
			if (isKeyboardActive && panelIndex == 0)
				GUILayout.BeginVertical(ResourcesHelper.style_Panel_Selected, GUILayout.Width(typeWidth));
			else
				GUILayout.BeginVertical(ResourcesHelper.style_Panel_NotSelected, GUILayout.Width(typeWidth));

			DrawTypePanelHeader();

			typeSearch = DrawSearchField(searchField_Types, typeSearch);

			typeScroll = EditorGUILayout.BeginScrollView(typeScroll);

			for (int i = 0; i < cachedTypes.Count; i++)
			{
				if (!string.IsNullOrWhiteSpace(typeSearch) && !cachedTypes[i].Name.Contains(typeSearch, StringComparison.OrdinalIgnoreCase))
					continue;

				bool selected = selectedType == cachedTypes[i];

				if (ButtonHelper.ListType(cachedTypes[i], cachedAssets[cachedTypes[i]].Count(x => !x.IsDeleted), selected, ResourcesHelper.style_Type_NameLabel, ResourcesHelper.style_Type_CountLabel))
				{
					selectedTypeIndex = i;
					SetPanelIndex(0);
					SelectType(cachedTypes[i]);
				}
			}

			EditorGUILayout.EndScrollView();

			GUILayout.EndVertical();

			if (isAutoScrollNeeded_Type)
				AutoScroll_Types();

			HandlePanelIndex(0);
		}

		/// <summary>
		/// Draws all assets from the <see cref="selectedType"/>.
		/// </summary>
		private void DrawAssetsPanel()
		{
			if (isKeyboardActive && panelIndex == 1)
				GUILayout.BeginVertical(ResourcesHelper.style_Panel_Selected, GUILayout.Width(assetWidth));
			else
				GUILayout.BeginVertical(ResourcesHelper.style_Panel_NotSelected, GUILayout.Width(assetWidth));

			DrawAssetPanelHeader();

			assetSearch = DrawSearchField(searchField_Asset, assetSearch);

			if (isListView)
				DrawListView();
			else
				DrawGridView();

			GUILayout.EndVertical();

			HandlePanelIndex(1);
		}

		/// <summary>
		/// Handles inspector drawing logic.
		/// </summary>
		private void DrawInspectorPanel()
		{
			Vector2 mousePos = default;
			bool isClicked;
			if (Event.current.type == EventType.MouseDown)
			{
				mousePos = Event.current.mousePosition;
				isClicked = true;
			}
			else
				isClicked = false;

			if (isKeyboardActive && panelIndex == 2)
				GUILayout.BeginVertical(ResourcesHelper.style_Panel_Selected);
			else
				GUILayout.BeginVertical(ResourcesHelper.style_Panel_NotSelected);

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
			if (isClicked && GUILayoutUtility.GetLastRect().Contains(mousePos))
				SetPanelIndex(2);
		}

		private void DrawGridView()
		{
			assetScroll = EditorGUILayout.BeginScrollView(assetScroll);

			float spacing = 4;

			gridColumns = Mathf.Max(1, Mathf.FloorToInt((assetWidth - spacing) / (gridItemSize + spacing)));

			int index = 0;

			while (index < assetsToDisplay.Count)
			{
				EditorGUILayout.BeginHorizontal();

				for (int i = 0; i < gridColumns && index < assetsToDisplay.Count; i++, index++)
				{
					GameDataSO asset = assetsToDisplay[index];

					if (!string.IsNullOrWhiteSpace(assetSearch) && !asset.EnumName.Contains(assetSearch, StringComparison.OrdinalIgnoreCase))
						continue;

					if (ButtonHelper.GridAsset(asset, selectedAssets.Contains(asset), gridItemSize, ResourcesHelper.style_Asset_GridLabel))
					{
						SetPanelIndex(1);
						SelectAsset(asset, index);
					}
				}

				GUILayout.FlexibleSpace();

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndScrollView();

			if (isAutoScrollNeeded_Asset)
				AutoScroll_Assets();
		}

		private void DrawListView()
		{
			assetScroll = EditorGUILayout.BeginScrollView(assetScroll);

			for (int i = 0; i < assetsToDisplay.Count; i++)
			{
				GameDataSO asset = assetsToDisplay[i];
				if (!string.IsNullOrWhiteSpace(assetSearch) &&
					!asset.EnumName.Contains(assetSearch, StringComparison.OrdinalIgnoreCase))
					continue;

				if (ButtonHelper.ListAsset(asset, selectedAssets.Contains(asset), ResourcesHelper.style_Asset_ListLabel))
				{
					SetPanelIndex(1);
					SelectAsset(asset, i);
				}
			}

			EditorGUILayout.EndScrollView();

			if (isAutoScrollNeeded_Asset)
				AutoScroll_Assets();
		}

		private void DrawResizeHandle(int id)
		{
			Rect rect = GUILayoutUtility.GetRect(HandleWidth, position.height, GUILayout.Width(HandleWidth));

			EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);

			Event e = Event.current;

			if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
			{
				activeResizeHandle = id;
				startMouse = e.mousePosition;

				if (id == 0)
					startWidth = TypePanelRatio;
				else
					startWidth = AssetPanelRatio;

				e.Use();
			}

			if (activeResizeHandle == id)
			{
				if (e.type == EventType.MouseDrag)
				{
					float availableWidth = position.width - HandleWidth * 2;
					float deltaRatio = (e.mousePosition.x - startMouse.x) / availableWidth;

					if (id == 0)
					{
						float newTypeRatio = startWidth + deltaRatio;

						float minTypeRatio = MinTypeWidth / availableWidth;
						float maxTypeRatio = 1f - InspectorPanelRatio - (MinAssetWidth / availableWidth);

						TypePanelRatio = Mathf.Clamp(newTypeRatio, minTypeRatio, maxTypeRatio);
						AssetPanelRatio = 1f - InspectorPanelRatio - TypePanelRatio;
					}
					else
					{
						float newAssetRatio = startWidth + deltaRatio;

						float minAssetRatio = MinAssetWidth / availableWidth;
						float maxAssetRatio = 1f - TypePanelRatio - (MinInspectorWidth / availableWidth);

						AssetPanelRatio = Mathf.Clamp(newAssetRatio, minAssetRatio, maxAssetRatio);
						InspectorPanelRatio = 1f - TypePanelRatio - AssetPanelRatio;
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

		private static string DrawSearchField(SearchField field, string value)
		{
			string hint = "Search...";

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

		private void AutoScroll_Assets()
		{
			isAutoScrollNeeded_Asset = false;
			float itemTop, itemBottom;

			if (isListView)
			{
				itemTop = selectedAssetIndex_Last * 24f;
				itemBottom = itemTop + 24f;
			}
			else
			{
				int row = selectedAssetIndex_Last / gridColumns;

				itemTop = row * gridItemSize;
				itemBottom = itemTop + gridItemSize;
			}

			float viewHeight = position.height - panelButtonSize - 20f - 9f; // search field height = 20f
			float viewTop = assetScroll.y;
			float viewBottom = viewTop + viewHeight;

			if (itemTop < viewTop)
				assetScroll.y = itemTop;
			else if (itemBottom > viewBottom)
				assetScroll.y = itemBottom - viewHeight;
		}

		private void AutoScroll_Types()
		{
			isAutoScrollNeeded_Type = false;

			float itemTop = selectedTypeIndex * 24f;
			float itemBottom = itemTop + 24f;

			float viewHeight = position.height - panelButtonSize - 20f - 9f; // search field height = 20f
			float viewTop = typeScroll.y;
			float viewBottom = viewTop + viewHeight;

			if (itemTop < viewTop)
				typeScroll.y = itemTop;
			else if (itemBottom > viewBottom)
				typeScroll.y = itemBottom - viewHeight;
		}

		private void CalculateWidths()
		{
			typeWidth = (position.width - HandleWidth * 2) * TypePanelRatio;
			assetWidth = (position.width - HandleWidth * 2) * AssetPanelRatio;
			//float inspectorWidth = (position.width - HandleWidth * 2) * InspectorPanelRatio;
		}

		#region Panels

		private void DrawTypePanelHeader()
		{
			EditorGUILayout.BeginHorizontal(ResourcesHelper.style_Header, GUILayout.Height(panelButtonSize));

			if (ButtonHelper.DrawPanelButton(panelButtonSize, ResourcesHelper.icon_Build, ResourcesHelper.style_Panel_Button, isKeyboardActive && panelIndex == 0 ? "1" : ""))
			{
				SetPanelIndex(0);
				ButtonAction_1();
			}

			if (ButtonHelper.DrawPanelButton(panelButtonSize, ResourcesHelper.icon_Refresh, ResourcesHelper.style_Panel_Button, isKeyboardActive && panelIndex == 0 ? "2" : ""))
			{
				SetPanelIndex(0);
				ButtonAction_2();
			}

			GUILayout.FlexibleSpace();

			if (ButtonHelper.DrawPanelButton(panelButtonSize, ResourcesHelper.icon_Help, ResourcesHelper.style_Panel_Button, isKeyboardActive && panelIndex == 0 ? "3" : ""))
			{
				SetPanelIndex(0);
				ButtonAction_3();
			}

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(1);
		}

		private void DrawAssetPanelHeader()
		{
			EditorGUILayout.BeginHorizontal(ResourcesHelper.style_Header, GUILayout.Height(panelButtonSize));

			if (ButtonHelper.DrawPanelButton(panelButtonSize, filters.ShowActive ? ResourcesHelper.icon_ActiveItems_On : ResourcesHelper.icon_ActiveItems_Off, ResourcesHelper.style_Panel_Button, isKeyboardActive && panelIndex == 1 ? "1" : ""))
			{
				SetPanelIndex(1);
				ButtonAction_1();
			}

			if (ButtonHelper.DrawPanelButton(panelButtonSize, filters.ShowDeleted ? ResourcesHelper.icon_DeletedItems_On : ResourcesHelper.icon_DeletedItems_Off, ResourcesHelper.style_Panel_Button, isKeyboardActive && panelIndex == 1 ? "2" : ""))
			{
				SetPanelIndex(1);
				ButtonAction_2();
			}

			GUILayout.FlexibleSpace();

			GUILayout.Label(GUIContent.none, GUILayout.Width(panelButtonSize * 2), GUILayout.Height(panelButtonSize));
			Rect sliderRect = GUILayoutUtility.GetLastRect();
			sliderRect.y += sliderOffset;

			gridItemSize = GUI.HorizontalSlider(sliderRect, gridItemSize, 40, 200);

			if (ButtonHelper.DrawPanelButton(panelButtonSize, isListView ? ResourcesHelper.icon_Grid : ResourcesHelper.icon_List, ResourcesHelper.style_Panel_Button, isKeyboardActive && panelIndex == 1 ? "3" : ""))
			{
				SetPanelIndex(1);
				ButtonAction_3();
			}

			GUI.enabled = selectedType != null;
			if (ButtonHelper.DrawPanelButton(panelButtonSize, ResourcesHelper.icon_AddAsset, ResourcesHelper.style_Panel_Button, isKeyboardActive && panelIndex == 1 ? "4" : ""))
			{
				SetPanelIndex(1);
				ButtonAction_4();
			}

			GUI.enabled = true;

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(1);
		}

		private void DrawInspectorPanelHeader()
		{
			if (isCreatingAsset)
				EditorGUILayout.BeginHorizontal(ResourcesHelper.style_Header_NewItem, GUILayout.Height(panelButtonSize));
			else
				EditorGUILayout.BeginHorizontal(ResourcesHelper.style_Header, GUILayout.Height(panelButtonSize));

			if (isCreatingAsset) ;
			else if (selectedAssets.Count == 1)
			{
				if (!lastSelectedAsset.IsDeleted)
				{
					if (GUILayout.Button(ResourcesHelper.icon_Delete, GUILayout.Width(panelButtonSize), GUILayout.Height(panelButtonSize)))
						AssetController.DeleteAsset(lastSelectedAsset);
				}
				else
				{
					if (GUILayout.Button(ResourcesHelper.icon_RestoreAsset, GUILayout.Width(panelButtonSize), GUILayout.Height(panelButtonSize)))
						AssetController.RestoreAsset(lastSelectedAsset, cachedAssets[selectedType]);
				}
			}
			else if (selectedAssets.Count > 1)
			{
				if (GUILayout.Button(ResourcesHelper.icon_Delete, GUILayout.Width(panelButtonSize), GUILayout.Height(panelButtonSize)))
					AssetController.DeleteAssets(selectedAssets);
				if (GUILayout.Button(ResourcesHelper.icon_RestoreAsset, GUILayout.Width(panelButtonSize), GUILayout.Height(panelButtonSize)))
					AssetController.RestoreAssets(selectedAssets, cachedAssets[selectedType]);
			}

			if (isCreatingAsset)
			{
				if (GUILayout.Button(ResourcesHelper.icon_SavePlus, GUILayout.Width(panelButtonSize), GUILayout.Height(panelButtonSize)))
				{
					var newAsset = AssetController.CreateNewAsset(selectedType, createdAsset, cachedAssets[selectedType], cachedSettings);
					if (newAsset != null)
					{
						Cache();
						GenerateEnums();

						isCreatingAsset = false;

						DestroyImmediate(inspector);
						DestroyImmediate(createdAsset, true);

						createdAsset = null;

						SelectAsset(newAsset);
					}
				}

				if (GUILayout.Button(ResourcesHelper.icon_Cancel, GUILayout.Width(panelButtonSize), GUILayout.Height(panelButtonSize)))
					CancelAssetCreation();
			}
			else
			{
				if (GUILayout.Button(ResourcesHelper.icon_Save, GUILayout.Width(panelButtonSize), GUILayout.Height(panelButtonSize)))
					SavePendingChanges();
			}

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(1);
		}

		#endregion

		#region Asset Creation

		private void StartAssetCreation()
		{
			SelectAsset(null);

			createdAsset = CreateInstance(selectedType) as GameDataSO;
			createdAsset.GenerateGuid();
			createdAsset.SetIcon(ResourcesHelper.sprite_DefaultItemIcon);

			isCreatingAsset = true;
			inspector = UnityEditor.Editor.CreateEditor(createdAsset);
		}

		private void CancelAssetCreation()
		{
			isCreatingAsset = false;

			if (inspector != null)
				DestroyImmediate(inspector);

			if (createdAsset != null)
				DestroyImmediate(createdAsset);

			inspector = null;
			SelectAsset(null);
		}

		#endregion

		private void ButtonAction_1()
		{
			if (panelIndex == 0)
			{
				Build();
			}
			else if (panelIndex == 1)
			{
				filters.ShowActive = !filters.ShowActive;
				UpdateAssetsToDisplay();
			}
		}

		private void ButtonAction_2()
		{
			if (panelIndex == 0)
			{
				Cache();
			}
			else if (panelIndex == 1)
			{
				filters.ShowDeleted = !filters.ShowDeleted;
				UpdateAssetsToDisplay();
			}
		}

		private void ButtonAction_3()
		{
			if (panelIndex == 0)
			{
				HelpWindow.Open();
			}
			else if (panelIndex == 1)
			{
				isListView = !isListView;
			}
		}

		private void ButtonAction_4()
		{
			if (panelIndex == 0)
			{
			}
			else if (panelIndex == 1)
			{
				StartAssetCreation();
			}
		}

		private void ButtonAction_5()
		{
			if (panelIndex == 0)
			{
			}
			else if (panelIndex == 1)
			{

			}
		}

	}
}