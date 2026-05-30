public class GameDataManagerWindow : EditorWindow
{
	private Type selectedType;
	private GameDataSO selectedAsset;

	private Vector2 typeScroll;
	private Vector2 assetScroll;
	private Vector2 inspectorScroll;

	private readonly Dictionary<Type, List<GameDataSO>> cache =
		new();

	private string search = "";

	[MenuItem("Tools/Game Data Manager")]
	public static void Open()
	{
		GetWindow<GameDataManagerWindow>("Game Data");
	}
}