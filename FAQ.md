# Frequently Asked Questions
### Known issues with the Technical Alpha

- **The shared stash does not work for TCP/IP games.** Back up your shared stash **before** playing TCP/IP, and do not put items in the stash during TCP/IP games, they will be lost.
- It’s possible to visit Act III, IV, and V in the game, but those acts have no remastered graphics or voice files. You can play those acts in classic graphics mode.
- Text boxes for characters with no voice files will scroll to the bottom, scroll up if you want to read it.
- Cows in the Secret Cow Level are invisible
- During loading screens, it’s possible to start moving your character early and be hit by nearby monsters
- Game can crash while entering a town by walking
- Game can crash if you teleport to an interactive object and click it too fast
- Game can crash when opening the stash, especially for brand new characters

### Does the alpha include some old bugs, like Andariel glitching?

- The Andriel quest glitch works
- Quest glitching other bosses still works in multiplayer
- It’s still possible to preserve the Nihlathak portal
- Ebugging armor still works
- Gloams and other elemental enemies can still deal huge damage
- “Drains mana” enemies can still wipe out 100% of your mana

### Where are my save files? 

`C:\Users\<Username>\Saved Games\Diablo II Resurrected Tech Alpha`

### Where are my screenshots?

`C:\Users\<Username>\Documents\Diablo II\Screenshots`

### Where is my shared stash file located?

`C:\Users\<Username>\Saved Games\Diablo II Resurrected Tech Alpha\SharedStash_SoftCore.d2i`

This file can be backed up and shared with other players to dupe/transfer items.

### Why can’t I resume playing a previously created character?

Once you quit the game after creating a new character, you will need to use the TCP/IP button and host a game with that character.

Alternatively, you can use the `-FixLocalSave` argument with `D2ROffline.exe` to adjust  your save files to allow offline play, see the Usage section in the [README](readme.md)

### How do I go to Act III, IV, or V?

Use the `-FixLocalSave` argument with `D2ROffline.exe` to adjust your save files to allow offline play, see the Usage section in the [README](readme.md)

### How do I enable ladder runewords?

D2R-Offline can't currently do that, sorry. The setting for ladder runewords is adjusted on a per-game basis (that is, whenever you open a server or start playing single player, that particular instance of the game figures out if you should have ladder runewords). D2R-Offline only runs during D2R's startup to patch the client as a whole.

### Why is my map overlapping/not displaying properly?

Deleting the `.ma` file for your character’s save will fix it.

### Why does it say that this patch is a virus or trojan?

D2ROffline modifies the memory of another computer process, which looks like suspicious activity under normal use. The .exe is also not currently signed.

You can disable Windows Defender temporarily to download D2ROffline, and you can also add your D2R game folder to the list of antivirus exceptions. Make sure to re-enable Windows Defender after you get everything running.

### Can I import old save files?

Version 1.07 saves created by Hero Editor will import natively. You can also try to modify 1.14d saves using one of the tools below. Once a save has been loaded into D2R, it can’t be used in the old version of the game anymore.

D2ROffline cannot provide support for these tools, use at your own risk and always back up your saves.

- <https://www.moddb.com/games/diablo-2-lod/downloads/hero-editor-v-104>
- <https://d07riv.github.io/d2r.html>
- <https://dschu012.github.io/d2s/>

### Why does it say “bad quest data” when loading a character?

You have generated a save file with an invalid quest configuration. For example, you may have marked that you completed the cow level without completing the quest to kill Diablo or Baal. If you are generating a character with Hero Editor, be very conservative with what you create.

### My game doesn’t start, it says “Agent Error”

You need to delete the D2R game folder and re-install it from scratch.
