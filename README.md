# SOManager
Editor window for managing Scriptable Objects.

## Startup
Make sure to initialize the GameData at the startup:

```csharp
using UnityEngine;
using SOManager.Runtime;
public class Bootstrap : MonoBehaviour
{
	[SerializeField] private GameDataRegistry registry;

	private void Awake()
	{
		GameData.Initialize(registry);
	}
}
```