using Gytis0.SOManager.Editor.Helpers;
using Gytis0.SOManager.Editor.Settings;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gytis0.SOManager.Editor.Windows
{
	public partial class MainWindow : EditorWindow
	{
		[SerializeField] private Filter filters = new();

		private List<Type> cachedTypes;
		private Dictionary<Type, List<GameDataSO>> cachedAssets = new();
		private GameDataSettings cachedSettings;

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
				cachedAssets[type] = type.GetAssets(IncludeDeleted.Both);

			if (selectedType != null)
				AssetController.FilterAssets(cachedAssets[selectedType], assetsToDisplay, filters);
		}
	}
}
