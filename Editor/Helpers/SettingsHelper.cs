using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Gytis0.SOManager.Runtime;

namespace Gytis0.SOManager.Editor.Helpers
{
	public static class SettingsHelper
	{
		public static (int converted, int skipped, List<string> convertedFiles, List<string> skippedFiles) ConvertNonGenericGameDataTypesToGeneric()
		{
			List<Type> targets = TypeCache.GetTypesDerivedFrom<GameDataSO>()
				.Where(t => t.BaseType != null && t.BaseType == typeof(GameDataSO))
				.Where(t => t != typeof(GameDataSO<>))
				.OrderBy(t => t.Name)
				.ToList();

			int converted = 0, skipped = 0;
			List<string> convertedFiles = new();
			List<string> skippedFiles = new();

			foreach (Type type in targets)
			{
				string path = FindScriptPath(type);
				if (string.IsNullOrEmpty(path))
				{
					skipped++;
					skippedFiles.Add(string.Format("{0} (Invalid path).", type.Name));
					continue;
				}

				string enumName = type.Name.Replace("SO", "") + "s";

				if (ConvertFile(path, type.Name, enumName))
				{
					converted++;
					convertedFiles.Add(Path.GetFileName(path));
				}
				else
				{
					skipped++;
					skippedFiles.Add(Path.GetFileName(path));
				}
			}

			AssetDatabase.Refresh();
			return (converted, skipped, convertedFiles, skippedFiles);
		}

		private static string FindScriptPath(Type type)
		{
			foreach (string guid in AssetDatabase.FindAssets("t:MonoScript"))
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
				if (script != null && script.GetClass() == type) return path;
			}
			return null;
		}

		private static bool ConvertFile(string path, string typeName, string enumName)
		{
			string text = File.ReadAllText(path);

			// Matches: class TypeName ... : GameDataSO   (word boundary, not GameDataSO<...>)
			var pattern = new Regex(
				$@"(class\s+{Regex.Escape(typeName)}\b[^{{:]*:\s*)GameDataSO(?!\s*<)\b");

			if (!pattern.IsMatch(text)) return false; // already generic or not found

			string replaced = pattern.Replace(text, $"$1GameDataSO<{enumName}>", 1);
			replaced = EnsureUsingDirective(replaced, "Gytis0.SOManager.Enums");

			File.WriteAllText(path, replaced);
			return true;
		}

		private static string EnsureUsingDirective(string text, string namespaceName)
		{
			string usingLine = $"using {namespaceName};";

			var existsPattern = new Regex($@"^\s*using\s+{Regex.Escape(namespaceName)}\s*;", RegexOptions.Multiline);
			if (existsPattern.IsMatch(text)) return text;

			var usingBlockPattern = new Regex(@"^(using\s+[^;]+;\r?\n)+", RegexOptions.Multiline);
			Match match = usingBlockPattern.Match(text);

			if (match.Success && match.Index == 0)
			{
				int insertAt = match.Index + match.Length;
				return text.Insert(insertAt, usingLine + Environment.NewLine);
			}

			return usingLine + Environment.NewLine + text;
		}
	}
}