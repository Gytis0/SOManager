using System.Collections.Generic;

// TODO : Why do you even need this?
namespace SOManager.Runtime
{
	public static class EnumUtility
	{
		private static readonly Dictionary<string, Dictionary<string, int>> _cache = new();

		public static void Register(string typeName, Dictionary<string, int> map)
		{
			_cache[typeName] = map;
		}

		public static int GetEnumKey(System.Type type, string enumName)
		{
			if (_cache.TryGetValue(type.Name, out var map) && map.TryGetValue(enumName, out var value))
				return value;

			return -1;
		}
	}
}