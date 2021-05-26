# Custom Mini Plugin

This unofficial TaleSpire plugin is for adding an unlimited number of custom minis.

Demo Video: https://youtu.be/sRYln7Gc6Dg

Adding Content Video: https://youtu.be/JJ0xJQUM01U

Please note that the Adding Content Video is currently out of date. Most of the video is still applicable but the content needs to use a Minis sub-folder (see info below) and PNG and JPG texture can now be used.

## Change Log

1.6.0: OBJ, MTL and texture files expected in a minis folder and then a sub-folder named after the asset (e.g. TaleSpire_CustomData\Minis\Wizard\Wizard.obj)
1.6.0: Added fake JPG and PNG support. Asset can use JPG/PNG textures which the plugin automatically converts to BMPs.
1.5.0: Fixed shader bug. Content uses the Standard shader (not the TaleSpire\Creature shader).
1.5.0: OBJ, MTL and texture files expected in a sub folder named after the asset (e.g. TaleSpire_CustomData\Wizard\Wizard.obj)
1.5.0: Includes the TaleSpire_CustomData folder in ZIP along with a Test content

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

## Adding Custom Content

Each piece of custom content needs to consist of a OBJ file and MTL file. It can, optionally also contain texture files, which
should be in BMP format. PNG and JPG can now be used but will be automatically converted to BMP by the plugin. References to files
should be relative and in the same directory (i.e. don't use full paths for texture file names). The name of OBJ file and MTL file
should be the same except for the extension. The texture files can have any name (e.g. if used by multiple models). The content
file name is the name that is used to access it in TaleSpire. For example, the content Warlock.OBJ and Warlock.MTL would be
accessed by Warlock (without the extension).

OBJ, MTL and texture files are to be placed in \Steam\steamapps\common\TaleSpire\TaleSpire_CustomData\Minis\{ContentName}\

The plugin ZIP contains the TaleSpire_CustomData folder (with a sample Test content) which needs to be moved to your TaleSpire
game directoy. The core game does not contain this directory. It is used for added content by Lord Ashes plugins.

Since the custom content files are only read when they are needed, the content can be added, removed or modified while TaleSpire
is running. This allows easy testing of custom models.

## Ideal Base

In order for the plugin to work best, a blank base is needed. For users who do not want to create their own blank base,
a blank base file is included which turns the TaleSpire Christmas Mimic into a blank base. There are two files that need
to be applied in order to get the blank base. Both are included in the plugin ZIP file.

Place "aTBvbnNj" in the ""\Steam\steamapps\common\TaleSpire\Taleweaver" folder.
Place "char_mimic02_1606620485" in the "\Steam\steamapps\common\TaleSpire\Taleweaver\Assets" folder.

Both of these files should already exist. Just replace them. You can back up the original files if you wish but Steam can
always revert the files back to their original if needed. 

Once these files are applied and TaleSpire is restarted, you should find the Blank Base under Creatures | Monsterous.

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
