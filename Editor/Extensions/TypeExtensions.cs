using Gytis0.SOManager.Editor.Enums;
using Gytis0.SOManager.Runtime;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Gytis0.SOManager.Editor.Extensions
{
	public static class TypeExtensions
	{
		/// <summary>
		/// Gets existing assets.
		/// </summary>
		/// <param name="type">Type of the asset.</param>
		/// <param name="includeDeleted">Wheteher to include deleted or not.</param>
		/// <returns>A list of <see cref="GameDataSO>"/>.</returns>
		public static List<GameDataSO> GetAssets(this Type type, IncludeDeleted includeDeleted = IncludeDeleted.NotDeleted)
		{
			List<GameDataSO> result = new();

			string[] guids = AssetDatabase.FindAssets($"t:{type.Name}");

			foreach (string guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);

				GameDataSO asset = AssetDatabase.LoadAssetAtPath(path, type) as GameDataSO;

				if (asset == null)
					continue;

				if (includeDeleted == IncludeDeleted.NotDeleted && asset.IsDeleted)
					continue;

				if (includeDeleted == IncludeDeleted.Deleted && !asset.IsDeleted)
					continue;

				result.Add(asset);
			}

			result.Sort((a, b) => string.Compare(a.EnumName, b.EnumName, StringComparison.Ordinal));

			return result;
		}
	}
}
