# Custom Mini Plugin

This unofficial TaleSpire plugin is for adding an unlimited number of custom minis.
Re-applies transformation automatically on re-load and requires no blank base.
Now supports assetBundles, assetBundle animations and emotes.

![Preview](https://i.imgur.com/xSRC6ZV.png)

Somewhat Outdated Demo Video: https://youtu.be/sRYln7Gc6Dg

Adding OBJ/MTL Content Video: https://youtu.be/JJ0xJQUM01U

(Adding AssetBundle content is very similar to adding OBJ/MTL content)

## Change Log

4.7.0: Text at the top of the screen indicates the common TaleSpire_CustomData folder where custom content is expected to be found
       (shows only when one of the transformation dialog windows are open)

4.7.0: When the indicated transformation content is not found, a message indicates the TaleSpire_CustomData folder where it was expected.

4.6.1: Moved CustomData folder to a plugins folder so that the CustomData contents don't get flattened when the plugin is installed

4.6.0: Modified to use FileAccessPlugin for loading assets. This means CMP now supports remote resource specification if desired.

4.6.0: Assets can be in the TaleSpire_CustomData folder (as before) or in any plugin's CustomData sub-folder.

4.5.1: Modification to get core TS Emotes working with CMP transformed assets

4.5.0: Fix for compatability with TS Build 6913204 and newer.

4.4.0: Access to CMP Effect is available from Radial menu from Transformation icon and then Effect icon

4.4.0: When a smaller or larger base is used to transform, the contents now load at the scaled size

4.3.1: Re-added posting of the plugin to the TaleSpire main page (which was accidentally removed in 4.3.0)

4.3.0: AssetBundle animations for both effects and minis are supported

4.3.0: Shortcut keys to play Anim1, Anim2, ..., Anim5

4.3.0: Shortcut key to open dialog for playing animation by name

4.2.1: Plugin is now shown on TaleSpire main page

4.2.1: Fixed issue with removal of old transformation when a new transformation is applied

4.2.0: Re-added animations and sounds for effects (not for mini transformations)

4.2.0: Fixed effect position issue 

4.1.0: Fixed height bar for transformed minis

4.1.0: Stealth now uses core TS mecahnism so stealth syncing is no longer needed in the update loop

4.1.0: PNG and JPG files can now be used without internal conversion to BMP

4.0.2: Correctly identifies the Stat Messaging plugin as a dependency onm Thunderstore

4.0.0: Fixed Fly and Emotes.

4.0.0: Uses Stat Messaging for synchronization of the transforms for higher compatability with other plugins. 

4.0.0: Major overhaul to actually replace the core mini mesh instead of attaching a game object to it.

3.2.0: Exposed StatHandler and provided callback for content load fails to allow plugin to be used as a dependency plugin.

3.1.1: Bug fix and readme correction. Effect are triggered with Left CTRL + E (not T).

3.1.0: Added effects support (Left CTRL + E). Like mini transformation but does not delete the character mesh.

3.1.0: Added remove buttons to the transformation dialogs to remove transformations.

3.1.0: Fixed issue with OBJ/MTL content

3.0.0: Added assetBundle support

3.0.0: Added animation support

2.0.0: Blank base is no longer needed

2.0.0: Transformation are automatically restored on loaded

2.0.0: Trasformation triggered using CTRL+M (can be changed in R2ModMan)

2.0.0: Moved from Chat distribution to Name distribution

1.6.1: Exposed Plugin Guid to allow it to be marked as a dependency

1.6.0: OBJ, MTL and texture files expected in a minis folder and then a sub-folder named after the asset (e.g. TaleSpire_CustomData\Minis\Wizard\Wizard.obj)

1.6.0: Added fake JPG and PNG support. Asset can use JPG/PNG textures which the plugin automatically converts to BMPs.

1.5.0: Fixed shader bug. Content uses the Standard shader (not the TaleSpire\Creature shader).

1.5.0: OBJ, MTL and texture files expected in a sub folder named after the asset (e.g. TaleSpire_CustomData\Wizard\Wizard.obj)

1.5.0: Includes the TaleSpire_CustomData folder in ZIP along with a Test content

## Install

Install using R2ModMan or similar and place custom contents (OBJ/MTL and texture files or AssetBundles) in TaleSpire_CustomData\Minis\{ContentName}\
Comes with a TaleSpire_CustomData ZIP file which can be expanded into the Tale Spire game directory to add the sample Kiki asset. Merge the ZIP file contents
with any other contents if the folder already exists.

## Usage

### Transforming Minis

Add a mini to the board and select it. To transform the mini, press the Transform shotcut (Left CTRL+M by default but can be changed in
R2ModMan configuration for the plugin). Enter the name of the content to which the mini should be transformed. Ensure that the entered
content name corresponds to an OBJ and MTL file or a assetBundle file in the location TaleSpire_CustomData\Minis\{ContentName}\

For example:

TaleSpire_CustomData\Minis\Wizard01\Wizard01.OBJ
TaleSpire_CustomData\Minis\Wizard01\Wizard01.MTL

or for an assetBundle:

TaleSpire_CustomData\Minis\Wizard01\Wizard01

Transformations are automatically loaded when the board is loaded as long as the client has the corresponding content files.

### Initiating Effect

Each mini can have one effect active at a time. This process works identically to the Mini Transformation process except it
is done to a separate Effects game object and does not remove the mesh of the original mini nor the Mini Transformation object.
Pressing the Effects shortcut (Left CTRL + E by default but can be changed in the R2ModMan config) brings up a similar dialog
to the Mini Transformation. Enter the name of the (effect) content as usual. It will be applied to the mini while maintaining
the mini's appearance. This is ideal for adding effects which are tied to the caster's position.

### Removing Effects

The transformation dialogs now has a Remove button which will remove mini transformations and effects (depending on which
dialog is used). However, please note that the Remove for Mini Transformation is less useful since the original mini mesh
is not restored.

### Sample Kiki Asset

If you have unzipped the TaleSpire_CustomData ZIP file into the proper location (as per installation instructions) you will have
access to the Kiki sample asset. Use the usual transformation steps to transform a mini into Kiki. When entering the content name
type "kiki" (all lower case without quotes). Once the asset is loaded you can cycle through the various animation by pressing the
keyboard shortcuts (default LCTRL+4 to LCTRL+8). Anim1=Ready, Anim2=Cast, Anim3=Die, Anim4=Dance you can also use the Play Animation
to trigger any of these animations or "T-Pose" (without quotes) to switch Kiki into her T-Pose.

## Adding Custom Content

### OBJ/MTL Content

Each piece of custom content needs to consist of a OBJ file and MTL file. It can, optionally also contain texture files, which
should be in BMP format. PNG and JPG can now be used but will be automatically converted to BMP by the plugin. References to files
should be relative and in the same directory (i.e. don't use full paths for texture file names). The name of OBJ file and MTL file
should be the same except for the extension. The texture files can have any name (e.g. if used by multiple models). The content
file name is the name that is used to access it in TaleSpire. For example, the content Warlock.OBJ and Warlock.MTL would be
accessed by Warlock (without the extension).

OBJ, MTL and texture files are to be placed in \Steam\steamapps\common\TaleSpire\TaleSpire_CustomData\Minis\{ContentName}\

Since the custom content files are only read when they are needed, the content can be added, removed or modified while TaleSpire
is running. This allows easy testing of custom models.

### AssetBundle Content

Place the assetBundle file (with no extension) into a folder with the same name as the content, as in:

\Steam\steamapps\common\TaleSpire\TaleSpire_CustomData\Minis\{ContentName}\

The folder, assetBundle file and the content within it should all have the same name. For example:

\Steam\steamapps\common\TaleSpire\TaleSpire_CustomData\Minis\Wizard01\Wizard01 should contain a Wizard01 prefab.

## Animations & Poses

Assets (minis or effect) which define one or more animations called Anim1, Anim2, Anim3, Anim4 or Anim5 can be triggered directly
by pressing the corresponing shortcut keys (default LCTRL+4 to LCTRL+8) which can be configured in R2ModMan for the CMP plugin.
Other animation on the asset can be played by name by pressing the Play Animation keyboard shortcut (default LCTRL+P) and then
entering name of the animation as it appears in the assetBundle.

Please note that animations should not be looping since this consumes significant CPU and can prevent TS from being reactive.
Use either the Once setting or the Clamp Forever setting. The Once setting is ideal for animations because the asset it returned
back to the default state afterwards. The Clamp Forever setting is ideal for poses because the last frame of the animation is held 
until a different animation/pose is activated.

## Limitations

### Stealth & Height Bar

The current implementation works with Steath Mode and Height Bar although the asset is hidden instead of being moved off the screen
when the height bar is low enough. However this is just a cosmetic difference and works the same otherwise.  

### Fly Mode

Fly mode does odd things with the mini. To make fly mode work, CMP makes the original mini visible with its mesh transformed. This
prevents the mini from disappearing during fly mode. However, this can cause shading differences since the original minis use the
TS/Creature shader whereas most custom assest do not. There is no current work around for this except to create custom assets as
assetBundles which use the TS/Creature shader. Hovever, this is a cosmetic issue, besides the shading difference fly mode works as
expected. 

### Multi-Creature Assets

TS Build 6913204 introduces the possibility for multiple creatures per base. Full compatability with such assets has not been tested. 

### Board Reload

When a board is loaded after the first one, the current detection algroithm does not always recognize that. If a different board
is loaded then there typically are no issues since all the minis will appears as different CreatureId and thus their messages
(for transformation) should be processed. However, if the same board is reload then the plugin will not see any of the messages
as changes and thus no process initial transformations. There are two work around for this:

#### Re-transform

You can re-apply any transformation. To do this select the Mini, press the Mini Transformation keyboard shortcut keys, and then
click the Remove button. Even if you don't see a transformation applied, you need to perform this step. If you don't, there is a
good chance that apply the transformation will not work. Once the remove has been applied, press the Mini Transformation keyboard
shortcut keys, again and enter the desired transformation (like applying it for the first time).

#### Restart

You can also reload the last board with full CMP processing by restarting TaleSpire. In such a case, the CMP plugin is restarted
and thus any transformation requests become new requests.


