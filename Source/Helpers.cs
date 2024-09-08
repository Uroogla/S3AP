using Archipelago.Core.Models;
using Newtonsoft.Json;
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
            Dictionary<string, Tuple<int, int>> levels = GetLevelData();
            var totalEggCount = levels.Select(x => x.Value.Item2).Sum();
            var homeworldList = new List<string>
            {
                "Sunrise Springs",
                "Midday Garden",
                "Evening Lake",
                "Midnight Mountain"
            };
            var bossList = new List<string>
            {
                "Buzz",
                "Spike",
                "Scorch",
                "Sorceress"
            };
            foreach (var level in levels)
            {                
                string levelName = level.Key;
                int levelId = level.Value.Item1;
                int levelEggCount = level.Value.Item2;
                Console.WriteLine($"Loading eggs for level {levelId}: {levelName}. {levelEggCount} eggs found");
                if (!homeworldList.Contains(levelName) && !bossList.Contains(levelName))
                {
                    // Level Completed (first egg)
                    Location location = new Location()
                    {
                        Name = levelName + " Completed",
                        Id = baseId + (levelOffset * (levelId - 1)) + levelEggCount,
                        AddressBit = 0,
                        CheckType = LocationCheckType.Bit,
                        Address = currentAddress,
                    };
                    locations.Add(location);
                }
                if (!homeworldList.Contains(levelName) && bossList.Contains(levelName))
                {
                    // Boss Defeated (first egg)
                    Location location = new Location()
                    {
                        Name = levelName + " Defeated",
                        Id = baseId + (levelOffset * (levelId - 1)) + levelEggCount,
                        AddressBit = 0,
                        CheckType = LocationCheckType.Bit,
                        Address = currentAddress,
                    };
                    locations.Add(location);
                }
                for (int i = 0; i < levelEggCount; i++)
                {
                    // Egg collected
                    Location location = new Location()
                    {
                        Name = $"Egg {processedEggs + 1}",
                        Id = baseId + (levelOffset * (levelId -1)) + i,
                        AddressBit = i,
                        CheckType = LocationCheckType.Bit,
                        Address = currentAddress,
                    };
                    locations.Add(location);
                    processedEggs++;
                }
                currentAddress++;
            }
            Console.WriteLine($"{totalEggCount} eggs loaded");
            return locations;
        }

        private static Dictionary<string, Tuple<int, int>> GetLevelData()
        {
            Dictionary<string, Tuple<int, int>> levels = new Dictionary<string, Tuple<int, int>>()
    {
        { "Sunrise Springs", new Tuple<int, int>(1, 5) },
        { "Sunny Villa", new Tuple<int, int>(2, 6) },
        { "Cloud Spires", new Tuple<int, int>(3, 6) },
        { "Molten Crater", new Tuple<int, int>(4, 6) },
        { "Seashell Shore", new Tuple<int, int>(5, 6) },
        { "Mushroom Speedway", new Tuple<int, int>(6, 3) },
        { "Shiela's Alp", new Tuple<int, int>(7, 3) },
        { "Buzz", new Tuple<int, int>(8, 1) },
        { "Crawdad Farm", new Tuple<int, int>(9, 1) },
        { "Midday Garden", new Tuple<int, int>(10, 5) },
        { "Icy Peak", new Tuple<int, int>(11, 6) },
        { "Enchanted Towers", new Tuple<int, int>(12, 6) },
        { "Spooky Swamp", new Tuple<int, int>(13, 6) },
        { "Bamboo Terrace", new Tuple<int, int>(14, 6) },
        { "Country Speedway", new Tuple<int, int>(15, 3) },
        { "Sgt. Byrd's Base", new Tuple<int, int>(16, 3) },
        { "Spike", new Tuple<int, int>(17, 1) },
        { "Spider Town", new Tuple<int, int>(18, 1) },
        { "Evening Lake", new Tuple<int, int>(19, 5) },
        { "Frozen Altars", new Tuple<int, int>(20, 6) },
        { "Lost Fleet", new Tuple<int, int>(21, 6) },
        { "Fireworks Factory", new Tuple<int, int>(22, 6) },
        { "Charmed Ridge", new Tuple<int, int>(23, 6) },
        { "Honey Speedway", new Tuple<int, int>(24, 3) },
        { "Bentley's Outpost", new Tuple<int, int>(25, 3) },
        { "Scorch", new Tuple<int, int>(26, 1) },
        { "Starfish Reef", new Tuple<int, int>(27, 1) },
        { "Midnight Mountain", new Tuple<int, int>(28, 5) },
        { "Crystal Islands", new Tuple<int, int>(29, 6) },
        { "Desert Ruins", new Tuple<int, int>(30, 6) },
        { "Haunted Tomb", new Tuple<int, int>(31, 6) },
        { "Dino Mines", new Tuple<int, int>(32, 6) },
        { "Harbor Speedway", new Tuple<int, int>(33, 3) },
        { "Agent 9's Lab", new Tuple<int, int>(34, 3) },
        { "Sorceress", new Tuple<int, int>(35, 1) },
        { "Bugbot Factory", new Tuple<int, int>(36, 1) },
        { "Super Bonus Round", new Tuple<int, int>(37, 1) }
    };
            return levels;
        }
    }
}
