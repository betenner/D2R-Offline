using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections;

namespace D2ROffline.Tools
{
    internal class KeyBindingSync : NativeMethods
    {
        const string DIABLO_KEYBINDING_CUSTOM_FILENAME = "Custom.key";
        const string DIABLO_KEYBINDING_FILE_EXTENSION = ".key";
        const string DIABLO_DEFAULT_SAVE_FOLDER = "Diablo II Resurrected Tech Alpha";

        static bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            return StructuralComparisons.StructuralEqualityComparer.Equals(a1, a2);
        }

        public static void SyncKeyBindings(string keyBindingFileName)
        {
            Program.ConsolePrint("Syncing Character Key Bindings...");

            string savedGamesPath = string.Empty;
            string customKeyBindingFileAbsolutePath = string.Empty;
            if (SHGetKnownFolderPath(KnownFolder.SavedGames, 0, IntPtr.Zero, out IntPtr pPath) == 0)
            {
                savedGamesPath = Marshal.PtrToStringUni(pPath);
                Marshal.FreeCoTaskMem(pPath);
            }

            if (string.IsNullOrWhiteSpace(savedGamesPath))
            {
                Program.ConsolePrint("WARNING: Could not find default save folder for Diablo", ConsoleColor.Yellow);
                Program.ConsolePrint("WARNING: Syncing key bindings failed", ConsoleColor.Yellow);
                return;
            }

            savedGamesPath = Path.Combine(savedGamesPath, DIABLO_DEFAULT_SAVE_FOLDER);
            customKeyBindingFileAbsolutePath = Path.Combine(savedGamesPath, DIABLO_KEYBINDING_CUSTOM_FILENAME);

            if (!File.Exists(customKeyBindingFileAbsolutePath))
            {
                Program.ConsolePrint($"WARNING: Could not find {DIABLO_KEYBINDING_CUSTOM_FILENAME} key binding file", ConsoleColor.Yellow);
                Program.ConsolePrint("WARNING: Syncing key bindings failed", ConsoleColor.Yellow);
                return;
            }

            string searchPattern;
            if (string.IsNullOrWhiteSpace(keyBindingFileName) || keyBindingFileName.Equals("*"))
            {
                searchPattern = "*" + DIABLO_KEYBINDING_FILE_EXTENSION;
            }
            else
            {
                searchPattern = keyBindingFileName + DIABLO_KEYBINDING_FILE_EXTENSION;
            }

            string[] KeyBindingFiles = Directory.GetFiles(savedGamesPath, searchPattern);

            if (KeyBindingFiles.Length == 0)
            {
                Program.ConsolePrint($"WARNING: Could not find {keyBindingFileName} key binding file", ConsoleColor.Yellow);
                return;
            }

            foreach (string keyBindingFileAbsolutePath in KeyBindingFiles)
            {
                if (Path.GetFileName(keyBindingFileAbsolutePath) != DIABLO_KEYBINDING_CUSTOM_FILENAME)
                {
                    ProcessKeyBindingSync(keyBindingFileAbsolutePath, customKeyBindingFileAbsolutePath);
                }
            }
        }

        private static void ProcessKeyBindingSync(string keyBindingFileAbsolutePath, string customKeyBindingFileAbsolutePath)
        {
            string keyBindingFileName = Path.GetFileName(keyBindingFileAbsolutePath);
            string customKeyBindingFileName = Path.GetFileName(customKeyBindingFileAbsolutePath);
            byte[] keyBindingFile = File.ReadAllBytes(keyBindingFileAbsolutePath);
            byte[] customKeyBindingFile = File.ReadAllBytes(customKeyBindingFileAbsolutePath);

            if (ByteArrayCompare(keyBindingFile, customKeyBindingFile))
            {
                Program.ConsolePrint($"{keyBindingFileName} already synced, skipping key binding file");
            }
            else
            {
                File.WriteAllBytes(keyBindingFileAbsolutePath + ".backup", keyBindingFile);
                Program.ConsolePrint($"Backup for {keyBindingFileName} created");

                File.WriteAllBytes(keyBindingFileAbsolutePath, customKeyBindingFile);
                Program.ConsolePrint($"{keyBindingFileName} synced with {customKeyBindingFileName} successfully");
            }
        }
    }
}