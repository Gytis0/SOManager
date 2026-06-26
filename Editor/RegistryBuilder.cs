using SOManager.Runtime;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SOManager.EditorTools
{
	public static class RegistryBuilder
	{
		/// <summary>
		/// Builds a registry file with all of the items inside of it. The registry file is used at runtime to initialize all data.
		/// </summary>
		/// <param name="outputPath"></param>
		public static void Build(string outputPath)
		{
			outputPath.EnsureFolder();
			Registry registry = ScriptableObject.CreateInstance<Registry>();

			List<GameDataSO> all = new();

			foreach (var type in TypeHelper.GetGameDataTypes())
			{
				var assets = type.GetAssets(IncludeDeleted.NotDeleted);
				all.AddRange(assets);
			}

			var so = new SerializedObject(registry);
			var prop = so.FindProperty("entries");
			prop.ClearArray();

			for(int i = 0; i < all.Count; i++)
			{
				prop.InsertArrayElementAtIndex(i);
				prop.GetArrayElementAtIndex(i).objectReferenceValue = all[i];
			}

			so.ApplyModifiedPropertiesWithoutUndo();

			AssetDatabase.CreateAsset(registry, string.Format("{0}/Registry.asset", outputPath));
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}
}