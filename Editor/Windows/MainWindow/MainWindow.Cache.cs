using Gytis0.SOManager.Editor.Enums;
using Gytis0.SOManager.Editor.Extensions;
using Gytis0.SOManager.Editor.Helpers;
using Gytis0.SOManager.Editor.Settings;
using Gytis0.SOManager.Runtime;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Gytis0.SOManager.Editor.Windows
{
	public partial class MainWindow : EditorWindow
	{
		private Filter filters = new();

		private List<Type> cachedTypes;
		private Dictionary<Type, List<GameDataSO>> cachedAssets = new();
		private GameDataSettings cachedSettings;
		private int defaultNameIndex = 0;

		private void EditorEvents_AssetsChanged()
		{
			Cache();
		}

		/// <summary>
		/// Fetches and caches <see cref="cachedSettings"/>, <see cref="cachedAssets"/>, <see cref="cachedTypes"/>.
		/// </summary>
		public void Cache()
		{
			cachedSettings = GameDataSettingsProvider.Settings;

			cachedAssets.Clear();

			cachedTypes = TypeHelper.GetGameDataTypes();
			foreach (Type type in cachedTypes)
			{
				defaultNameIndex = 0;
				cachedAssets[type] = type.GetAssets(IncludeDeleted.Both);
				foreach (GameDataSO asset in cachedAssets[type])
				{
					if (string.IsNullOrWhiteSpace(asset.Name) || string.IsNullOrWhiteSpace(asset.Guid))
					{
						if (string.IsNullOrWhiteSpace(asset.Name))
						{

							asset.SetName(string.Format("{0}_{1}", type.Name, defaultNameIndex));
							asset.SetEnumName(string.Format("{0}_{1}", type.Name, defaultNameIndex));
							defaultNameIndex++;
						}
						if (string.IsNullOrWhiteSpace(asset.Guid))
							asset.GenerateGuid();
						EditorUtility.SetDirty(asset);
						AssetDatabase.SaveAssets();
					}
				}
			}

			if (selectedType != null)
				AssetController.FilterAssets(cachedAssets[selectedType], assetsToDisplay, filters);
		}
	}
}
