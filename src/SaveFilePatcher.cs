using System;
using System.IO;
using System.Runtime.InteropServices;

namespace D2ROffline
{
    internal class SaveFilePatcher : NativeMethods
    {
        const int CHARACTER_PROGRESSION_OFFSET = 0x25;
        const int GAME_FINISHED_ON_HELL = 0x0F;
        const int CHECKSUM_OFFSET = 0x0C;

        const string DIABLO_SAVE_FILE_EXTENSION = ".d2s";
        const string DIABLO_DEFAULT_SAVE_FOLDER = "Diablo II Resurrected Tech Alpha";

        public static void PatchSaveFiles(string saveFileName)
        {
            Program.ConsolePrint("Patching save files...");

            string savedGamesPath = string.Empty;
            if (SHGetKnownFolderPath(KnownFolder.SavedGames, 0, IntPtr.Zero, out IntPtr pPath) == 0)
            {
                savedGamesPath = Marshal.PtrToStringUni(pPath);
                Marshal.FreeCoTaskMem(pPath);
            }

            if (string.IsNullOrWhiteSpace(savedGamesPath))
            {
                Program.ConsolePrint("WARNING: Could not find default save folder for Diablo", ConsoleColor.Yellow);
                Program.ConsolePrint("WARNING: Patching save files failed", ConsoleColor.Yellow);
                return;
            }

            savedGamesPath = Path.Combine(savedGamesPath, DIABLO_DEFAULT_SAVE_FOLDER);

            string searchPattern;
            if (string.IsNullOrWhiteSpace(saveFileName) || saveFileName.Equals("*"))
            {
                searchPattern = "*" + DIABLO_SAVE_FILE_EXTENSION;
            }
            else 
            {
                searchPattern = saveFileName + DIABLO_SAVE_FILE_EXTENSION;
            }

            string[] saveFiles = Directory.GetFiles(savedGamesPath, searchPattern);

            if (saveFiles.Length == 0)
            {
                Program.ConsolePrint($"WARNING: Could not find {saveFileName} save file", ConsoleColor.Yellow);
                return;
            }

            foreach (string saveFileAbsolutePath in saveFiles)
            {
                ProcessSaveFile(saveFileAbsolutePath);
            }
        }

        private static void ProcessSaveFile(string saveFileAbsolutePath)
        {
            string saveFileName = Path.GetFileName(saveFileAbsolutePath);
            byte[] saveFile = File.ReadAllBytes(saveFileAbsolutePath);

            if (saveFile[CHARACTER_PROGRESSION_OFFSET] == GAME_FINISHED_ON_HELL)
            {
                Program.ConsolePrint($"{saveFileName} already patched, skipping save file");
            }
            else
            {
                File.WriteAllBytes(saveFileAbsolutePath + ".backup", saveFile);
                Program.ConsolePrint($"Backup for {saveFileName} created");

                saveFile[CHARACTER_PROGRESSION_OFFSET] = GAME_FINISHED_ON_HELL;
                UpdateChecksum(saveFile);
                File.WriteAllBytes(saveFileAbsolutePath, saveFile);
                Program.ConsolePrint($"{saveFileName} patched successfully");
            }
        }

        // credits ternence-li & VoidSt4r : https://github.com/ternence-li/Diablo2HeroEditor/blob/18b7633c437c8da6a1f8b5797b126458489e8bc5/Diablo2FileFormat/Checksum.cs
        private static void UpdateChecksum(byte[] fileData)
        {
            if (fileData == null || fileData.Length < CHECKSUM_OFFSET + 4) return;

            // Clear out the old checksum
            Array.Clear(fileData, CHECKSUM_OFFSET, 4);

            int[] checksum = new int[4];
            bool carry = false;

            for (int i = 0; i < fileData.Length; ++i)
            {
                int temp = fileData[i] + (carry ? 1 : 0);

                checksum[0] = checksum[0] * 2 + temp;
                checksum[1] *= 2;

                if (checksum[0] > 255)
                {
                    checksum[1] += (checksum[0] - checksum[0] % 256) / 256;
                    checksum[0] %= 256;
                }

                checksum[2] *= 2;

                if (checksum[1] > 255)
                {
                    checksum[2] += (checksum[1] - checksum[1] % 256) / 256;
                    checksum[1] %= 256;
                }

                checksum[3] *= 2;

                if (checksum[2] > 255)
                {
                    checksum[3] += (checksum[2] - checksum[2] % 256) / 256;
                    checksum[2] %= 256;
                }

                if (checksum[3] > 255)
                {
                    checksum[3] %= 256;
                }

                carry = (checksum[3] & 128) != 0;
            }

            for (int i = CHECKSUM_OFFSET; i < CHECKSUM_OFFSET + 4; ++i)
            {
                fileData[i] = (byte)checksum[i - CHECKSUM_OFFSET];
            }
        }
    }
}
