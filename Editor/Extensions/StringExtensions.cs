using System;
using System.IO;
using System.Text;
using UnityEditor;

namespace Gytis0.SOManager.Editor.Extensions
{
	public static class StringExtensions
	{
		public static string ToPascalCase(this string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return string.Empty;

			StringBuilder sb = new();

			string[] parts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (string part in parts)
			{
				if (part.Length == 0)
					continue;

				sb.Append(char.ToUpperInvariant(part[0]));

				if (part.Length > 1)
					sb.Append(part.Substring(1));
			}

			return sb.ToString();
		}

		public static void EnsureFolder(this string path)
		{
			path = path.Replace('\\', '/');

			if (Path.HasExtension(path))
				path = Path.GetDirectoryName(path)?.Replace('\\', '/');

			if (string.IsNullOrEmpty(path))
				return;

			string[] parts = path.Split('/');

			string current = parts[0];

			for (int i = 1; i < parts.Length; i++)
			{
				string next = string.Format("{0}/{1}", current, parts[i]);

				if (!AssetDatabase.IsValidFolder(next))
					AssetDatabase.CreateFolder(current, parts[i]);

				current = next;
			}
		}
	}
}