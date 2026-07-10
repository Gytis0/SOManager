public static class GameDataSOExtensions
{
	public static string GetIdentifyingName(this GameDataSO gameDataSO)
	{
		return !string.IsNullOrWhiteSpace(gameDataSO.Name) ? gameDataSO.Name :
			!string.IsNullOrWhiteSpace(gameDataSO.EnumName) ? gameDataSO.EnumName :
			!string.IsNullOrWhiteSpace(gameDataSO.Guid) ? gameDataSO.Guid :
			gameDataSO.GetInstanceID().ToString();
	}
}
