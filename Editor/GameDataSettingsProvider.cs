using UnityEditor;
using UnityEngine;

namespace SOManager.EditorTools
{
	public static class GameDataSettingsProvider
	{
		private static GameDataSettings settings;

		public static GameDataSettings Settings
		{
			get
			{
				if (settings == null)
					LoadOrCreate();

				return settings;
			}
		}

		private static void LoadOrCreate()
		{
			settings = null;
			string[] guids = AssetDatabase.FindAssets("GameDataSettings");
			foreach (string guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				try
				{
					settings = AssetDatabase.LoadAssetAtPath<GameDataSettings>(path);
					if (settings != null) break;
				}
				catch { }
			}

			if (settings != null)
				return;

			settings = ScriptableObject.CreateInstance<GameDataSettings>();

			if (!AssetDatabase.IsValidFolder("Assets/SOManager"))
				AssetDatabase.CreateFolder("Assets", "SOManager");
			AssetDatabase.CreateAsset(settings, "Assets/SOManager/GameDataSettings.asset");
			AssetDatabase.SaveAssets();
		}
	}
}