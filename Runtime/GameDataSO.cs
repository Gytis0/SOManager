using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("com.gytis0.somanager.Editor")]
public abstract class GameDataSO : ScriptableObject, IGameDataSO
{
	[SerializeField, HideInInspector] private string guid;
	[SerializeField] private string enumName;
	[SerializeField, HideInInspector] private int enumId;
	[SerializeField, HideInInspector] private bool isDeleted;
	[SerializeField] private string displayName;
	[SerializeField] private Sprite icon;

	public string Guid => guid;
	public string EnumName => enumName;
	public int EnumId => enumId;
	public bool IsDeleted => isDeleted;

	public string Name => displayName;
	public Sprite Icon => icon;

	/// <summary>
	/// Checks whether this asset is valid.
	/// </summary>
	/// <remarks>
	/// By default it always returns true. If you want to use the validation, override this method.
	/// </remarks>
	/// <returns><see langword="true"/> if valid. Otherwise, <see langword="false"/></returns>
	public virtual bool IsValid()
	{
		return true;
	}

	#region Internal

	internal void GenerateGuid()
	{
		guid = System.Guid.NewGuid().ToString("N");
	}

	internal void SetEnumName(string value)
	{
		enumName = value;
	}

	internal void SetEnumId(int value)
	{
		enumId = value;
	}

	internal void SetSoftDeleted(bool value)
	{
		isDeleted = value;
	}

	internal void SetName(string value)
	{
		displayName = value;
	}

	internal void SetIcon(Sprite icon)
	{
		this.icon = icon;
	}

	internal string GetIdentifyingName()
	{
#if UNITY_6000_5_OR_NEWER
		return !string.IsNullOrWhiteSpace(Name) ? Name :
			!string.IsNullOrWhiteSpace(EnumName) ? EnumName :
			!string.IsNullOrWhiteSpace(Guid) ? Guid :
			GetEntityId().ToString();
#elif UNITY_6000_1_OR_NEWER
		return !string.IsNullOrWhiteSpace(Name) ? Name :
			!string.IsNullOrWhiteSpace(EnumName) ? EnumName :
			!string.IsNullOrWhiteSpace(Guid) ? Guid :
			GetInstanceID().ToString();
#endif
	}

	#endregion
}