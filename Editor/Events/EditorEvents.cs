using System;
using UnityEngine;

namespace Gytis0.SOManager.Editor.Events
{
	public class EditorEvents : MonoBehaviour
	{
		public static event Action OnAssetsChanged;

		public static void RaiseAssetsChanged()
		{
			OnAssetsChanged?.Invoke();
		}
	}
}
