using Gytis0.SOManager.Editor.Extensions;
using Gytis0.SOManager.Editor.Helpers;
using Gytis0.SOManager.Editor.Settings;
using Gytis0.SOManager.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Gytis0.SOManager.Editor
{
	public static class AssetController
	{
		public static EventHandler OnDeleteChanged;

		public static GameDataSO CreateNewAsset(Type type, GameDataSO asset, List<GameDataSO> cachedAssets, GameDataSettings settings)
		{
			if (string.IsNullOrWhiteSpace(asset.EnumName))
				asset.SetEnumName(asset.Name.ToPascalCase());
			else
				asset.SetEnumName(asset.EnumName.ToPascalCase());

			try
			{
				EnsureValidAsset(asset, cachedAssets);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				return null;
			}

			string folderPath = Path.Combine(settings.DataPath, type.Name).Replace('\\', '/');

			settings.DataPath.EnsureFolder();
			folderPath.EnsureFolder();

			string assetPath = string.Format("{0}/{1}.asset", folderPath, asset.EnumName);

			if (AssetDatabase.LoadAssetAtPath<GameDataSO>(assetPath) != null)
			{
				Debug.LogError(string.Format("Asset '{0}' already exists.", asset.EnumName));
				return null;
			}

			GameDataSO newAsset = ScriptableObject.CreateInstance(type) as GameDataSO;

			EditorUtility.CopySerialized(asset, newAsset);

			AssetDatabase.CreateAsset(newAsset, assetPath);
			AssetDatabase.SaveAssets();

			return newAsset;
		}

		public static void EnsureValidAsset(GameDataSO asset, List<GameDataSO> cachedAssets)
		{
			List<Exception> exceptions = new();

			if (string.IsNullOrWhiteSpace(asset.EnumName))
				exceptions.Add(new Exception("The asset EnumName cannot be empty."));
			else if (!IsAssetUnique(asset, cachedAssets, out string existingName))
				exceptions.Add(new Exception(string.Format("Another enum value of '{0}' is already in use by {1}.", asset.EnumName, existingName)));

			if (string.IsNullOrWhiteSpace(asset.Name))
				exceptions.Add(new Exception("The asset Name cannot be empty."));

			if (!asset.IsValid())
				Debug.LogWarningFormat("Asset {0} is not valid. Check the implemented IsValid()", asset.GetIdentifyingName(), asset);

			if (exceptions.Count > 0)
				throw new AggregateException(string.Format("Asset '{0}' is not valid.", asset.GetIdentifyingName()), exceptions);
		}

		public static bool IsAssetUnique(GameDataSO asset, List<GameDataSO> assetList, out string existingName)
		{
			var existing = assetList.FirstOrDefault(x => x.EnumName == asset.EnumName && x.Guid != asset.Guid && !x.IsDeleted);
			if (existing != null)
			{
				existingName = existing.name;
				return false;
			}

			existingName = null;
			return true;
		}

		public static bool IsAssetUnique(GameDataSO asset, HashSet<GameDataSO> assetList, out string existingName)
		{
			var existing = assetList.FirstOrDefault(x => x.EnumName == asset.EnumName && x.Guid != asset.Guid && !x.IsDeleted);
			if (existing != null)
			{
				existingName = existing.name;
				return false;
			}

			existingName = null;
			return true;
		}

		public static void DeleteAssets(HashSet<GameDataSO> assets)
		{
			foreach (GameDataSO asset in assets)
				asset.SetSoftDeleted(true);
			OnDeleteChanged?.Invoke(null, EventArgs.Empty);
		}

		public static void DeleteAsset(GameDataSO asset)
		{
			asset.SetSoftDeleted(true);
			OnDeleteChanged?.Invoke(null, EventArgs.Empty);
		}

		public static void RestoreAssets(HashSet<GameDataSO> assets, List<GameDataSO> allAssets)
		{
			foreach (GameDataSO asset in assets)
			{
				if (!IsAssetUnique(asset, allAssets, out string existingName))
				{
					Debug.LogException(new Exception(string.Format("Can't restore asset '{0}' because another enum value of '{1}' is already in use by {2}.", asset.GetIdentifyingName(), asset.EnumName, existingName)));
					continue;
				}

				if (!IsAssetUnique(asset, assets, out existingName))
				{
					Debug.LogException(new Exception(string.Format("Can't restore asset '{0}' because another enum value of '{1}' is already in use by {2}.", asset.GetIdentifyingName(), asset.EnumName, existingName)));
					continue;
				}

				asset.SetSoftDeleted(false);
			}
			OnDeleteChanged?.Invoke(null, EventArgs.Empty);
		}

		public static void RestoreAsset(GameDataSO asset, List<GameDataSO> allAssets)
		{
			if (!IsAssetUnique(asset, allAssets, out string existingName))
			{
				Debug.LogException(new Exception(string.Format("Can't restore asset '{0}' because another enum value of '{1}' is already in use by {2}.", asset.GetIdentifyingName(), asset.EnumName, existingName)));
				return;
			}

			asset.SetSoftDeleted(false);
			OnDeleteChanged?.Invoke(null, EventArgs.Empty);
		}

		public static void FilterAssets(List<GameDataSO> allAssets, List<GameDataSO> resultList, Filter filter)
		{
			resultList.Clear();
			if (filter.ShowActive && !filter.ShowDeleted)
				resultList.AddRange(allAssets.Where(x => !x.IsDeleted));
			else if (filter.ShowActive && filter.ShowDeleted)
				resultList.AddRange(allAssets);
			else if (!filter.ShowActive && filter.ShowDeleted)
				resultList.AddRange(allAssets.Where(x => x.IsDeleted));
		}

		public static bool DoAssetsDiffer(UnityEngine.Object a, UnityEngine.Object b)
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
	}
}
