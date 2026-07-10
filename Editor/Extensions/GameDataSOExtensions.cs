public static class GameDataSOExtensions
{
	public static string GetIdentifyingName(this GameDataSO gameDataSO)
	{
#if UNITY_6000_5_OR_NEWER
		return !string.IsNullOrWhiteSpace(gameDataSO.Name) ? gameDataSO.Name :
			!string.IsNullOrWhiteSpace(gameDataSO.EnumName) ? gameDataSO.EnumName :
			!string.IsNullOrWhiteSpace(gameDataSO.Guid) ? gameDataSO.Guid :
			gameDataSO.GetEntityId().ToString();
#elif UNITY_6000_1_OR_NEWER
		return !string.IsNullOrWhiteSpace(gameDataSO.Name) ? gameDataSO.Name :
			!string.IsNullOrWhiteSpace(gameDataSO.EnumName) ? gameDataSO.EnumName :
			!string.IsNullOrWhiteSpace(gameDataSO.Guid) ? gameDataSO.Guid :
			gameDataSO.GetInstanceID().ToString();
#endif
	}
}
