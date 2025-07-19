namespace S3AP.Models
{
    public class Enums
    {
        public enum GameStatus
        {
            InGame = 0,
            Talking = 1,
            Spawning = 3,
            Paused = 4,
            Loading = 5,
            Cutscene = 6,
            LoadingWorld = 7,
            TitleScreen = 11,
            LoadingHomeworld = 12,
            GettingEgg = 15,
            StartingGame = 16

        }

        public enum CompletionGoal
        {
            Sorceress1 = 0,
            EggForSale = 1,
            Sorceress2 = 2,
            AllSkillPoints = 3,
            Epilogue = 4
        }

        public enum SpyroColor : short
        {
            SpyroColorDefault = 0,
            SpyroColorRed = 1,
            SpyroColorBlue = 2,
            SpyroColorPink = 3,
            SpyroColorGreen = 4,
            SpyroColorYellow = 5,
            SpyroColorBlack = 6
        }

        public enum MoneybagsOptions
        {
            Vanilla = 0,
            Companionsanity = 1,
            Moneybagssanity = 3
        }

        public enum ProgressiveSparxHealthOptions
        {
            Off = 0,
            Blue = 1,
            Green = 2,
            Sparxless = 3,
            TrueSparxless = 4
        }

        public enum ImportantLocationIDs : int
        {
            SunnyEndOfLevelEgg = 1231000,
            SorceressEgg = 1264000,
            EggForSale = 1257005,
            SuperBonusRoundEgg = 1266000
        }
    }
}
