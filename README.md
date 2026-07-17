# SOManager
SOManager is a manager used to create, view, modify and delete your Scriptable Objects (SOs). It can also be used in code to easily access said SOs.

## Features in Editor Window
* Create, modify and delete SO instances
* View in list and grid view (soon object view as well)
* Validate SO instances
* Select multiple SO instances for bulk actions

## Features in Background
* Generates GUIDs for each new item for referencing the instance across game versions
* Validates instances (if ```IsValid()``` is implemented)
* Generates enums based on your input for easier item referencing in code
* Sets up the instances to be accessed easily from ```GameData```

## Features in Code
* Access any SO instance
* Compare different SO instances
* Validate manually in code
* Load and unload specific SO instances for better memory management (soon)

## How to use
See [Wiki](https://github.com/Gytis0/SOManager/wiki) for help.

## Unity Versions
Currently supports 6.1+

## Future work

1. Selective item loading
1. "Where" filters
1. Better "Object" view
1. Easy import / export

## Credits
https://flatuicolors.com/palette/defo

https://lucide.dev