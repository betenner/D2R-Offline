using System;
using System.IO;
using System.Runtime.InteropServices;

namespace D2ROffline.Tools
{
    internal class SaveFilePatcher : NativeMethods
    {
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

            savedGamesPath = Path.Combine(savedGamesPath, Constants.DIABLO_DEFAULT_SAVE_FOLDER);

            string searchPattern;
            if (string.IsNullOrWhiteSpace(saveFileName) || saveFileName.Equals("*"))
            {
                searchPattern = "*" + Constants.DIABLO_SAVE_FILE_EXTENSION;
            }
            else 
            {
                searchPattern = saveFileName + Constants.DIABLO_SAVE_FILE_EXTENSION;
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
            byte[] rawSaveFile = File.ReadAllBytes(saveFileAbsolutePath);

            File.WriteAllBytes(saveFileAbsolutePath + ".backup", rawSaveFile);
            Program.ConsolePrint($"Backup for {saveFileName} created");

            EnableAllWaypoints(rawSaveFile);
            CompleteGame(rawSaveFile);

            UpdateChecksum(rawSaveFile);

            File.WriteAllBytes(saveFileAbsolutePath, rawSaveFile);
            Program.ConsolePrint($"{saveFileName} patched successfully");
        }

        // credits for below methods : ternence-li & VoidSt4r : https://github.com/ternence-li/Diablo2HeroEditor
        private static void EnableAllWaypoints(byte[] rawSaveFile)
        {
            for (int difficulty = 0; difficulty < 3; difficulty++)
            {
                int firstWpOffset = Constants.WAYPOINTS_SECTION_OFFSET + 
                    (Constants.WAYPOINTS_DATA_OFFSET + difficulty * Constants.WAYPOINTS_DIFFICULTY_OFFSET);
                rawSaveFile[firstWpOffset + 2] = 0xFF;
                rawSaveFile[firstWpOffset + 3] = 0xFF;
                rawSaveFile[firstWpOffset + 4] = 0xFF;
                rawSaveFile[firstWpOffset + 5] = 0xFF;
                rawSaveFile[firstWpOffset + 6] = 0x7F;
            }
        }

        private static void ChangeQuests(Constants.Difficulty difficulty, Constants.Act act, bool complete, byte[] rawSaveFile)
        {
            ChangeQuest(difficulty, act, Constants.Quest.FirstQuest, complete, rawSaveFile);
            ChangeQuest(difficulty, act, Constants.Quest.SecondQuest, complete, rawSaveFile);
            ChangeQuest(difficulty, act, Constants.Quest.ThirdQuest, complete, rawSaveFile);
            if (act != Constants.Act.TheHarrowing)
            {
                ChangeQuest(difficulty, act, Constants.Quest.FourthQuest, complete, rawSaveFile);
                ChangeQuest(difficulty, act, Constants.Quest.FifthQuest, complete, rawSaveFile);
                ChangeQuest(difficulty, act, Constants.Quest.SixthQuest, complete, rawSaveFile);
            }
        }

        private static void ChangeQuests(Constants.Difficulty difficulty, bool complete, byte[] rawSaveFile)
        {
            ChangeQuests(difficulty, Constants.Act.TheSightlessEye, complete, rawSaveFile);
            ChangeQuests(difficulty, Constants.Act.SecretOfTheVizjerei, complete, rawSaveFile);
            ChangeQuests(difficulty, Constants.Act.TheInfernalGate, complete, rawSaveFile);
            ChangeQuests(difficulty, Constants.Act.TheHarrowing, complete, rawSaveFile);
            ChangeQuests(difficulty, Constants.Act.LordOfDestruction, complete, rawSaveFile);
        }
        private static void ChangeQuests(bool complete, byte[] rawSaveFile)
        {
            ChangeQuests(Constants.Difficulty.Normal, complete, rawSaveFile);
            ChangeQuests(Constants.Difficulty.Nightmare, complete, rawSaveFile);
            ChangeQuests(Constants.Difficulty.Hell, complete, rawSaveFile);
        }

        private static void CompleteGame(byte[] rawSaveFile) 
        {
            rawSaveFile[Constants.CHARACTER_PROGRESSION_OFFSET] = Constants.GAME_COMPLETED_ON_HELL;
            ChangeQuests(true, rawSaveFile);
        }

        private static int GetQuestOffset(Constants.Difficulty difficulty, Constants.Act act, Constants.Quest quest)
        {
            int offset = -1;

            if (act != Constants.Act.TheHarrowing || quest < Constants.Quest.FourthQuest)
            {
                offset = 12;                    // 10 bytes for the quest header, 2 bytes for the act introduction

                offset += (int)difficulty * 96; // choose to the right difficulty
                offset += (int)act * 16;        // choose to the right act
                offset += (int)quest * 2;       // choose the right quest

                if (act == Constants.Act.LordOfDestruction)
                {
                    offset += 4;                // there are additional bytes in act 4
                }
            }

            return offset;
        }

        private static void ChangeQuest(Constants.Difficulty difficulty, Constants.Act act, Constants.Quest quest, bool complete, byte[] rawSaveFile)
        {
            int offset = Constants.QUESTS_SECTION_OFFSET + GetQuestOffset(difficulty, act, quest);

            if (offset == -1)
            {
                return;
            }

            if (complete)
            {
                rawSaveFile[offset] = 0x01;     // Quest complete
                rawSaveFile[offset + 1] = 0x10; // Quest log animation viewed

                if (act == Constants.Act.LordOfDestruction && quest == Constants.Quest.ThirdQuest)
                {
                    // Scroll of resist
                    rawSaveFile[offset] += 0xC0;
                }
            }
            else
            {
                rawSaveFile[offset] = 0;
                rawSaveFile[offset + 1] = 0;
            }

            // Allow travel to the next act.
            // For Act4, the diablo quest is quest2
            if (complete && (quest == Constants.Quest.SixthQuest || (act == Constants.Act.TheHarrowing && quest == Constants.Quest.SecondQuest)))
            {
                if (act != Constants.Act.TheHarrowing)
                {
                    rawSaveFile[offset + 2] = 1;
                }
                else
                {
                    rawSaveFile[offset + 4] = 1;
                }
            }
        }

        private static void UpdateChecksum(byte[] fileData)
        {
            if (fileData == null || fileData.Length < Constants.CHECKSUM_OFFSET + 4) return;

            // Clear out the old checksum
            Array.Clear(fileData, Constants.CHECKSUM_OFFSET, 4);

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

            for (int i = Constants.CHECKSUM_OFFSET; i < Constants.CHECKSUM_OFFSET + 4; ++i)
            {
                fileData[i] = (byte)checksum[i - Constants.CHECKSUM_OFFSET];
            }
        }
    }
}
