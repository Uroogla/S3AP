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

        public enum LevelInGameIDs : byte
        {
            SunriseSpring = 10,
            SunnyVilla = 11,
            CloudSpires = 12,
            MoltenCrater = 13,
            SeashellShore = 14,
            MushroomSpeedway = 15,
            SheilasAlp = 16,
            BuzzsDungeon = 17,
            CrawdadFarm = 18,
            // 19 is skipped
            MiddayGardens = 20,
            IcyPeak = 21,
            EnchantedTowers = 22,
            SpookySwamp = 23,
            BambooTerrace = 24,
            CountrySpeedway = 25,
            SgtByrdsBase = 26,
            SpikesArena = 27,
            SpiderTown = 28,
            // 29 is skipped
            EveningLake = 30,
            FrozenAltars = 31,
            LostFleet = 32,
            FireworksFactory = 33,
            CharmedRidge = 34,
            HoneySpeedway = 35,
            BentleysOutpost = 36,
            ScorchsPit = 37,
            StarfishReef = 38,
            // 39 is skipped
            MidnightMountain = 40,
            CrystalIslands = 41,
            DesertRuins = 42,
            HauntedTomb = 43,
            DinoMines = 44,
            HarborSpeedway = 45,
            Agent9sLab = 46,
            SorceressLair = 47,
            BugbotFactory = 48,
            // 49 is skipped
            SuperBonusRound = 50
        }
    }
}
