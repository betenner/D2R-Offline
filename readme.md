# Diablo II: Resurrected - Offline Patcher

A simple tool that remaps & bypasses *Diablo II Resurrected* module and then continues to patch connection functions to allow local gameplay. 

More information about the crc32 bypassing and remapping can be found in my '[Bypassing World of Warcraft's Read-Only Code Protection (crc32)](https://ferib.dev/blog.php?l=post/Bypassing_World_of_Warcraft_Crc32_Integrity_Checks)' blog post

![diablo 2 resurrected screenshot](https://github.com/ferib/D2R-Offline/blob/master/img/weird_group_flex.jpg?raw=true)

# Setup
## Installation


1. You will need the Diablo II: Resurrected game files.

1. You will need .NET Framework v4.7.2, which you can download at:  
https://dotnet.microsoft.com/download/dotnet-framework/thank-you/net472-web-installer

1. Download the latest release *(or clone the project and build it)* :  
https://github.com/ferib/D2R-Offline/releases  
Unzip and copy **both** `D2ROffline.exe` and `patches.txt` to your Diablo II: Resurrected game folder. They should be in the same directory as `Game.exe`.

## Run

1. Double-click `D2ROffline.exe` to start the tool.

1. A cmd window should open and begin the patching process, then prompt you to hit any key to continue, then the game should open. You can then close the cmd window.

1. When loading a character, use the TCP/IP button and host a game, double clicking the character name or clicking Play will not work by default. See the `-FixLocalSave` argument under [Usage](#Usage) if you would like to edit your saves to work in offline mode.

## Please see the [FAQ](FAQ.md) if you have any issues.

# Usage

## Arguments
You can run `D2ROffline.exe` with arguments to solve some problems.

### -FixLocalSave
`.\D2ROffline.exe -FixLocalSave` This will update your save files to allow you to play your characters in single player mode instead of hosting a TCP/IP game. This argument will not also run the game, it just runs a one-off process.

### -UpdateKeyBinds
`.\D2ROffline.exe -UpdateKeyBinds` This will sync your keybindings between characters.

### -Delay \<delay in ms>
`.\D2ROffline.exe -Delay 35` This will change the delay amount when patching your game, default is 25. Use this and try different values to help with crashes after being in-game for certain amounts of time.

### "Game.exe file path"
`.\D2ROffline.exe "C:\D2R\Game.exe"` Use this to specify a path to Game.exe if you're not running `D2ROffline.exe` from the same directory.

## Custom Patches

There is a neat little feature that allows you to use the `patches.txt` file and create your own patching rules, the `patches.txt` file **MUST** be in the same folder as the executable!
This allows you to create your own patches.

### Patches

Offline/Local patch, thanks to [king48488](https://www.ownedcore.com/forums/diablo-2-resurrected/diablo-2-resurrected-bots-programs/940315-some-basic-offsets-let-you-play-offline.html)
```
0xD4AD68:9090
0xD4E25F:909090909090
0xCAFB9D:90B001
0x597E1C:90909090909090
```

All classes, Multiplayer access and unlock act3~5 thanks to [shalzuth]()
```
0xD615F2:909090909090909090909090909090909090909090909090909090: ~ show all calsses on load (shalzuth)
0x39FC03:9090909090909090909090909090909090909090: ~ allow chars to load (shalzuth)
0x39FCB6:909090909090909090: ~ enable loading into a3-a5 (shalzuth)
```

Language patches to force the client into loading a language, thanks to Ferib (me).
```
0x1446C8:+0x00: ~ English *[enUS]*
0x1446C8:+0x270A4: ~ German *[deDE]*
0x1446C8:+0x270AC: ~ Spanish *[esES]*
0x1446C8:+0x270B4: ~ French *[frFR]*
0x1446C8:+0x270BC: ~ Italian *[itIT]*
0x1446C8:+0x270C4: ~ Korean *[koKR]*
0x1446C8:+0x270CC: ~ Polski *[plPL]*
0x1446C8:+0x270D4: ~ Russian *[ruRU]*
0x1446C8:+0x270DC: ~ Chinese (simplified) *[zhCN]*
0x1446C8:+0x270E4: ~ Chinese *[zhTW]*
0x1446C8:+0x270EC: ~ Spanish *[esMX]*
0x1446C8:+0x270F4: ~ Japanese *[jaJP]*
0x1446C8:+0x270FC: ~ Brazilian *[ptBR]*
```

## Mutiplayer (tcp/ip)
Want to test out multiplayer mode? feel free to join my server at `ferib.dev` and come say hi!

Also, feel free to [donate money](https://github.com/sponsors/ferib) in case you made it this far, this will keep me motivated to work on Diablo2Resurrected related projects and to keep the server going, thanks!

## Notices
This repository is for educational purposes only. 
Please do not perform any of the above actions on the Game client.

Diablo II and Diablo II: Resurrected are registered trademarks of Blizzard Entertainment. 
This project is not affiliated with Blizzard Entertainment in any way.


## Credits
 - Ferib *(me)*: [crc32 bypass](https://ferib.dev/blog.php?l=post/Bypassing_World_of_Warcraft_Crc32_Integrity_Checks)
 - king48488: [Patch locations](https://www.ownedcore.com/forums/diablo-2-resurrected/diablo-2-resurrected-bots-programs/940315-some-basic-offsets-let-you-play-offline.html)