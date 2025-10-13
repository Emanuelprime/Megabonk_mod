# Megabonk Simple Mod (Template)

## Guide for setting up the project
1. Clone the repository (https or ssh clone in Github)
2. Open the project in your preferred IDE (Visual Studio, Visual Studio Code, etc)
3. Rename all occurences of `MEGABONK_SIMPLE_MOD` with your mods name. (including file names)
4. Edit the project description in the `.sln` file (`MEGABONK_SIMPLE_MOD.sln` initially)
5. Download the preferred BepInEx version from https://builds.bepinex.dev/projects/bepinex_be (Version #733, BepInEx Unity (IL2CPP) for Windows (x64) games)
6. From within the ZIP file copy the following files to the projects `Libs` folder (these are a example set, you might need additional ones depending on your mod and development):
- `\BepInEx\core\0Harmony.dll`
- `\BepInEx\core\BepInEx.Core.dll`
- `\BepInEx\core\BepInEx.Unity.IL2CPP.dll`
- `\BepInEx\core\Il2CppInterop.Runtime.dll`
7. Open `Plugin.cs` and replace `AUHTOR_NAME` with your preferred user/team name

## Building the project
At this point you should have a project that you can build to a BepInEx mod plugin. How you build the project differs a bit based on your preferred IDE and/or build tools.

With Visual Studio Code you can use the .NET Install Tool to build the project: https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.vscode-dotnet-runtime. Please ensure you've installed .NET 6 SDK before trying to build https://dotnet.microsoft.com/en-us/download/dotnet/6.0

When you've installed both you should be able to build the project by pressing CTRL+SHIFT+P and selecting the `.NET: Build` command. After a while you should have a `MEGABONK_SIMPLE_MOD.dll` (file name depends on your renamed project) in `obj\Debug\net6.0\`.

For other IDEs and build tools you can Google how to build your project.

## Testing the mod
Next you'll want to either setup Thunderstore Mod Manager, R2modman or manual setup of BepInEx

### Thunderstore Mod Manager and R2modman
Go through "Packaging for Thunderstore" before continuing with this.

Welcome back! Now that you've got your Thunderstore package; you can locally install it by:
1. Open up Thunderstore Mod Manager or R2modman
2. Select the game and a profile
3. Go to Settings -> Profile -> Import local mod -> Select file
4. Select the mod package you've made
5. Check that the information is correct and click `Import local mod`
6. Start the game through the mod manager

If you want update your mod while developing, you can replace the `.dll` in your profile after rebuilding your project. You can find the `.dll` at Settings -> Locations -> Browse profile folder -> BepInEx -> Plugins -> YOUR_PACKAGE


### Manual BepInEx installation and usage
1. Open the above mentione BepInEx ZIP again and copy the contents of that ZIP into the games root directory (Steam default installation is `C:\Program Files (x86)\Steam\steamapps\common\Megabonk`)
2. Run the game once and then close it, so that BepInEx can properly set it self up.
3. Take your built mod plugin at `obj\Debug\net6.0\MEGABONK_SIMPLE_MOD.dll` and copy it to the `C:\Program Files (x86)\Steam\steamapps\common\Megabonk\BepInEx\plugins` folder.
4. Start the game `C:\Program Files (x86)\Steam\steamapps\common\Megabonk\Megabonk.exe` or through Steam. Be aware that starting the game through Steam keeps the leaderboard enabled, sometimes you might not want this.

When you develop your mod and want to "refresh" to currently used code; you can rebuild the project and replace `.dll` in `C:\Program Files (x86)\Steam\steamapps\common\Megabonk\BepInEx\plugins` folder.

## Packaging for Thunderstore
Packaging for Thunderstore is simple, but there few steps that you have to carefully go through.

Here's a list of resources for help:
- https://thunderstore.io/c/megabonk/create/docs/
- https://thunderstore.io/tools/markdown-preview/
- https://thunderstore.io/tools/manifest-v1-validator/

To package do the following;
1. Copy your mod `.dll` at `obj\Debug\net6.0\MEGABONK_SIMPLE_MOD.dll`(file name depends on the project renaming) into to the `thunderstore_packaging` folder.
2. Open the `manifest.json` and correct the information accordingly. (`name`, `version_number`, `website_url`, `description` and `dependencies`)
- `website_url` is not mandatory, but very much preferred so that people can go check your code
- `dependencies` should usually always have `BepInEx-BepInExPack_IL2CPP-6.0.733` in it. Add more dependencies if your package depends on another package.
3. Replace the `thunderstore_packaging\icon.png` image with your preferred package image. The dimensions should be 256x256 pixels, `.png` format and preferrably under 20KB in size.
4. Edit the `thunderstore_packaging\README.md` file to your liking, this will be shown as the package details in Thunderstore.
5. Take the contents of the `thunderstore_packaging` folder `MEGABONK_SIMPLE_MOD.dll`, `icon.png`, `manifest.json` and `README.md`; and put them into a ZIP file. (on windows select the files, right click one and choose Compress to... -> ZIP file)
6. Rename the ZIP file in to the following format; `THUNDERSTORE_TEAM_NAME-PACKAGE_NAME-VERSION_NUMBER` e.g. `Oksamies-MEGABONK_SIMPLE_MOD-0.1.0`

Now you have a Thunderstore mod package

## Uploading to Thunderstore

Simple stuff;
1. Log into Thunderstore (website)
2. Go create a team that is named just like your ZIP files begining (the `THUNDERSTORE_TEAM_NAME` you replaced), at https://thunderstore.io/settings/teams/
2. Go to https://thunderstore.io/package/create/
3. Drag and drop the ZIP file into the the field that says "Choose or drag file here"
4. Select the team you want to upload the package under (same as in the ZIP files name)
5. Select the community (Megabonk)
6. Select categories accordingly
7. If the package contains NSFW stuff, check the checkbox
8. Submit and wait for the package to be submitted

## Uploading a new version to Thunderstore
Essentially this is the same as `Packaging for Thunderstore` and `Uploading to Thunderstore`, but you need to increment the versions everywhere. The locations to update the versions should be `Plugin.cs`(if you have defined it there. Remember to rebuild the dll after), `thunderstore_packaging\manifest.json`and the `.csproj` of your project.

Then just upload the package as you would upload a new package and the new version will be uploaded.