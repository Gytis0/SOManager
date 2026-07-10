using UnityEngine;

public static class KeyCodeExtensions
{
	public static bool IsArrowKeys(this KeyCode keyCode)
	{
		return keyCode == KeyCode.UpArrow || keyCode == KeyCode.DownArrow || keyCode == KeyCode.LeftArrow || keyCode == KeyCode.RightArrow;
	}

	public static bool IsAlpha(this KeyCode keyCode)
	{
		return keyCode == KeyCode.Alpha0 ||
			keyCode == KeyCode.Alpha1 ||
			keyCode == KeyCode.Alpha2 ||
			keyCode == KeyCode.Alpha3 ||
			keyCode == KeyCode.Alpha4 ||
			keyCode == KeyCode.Alpha5 ||
			keyCode == KeyCode.Alpha6 ||
			keyCode == KeyCode.Alpha7 ||
			keyCode == KeyCode.Alpha8 ||
			keyCode == KeyCode.Alpha9;
	}
}
