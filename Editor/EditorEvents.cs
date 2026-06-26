using System;
using UnityEngine;

public class EditorEvents : MonoBehaviour
{
	public static event Action AssetsChanged;

	public static void RaiseAssetsChanged()
	{
		AssetsChanged?.Invoke();
	}
}
