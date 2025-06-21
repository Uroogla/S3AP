using Archipelago.Core.Models;
using Archipelago.Core.Util;
using Newtonsoft.Json;
using S3AP.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static S3AP.Models.Enums;
using Location = Archipelago.Core.Models.Location;
namespace S3AP
{
    public class Helpers
    {
        public static string OpenEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string jsonFile = reader.ReadToEnd();
                return jsonFile;
            }
        }
        public static ulong GetDuckstationOffset()
        {
            var baseAddress = Memory.GetBaseAddress("duckstation-qt-x64-ReleaseLTCG");
            var offset = Memory.ReadULong(baseAddress + 0x008C4FA8);
            return offset;
        }
        public static Dictionary<string, int> GetLevelGemCounts()
        {
            return new Dictionary<string, int>
            {
                {"Sunrise Spring", Memory.ReadInt(Addresses.SunriseSpringGems)},
                {"Sunny Villa", Memory.ReadInt(Addresses.SunnyVillaGems)},
                {"Cloud Spire", Memory.ReadInt(Addresses.CloudSpireLevelGems)},
                {"Molten Crater", Memory.ReadInt(Addresses.MoltenCraterGems)},
                {"Seashell Shore", Memory.ReadInt(Addresses.SeashellShoreGems)},
                {"Mushroom Speedway", Memory.ReadInt(Addresses.MushroomSpeedwayGems)},
                {"Sheila's Alp", Memory.ReadInt(Addresses.SheilaAlpGems)},
                {"Buzz's Dungeon", Memory.ReadInt(Addresses.BuzzDungeonGems)},
                {"Crawdad Farm", Memory.ReadInt(Addresses.CrawdadFarmGems)},
                {"Midday Garden", Memory.ReadInt(Addresses.MiddayGardenGems)},
                {"Icy Peak", Memory.ReadInt(Addresses.IcyPeakGems)},
                {"Enchanted Towers", Memory.ReadInt(Addresses.EnchantedTowersGems)},
                {"Spooky Swamp", Memory.ReadInt(Addresses.SpookySwampGems)},
                {"Bamboo Terrace", Memory.ReadInt(Addresses.BambooTerraceGems)},
                {"Country Speedway", Memory.ReadInt(Addresses.CountrySpeedwayGems)},
                {"Sgt. Byrd's Base", Memory.ReadInt(Addresses.SgtByrdBaseGems)},
                {"Spike's Arena", Memory.ReadInt(Addresses.SpikesArenaGems)},
                {"Spider Town", Memory.ReadInt(Addresses.SpiderTownGems)},
                {"Evening Lake", Memory.ReadInt(Addresses.EveningLakeGems)},
                {"Frozen Altars", Memory.ReadInt(Addresses.FrozenAltarsGems)},
                {"Lost Fleet", Memory.ReadInt(Addresses.LostFleetGems)},
                {"Fireworks Factory", Memory.ReadInt(Addresses.FireworksFactoryGems)},
                {"Charmed Ridge", Memory.ReadInt(Addresses.CharmedRidgeGems)},
                {"Honey Speedway", Memory.ReadInt(Addresses.HoneySpeedwayGems)},
                {"Bentley's Outpost", Memory.ReadInt(Addresses.BentleyOutpostGems)},
                {"Scorch's Pit", Memory.ReadInt(Addresses.ScorchPitGems)},
                {"Starfish Reef", Memory.ReadInt(Addresses.StarfishReefGems)},
                {"Midnight Mountain", Memory.ReadInt(Addresses.MidnightMountainGems)},
                {"Crystal Islands", Memory.ReadInt(Addresses.CrystalIslandsGems)},
                {"Desert Ruins", Memory.ReadInt(Addresses.DesertRuinsGems)},
                {"Haunted Tomb", Memory.ReadInt(Addresses.HauntedTombGems)},
                {"Dino Mines", Memory.ReadInt(Addresses.DinoMinesGems)},
                {"Harbor Speedway", Memory.ReadInt(Addresses.HarborSpeedwayGems)},
                {"Agent 9's Lab", Memory.ReadInt(Addresses.AgentNineLabGems)},
                {"Sorcerer's Lair", Memory.ReadInt(Addresses.SorcererLairGems)},
                {"Bugbot Factory", Memory.ReadInt(Addresses.BugbotFactoryGems)},
                {"Super Bonus Round", Memory.ReadInt(Addresses.SuperBonusRoundGems)}
            };
        }
        public static bool IsInDemoMode()
        {
            return Memory.ReadByte(Addresses.IsInDemoMode) == 1;
        }
        public static bool IsInGame()
        {
            return !IsInDemoMode() && GetGameStatus() != GameStatus.TitleScreen;
        }
        public static GameStatus GetGameStatus()
        {
            var status = Memory.ReadByte(Addresses.GameStatus);
            var result = (GameStatus)status;
            return result;
        }
        public static List<Location> BuildLocationList()
        {
            int baseId = 1230000;
            int levelOffset = 1000;
            int processedEggs = 0;
            List<Location> locations = new List<Location>();
            var currentAddress = Addresses.EggStartAddress;
            List<LevelData> levels = GetLevelData();
            var totalEggCount = levels.Select(x => x.EggCount).Sum();
            var homeworldList = levels.Where(x => x.IsHomeworld).ToList();
            var bossList = levels.Where(x => x.IsBoss).ToList();
            foreach (var level in levels)
            {
                Log.Debug($"Loading eggs for level {level.LevelId}: {level.Name}. {level.EggCount} eggs found");
                if (!level.IsHomeworld && !level.IsBoss)
                {
                    // Level Completed (first egg)
                    Location location = new Location()
                    {
                        Name = level.Name + " Completed",
                        Id = baseId + (levelOffset * (level.LevelId - 1)) + level.EggCount,
                        AddressBit = 0,
                        CheckType = LocationCheckType.Bit,
                        Address = currentAddress,
                        Category = "Event"
                    };
                    locations.Add(location);
                }
                if (!level.IsHomeworld && level.IsBoss)
                {
                    // Boss Defeated (first egg)
                    Location location = new Location()
                    {
                        Name = level.Name + " Defeated",
                        Id = baseId + (levelOffset * (level.LevelId - 1)) + level.EggCount,
                        AddressBit = 0,
                        CheckType = LocationCheckType.Bit,
                        Address = currentAddress,
                        Category = "Boss"
                    };
                    locations.Add(location);
                }
                for (int i = 0; i < level.EggCount; i++)
                {
                    // Egg collected
                    Location location = new Location()
                    {
                        Name = $"Egg {processedEggs + 1}",
                        Id = baseId + (levelOffset * (level.LevelId - 1)) + i,
                        AddressBit = i,
                        CheckType = LocationCheckType.Bit,
                        Address = currentAddress,
                        Category = "Egg"
                    };
                    locations.Add(location);
                    processedEggs++;
                }
                currentAddress++;
            }
            Log.Debug($"{totalEggCount} eggs loaded");
            return locations;
        }

        private static List<LevelData> GetLevelData()
        {
            List<LevelData> levels = new List<LevelData>()
            {
                new LevelData("Sunrise Springs", 1, 5, true, false),
                new LevelData("Sunny Villa", 2, 6, false, false),
                new LevelData("Cloud Spires", 3, 6, false, false),
                new LevelData("Molten Crater", 4, 6, false, false),
                new LevelData("Seashell Shore", 5, 6, false, false),
                new LevelData("Mushroom Speedway", 6, 3, false, false),
                new LevelData("Shiela's Alp", 7, 3, false, false),
                new LevelData("Buzz", 8, 1, false, true),
                new LevelData("Crawdad Farm", 9, 1, false, false),
                new LevelData("Midday Garden", 10, 5, true, false),
                new LevelData("Icy Peak", 11, 6, false, false),
                new LevelData("Enchanted Towers", 12, 6, false, false),
                new LevelData("Spooky Swamp", 13, 6, false, false),
                new LevelData("Bamboo Terrace", 14, 6, false, false),
                new LevelData("Country Speedway", 15, 3, false, false),
                new LevelData("Sgt Byrd's Base", 16, 3, false, false),
                new LevelData("Spike", 17, 1, false, true),
                new LevelData("Spider Town", 18, 1, false, false),
                new LevelData("Evening Lake", 19, 5, true, false),
                new LevelData("Frozen Altars", 20, 6, false, false),
                new LevelData("Lost Fleet", 21, 6, false, false),
                new LevelData("Fireworks Factory", 22, 6, false, false),
                new LevelData("Charmed Ridge", 23, 6, false, false),
                new LevelData("Honey Speedway", 24, 3, false, false),
                new LevelData("Bentley's Outpost", 25, 3, false, false),
                new LevelData("Scorch", 26, 1, false, true),
                new LevelData("Starfish Reef", 27, 1, false, false),
                new LevelData("Midnight Mountain", 28, 6, true, false),
                new LevelData("Crystal Islands", 29, 6, false, false),
                new LevelData("Desert Ruins", 30, 6, false, false),
                new LevelData("Haunted Tomb", 31, 6, false, false),
                new LevelData("Dino Mines", 32, 6, false, false),
                new LevelData("Harbor Speedway", 33, 3, false, false),
                new LevelData("Agent 9's Lab", 34, 3, false, false),
                new LevelData("Sorceress", 35, 1, false, true),
                new LevelData("Bugbot Factory", 36, 1, false, false),
                new LevelData("Super Bonus Round", 37, 1, false, false),
            };
            return levels;
        }
    }
}
