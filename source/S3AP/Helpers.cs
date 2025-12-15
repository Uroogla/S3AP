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
        public static string gameVersion = "";
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
                {"Sunrise Spring", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.SunriseSpringGems)), Addresses.GetVersionAddress(Addresses.SunriseSpringGems))},
                {"Sunny Villa", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.SunnyVillaGems)), Addresses.GetVersionAddress(Addresses.SunnyVillaGems))},
                {"Cloud Spires", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.CloudSpireLevelGems)), Addresses.GetVersionAddress(Addresses.CloudSpireLevelGems))},
                {"Molten Crater", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.MoltenCraterGems)), Addresses.GetVersionAddress(Addresses.MoltenCraterGems))},
                {"Seashell Shore", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.SeashellShoreGems)), Addresses.GetVersionAddress(Addresses.SeashellShoreGems))},
                {"Mushroom Speedway", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.MushroomSpeedwayGems)), Addresses.GetVersionAddress(Addresses.MushroomSpeedwayGems))},
                {"Sheila's Alp", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.SheilaAlpGems)), Addresses.GetVersionAddress(Addresses.SheilaAlpGems))},
                {"Buzz's Dungeon", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.BuzzDungeonGems)), Addresses.GetVersionAddress(Addresses.BuzzDungeonGems))},
                {"Crawdad Farm", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.CrawdadFarmGems)), Addresses.GetVersionAddress(Addresses.CrawdadFarmGems))},
                {"Midday Gardens", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.MiddayGardenGems)), Addresses.GetVersionAddress(Addresses.MiddayGardenGems))},
                {"Icy Peak", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.IcyPeakGems)), Addresses.GetVersionAddress(Addresses.IcyPeakGems))},
                {"Enchanted Towers", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.EnchantedTowersGems)), Addresses.GetVersionAddress(Addresses.EnchantedTowersGems))},
                {"Spooky Swamp", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.SpookySwampGems)), Addresses.GetVersionAddress(Addresses.SpookySwampGems))},
                {"Bamboo Terrace", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.BambooTerraceGems)), Addresses.GetVersionAddress(Addresses.BambooTerraceGems))},
                {"Country Speedway", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.CountrySpeedwayGems)), Addresses.GetVersionAddress(Addresses.CountrySpeedwayGems))},
                {"Sgt. Byrd's Base", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.SgtByrdBaseGems)), Addresses.GetVersionAddress(Addresses.SgtByrdBaseGems))},
                {"Spike's Arena", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.SpikesArenaGems)), Addresses.GetVersionAddress(Addresses.SpikesArenaGems))},
                {"Spider Town", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.SpiderTownGems)), Addresses.GetVersionAddress(Addresses.SpiderTownGems))},
                {"Evening Lake", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.EveningLakeGems)), Addresses.GetVersionAddress(Addresses.EveningLakeGems))},
                {"Frozen Altars", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.FrozenAltarsGems)), Addresses.GetVersionAddress(Addresses.FrozenAltarsGems))},
                {"Lost Fleet", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.LostFleetGems)), Addresses.GetVersionAddress(Addresses.LostFleetGems))},
                {"Fireworks Factory", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.FireworksFactoryGems)), Addresses.GetVersionAddress(Addresses.FireworksFactoryGems))},
                {"Charmed Ridge", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.CharmedRidgeGems)), Addresses.GetVersionAddress(Addresses.CharmedRidgeGems))},
                {"Honey Speedway", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.HoneySpeedwayGems)), Addresses.GetVersionAddress(Addresses.HoneySpeedwayGems))},
                {"Bentley's Outpost", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.BentleyOutpostGems)), Addresses.GetVersionAddress(Addresses.BentleyOutpostGems))},
                {"Scorch's Pit", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.ScorchPitGems)), Addresses.GetVersionAddress(Addresses.ScorchPitGems))},
                {"Starfish Reef", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.StarfishReefGems)), Addresses.GetVersionAddress(Addresses.StarfishReefGems))},
                {"Midnight Mountain", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.MidnightMountainGems)), Addresses.GetVersionAddress(Addresses.MidnightMountainGems))},
                {"Crystal Islands", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.CrystalIslandsGems)), Addresses.GetVersionAddress(Addresses.CrystalIslandsGems))},
                {"Desert Ruins", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.DesertRuinsGems)), Addresses.GetVersionAddress(Addresses.DesertRuinsGems))},
                {"Haunted Tomb", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.HauntedTombGems)), Addresses.GetVersionAddress(Addresses.HauntedTombGems))},
                {"Dino Mines", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.DinoMinesGems)), Addresses.GetVersionAddress(Addresses.DinoMinesGems))},
                {"Harbor Speedway", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.HarborSpeedwayGems)), Addresses.GetVersionAddress(Addresses.HarborSpeedwayGems))},
                {"Agent 9's Lab", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.AgentNineLabGems)), Addresses.GetVersionAddress(Addresses.AgentNineLabGems))},
                {"Sorcerer's Lair", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.SorcererLairGems)), Addresses.GetVersionAddress(Addresses.SorcererLairGems))},
                {"Bugbot Factory", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.BugbotFactoryGems)), Addresses.GetVersionAddress(Addresses.BugbotFactoryGems))},
                {"Super Bonus Round", new Tuple<int, uint>(Memory.ReadInt(Addresses.GetVersionAddress(Addresses.SuperBonusRoundGems)), Addresses.GetVersionAddress(Addresses.SuperBonusRoundGems))}
            };
        }
        public static bool IsInDemoMode()
        {
            return Memory.ReadByte(Addresses.GetVersionAddress(Addresses.IsInDemoMode)) == 1;
        }
        public static bool IsCorrectVersion()
        {
            if (Memory.ReadString(Addresses.GreenLabelAtlasAddress, 5) == "Atlas")
            {
                gameVersion = "1.1";
                return true;
            }
            else if (Memory.ReadString(Addresses.BlackLabelAtlasAddress, 5) == "Atlas")
            {
                if (gameVersion != "1.0")
                {
                    Log.Logger.Warning("You are playing on the NTSC-U 1.0 release of Spyro 3.");
                    Log.Logger.Warning("Support for this version is in beta.");
                    Log.Logger.Warning("Please report any issues you encounter!");
                }
                gameVersion = "1.0";
                return true;
            }
            return false;
        }
        public static bool IsInGame(bool includeLoading = true)
        {
            bool isCorrectGameVersion = IsCorrectVersion();
            var status = GetGameStatus();
            return isCorrectGameVersion &&
                !IsInDemoMode() &&
                status != GameStatus.TitleScreen &&
                (!includeLoading || status != GameStatus.Loading) && // Handle loading into and out of demo mode.
                Memory.ReadInt(Addresses.GetVersionAddress(Addresses.ResetCheckAddress)) != 0 && // Handle status being 0 on console reset.
                lastNonZeroStatus != GameStatus.StartingGame; // Handle status swapping from 16 to 0 temporarily on game load
        }
        public static GameStatus GetGameStatus()
        {
            var status = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.GameStatus));
            var result = (GameStatus)status;
            if (result != GameStatus.InGame)
            {
                lastNonZeroStatus = result;
            }
            return result;
        }
        public static List<ILocation> BuildLocationList(bool includeGemsanity = false, List<int> gemsanityIDs = null)
        {
            if (gemsanityIDs == null)
            {
                gemsanityIDs = new List<int>();
            }
            int baseId = 1230000;
            int levelOffset = 1000;
            int processedEggs = 0;
            int processedSkillPoints = 0;
            List<ILocation> locations = new List<ILocation>();
            var currentAddress = Addresses.GetVersionAddress(Addresses.EggStartAddress);
            var currentSkillPointAddress = Addresses.GetVersionAddress(Addresses.SkillPointAddress);
            var currentSkillPointGoalAddress = Addresses.GetVersionAddress(Addresses.SkillPointAddress);
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
                for (int i = 0; i < level.ZoeHintAddresses.Length; i++)
                {
                    Location hintLocation = new Location()
                    {
                        Name = $"Hint {hintID}",
                        Id = baseId + (levelOffset * (level.LevelId - 1)) + locationOffset,
                        Address = Addresses.GetVersionAddress(level.ZoeHintAddresses[i][0]),
                        AddressBit = (int)level.ZoeHintAddresses[i][1],
                        CheckType = LocationCheckType.Bit,
                        Category = "Zoe Hint"
                    };
                    locations.Add(hintLocation);
                    locationOffset++;
                    hintID++;
                }
                for (int i = 0; i < level.LifeBottles.Length; i++)
                {
                    Location lifeBottleLocation = new Location()
                    {
                        Name = $"{level.Name}: Life Bottle {i}",
                        Id = baseId + (levelOffset * (level.LevelId - 1)) + locationOffset,
                        Address = Addresses.GetVersionAddress(level.LifeBottles[i][0]),
                        AddressBit = (int)level.LifeBottles[i][1],
                        CheckType = LocationCheckType.Bit,
                        Category = "Life Bottle"
                    };
                    locations.Add(lifeBottleLocation);
                    locationOffset++;
                }
                int gemIndex = 1;
                for (int i = 0; i < level.TotalGemCount + level.GemSkipIndices.Length; i++)
                {
                    if (!level.GemSkipIndices.Contains(i + 1))
                    {
                        Location gemsLocation = new Location()
                        {
                            Name = $"{level.Name}: Gem {gemIndex}",
                            Id = baseId + (levelOffset * (level.LevelId - 1)) + locationOffset,
                            Address = level.GemMaskAddress + (uint)(i / 8),
                            AddressBit = i % 8,
                            CheckType = LocationCheckType.Bit,
                            Category = "Gemsanity"
                        };
                        if (includeGemsanity && gemsanityIDs.Contains(baseId + (levelOffset * (level.LevelId - 1)) + locationOffset))
                        {
                            locations.Add(gemsLocation);
                        }
                        locationOffset++;
                        gemIndex++;
                    }
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
                    Address = Addresses.GetVersionAddress(Addresses.TotalGemAddress),
                    CompareType = LocationCheckCompareType.GreaterThan,
                    CheckValue = $"{500 * (i + 1) - 1}",
                    Category = "Total Gems"
                };
                locations.Add(totalGemLocation);
            }
            int offset = 40;
            foreach (var level in levels)
            {
                if (!level.IsBoss)
                {
                    Location gem25Location = new Location()
                    {
                        Name = $"{level.Name}: 25% Gems",
                        Id = baseId + offset,
                        Address = gemDict[level.Name].Item2,
                        CheckType = LocationCheckType.Int,
                        CompareType = LocationCheckCompareType.GreaterThan,
                        CheckValue = $"{level.GemCount / 4 - 1}",
                        Category = "Gem25"
                    };
                    locations.Add(gem25Location);
                    offset++;

                    Location gem50Location = new Location()
                    {
                        Name = $"{level.Name}: 50% Gems",
                        Id = baseId + offset,
                        Address = gemDict[level.Name].Item2,
                        CheckType = LocationCheckType.Int,
                        CompareType = LocationCheckCompareType.GreaterThan,
                        CheckValue = $"{level.GemCount / 2 - 1}",
                        Category = "Gem50"
                    };
                    locations.Add(gem50Location);
                    offset++;

                    Location gem75Location = new Location()
                    {
                        Name = $"{level.Name}: 75% Gems",
                        Id = baseId + offset,
                        Address = gemDict[level.Name].Item2,
                        CheckType = LocationCheckType.Int,
                        CompareType = LocationCheckCompareType.GreaterThan,
                        CheckValue = $"{3 * level.GemCount / 4 - 1}",
                        Category = "Gem75"
                    };
                    locations.Add(gem75Location);
                    offset++;

                    Location gem100Location = new Location()
                    {
                        Name = $"{level.Name}: All Gems",
                        Id = baseId + offset,
                        Address = gemDict[level.Name].Item2,
                        CheckType = LocationCheckType.Int,
                        CompareType = LocationCheckCompareType.GreaterThan,
                        CheckValue = $"{level.GemCount - 1}",
                        Category = "Gem"
                    };
                    locations.Add(gem100Location);
                    offset++;
                }
            }
            return locations;
        }

        public static List<LevelData> GetLevelData()
        {
            List<LevelData> levels = new List<LevelData>()
            {
                new LevelData("Sunrise Spring", 1, 5, true, false, 400, [], [Addresses.SunrisePondLifeBottleAddress, Addresses.SunriseSheilaLifeBottleAddress], [Addresses.SunriseFirstZoeAddress, Addresses.SunriseSuperflyZoeAddress, Addresses.SunriseCameraZoeAddress], Addresses.GetVersionAddress(Addresses.SunriseGemMask), 142, [27, 60, 61, 62, 93, 122, 123, 143]),
                new LevelData("Sunny Villa", 2, 6, false, false, 400, ["Flame all trees", "Skateboard course record I"], [Addresses.SunnyFirstLifeBottleAddress, Addresses.SunnySecondLifeBottleAddress], [Addresses.SunnyBigRhynocZoeAddress, Addresses.SunnyCheckpointZoeAddress], Addresses.GetVersionAddress(Addresses.SunnyGemMask), 198, [13, 33, 34, 58, 109, 154, 172, 173, 174, 175, 193, 194, 195, 196, 197, 203, 205, 206, 213, 214, 216]),
                new LevelData("Cloud Spires", 3, 6, false, false, 400, [], [Addresses.CloudLifeBottleAddress], [Addresses.CloudMetalArmorZoeAddress, Addresses.CloudGlideZoeAddress], Addresses.GetVersionAddress(Addresses.CloudGemMask), 148, [2, 81, 92, 145]),
                new LevelData("Molten Crater", 4, 6, false, false, 400, ["Assemble tiki heads", "Supercharge the wall"], [Addresses.MoltenLifeBottleAddress], [Addresses.MoltenFodderZoeAddress], Addresses.GetVersionAddress(Addresses.MoltenGemMask), 147, [5, 6, 29, 63, 64, 71, 105, 124, 131, 145]),
                new LevelData("Seashell Shore", 5, 6, false, false, 400, ["Catch the funky chicken"], [Addresses.SeashellLifeBottleAddress], [Addresses.SeashellAtlasZoeAddress, Addresses.SeashellHoverZoeAddress], Addresses.GetVersionAddress(Addresses.SeashellGemMask), 169, [33, 60, 97, 99, 100, 101, 102, 116, 121, 122, 134, 135, 138, 140, 141, 142, 143, 144, 145, 146, 147, 149, 150, 151, 152, 159, 164, 165, 166, 167, 168, 170, 171, 172, 173, 174, 175, 178, 180, 181, 182, 183, 185, 189, 191, 192, 193, 194, 196, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 209, 210, 227]),
                new LevelData("Mushroom Speedway", 6, 3, false, false, 400, [], [], [], Addresses.GetVersionAddress(Addresses.MushroomGemMask), 32, [13, 14, 15, 16, 17, 18, 23]),
                new LevelData("Sheila's Alp", 7, 3, false, false, 400, [], [Addresses.SheilaLifeBottleAddress], [Addresses.SheilaControlsZoeAddress], Addresses.GetVersionAddress(Addresses.SheilaGemMask), 117, [14, 15, 21, 23, 30, 50, 59, 99, 100, 101, 102, 109, 110, 111, 112, 113, 121, 123, 133]),
                new LevelData("Buzz", 8, 1, false, true, 0, [], [], []),
                new LevelData("Crawdad Farm", 9, 1, false, false, 200, [], [], [], Addresses.GetVersionAddress(Addresses.CrawdadGemMask), 47, [1, 2, 4, 5, 6, 7, 8, 9, 10, 11, 12, 16, 17, 18, 20, 21, 23, 24, 25, 26, 27, 28, 31, 32, 33, 44, 45, 46, 47, 48, 52, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 86, 89, 90, 91, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 132, 133, 134, 135, 142, 144, 145, 146, 147, 148, 149]),
                new LevelData("Midday Gardens", 10, 5, true, false, 400, [], [], [], Addresses.GetVersionAddress(Addresses.MiddayGemMask), 123, [72, 89, 90, 109, 126]),
                new LevelData("Icy Peak", 11, 6, false, false, 500, ["Glide to pedestal"], [Addresses.IcyLifeBottleAddress], [], Addresses.GetVersionAddress(Addresses.IcyGemMask), 175, [13, 19, 25, 50, 72, 73, 91, 122, 171, 175]),
                new LevelData("Enchanted Towers", 12, 6, false, false, 500, ["Skateboard course record II"], [Addresses.EnchantedLifeBottleAddress], [], Addresses.GetVersionAddress(Addresses.EnchantedGemMask), 174, [51, 52, 53, 54, 55, 56, 57, 84, 87, 88, 89, 90, 91, 92, 93, 108, 147, 172]),
                new LevelData("Spooky Swamp", 13, 6, false, false, 500, ["Destroy all piranha signs"], [], [], Addresses.GetVersionAddress(Addresses.SpookyGemMask), 151, [5, 18, 54, 59, 76, 80, 104, 110, 122]),
                new LevelData("Bamboo Terrace", 14, 6, false, false, 500, [], [Addresses.BambooBentleyLifeBottleAddress, Addresses.BambooFirstUnderwaterLifeBottleAddress, Addresses.BambooSecondUnderwaterLifeBottleAddress, Addresses.BambooThirdUnderwaterLifeBottleAddress, Addresses.BambooFourthUnderwaterLifeBottleAddress], [], Addresses.GetVersionAddress(Addresses.BambooGemMask), 159, [53, 54, 55, 56, 77, 95, 96, 115, 125, 126, 142, 159, 160, 161, 163, 167, 168, 171, 172, 175, 176, 178, 179, 180, 181, 182, 183, 185, 186, 187, 188, 190]),
                new LevelData("Country Speedway", 15, 3, false, false, 400, [], [], [], Addresses.GetVersionAddress(Addresses.CountryGemMask), 32),
                new LevelData("Sgt. Byrd's Base", 16, 3, false, false, 500, ["Bomb the gophers"], [Addresses.ByrdLifeBottleAddress], [], Addresses.GetVersionAddress(Addresses.ByrdGemMask), 117, [6, 10, 13, 51, 56, 57, 58, 59, 60, 63, 64, 65, 102, 103, 104, 118]),
                new LevelData("Spike", 17, 1, false, true, 0, [], [], []),
                new LevelData("Spider Town", 18, 1, false, false, 200, [], [], [], Addresses.GetVersionAddress(Addresses.SpiderGemMask), 47, [9, 33, 34, 35, 36, 37, 38, 39, 40, 41, 46, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109]),
                new LevelData("Evening Lake", 19, 5, true, false, 400, [], [Addresses.EveningLifeBottleAddress], [], Addresses.GetVersionAddress(Addresses.EveningGemMask), 75, [31, 37, 60, 61, 66]),
                new LevelData("Frozen Altars", 20, 6, false, false, 600, ["Beat yeti in two rounds"], [], [], Addresses.GetVersionAddress(Addresses.FrozenGemMask), 129, [52, 53, 54, 74, 82, 86, 88, 94, 108, 111, 112, 116, 117]),
                new LevelData("Lost Fleet", 21, 6, false, false, 600, ["Skateboard record time"], [Addresses.FleetAcidLifeBottleAddress, Addresses.FleetSkateboardingLifeBottleAddress], [], Addresses.GetVersionAddress(Addresses.FleetGemMask), 223, [41, 50, 52, 69, 78, 93, 165, 166, 167, 169, 221, 227, 231, 232, 233, 235]),
                new LevelData("Fireworks Factory", 22, 6, false, false, 600, ["Find Agent 9's powerup"], [Addresses.FireworksAgentLifeBottleAddress, Addresses.FireworksOOBLifeBottleAddress], [], Addresses.GetVersionAddress(Addresses.FireworksGemMask), 233, [14, 20, 46, 51, 77, 82, 97, 98, 104, 105, 106, 108, 141, 153, 155, 177]),
                new LevelData("Charmed Ridge", 23, 6, false, false, 600, ["The Impossible Tower", "Shoot the temple windows"], [Addresses.CharmedFirstLifeBottleAddress, Addresses.CharmedSecondLifeBottleAddress], [], Addresses.GetVersionAddress(Addresses.CharmedGemMask), 175, [20, 21, 53, 58, 59, 60, 61, 66, 100, 101, 102, 129, 130, 136, 137, 144, 145, 146, 147, 150, 152, 162]),
                new LevelData("Honey Speedway", 24, 3, false, false, 400, [], [], [], Addresses.GetVersionAddress(Addresses.HoneyGemMask), 32),
                new LevelData("Bentley's Outpost", 25, 3, false, false, 600, ["Push box off the cliff"], [], [], Addresses.GetVersionAddress(Addresses.BentleyGemMask), 109, [7, 38, 39, 43, 59, 60, 62, 72, 90, 96, 104]),
                new LevelData("Scorch", 26, 1, false, true, 0, [], [], []),
                new LevelData("Starfish Reef", 27, 1, false, false, 200, [], [], [], Addresses.GetVersionAddress(Addresses.StarfishGemMask), 53, [1, 2, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 18, 19, 20, 21, 22, 23, 25, 26, 27, 28, 29, 36, 38, 39, 40, 41, 42, 43, 44, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 69, 70, 71, 72, 73, 74, 75, 76, 78, 79, 80, 81, 82, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 110, 111, 113, 114, 115, 116, 117, 118, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 143, 145, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 175, 176, 177, 178, 179, 180, 181, 182, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213, 215, 216, 218, 219, 220]),
                new LevelData("Midnight Mountain", 28, 6, true, false, 400, [], [], [], Addresses.GetVersionAddress(Addresses.MidnightGemMask), 105, [10, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 67, 68, 69, 70, 100]),
                new LevelData("Crystal Islands", 29, 6, false, false, 700, [], [], [], Addresses.GetVersionAddress(Addresses.CrystalGemMask), 205, [26, 41, 50, 51, 60, 61, 62, 63, 69, 102, 201, 202, 203, 204, 212, 216, 217, 218, 219, 220, 221, 222, 223, 224, 225]),
                new LevelData("Desert Ruins", 30, 6, false, false, 700, ["Destroy all seaweed"], [Addresses.DesertInsideLifeBottleAddress, Addresses.DesertOutsideLifeBottleAddress], [], Addresses.GetVersionAddress(Addresses.DesertGemMask), 144, [15, 20, 21, 50, 56, 63, 83, 89, 90, 91, 92, 93, 103, 104, 106, 107, 108, 109, 123, 124, 125, 126, 150, 151, 152, 153, 154, 155, 156, 157, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181]),
                new LevelData("Haunted Tomb", 31, 6, false, false, 700, ["Swim into the dark hole"], [Addresses.HauntedLifeBottleAddress], [], Addresses.GetVersionAddress(Addresses.HauntedGemMask), 115, [7, 85, 90, 97, 102, 106, 111, 116, 117, 123, 125]),
                new LevelData("Dino Mines", 32, 6, false, false, 700, ["Hit all the seahorses", "Hit the secret dino"], [Addresses.DinoLifeBottleAddress], [], Addresses.GetVersionAddress(Addresses.DinoGemMask), 143, [24, 41, 61, 77, 79, 82, 83, 88, 90, 98, 104, 111, 117, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165, 166, 167, 168, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181, 182, 183, 184, 186, 192, 196, 197, 202, 206, 207, 208, 212, 213, 214, 215, 216, 217, 218, 219, 225]),
                new LevelData("Harbor Speedway", 33, 3, false, false, 400, [], [], [], Addresses.GetVersionAddress(Addresses.HarborGemMask), 32),
                new LevelData("Agent 9's Lab", 34, 3, false, false, 700, ["Blow up all palm trees"], [], [], Addresses.GetVersionAddress(Addresses.Agent9GemMask), 106, [19, 21, 28, 43, 78, 86, 87, 88, 89, 90, 91, 93, 94, 99, 100, 101, 102, 103, 104, 105, 107, 108, 109, 110, 111, 112, 113, 125, 130, 131, 132, 133, 134, 135]),
                new LevelData("Sorceress", 35, 1, false, true, 0, [], [], []),
                new LevelData("Bugbot Factory", 36, 1, false, false, 200, [], [], [], Addresses.GetVersionAddress(Addresses.BugbotGemMask), 42, [1, 3, 4, 6, 7, 8, 9, 10, 11, 12, 13, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 38, 40, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 59, 64, 65, 66, 67, 68, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 95, 97, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 112, 116, 117, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 139, 140, 143, 144, 145, 148, 149, 150, 151, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 166]),
                new LevelData("Super Bonus Round", 37, 1, false, false, 5000, [], [], []),
            };
            return levels;
        }
    }
}
