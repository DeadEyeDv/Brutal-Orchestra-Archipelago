Brutal Orchestra Archipelago Mod

Archipelago randomizer integration for Brutal Orchestra.

Archipelago: [GitHub](https://github.com/ArchipelagoMW/Archipelago)

#	Installation 

1. Download "Brutal Orchestra Archipelago" from [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3767731509) 
2. Turn on this mod in the game and reboot the game
3. Change / delete your save file
4. Connect to your slot and start your game (If you unable to start a run after connecting, reboot game and connect AFTER you started a new run)

   
	APWorld (generator)
1. Download `brutal_orchestra.apworld` from [Releases](https://github.com/DeadEye_Dv/BrutalOrchestraArchipelago/releases).
2. Place it in the `custom_worlds` folder of your Archipelago installation.
3. Make your .yaml (your options) file in the Option Creator in Archipelago Launcher (Don't use Template)

	Features
- Sends checks for battles, chests, shops, hero purchases
- Locked zones require access items
- Configurable shop, battle and chest counts via options
- Ability sanity for every hero (including Nowak) (For this to work you must have 50+ additional locations via Bittle, Chests or Shop)
- Supports multiworld

	Building from source
Open the `.csproj` in Visual Studio or Rider, add references to `Assembly-CSharp.dll`, `UnityEngine.dll`, `BepInEx.dll`, `0Harmony.dll`, `UnityEngine.IMGUIModule.dll` from the game folder, and build.

(NOT RECOMENDED) You can still download the mod as .dll file from releases, but it's better to download it from Steam Workshop


Created by DeadEye
