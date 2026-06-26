# SOManager
Editor window for managing Scriptable Objects.

## Initialization
GameData will be initialized automatically before any first scene load.\
To make sure that you access the data only when it is available, use ```Initialized``` flag and ```OnInitialized``` event:

```csharp
using UnityEngine;
using SOManager.Runtime;
public class MyClass : MonoBehaviour
{
	private void Awake()
	{
		if(!GameData.Initialized)
			GameData.OnInitialize += OnInitialize;
		else
			Debug.Log($"Fireball: {GameData.Get<Spell>(Spells.Fire).Damage}");
	}

	private void OnInitialize(object sender, EventArgs e)
	{
		Debug.Log($"Fireball: {GameData.Get<Spell>(Spells.Fire).Damage}");
	}
}
```

## Usage in code

For any item to be considered by the SOManager, it has to inherit from ```GameDataSO```.

Use:
```csharp
var spell = GameData.Get<Spell>(Spells.Fire);
```

Where type ```Spell``` could be any class inheriting from ```GameDataSO```.\
Where enum ```Spells.Fire``` could be any enum belonging to any item (like ```Spell```). Enums are generated automatically and should not be edited manually.

## Usage in Editor window
wip

## Future work

1. Selective item loading
1. Snapshots
1. "Where" filters

### Color palette
https://flatuicolors.com/palette/defo