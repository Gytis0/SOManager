using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace SOManager.EditorTools
{
	public static class TypeHelper
	{
		/// <summary>
		/// Gets all types that inherit <see cref="GameDataSO"/>.
		/// </summary>
		/// <returns></returns>
		public static List<Type> GetGameDataTypes()
		{
			return TypeCache.GetTypesDerivedFrom<GameDataSO>()
				.Where(t => !t.IsAbstract)
				.OrderBy(t => t.Name)
				.ToList();
		}
	}
}