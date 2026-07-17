using Gytis0.SOManager.Editor.Extensions;
using UnityEditor;
using UnityEngine;

#pragma warning disable CS0642
namespace Gytis0.SOManager.Editor.Windows
{
	public partial class MainWindow : EditorWindow
	{
		private bool isKeyboardActive = false;
		private int panelIndex = 0;

		private void HandleKeyboard()
		{
			Event e = Event.current;

			if (e.type == EventType.MouseDown)
			{
				isKeyboardActive = false;
				return;
			}

			if (IsFieldSelected() || e.type != EventType.KeyDown)
				return;

			if (e.keyCode.IsArrowKeys())
			{
				isKeyboardActive = true;

				if (e.control)
				{
					if (e.keyCode == KeyCode.LeftArrow)
						SetPanelIndex(panelIndex - 1);
					else if (e.keyCode == KeyCode.RightArrow)
						SetPanelIndex(panelIndex + 1);
				}
				else
				{
					Navigate(e);
				}
				e.Use();
				return;
			}
			if (e.keyCode == KeyCode.Delete)
			{
				isKeyboardActive = true;

				if (isCreatingAsset) ;
				else if (selectedAssets.Count == 1)
				{
					if (!lastSelectedAsset.IsDeleted)
					{
						AssetController.DeleteAsset(lastSelectedAsset);
					}
					else
					{
						AssetController.RestoreAsset(lastSelectedAsset, cachedAssets[selectedType]);
					}
				}
				else if (selectedAssets.Count > 1)
				{
					if (!lastSelectedAsset.IsDeleted)
						AssetController.DeleteAssets(selectedAssets);
					else
						AssetController.RestoreAssets(selectedAssets, cachedAssets[selectedType]);
				}

				e.Use();
				return;
			}
			if (e.keyCode == KeyCode.Home)
			{
				isKeyboardActive = true;

				if (panelIndex == 0)
					SelectType(cachedTypes[0]);
				else if (panelIndex == 1)
					SelectAsset(cachedAssets[selectedType][0], 0);

				e.Use();
				return;
			}

			if (e.keyCode == KeyCode.End)
			{
				isKeyboardActive = true;

				if (panelIndex == 0)
					SelectType(cachedTypes[cachedTypes.Count - 1]);
				else if (panelIndex == 1)
					SelectAsset(cachedAssets[selectedType][cachedAssets[selectedType].Count - 1], cachedAssets[selectedType].Count - 1);

				e.Use();
				return;
			}

			if (e.keyCode.IsAlpha())
			{
				isKeyboardActive = true;

				UseButtons(e);
				e.Use();
				return;
			}

			Vector2 mouse = e.mousePosition;
		}

		private void Navigate(Event e)
		{
			if (panelIndex == 0)
				Navigate_Type(e);
			else if (panelIndex == 1)
				Navigate_Asset(e);
		}

		private void Navigate_Type(Event e)
		{
			if (e.keyCode == KeyCode.DownArrow)
				selectedTypeIndex = Mathf.Min(selectedTypeIndex + 1, cachedTypes.Count - 1);
			else if (e.keyCode == KeyCode.UpArrow)
				selectedTypeIndex = Mathf.Max(selectedTypeIndex - 1, 0);
			else return;

			SelectType(cachedTypes[selectedTypeIndex]);
		}

		private void Navigate_Asset(Event e)
		{
			int newIndex;
			if (isListView)
			{
				if (e.keyCode != KeyCode.DownArrow && e.keyCode != KeyCode.UpArrow) return;

				int currentIndex = selectedAssetIndex_Last == selectedAssetIndex_Bottom ? selectedAssetIndex_Top : selectedAssetIndex_Bottom;
				int delta = e.keyCode == KeyCode.DownArrow ? 1 : -1;

				newIndex = Mathf.Clamp(currentIndex + delta, 0, assetsToDisplay.Count - 1);

				SelectAsset(assetsToDisplay[newIndex], newIndex);
			}
			else
			{
				int currentIndex = selectedAssetIndex_Last == selectedAssetIndex_Bottom ? selectedAssetIndex_Top : selectedAssetIndex_Bottom;
				int delta = (e.keyCode == KeyCode.DownArrow && !IsInLastRow(currentIndex, gridColumns, assetsToDisplay.Count)) ? gridColumns :
							(e.keyCode == KeyCode.UpArrow && !IsInFirstRow(currentIndex, gridColumns)) ? -gridColumns :
							e.keyCode == KeyCode.RightArrow ? 1 :
							e.keyCode == KeyCode.LeftArrow ? -1 : 0;

				newIndex = Mathf.Clamp(currentIndex + delta, 0, assetsToDisplay.Count - 1);

				SelectAsset(assetsToDisplay[newIndex], newIndex);
			}
		}

		private void UseButtons(Event e)
		{
			if (e.keyCode == KeyCode.Alpha1)
				ButtonAction_1();
			else if (e.keyCode == KeyCode.Alpha2)
				ButtonAction_2();
			else if (e.keyCode == KeyCode.Alpha3)
				ButtonAction_3();
			else if (e.keyCode == KeyCode.Alpha4)
				ButtonAction_4();
			else if (e.keyCode != KeyCode.Alpha5)
				ButtonAction_5();
		}

		private bool IsFieldSelected()
		{
			return EditorGUIUtility.keyboardControl != 0;
		}

		private void HandlePanelIndex(int panelId)
		{
			Rect rect = GUILayoutUtility.GetLastRect();
			if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
				SetPanelIndex(panelId);
		}

		private bool IsInFirstRow(int index, int columnCount)
		{
			return index < columnCount;
		}

		private bool IsInLastRow(int index, int columnCount, int itemCount)
		{
			int firstIndexInLastRow = (itemCount - 1) / columnCount * columnCount;
			return index >= firstIndexInLastRow;
		}

		private void SetPanelIndex(int panelId)
		{
			panelIndex = Mathf.Clamp(panelId, 0, 2);
			Repaint();
		}
	}
}
