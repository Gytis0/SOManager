# SOManager
SOManager is an editor window used for easier Scriptable Objects management.

It can be used to:
* Create new instances
* Modify existing instances
* Validate instances
* Do bulk actions with many instances at once
* Quickly navigate through different SOs and their instances

SOManager automatically:
* Generates GUIDs for each new item for referencing the instance across game versions
* Validates instances (if ```IsValid()``` is implemented)
* Generates enums based on your input for easier item referencing in code
* Sets up the instances to be accessed easily from ```GameData```

## How to use
See [Wiki](https://github.com/Gytis0/SOManager/wiki) for help.

## Unity Versions
Currently supports 6.1+

## Future work

1. Selective item loading
1. Snapshots
1. "Where" filters
1. Better "Object" view

## Credits
https://flatuicolors.com/palette/defo

https://lucide.dev