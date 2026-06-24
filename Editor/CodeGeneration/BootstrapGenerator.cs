using System;
using System.IO;
using System.Text;

namespace SOManager.EditorTools
{
	public static class GameDataBtstrapGenerator
	{
		public static void Generate(string outputFolder)
		{
			Directory.CreateDirectory(outputFolder);

			StringBuilder sb = new();

			sb.AppendLine("using SOManager;");
			sb.AppendLine();
			sb.AppendLine("namespace SOManager.Generated");
			sb.AppendLine("{");
			sb.AppendLine("    public static class GameDataGeneratedBootstrap");
			sb.AppendLine("    {");
			sb.AppendLine("        public static void Register()");
			sb.AppendLine("        {");

			foreach (Type type in TypeHelper.GetGameDataTypes())
			{
				string enumName = type.Name.Replace("SO", "Id");

				sb.AppendLine(string.Format("            GameData.RegisterType<{0}, {1}>();", type.Name, enumName));
			}

			sb.AppendLine("        }");
			sb.AppendLine("    }");
			sb.AppendLine("}");

			File.WriteAllText(Path.Combine(outputFolder, "GameDataGeneratedBootstrap.cs"), sb.ToString());
		}
	}
}