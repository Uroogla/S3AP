using Archipelago.Core.Models;
using Newtonsoft.Json;
using S3AP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace S3AP
{
    public class Helpers
    {
        public static List<Location> GetLocations()
        {
            var json = OpenEmbeddedResource("S3AP.Resources.Locations.json");
            var list = JsonConvert.DeserializeObject<List<Location>>(json);
            return list;
        }
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

        public static List<Location> BuildEggLocationList()
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
                Console.WriteLine($"Loading eggs for level {level.LevelId}: {level.Name}. {level.EggCount} eggs found");
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
            Console.WriteLine($"{totalEggCount} eggs loaded");
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
