using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gytis0.SOManager.Runtime
{
	public static class GameData
	{
		private static readonly Dictionary<Type, Dictionary<int, GameDataSO>> _data = new();
		public static bool Initialized { get; private set; }

		public static EventHandler OnInitialize;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void InitializeOnLoad()
		{
			if (Initialized)
				return;

			Registry registry = Resources.Load<Registry>("SOManager/Registry");

			if (registry == null)
			{
				Debug.LogError("Registry could not be loaded.");
				return;
			}

			Initialize(registry);
		}

		private static void Initialize(Registry registry)
		{
			if (Initialized) return;

			_data.Clear();

			foreach (var entry in registry.Entries)
			{
				if (entry == null) continue;
				if (entry.IsDeleted) continue;

				Type type = entry.GetType();

				if (!_data.TryGetValue(type, out var map))
				{
					map = new Dictionary<int, GameDataSO>();
					_data[type] = map;
				}

				map[entry.EnumId] = UnityEngine.Object.Instantiate(entry);
			}

			Initialized = true;
			OnInitialize?.Invoke(null, EventArgs.Empty);
		}

		/// <summary>
		/// Gets all items of type <typeparamref name="T"/>.
		/// </summary>
		/// <remarks>
		/// Gets the original assets from the SOManager. Do not modify the properties.
		/// </remarks>
		/// <typeparam name="T">Type of the item.</typeparam>
		/// <returns>All items, if any found. Else, an empty list.</returns>
		/// <exception cref="Exception"></exception>
		public static IEnumerable<T> GetAll<T>() where T : GameDataSO
		{
			if (!Initialized)
				throw new Exception(string.Format("GameData not initialized. You can use a '{0}' flag and / or '{1}' event to wait for initialization.", nameof(Initialized), nameof(OnInitialize)));

			if (!_data.TryGetValue(typeof(T), out var map))
				yield break;

			foreach (GameDataSO value in map.Values)
				yield return (T)value;
		}

		/// <summary>
		/// Gets copies of all items of type <typeparamref name="T"/>.
		/// </summary>
		/// <remarks>
		/// Gets copies of the assets from the SOManager. You may modify the properties.
		/// </remarks>
		/// <typeparam name="T">Type of the items.</typeparam>
		/// <returns>A list containing copies of all items.</returns>
		/// <exception cref="Exception"></exception>
		public static List<T> GetAllCopy<T>()
			where T : GameDataSO
		{
			if (!Initialized)
				throw new Exception(string.Format("GameData not initialized. You can use a '{0}' flag and / or '{1}' event to wait for initialization.", nameof(Initialized), nameof(OnInitialize)));

			List<T> result = !_data.TryGetValue(typeof(T), out var map) ? new List<T>() : new List<T>(map.Count);

			if (map == null)
				return result;

			foreach (GameDataSO value in map.Values)
				result.Add(UnityEngine.Object.Instantiate((T)value));

			return result;
		}

		/// <summary>
		/// Gets an item of type <typeparamref name="T"/>.
		/// </summary>
		/// <remarks>
		/// Gets the original asset from the SOManager. Do not modify the properties.
		/// </remarks>
		/// <typeparam name="T">Type of the item.</typeparam>
		/// <param name="id">Id of the item.</param>
		/// <returns>The item, if found. Otherwise, <see langword="null"/>.</returns>
		/// <exception cref="Exception"></exception>
		public static T Get<T>(Enum id)
			where T : GameDataSO
		{
			if (!Initialized)
				throw new Exception(string.Format("GameData not initialized. You can use a '{0}' flag and / or '{1}' event to wait for initialization.", nameof(Initialized), nameof(OnInitialize)));

			if (_data.TryGetValue(typeof(T), out var map) && map.TryGetValue(Convert.ToInt32(id), out var value))
				return (T)value;

			return null;
		}

		/// <summary>
		/// Gets an item of type <typeparamref name="T"/>.
		/// </summary>
		/// <remarks>
		/// Gets the asset copy from the SOManager. You may modify the properties.
		/// </remarks>
		/// <typeparam name="T">Type of the item.</typeparam>
		/// <param name="id">Id of the item.</param>
		/// <returns>The item, if found. Otherwise, <see langword="null"/>.</returns>
		/// <exception cref="Exception"></exception>
		public static T GetCopy<T>(Enum id)
			where T : GameDataSO
		{
			if (!Initialized)
				throw new Exception("GameData not initialized. Wait for it to initialize first.");

			if (_data.TryGetValue(typeof(T), out var map) && map.TryGetValue(Convert.ToInt32(id), out var value))
				return UnityEngine.Object.Instantiate((T)value);

			return null;
		}
	}
}