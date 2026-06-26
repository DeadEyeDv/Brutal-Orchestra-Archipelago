Brutal Orchestra Archipelago Mod

Archipelago randomizer integration for Brutal Orchestra.

	Installation

	Mod (C# client)
1. Install [BepInEx 5.4](https://github.com/BepInEx/BepInEx) for Brutal Orchestra.
2. Download `BrutalOrchestraArchipelago.dll` from [Releases](https://github.com/ТВОЙ_НИК/BrutalOrchestraArchipelago/releases).
3. Place the DLL in `BepInEx/plugins/`.
4. Run the game once to generate the config file at `BepInEx/config/brutal.ap.mod.cfg`.
5. Edit the config to set your server URL and slot name.

	APWorld (generator)
1. Download `brutal_orchestra.apworld` from [Releases](https://github.com/ТВОЙ_НИК/BrutalOrchestraArchipelago/releases).
2. Place it in the `custom_worlds` folder of your Archipelago installation.

	Features
- Sends checks for battles, chests, shops, hero purchases
- Locked zones require access items
- Configurable shop and battle counts via options
- Supports multiworld

	Building from source
Open the `.csproj` in Visual Studio or Rider, add references to `Assembly-CSharp.dll`, `UnityEngine.dll`, `BepInEx.dll`, `0Harmony.dll` from the game folder, and build.

	Credits
Created by DeadEye