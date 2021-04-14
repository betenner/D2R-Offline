# Diablo II: Resurrected - Offline Patcher

A simple tool that remaps & bypasses *Diablo II Resurrected* module and then continues to patch connection functions to allow local gameplay. 

More information about the crc32 bypassing and remapping can be found in my '[Bypassing World of Warcraft's Read-Only Code Protection (crc32)](https://ferib.dev/blog.php?l=post/Bypassing_World_of_Warcraft_Crc32_Integrity_Checks)' blog post

![diablo 2 resurrected screenshot](https://github.com/ferib/D2R-Offline/blob/master/img/weird_flex.png?raw=true)

# Usage

Dotnet core 3.1 is required, which you can download at https://dotnet.microsoft.com/download/dotnet/3.1


Clone the project
``git clone ``

Build using Visual Studio

Move to output
``cd bin/Debug/dotnetcore3.1/``

Finally, launch the executable using `path` to `game.exe` as argument
``./D2ROffline.exe C:/D2R/Game.exe``

# Download Game Files

Download this project: https://github.com/barncastle/Battle.Net-Installer/releases/tag/v1.3
Then use as `BNetInstaller.exe --prod osib --lang enus --dir "C:\D2R"`

## Credits
 - king48488: [Patch locations](https://www.ownedcore.com/forums/diablo-2-resurrected/diablo-2-resurrected-bots-programs/940315-some-basic-offsets-let-you-play-offline.html)
