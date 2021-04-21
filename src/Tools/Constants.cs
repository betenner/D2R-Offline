namespace D2ROffline.Tools
{
    internal class Constants
    {
        public const int GAME_COMPLETED_ON_HELL = 0x0F;

        public const int CHARACTER_PROGRESSION_OFFSET = 0x25;
        public const int CHECKSUM_OFFSET = 0x0C;

        public const int QUESTS_SECTION_OFFSET = 0x014F;

        public const int WAYPOINTS_SECTION_OFFSET = 0x0279;
        public const int WAYPOINTS_DATA_OFFSET = 0x08;
        public const int WAYPOINTS_DIFFICULTY_OFFSET = 0x18;

        public const string DIABLO_SAVE_FILE_EXTENSION = ".d2s";
        public const string DIABLO_DEFAULT_SAVE_FOLDER = "Diablo II Resurrected Tech Alpha";

        public enum Difficulty
        {
            Normal,
            Nightmare,
            Hell
        }

        public enum Act
        {
            TheSightlessEye,
            SecretOfTheVizjerei,
            TheInfernalGate,
            TheHarrowing,
            LordOfDestruction
        };

        public enum Quest
        {
            FirstQuest,
            SecondQuest,
            ThirdQuest,
            FourthQuest,
            FifthQuest,
            SixthQuest
        };
    }
}
