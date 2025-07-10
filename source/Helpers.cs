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

        public static Dictionary<string, Tuple<int, uint>> GetLevelGemCounts()
        {
            return new Dictionary<string, Tuple<int, uint>>
            {
                {"Sunrise Spring", new Tuple<int, uint>(Memory.ReadInt(Addresses.SunriseSpringGems), Addresses.SunriseSpringGems)},
                {"Sunny Villa", new Tuple<int, uint>(Memory.ReadInt(Addresses.SunnyVillaGems), Addresses.SunnyVillaGems)},
                {"Cloud Spires", new Tuple<int, uint>(Memory.ReadInt(Addresses.CloudSpireLevelGems), Addresses.CloudSpireLevelGems)},
                {"Molten Crater", new Tuple<int, uint>(Memory.ReadInt(Addresses.MoltenCraterGems), Addresses.MoltenCraterGems)},
                {"Seashell Shore", new Tuple<int, uint>(Memory.ReadInt(Addresses.SeashellShoreGems), Addresses.SeashellShoreGems)},
                {"Mushroom Speedway", new Tuple<int, uint>(Memory.ReadInt(Addresses.MushroomSpeedwayGems), Addresses.MushroomSpeedwayGems)},
                {"Sheila's Alp", new Tuple<int, uint>(Memory.ReadInt(Addresses.SheilaAlpGems), Addresses.SheilaAlpGems)},
                {"Buzz's Dungeon", new Tuple<int, uint>(Memory.ReadInt(Addresses.BuzzDungeonGems), Addresses.BuzzDungeonGems)},
                {"Crawdad Farm", new Tuple<int, uint>(Memory.ReadInt(Addresses.CrawdadFarmGems), Addresses.CrawdadFarmGems)},
                {"Midday Garden", new Tuple<int, uint>(Memory.ReadInt(Addresses.MiddayGardenGems), Addresses.MiddayGardenGems)},
                {"Icy Peak", new Tuple<int, uint>(Memory.ReadInt(Addresses.IcyPeakGems), Addresses.IcyPeakGems)},
                {"Enchanted Towers", new Tuple<int, uint>(Memory.ReadInt(Addresses.EnchantedTowersGems), Addresses.EnchantedTowersGems)},
                {"Spooky Swamp", new Tuple<int, uint>(Memory.ReadInt(Addresses.SpookySwampGems), Addresses.SpookySwampGems)},
                {"Bamboo Terrace", new Tuple<int, uint>(Memory.ReadInt(Addresses.BambooTerraceGems), Addresses.BambooTerraceGems)},
                {"Country Speedway", new Tuple<int, uint>(Memory.ReadInt(Addresses.CountrySpeedwayGems), Addresses.CountrySpeedwayGems)},
                {"Sgt. Byrd's Base", new Tuple<int, uint>(Memory.ReadInt(Addresses.SgtByrdBaseGems), Addresses.SgtByrdBaseGems)},
                {"Spike's Arena", new Tuple<int, uint>(Memory.ReadInt(Addresses.SpikesArenaGems), Addresses.SpikesArenaGems)},
                {"Spider Town", new Tuple<int, uint>(Memory.ReadInt(Addresses.SpiderTownGems), Addresses.SpiderTownGems)},
                {"Evening Lake", new Tuple<int, uint>(Memory.ReadInt(Addresses.EveningLakeGems), Addresses.EveningLakeGems)},
                {"Frozen Altars", new Tuple<int, uint>(Memory.ReadInt(Addresses.FrozenAltarsGems), Addresses.FrozenAltarsGems)},
                {"Lost Fleet", new Tuple<int, uint>(Memory.ReadInt(Addresses.LostFleetGems), Addresses.LostFleetGems)},
                {"Fireworks Factory", new Tuple<int, uint>(Memory.ReadInt(Addresses.FireworksFactoryGems), Addresses.FireworksFactoryGems)},
                {"Charmed Ridge", new Tuple<int, uint>(Memory.ReadInt(Addresses.CharmedRidgeGems), Addresses.CharmedRidgeGems)},
                {"Honey Speedway", new Tuple<int, uint>(Memory.ReadInt(Addresses.HoneySpeedwayGems), Addresses.HoneySpeedwayGems)},
                {"Bentley's Outpost", new Tuple<int, uint>(Memory.ReadInt(Addresses.BentleyOutpostGems), Addresses.BentleyOutpostGems)},
                {"Scorch's Pit", new Tuple<int, uint>(Memory.ReadInt(Addresses.ScorchPitGems), Addresses.ScorchPitGems)},
                {"Starfish Reef", new Tuple<int, uint>(Memory.ReadInt(Addresses.StarfishReefGems), Addresses.StarfishReefGems)},
                {"Midnight Mountain", new Tuple<int, uint>(Memory.ReadInt(Addresses.MidnightMountainGems), Addresses.MidnightMountainGems)},
                {"Crystal Islands", new Tuple<int, uint>(Memory.ReadInt(Addresses.CrystalIslandsGems), Addresses.CrystalIslandsGems)},
                {"Desert Ruins", new Tuple<int, uint>(Memory.ReadInt(Addresses.DesertRuinsGems), Addresses.DesertRuinsGems)},
                {"Haunted Tomb", new Tuple<int, uint>(Memory.ReadInt(Addresses.HauntedTombGems), Addresses.HauntedTombGems)},
                {"Dino Mines", new Tuple<int, uint>(Memory.ReadInt(Addresses.DinoMinesGems), Addresses.DinoMinesGems)},
                {"Harbor Speedway", new Tuple<int, uint>(Memory.ReadInt(Addresses.HarborSpeedwayGems), Addresses.HarborSpeedwayGems)},
                {"Agent 9's Lab", new Tuple<int, uint>(Memory.ReadInt(Addresses.AgentNineLabGems), Addresses.AgentNineLabGems)},
                {"Sorcerer's Lair", new Tuple<int, uint>(Memory.ReadInt(Addresses.SorcererLairGems), Addresses.SorcererLairGems)},
                {"Bugbot Factory", new Tuple<int, uint>(Memory.ReadInt(Addresses.BugbotFactoryGems), Addresses.BugbotFactoryGems)},
                {"Super Bonus Round", new Tuple<int, uint>(Memory.ReadInt(Addresses.SuperBonusRoundGems), Addresses.SuperBonusRoundGems)}
            };
        }
        public static bool IsInDemoMode()
        {
            return Memory.ReadByte(Addresses.IsInDemoMode) == 1;
        }
        public static bool IsInGame()
        {
            // TODO: Handle emulator reset
            return !IsInDemoMode() && GetGameStatus() != GameStatus.TitleScreen;
        }
        public static GameStatus GetGameStatus()
        {
            var status = Memory.ReadByte(Addresses.GameStatus);
            var result = (GameStatus)status;
            return result;
        }
        public static List<Location> BuildLocationList(bool includeGems = true, bool includeSkillPoints = true)
        {
            int baseId = 1230000;
            int levelOffset = 1000;
            int processedEggs = 0;
            int processedSkillPoints = 0;
            List<Location> locations = new List<Location>();
            var currentAddress = Addresses.EggStartAddress;
            var currentSkillPointAddress = Addresses.SkillPointAddress;
            List<LevelData> levels = GetLevelData();
            var totalEggCount = levels.Select(x => x.EggCount).Sum();
            var homeworldList = levels.Where(x => x.IsHomeworld).ToList();
            var bossList = levels.Where(x => x.IsBoss).ToList();
            var gemDict = GetLevelGemCounts();
            foreach (var level in levels)
            {
                if (!level.IsHomeworld && !level.IsBoss)
                {
                    // Level Completed (first egg)
                    Location location = new Location()
                    {
                        Name = level.Name + " Complete",
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
                if (level.Name.Equals("Midnight Mountain"))
                {
                    // Moneybags Chase Completed
                    Location location = new Location()
                    {
                        Name = "Moneybags Chase Complete",
                        Id = baseId + (levelOffset * (level.LevelId - 1)) + level.EggCount,
                        AddressBit = 5,
                        CheckType = LocationCheckType.Bit,
                        Address = currentAddress,
                        Category = "Event"
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
                if (includeGems && !level.IsBoss)
                {
                    var gemCheckOffset = level.IsHomeworld && !level.Name.Equals("Midnight Mountain") ? 0 : 1;
                    Location gemLocation = new Location()
                    {
                        Name = $"{level.Name}: All Gems",
                        Id = baseId + (levelOffset * (level.LevelId - 1)) + level.EggCount + gemCheckOffset,
                        Address = gemDict[level.Name].Item2,
                        CheckType = LocationCheckType.Int,
                        CompareType = LocationCheckCompareType.GreaterThan,
                        CheckValue = $"{level.GemCount - 1}",
                        Category = "Gem"
                    };
                    locations.Add(gemLocation);
                }
                if (includeSkillPoints && level.SkillPoints.Length > 0)
                {
                    for (int i = 0; i < level.SkillPoints.Length; i++)
                    {
                        string skillPointName = level.SkillPoints[i];
                        Location skillLocation = new Location()
                        {
                            Name = $"{level.Name}: {skillPointName} (Skill Point)",
                            // All skill points are in levels with gems and a completion "location".
                            Id = baseId + (levelOffset * (level.LevelId - 1)) + level.EggCount + 2 + i,
                            Address = currentSkillPointAddress,
                            CheckType = LocationCheckType.Byte,
                            CompareType = LocationCheckCompareType.GreaterThan,
                            // Active skill points appear to be set to 3
                            CheckValue = "2",
                            Category = "Skill Point"
                        };
                        locations.Add(skillLocation);
                        currentSkillPointAddress++;
                    }
                }
                currentAddress++;
            }
            return locations;
        }

        private static List<LevelData> GetLevelData()
        {
            List<LevelData> levels = new List<LevelData>()
            {
                new LevelData("Sunrise Spring", 1, 5, true, false, 400, []),
                new LevelData("Sunny Villa", 2, 6, false, false, 400, ["Flame all trees", "Skateboard course record I"]),
                new LevelData("Cloud Spires", 3, 6, false, false, 400, []),
                new LevelData("Molten Crater", 4, 6, false, false, 400, ["Assemble tiki heads", "Supercharge the wall"]),
                new LevelData("Seashell Shore", 5, 6, false, false, 400, ["Catch the funky chicken"]),
                new LevelData("Mushroom Speedway", 6, 3, false, false, 400, []),
                new LevelData("Sheila's Alp", 7, 3, false, false, 400, []),
                new LevelData("Buzz", 8, 1, false, true, 0, []),
                new LevelData("Crawdad Farm", 9, 1, false, false, 200, []),
                new LevelData("Midday Garden", 10, 5, true, false, 400, []),
                new LevelData("Icy Peak", 11, 6, false, false, 500, ["Glide to pedestal"]),
                new LevelData("Enchanted Towers", 12, 6, false, false, 500, ["Skateboard course record II"]),
                new LevelData("Spooky Swamp", 13, 6, false, false, 500, ["Destroy all piranha signs"]),
                new LevelData("Bamboo Terrace", 14, 6, false, false, 500, []),
                new LevelData("Country Speedway", 15, 3, false, false, 400, []),
                new LevelData("Sgt. Byrd's Base", 16, 3, false, false, 500, ["Bomb the gophers"]),
                new LevelData("Spike", 17, 1, false, true, 0, []),
                new LevelData("Spider Town", 18, 1, false, false, 200, []),
                new LevelData("Evening Lake", 19, 5, true, false, 400, []),
                new LevelData("Frozen Altars", 20, 6, false, false, 600, ["Beat yeti in two rounds"]),
                new LevelData("Lost Fleet", 21, 6, false, false, 600, ["Skateboard record time"]),
                new LevelData("Fireworks Factory", 22, 6, false, false, 600, ["Find Agent 9's powerup"]),
                new LevelData("Charmed Ridge", 23, 6, false, false, 600, ["The Impossible Tower", "Shoot the temple windows"]),
                new LevelData("Honey Speedway", 24, 3, false, false, 400, []),
                new LevelData("Bentley's Outpost", 25, 3, false, false, 600, ["Push box off the cliff"]),
                new LevelData("Scorch", 26, 1, false, true, 0, []),
                new LevelData("Starfish Reef", 27, 1, false, false, 200, []),
                new LevelData("Midnight Mountain", 28, 6, true, false, 400, []),
                new LevelData("Crystal Islands", 29, 6, false, false, 700, []),
                new LevelData("Desert Ruins", 30, 6, false, false, 700, ["Destroy all seaweed"]),
                new LevelData("Haunted Tomb", 31, 6, false, false, 700, ["Swim into the dark hole"]),
                new LevelData("Dino Mines", 32, 6, false, false, 700, ["Hit all the seahorses", "Hit the secret dino"]),
                new LevelData("Harbor Speedway", 33, 3, false, false, 400, []),
                new LevelData("Agent 9's Lab", 34, 3, false, false, 700, ["Blow up all palm trees"]),
                new LevelData("Sorceress", 35, 1, false, true, 0, []),
                new LevelData("Bugbot Factory", 36, 1, false, false, 200, []),
                new LevelData("Super Bonus Round", 37, 1, false, false, 5000, []),
            };
            return levels;
        }
    }
}
