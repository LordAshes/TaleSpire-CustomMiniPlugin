# Custom Mini Plugin

This unofficial TaleSpire plugin is for adding an unlimited number of custom minis.
Re-applies transformation automatically on re-load and requires no blank base.
Now supports assetBundles, assetBundle animations and emotes.

![Preview](https://i.imgur.com/xSRC6ZV.png)

Somewhat Outdated Demo Video: https://youtu.be/sRYln7Gc6Dg

Adding OBJ/MTL Content Video: https://youtu.be/JJ0xJQUM01U 

Making AssetBundles Content Video: https://youtu.be/VMIengmpbFw

Distributing CMP Content: https://youtu.be/wckyKTD7nPw

## Change Log

5.4.0: Added Poses and keyboard shortcut for quick access to default poses.

5.4.0: Modified animations to use individual prefabs for easier creation of non-t-pose base and related animations.

5.3.2: AssetBundles are now always unload after prefab is made to free up memory and prevent issues with reusing an assetBundle

5.3.1: Bug fix to prevent non-compatible minis from locking up CMP

5.3.1: Add Radial options for Effect Scaled and Effect Temporary

5.3.0: Comes with a sample "BloodPool" asset intended for Temporary Effect use

5.3.0: Added Temporary Effect (LCTRL+W) which disappears after a fixed time intermal (to be configurable in the future)

5.3.0: Added Scaled Effect Transform (scales to Mini size when added unlike regular Effect which is always medium size to start)

5.3.0: When transfromations are removed the key is removed from Stat Messaging

5.2.0: Mini Transformation added to radial menu under the Transfromation selection. Can be turned of in settings.

5.2.0: Added sample sphere effect content demonstrating transparency. Use "sphere" to apply.

5.1.0: Added manual "load" transformations mode (default LCTRL+T)

5.0.1: Corrected orientation of the transformed mini (so that it matche the mouse facing direction)

5.0.1: Added optional diagnostic mode (off by default) to provide more console logs regarding startup and board reload

5.0.0: Switched to using mesh transfer for normal and fly mode with added GO being used only for effects and animation.
5.0.0: Fixes Line of Sight Visility.

5.0.0: Fixes ability to select mini by any part of the mini (not just the base).

4.9.2: Fixed bug with board reload and transformed mini delete

4.9.1: Bug fix to support both MeshRenderer and SkinnedMeshRenderer assets.

4.9.0: Flying mode is now done the same way as regular mode. This means consistent texture (with transparency) and stealth in both modes.
       This fix also allows animations to be used in fly mode.

4.8.0: Fixed support for BMP, CRN, DDS, JPG, PNG and TGA texture files

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

### Transforming Minis And Accessign Poses

Add a mini to the board and select it. To transform the mini, press the Transform shotcut (Left CTRL+M by default but can be changed in
R2ModMan configuration for the plugin). Enter the name of the content to which the mini should be transformed. Ensure that the entered
content name corresponds to an OBJ and MTL file or a assetBundle file in the location TaleSpire_CustomData\Minis\{ContentName}\

For example:

TaleSpire_CustomData\Minis\Wizard01\Wizard01.OBJ
TaleSpire_CustomData\Minis\Wizard01\Wizard01.MTL

or for an assetBundle:

TaleSpire_CustomData\Minis\Wizard01\Wizard01

Transformations are automatically loaded when the board is loaded as long as the client has the corresponding content files.

Difference poses, if available, can be accessed by specifying the content name followed by a dot and the pose. For example, the content
name 'Trix.Crouch' would look up the Crouch pose in the Trix assetBundle.

### Initiating Effect

Each mini can have one effect active at a time. This process works identically to the Mini Transformation process except it
is done to a separate Effects game object and does not remove the mesh of the original mini nor the Mini Transformation object.
Pressing the Effects shortcut (Left CTRL + E by default but can be changed in the R2ModMan config) brings up a similar dialog
to the Mini Transformation. Enter the name of the (effect) content as usual. It will be applied to the mini while maintaining
the mini's appearance. This is ideal for adding effects which are tied to the caster's position.

The Scaled Effect (LCTRL+S) and the Temporary Effect (LCTRL+W) work the same way but are, for now, only available through
keyboard shortcut. Scaled And Temporary Effect scale the effect size to the mini size if the mini is not regular size (regular
Effect always loads as if a standard sized character). In addition, Temporary Effect disappears after a few seconds.

### Removing Effects

The transformation dialogs now has a Remove button which will remove mini transformations and effects (depending on which
dialog is used). However, please note that the Remove for Mini Transformation is less useful since the original mini mesh
is not restored.

### Automatic And Manual Load On Session/Board Reload

CMP has code to try to detected when minis have fully loaded and when it is safe to apply any previously saved transformations.
However, this automatic detection is not always accurate since TS does not provide a nice signal as to when all of the minis
have loaded. There is a CMP R2ModMan settings that can be adjusted to allow CMP more time to detect new minis but making the
setting a larger negative number means that CMP has better chances of detecting still loading minis but it also means a longer
wait time when loading smaller boards. As such, CMP also provides a manual load option where the user tells CMP to apply any
saved transformation. Basically the user waits until board and all minis have fully loaded and then uses this maual "load"
option. In such a case, the CMP automatic setting should be kept fairly low such as the default of -200 or less. However, the
setting should never be 0 or a positive number. 

### Sample Trix Asset

Use the usual transformation steps to transform a mini into Kiki. When entering the content name type "trix" (all lower case
without quotes). Once the asset is loaded you can cycle through the various animation by pressing the keyboard shortcuts
(default LCTRL+4 to LCTRL+6 since only 3 animations are defined) and the various poses (default RCTRL+4 to RCTRL+8). 

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

## Base Appearance, Poses And Animations

The base appearance (the appearance when the mini is not animated) is defined in a prefab that matches the name of the content.
For example if the asebtBundle is named Trix then the base appearance is expected in a prefab called Trix. This prefab does not
need to have an armature since it is not animated. Any animations on this prefab will be ignored.

Poses are additional static prefabs (prefabs with no animations) which have a different name than the content name. Normally they
can be set using the Mini Transformation option and specifying the content name (assetBundle name) followed by a dot and then the
desired pose name. However, there are 5 poses which can be accessed through CMP keyboard shortcuts (default RCTRL+3 to RCTRL+8).
These correspond to the base appaerance (RCLRTL+3) and then 4 additonal poses called Pose1 to Pose4.

Animations are additional prefabs which have a single animation whose name matches the prefab. These can be applied using the Play
Animation option (default LCTRL+P) but there are 5 animations which can be triggered by CMP keyboard shortcut keys (default
LCTRL+3 to LCTRL+8). 


## Limitations

1. Original mini rotation/position is applied to transformed minis
2. Temporary Effect duration is not configurable
3. Triggering poses while an animation is in progress can cause issues. 

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
