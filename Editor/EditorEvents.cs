using System;
using UnityEngine;

public class EditorEvents : MonoBehaviour
{
	public static event Action OnAssetsChanged;

	public static void RaiseAssetsChanged()
	{
		OnAssetsChanged?.Invoke();
	}
}
