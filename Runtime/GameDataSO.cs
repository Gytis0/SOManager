using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("com.gytis0.somanager.Editor")]
public abstract class GameDataSO : ScriptableObject, IGameDataSO
{
	[SerializeField, HideInInspector] private string guid;
	[SerializeField, HideInInspector] private string enumName;
	[SerializeField, HideInInspector] private int enumId;
	[SerializeField, HideInInspector] private bool isDeleted;

	public string Guid => guid;
	public string EnumName => enumName;
	public int EnumId => enumId;
	public bool IsDeleted => isDeleted;

	public string Name;
	public Sprite Icon;

	public abstract bool IsValid();

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

	#endregion
}