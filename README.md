# CharacterView Plugin

This unofficial TaleSpire plugin is for adding an unlimited number of custom minis.

## Install

Go to the releases folder and download the latest and extract to the contents of your TaleSpire game folder.

## Usage

Best results are gained by using a small existing mini or by using the content swap technique to create an empty content mini.

Add a mini to the board, pick it up and drop it. This will copy the mini's id to the clipboard along with the chat request
command for transforming the mini. Open the chat bar (by pressing enter) and paste in clipboard contents. This will paste in
an ugly looking code ending in "Make me a ". Type the name of the custom content file (without the path or the OBJ suffix) and
press ENTER. The corresponding mini will say "Make me a " followed by the content name and the transformation will occur.

Transformations are not automatically loaded when the campaign and board are loaded but they are stored automatically in a
campaign and board specific file which can be easily retrieved...

Open the chat bar and type: ! TRANSFORM

This will apply all the transformation for that campaign board. This way the GM can set up all the transformations during build
time and then when the board is loaded, the GM just needs to issue the ! TRANSFORM chat request and the transformations will be
applied on the GM side and on all players' sides.

Once a board is no longer going to be used, the corresponding transformation file can be deleted by issuing the following command
in the chat bar: ! DELETE

## Current Limitations

1. The custom content is limited to OBJ/MTL files. If the MTL files uses textures they must be BMP (not PNG or JPG).
2. When issuing the ! TRANSFORM or ! DELETE command, a mini must be seelcted.

## How to Compile / Modify

Open ```CustomMiniPlaugin.sln``` in Visual Studio.

You will need to add references to:

```
* BepInEx.dll  (Download from the BepInEx project.)
* Bouncyrock.TaleSpire.Runtime (found in Steam\steamapps\common\TaleSpire\TaleSpire_Data\Managed)
* UnityEngine.dll
* UnityEngine.CoreModule.dll
* UnityEngine.InputLegacyModule.dll 
```

Build the project.

Browse to the newly created ```bin/Debug``` or ```bin/Release``` folders and copy the ```CharacterViewPlugin.dll``` to ```Steam\steamapps\common\TaleSpire\BepInEx\plugins```
