using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Gytis0.SOManager.Editor.Windows
{
	public partial class MainWindow : EditorWindow
	{
		private UnityEditor.Editor inspector;

		private int selectedTypeIndex = -1;
		private int selectedAssetIndex_Top = -1;
		private int selectedAssetIndex_Bottom = -1;
		private int selectedAssetIndex_Last = -1;

		private GameDataSO lastSelectedAsset;
		private readonly HashSet<GameDataSO> selectedAssets = new();

		/// <summary>
		/// Selects asset and updates inspector based on that asset.
		/// </summary>
		/// <param name="asset">Asset to select.</param>
		private void SelectAsset(GameDataSO asset, int index = -1)
		{
			if (isCreatingAsset)
			{
				Debug.LogWarning("Finish creating the new asset first.");
				return;
			}

			SavePendingChanges();

			UpdateSelection(asset, index);

			CreateEditingCopies();

			CreateInspectorEditor();
		}

		private void UpdateSelection(GameDataSO asset, int index = -1)
		{
			if (asset == null)
			{
				selectedAssets.Clear();
				lastSelectedAsset = null;
				selectedAssetIndex_Last = -1;
				return;
			}

			if (index == -1)
			{
				lastSelectedAsset = asset;
				selectedAssetIndex_Last = selectedAssetIndex_Top = selectedAssetIndex_Bottom = assetsToDisplay.IndexOf(asset);
				isAutoScrollNeeded_Asset = true;
				return;
			}

			Event e = Event.current;

			EditorGUI.FocusTextInControl(null);

			if (e != null && e.shift && lastSelectedAsset != null)
			{
				int from = selectedAssetIndex_Last;
				int to = index;

				if (from > to)
					(from, to) = (to, from);

				selectedAssets.Clear();

				for (int i = from; i <= to; i++)
					selectedAssets.Add(assetsToDisplay[i]);

				selectedAssetIndex_Top = from;
				selectedAssetIndex_Bottom = to;
			}
			else if (e != null && e.control)
			{
				if (!selectedAssets.Add(asset))
					selectedAssets.Remove(asset);
				selectedAssetIndex_Last = selectedAssetIndex_Top = selectedAssetIndex_Bottom = index;
				lastSelectedAsset = asset;
			}
			else
			{
				selectedAssets.Clear();
				selectedAssets.Add(asset);
				selectedAssetIndex_Last = selectedAssetIndex_Top = selectedAssetIndex_Bottom = index;
				lastSelectedAsset = asset;
			}

			isAutoScrollNeeded_Asset = true;
		}

		private void SelectType(Type type)
		{
			if (type == selectedType) return;

			if (isCreatingAsset)
			{
				Debug.LogWarning("Finish creating the new asset first.");
				return;
			}

			EditorGUI.FocusTextInControl(null);

			selectedTypeName = type.FullName;
			selectedType = type;
			isAutoScrollNeeded_Type = true;

			UpdateAssetsToDisplay();

			SelectAsset(assetsToDisplay[0], 0);
		}

		private void RestoreState()
		{
			searchField_Types = new();
			searchField_Asset = new();

			var tempAssetGuid = selectedAssetGuid;
			SelectType(cachedTypes.FirstOrDefault(x => x.FullName == selectedTypeName));

			if (!string.IsNullOrEmpty(tempAssetGuid))
			{
				string path = AssetDatabase.GUIDToAssetPath(tempAssetGuid);
				SelectAsset(AssetDatabase.LoadAssetAtPath<GameDataSO>(path));
			}
		}

		private void CreateInspectorEditor()
		{
			if (inspector != null)
				DestroyImmediate(inspector);

			if (selectedAssets.Count == 0)
			{
				inspector = null;
				return;
			}

			inspector = UnityEditor.Editor.CreateEditor(tempAssets.Values.Cast<UnityEngine.Object>().ToArray());
		}

		private void CreateEditingCopies()
		{
			tempAssets.Clear();

			foreach (GameDataSO asset in selectedAssets)
			{
				GameDataSO temp = CreateInstance(asset.GetType()) as GameDataSO;
				EditorUtility.CopySerialized(asset, temp);

				tempAssets.Add(asset, temp);
			}
		}

		private void AssetController_OnDeleteChanged(object sender, EventArgs e)
		{
			UpdateAssetsToDisplay();
		}

		private void UpdateAssetsToDisplay()
		{
			AssetController.FilterAssets(cachedAssets[selectedType], assetsToDisplay, filters);
		}

		private void SavePendingChanges()
		{
			List<GameDataSO> tempAssetsValues = tempAssets.Values.ToList();
			foreach (var pair in tempAssets)
			{
				GameDataSO original = pair.Key;
				GameDataSO temp = pair.Value;

				if (!AssetController.DoAssetsDiffer(temp, original))
					continue;

				try
				{
					AssetController.EnsureValidAsset(temp, cachedAssets[selectedType]);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
					continue;
				}

				if (!AssetController.IsAssetUnique(temp, tempAssetsValues, out string existingName))
				{
					Debug.LogException(new Exception(string.Format("Can't save asset '{0}' because another enum value of '{1}' is already in use by {2}.", temp.GetIdentifyingName(), temp.EnumName, existingName)));
					continue;
				}

				EditorUtility.CopySerialized(temp, original);
			}
		}
	}
}
