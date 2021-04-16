# Diablo II: Resurrected - Offline Patcher

A simple tool that remaps & bypasses *Diablo II Resurrected* module and then continues to patch connection functions to allow local gameplay. 

More information about the crc32 bypassing and remapping can be found in my '[Bypassing World of Warcraft's Read-Only Code Protection (crc32)](https://ferib.dev/blog.php?l=post/Bypassing_World_of_Warcraft_Crc32_Integrity_Checks)' blog post

![diablo 2 resurrected screenshot](https://github.com/ferib/D2R-Offline/blob/master/img/weird__flex.png?raw=true)

# Usage

.NET Framework v4.7.2 is required, which you can download at https://dotnet.microsoft.com/download/dotnet-framework/thank-you/net472-web-installer


Clone the project
``git clone ``

Build using Visual Studio

Move to output
``cd bin/Debug/netcoreapp3.1/``

Finally, launch the executable using `path` to `game.exe` as argument
``./D2ROffline.exe C:/D2R/Game.exe``

## Custom Patches

There is a neat little feature that allows you to use the `patches.txt` file and create your own patching rules, the `patches.txt` file **MUST** be in the same folder as the executable!
This allows you to create your own patches, for example:
```
0xD4AD68:9090
0xD4E25F:909090909090
0xCAFB9D:90B001
0x597E1C:90909090909090
```
~~These are already hardcoded inside the tool, but will be overruled once the `patches.txt` file is found.~~
These are no longer hardcoded inside the tool to prevent conflicts or unwanted behavior in game future updates.

## Download Game Files

Download this project: https://github.com/barncastle/Battle.Net-Installer/releases/tag/v1.3
Then use as `BNetInstaller.exe --prod osib --uid osi_beta --lang enus --dir "C:\D2R"`

## Notices
This repository is for educational purposes only. 
Please do not perform any of the above actions on the Game client.

Diablo II and Diablo II: Resurrected are registered trademarks of Blizzard Entertainment. 
This project is not affiliated with Blizzard Entertainment in any way.


## Credits
 - Ferib *(me)*: [crc32 bypass](https://ferib.dev/blog.php?l=post/Bypassing_World_of_Warcraft_Crc32_Integrity_Checks)
 - king48488: [Patch locations](https://www.ownedcore.com/forums/diablo-2-resurrected/diablo-2-resurrected-bots-programs/940315-some-basic-offsets-let-you-play-offline.html)
