using Archipelago.Core.Models;
using Archipelago.Core.Util;
using S3AP.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static S3AP.Models.Enums;
using Location = Archipelago.Core.Models.Location;
namespace S3AP
{
    public class Helpers
    {
        private static GameStatus lastNonZeroStatus = GameStatus.Spawning;
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
            var status = GetGameStatus();
            return !IsInDemoMode() &&
                status != GameStatus.TitleScreen &&
                status != GameStatus.Loading && // Handle loading into and out of demo mode.
                Memory.ReadInt(Addresses.ResetCheckAddress) != 0 && // Handle status being 0 on console reset.
                lastNonZeroStatus != GameStatus.StartingGame; // Handle status swapping from 16 to 0 temporarily on game load
        }
        public static GameStatus GetGameStatus()
        {
            var status = Memory.ReadByte(Addresses.GameStatus);
            var result = (GameStatus)status;
            if (result != GameStatus.InGame)
            {
                lastNonZeroStatus = result;
            }
            return result;
        }
        public static List<ILocation> BuildLocationList(bool includeGems = true, bool includeSkillPoints = true)
        {
            int baseId = 1230000;
            int levelOffset = 1000;
            int processedEggs = 0;
            int processedSkillPoints = 0;
            List<ILocation> locations = new List<ILocation>();
            var currentAddress = Addresses.EggStartAddress;
            var currentSkillPointAddress = Addresses.SkillPointAddress;
            var currentSkillPointGoalAddress = Addresses.SkillPointAddress;
            List<LevelData> levels = GetLevelData();
            var totalEggCount = levels.Select(x => x.EggCount).Sum();
            var homeworldList = levels.Where(x => x.IsHomeworld).ToList();
            var bossList = levels.Where(x => x.IsBoss).ToList();
            var gemDict = GetLevelGemCounts();
            var hintID = 1;
            var lifeBottleID = 1;
            foreach (var level in levels)
            {
                int locationOffset = 0;
                for (int i = 0; i < level.EggCount; i++)
                {
                    // Egg collected
                    Location location = new Location()
                    {
                        Name = $"Egg {processedEggs + 1}",
                        Id = baseId + (levelOffset * (level.LevelId - 1)) + locationOffset,
                        AddressBit = i,
                        CheckType = LocationCheckType.Bit,
                        Address = currentAddress,
                        Category = "Egg"
                    };
                    locations.Add(location);
                    processedEggs++;
                    locationOffset++;
                }
                if (!level.IsHomeworld && !level.IsBoss && !level.Name.Contains("Speedway"))
                {
                    // Level Completed (first egg)
                    Location location = new Location()
                    {
                        Name = level.Name + " Complete",
                        Id = baseId + (levelOffset * (level.LevelId - 1)) + locationOffset,
                        AddressBit = 0,
                        CheckType = LocationCheckType.Bit,
                        Address = currentAddress,
                        Category = "Event"
                    };
                    locations.Add(location);
                    locationOffset++;
                }
                else if (!level.IsHomeworld && level.IsBoss)
                {
                    // Boss Defeated (first egg)
                    Location location = new Location()
                    {
                        Name = level.Name + " Defeated",
                        Id = baseId + (levelOffset * (level.LevelId - 1)) + locationOffset,
                        AddressBit = 0,
                        CheckType = LocationCheckType.Bit,
                        Address = currentAddress,
                        Category = "Boss"
                    };
                    locations.Add(location);
                    locationOffset++;
                }
                else if (level.Name.Equals("Midnight Mountain"))
                {
                    // Moneybags Chase Completed
                    Location location = new Location()
                    {
                        Name = "Moneybags Chase Complete",
                        Id = baseId + (levelOffset * (level.LevelId - 1)) + locationOffset,
                        AddressBit = 5,
                        CheckType = LocationCheckType.Bit,
                        Address = currentAddress,
                        Category = "Event"
                    };
                    locations.Add(location);
                    locationOffset++;
                }
                if (!level.IsBoss)
                {
                    Location gemLocation = new Location()
                    {
                        Name = $"{level.Name}: All Gems",
                        Id = baseId + (levelOffset * (level.LevelId - 1)) + locationOffset,
                        Address = gemDict[level.Name].Item2,
                        CheckType = LocationCheckType.Int,
                        CompareType = LocationCheckCompareType.GreaterThan,
                        CheckValue = $"{level.GemCount - 1}",
                        Category = "Gem"
                    };
                    locations.Add(gemLocation);
                    locationOffset++;
                }
                for (int i = 0; i < level.SkillPoints.Length; i++)
                {
                    string skillPointName = level.SkillPoints[i];
                    Location skillLocation = new Location()
                    {
                        Name = $"{level.Name}: {skillPointName} (Skill Point)",
                        Id = baseId + (levelOffset * (level.LevelId - 1)) + locationOffset,
                        Address = currentSkillPointAddress,
                        CheckType = LocationCheckType.Byte,
                        CompareType = LocationCheckCompareType.GreaterThan,
                        // Active skill points appear to be set to 3
                        CheckValue = "2",
                        Category = "Skill Point"
                    };
                    locations.Add(skillLocation);
                    currentSkillPointAddress++;
                    locationOffset++;
                }
                for (int i = 0; i < level.SkillPoints.Length; i++)
                {
                    string skillPointName = level.SkillPoints[i];
                    Location skillLocation = new Location()
                    {
                        Name = $"{level.Name}: {skillPointName} (Goal)",
                        Id = baseId + (levelOffset * (level.LevelId - 1)) + locationOffset,
                        Address = currentSkillPointGoalAddress,
                        CheckType = LocationCheckType.Byte,
                        CompareType = LocationCheckCompareType.GreaterThan,
                        // Active skill points appear to be set to 3
                        CheckValue = "2",
                        Category = "Skill Point"
                    };
                    locations.Add(skillLocation);
                    currentSkillPointGoalAddress++;
                    locationOffset++;
                }
                if (!level.IsBoss)
                {
                    Location gem25Location = new Location()
                    {
                        Name = $"{level.Name}: 25% Gems",
                        Id = baseId + (levelOffset * (level.LevelId - 1)) + locationOffset,
                        Address = gemDict[level.Name].Item2,
                        CheckType = LocationCheckType.Int,
                        CompareType = LocationCheckCompareType.GreaterThan,
                        CheckValue = $"{level.GemCount / 4 - 1}",
                        Category = "Gem 25%"
                    };
                    locations.Add(gem25Location);
                    locationOffset++;

                    Location gem50Location = new Location()
                    {
                        Name = $"{level.Name}: 50% Gems",
                        Id = baseId + (levelOffset * (level.LevelId - 1)) + locationOffset,
                        Address = gemDict[level.Name].Item2,
                        CheckType = LocationCheckType.Int,
                        CompareType = LocationCheckCompareType.GreaterThan,
                        CheckValue = $"{level.GemCount / 2 - 1}",
                        Category = "Gem 50%"
                    };
                    locations.Add(gem50Location);
                    locationOffset++;

                    Location gem75Location = new Location()
                    {
                        Name = $"{level.Name}: 75% Gems",
                        Id = baseId + (levelOffset * (level.LevelId - 1)) + locationOffset,
                        Address = gemDict[level.Name].Item2,
                        CheckType = LocationCheckType.Int,
                        CompareType = LocationCheckCompareType.GreaterThan,
                        CheckValue = $"{3 * level.GemCount / 4 - 1}",
                        Category = "Gem 75%"
                    };
                    locations.Add(gem75Location);
                    locationOffset++;
                }
                for (int i = 0; i < level.ZoeHintAddresses.Length; i++)
                {
                    // The current level variable starts at 10 and skips 19, 29, 39, and 49.
                    // Convert our internal IDs to this system.
                    int currentLevelID = 9 + level.LevelId + ((level.LevelId - 1) / 9);
                    List<ILocation> conditionsList = new List<ILocation>();
                    Location memoryLocation = new Location()
                    {
                        Name = $"Hint {hintID}",
                        Id = -1,
                        CheckType = LocationCheckType.Byte,
                        Address = level.ZoeHintAddresses[i],
                        CompareType = LocationCheckCompareType.Match,
                        CheckValue = "7"
                    };
                    Location levelLocation = new Location()
                    {
                        Name = $"Hint {hintID} level check",
                        Id = -1,
                        CheckType = LocationCheckType.Byte,
                        Address = Addresses.CurrentLevelAddress,
                        CompareType = LocationCheckCompareType.Match,
                        CheckValue = $"{currentLevelID}",
                    };
                    Location sublevelLocation = new Location()
                    {
                        Name = $"Hint {hintID} sublevel check",
                        Id = -1,
                        CheckType = LocationCheckType.Byte,
                        Address = Addresses.CurrentSubareaAddress,
                        CompareType = LocationCheckCompareType.Match,
                        CheckValue = "0",
                    };
                    Location gameStateLocation = new Location()
                    {
                        Name = $"Hint {hintID} state check",
                        Id = -1,
                        CheckType = LocationCheckType.Byte,
                        Address = Addresses.GameStatus,
                        CompareType = LocationCheckCompareType.Match,
                        CheckValue = $"{(int)GameStatus.Talking}"
                    };
                    conditionsList.Add(memoryLocation);
                    conditionsList.Add(levelLocation);
                    conditionsList.Add(sublevelLocation);
                    conditionsList.Add(gameStateLocation);

                    CompositeLocation hintLocation = new CompositeLocation()
                    {
                        Name = $"Hint {hintID}",
                        Id = baseId + (levelOffset * (level.LevelId - 1)) + locationOffset,
                        CheckType = LocationCheckType.AND,
                        Conditions = conditionsList,
                        Category = "Hint"
                    };
                    locations.Add(hintLocation);
                    locationOffset++;
                    hintID++;
                }
                // Life bottle checks are mostly ready, but a few memory addresses are wrong still.
                // Life bottle RAM gets wiped when you enter a new level, so there needs to be a
                // complex check confirming which level you're in as well.
                for (int i = 0; i < level.LifeBottles.Length; i++)
                {
                    List<ILocation> conditionsList = new List<ILocation>();
                    // 1 is flame/rocket, 2 is charge/headbash/kick, 4 is Bentley smash
                    Location bottleLocation = new Location()
                    {
                        Name = $"Life Bottle {lifeBottleID} break check",
                        Id = -1,
                        CheckType = LocationCheckType.Byte,
                        Address = level.LifeBottles[i],
                        CompareType = LocationCheckCompareType.Range,
                        RangeStartValue = "1",
                        RangeEndValue = "4"
                    };
                    // The current level variable starts at 10 and skips 19, 29, 39, and 49.
                    // Convert our internal IDs to this system.
                    int currentLevelID = 9 + level.LevelId + ((level.LevelId - 1) / 9);
                    Location levelLocation = new Location()
                    {
                        Name = $"Life Bottle {lifeBottleID} level check",
                        Id = -1,
                        CheckType = LocationCheckType.Byte,
                        Address = Addresses.CurrentLevelAddress,
                        CompareType = LocationCheckCompareType.Match,
                        CheckValue = $"{currentLevelID}",
                    };
                    // RAM doesn't clear fully until level is loaded in
                    Location gameStateLocation = new Location()
                    {
                        Name = $"Life Bottle {lifeBottleID} state check",
                        Id = -1,
                        CheckType = LocationCheckType.Byte,
                        Address = Addresses.GameStatus,
                        CompareType = LocationCheckCompareType.Range,
                        RangeStartValue = $"{(int)GameStatus.InGame}",
                        RangeEndValue = $"{(int)GameStatus.Talking}"
                    };
                    // Make sure we're in the right sub-area.
                    int subarea = 0;
                    if (
                        level.LifeBottles[i] == Addresses.BambooBentleyLifeBottleAddress ||
                        level.LifeBottles[i] == Addresses.FireworksAgentLifeBottleAddress ||
                        level.LifeBottles[i] == Addresses.FleetSkateboardingLifeBottleAddress ||
                        level.LifeBottles[i] == Addresses.MoltenLifeBottleAddress
                    )
                    {
                        subarea = 2;
                    }
                        Location subareaLocation = new Location()
                        {
                            Name = $"Life Bottle {lifeBottleID} sub-area check",
                            Id = -1,
                            CheckType = LocationCheckType.Byte,
                            Address = Addresses.CurrentSubareaAddress,
                            CompareType = LocationCheckCompareType.Match,
                            CheckValue = $"{subarea}"
                        };
                    conditionsList.Add(bottleLocation);
                    conditionsList.Add(levelLocation);
                    conditionsList.Add(gameStateLocation);
                    conditionsList.Add(subareaLocation);

                    CompositeLocation lifeBottleLocation = new CompositeLocation()
                    {
                        Name = $"Life Bottle {lifeBottleID}",
                        Id = baseId + (levelOffset * (level.LevelId - 1)) + locationOffset,
                        CheckType = LocationCheckType.AND,
                        Conditions = conditionsList,
                        Category = "Life Bottle"
                    };
 
                    locations.Add(lifeBottleLocation);
                    locationOffset++;
                    lifeBottleID++;
                }
                currentAddress++;
            }
            baseId = 1267000;
            for (int i = 0; i < 40; i++)
            {
                Location totalGemLocation = new Location()
                {
                    Name = $"{500 * (i + 1)} Total Gems",
                    Id = baseId + i,
                    CheckType = LocationCheckType.Int,
                    Address = Addresses.TotalGemAddress,
                    CompareType = LocationCheckCompareType.GreaterThan,
                    CheckValue = $"{500 * (i + 1) - 1}",
                    Category = "Total Gems"
                };
                locations.Add(totalGemLocation);
            }
            return locations;
        }

        private static List<LevelData> GetLevelData()
        {
            List<LevelData> levels = new List<LevelData>()
            {
                new LevelData("Sunrise Spring", 1, 5, true, false, 400, [], [Addresses.SunrisePondLifeBottleAddress, Addresses.SunriseSheilaLifeBottleAddress], [Addresses.SunriseFirstZoeAddress, Addresses.SunriseSuperflyZoeAddress, Addresses.SunriseCameraZoeAddress]),
                new LevelData("Sunny Villa", 2, 6, false, false, 400, ["Flame all trees", "Skateboard course record I"], [Addresses.SunnyFirstLifeBottleAddress, Addresses.SunnySecondLifeBottleAddress], [Addresses.SunnyBigRhynocZoeAddress, Addresses.SunnyCheckpointZoeAddress]),
                new LevelData("Cloud Spires", 3, 6, false, false, 400, [], [Addresses.CloudLifeBottleAddress], [Addresses.CloudMetalArmorZoeAddress, Addresses.CloudGlideZoeAddress]),
                new LevelData("Molten Crater", 4, 6, false, false, 400, ["Assemble tiki heads", "Supercharge the wall"], [Addresses.MoltenLifeBottleAddress], [Addresses.MoltenFodderZoeAddress]),
                new LevelData("Seashell Shore", 5, 6, false, false, 400, ["Catch the funky chicken"], [Addresses.SeashellLifeBottleAddress], [Addresses.SeashellAtlasZoeAddress, Addresses.SeashellHoverZoeAddress]),
                new LevelData("Mushroom Speedway", 6, 3, false, false, 400, [], [], []),
                new LevelData("Sheila's Alp", 7, 3, false, false, 400, [], [Addresses.SheilaLifeBottleAddress], [Addresses.SheilaControlsZoeAddress]),
                new LevelData("Buzz", 8, 1, false, true, 0, [], [], []),
                new LevelData("Crawdad Farm", 9, 1, false, false, 200, [], [], []),
                new LevelData("Midday Garden", 10, 5, true, false, 400, [], [], []),
                new LevelData("Icy Peak", 11, 6, false, false, 500, ["Glide to pedestal"], [Addresses.IcyLifeBottleAddress], []),
                new LevelData("Enchanted Towers", 12, 6, false, false, 500, ["Skateboard course record II"], [Addresses.EnchantedLifeBottleAddress], []),
                new LevelData("Spooky Swamp", 13, 6, false, false, 500, ["Destroy all piranha signs"], [], []),
                new LevelData("Bamboo Terrace", 14, 6, false, false, 500, [], [Addresses.BambooBentleyLifeBottleAddress, Addresses.BambooFirstUnderwaterLifeBottleAddress, Addresses.BambooSecondUnderwaterLifeBottleAddress, Addresses.BambooThirdUnderwaterLifeBottleAddress, Addresses.BambooFourthUnderwaterLifeBottleAddress], []),
                new LevelData("Country Speedway", 15, 3, false, false, 400, [], [], []),
                new LevelData("Sgt. Byrd's Base", 16, 3, false, false, 500, ["Bomb the gophers"], [Addresses.ByrdLifeBottleAddress], []),
                new LevelData("Spike", 17, 1, false, true, 0, [], [], []),
                new LevelData("Spider Town", 18, 1, false, false, 200, [], [], []),
                new LevelData("Evening Lake", 19, 5, true, false, 400, [], [Addresses.EveningLifeBottleAddress], []),
                new LevelData("Frozen Altars", 20, 6, false, false, 600, ["Beat yeti in two rounds"], [], []),
                new LevelData("Lost Fleet", 21, 6, false, false, 600, ["Skateboard record time"], [Addresses.FleetAcidLifeBottleAddress, Addresses.FleetSkateboardingLifeBottleAddress], []),
                new LevelData("Fireworks Factory", 22, 6, false, false, 600, ["Find Agent 9's powerup"], [Addresses.FireworksAgentLifeBottleAddress, Addresses.FireworksOOBLifeBottleAddress], []),
                new LevelData("Charmed Ridge", 23, 6, false, false, 600, ["The Impossible Tower", "Shoot the temple windows"], [Addresses.CharmedFirstLifeBottleAddress, Addresses.CharmedSecondLifeBottleAddress], []),
                new LevelData("Honey Speedway", 24, 3, false, false, 400, [], [], []),
                new LevelData("Bentley's Outpost", 25, 3, false, false, 600, ["Push box off the cliff"], [], []),
                new LevelData("Scorch", 26, 1, false, true, 0, [], [], []),
                new LevelData("Starfish Reef", 27, 1, false, false, 200, [], [], []),
                new LevelData("Midnight Mountain", 28, 6, true, false, 400, [], [], []),
                new LevelData("Crystal Islands", 29, 6, false, false, 700, [], [], []),
                new LevelData("Desert Ruins", 30, 6, false, false, 700, ["Destroy all seaweed"], [Addresses.DesertInsideLifeBottleAddress, Addresses.DesertOutsideLifeBottleAddress], []),
                new LevelData("Haunted Tomb", 31, 6, false, false, 700, ["Swim into the dark hole"], [Addresses.HauntedLifeBottleAddress], []),
                new LevelData("Dino Mines", 32, 6, false, false, 700, ["Hit all the seahorses", "Hit the secret dino"], [Addresses.DinoLifeBottleAddress], []),
                new LevelData("Harbor Speedway", 33, 3, false, false, 400, [], [], []),
                new LevelData("Agent 9's Lab", 34, 3, false, false, 700, ["Blow up all palm trees"], [], []),
                new LevelData("Sorceress", 35, 1, false, true, 0, [], [], []),
                new LevelData("Bugbot Factory", 36, 1, false, false, 200, [], [], []),
                new LevelData("Super Bonus Round", 37, 1, false, false, 5000, [], [], []),
            };
            return levels;
        }
    }
}
