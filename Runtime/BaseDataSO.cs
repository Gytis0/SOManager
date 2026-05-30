using UnityEngine;

public abstract class GameDataSO : ScriptableObject
{
	[SerializeField]
	private string guid;

	[SerializeField]
	private string enumName;

	[SerializeField]
	private bool softDeleted;

	public string Guid => guid;
	public string EnumName => enumName;
	public bool SoftDeleted => softDeleted;

#if UNITY_EDITOR

    public void SetGuid(string value)
    {
        guid = value;
    }

    public void SetEnumName(string value)
    {
        enumName = value;
    }

    public void SetSoftDeleted(bool value)
    {
        softDeleted = value;
    }

#endif
}