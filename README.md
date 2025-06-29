# chefrpg.curry

## Requirements
You have to have downloaded and installed BepInEx for this mod to work.
BepInEx Installation Guide: https://docs.bepinex.dev/articles/user_guide/installation/index.html

## Installing Curry﻿
1. Download the .zip file.
2. Extract the contents of the .zip file.
3. Move the "Curry" folder and its contents into the "plugins" folder of your BepInEx install.
(On Steam, this folder is typically here: C:\Program Files (x86)\Steam\steamapps\common\Chef RPG\BepInEx\plugins)

If done correctly, your files should look like so:
```
└── plugins/
       └── Curry/
          ├── Assets/
          │  ├── Empty Curry.png
          │  ├── FA Curry.png
          ﻿﻿│  └── Small Food Curry.png
          ├── delete.txt
          ├── dependentspiritual.chefrpg.curry.dll
          └── choose_saves.txt
```

## Using Curry
1. Make a backup of your save file, on Steam, it's probably here: C:\Users\[USERNAME]\AppData\LocalLow\World 2 Studio Inc\Chef RPG\FullData

2. Choose in which files you want to have the recipe and write that in "choose_saves.txt". So if you want to have the recipe in saves 1 and 2, the text file should have this inside:
y,y,n,n
If you have only one or you don't care, you can just leave it as the default, which is this:
y,y,y,y

3. Launch the game and enjoy!

## Delete.txt
Deleting this file will delete the recipe from any save that is specified in "choose_saves.txt". You can just create it again with the same name to bring back the recipe.

## Removing Curry
1. Choose from what saves you want to remove curry from, if everywhere, put y,y,y,y into "choose_saves.txt".
2. Delete the "delete.txt" file.
3. Launch the game and wait for the BepInEx console to say the curry files have been deleted. ("Recipe has been successfully deleted in save 1...")
4. Close it.
5. Delete the entire "Curry" folder from "plugins".

## Fixing errors
If anything breaks, contact me on discord, my tag is #dependentspiritual.
You could also try just deleting the "delete.txt" file and hoping for the best.

## Disclaimer
Again, please backup your file before doing this.
