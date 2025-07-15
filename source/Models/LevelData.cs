using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3AP.Models
{
    public class LevelData
    {
        public string Name { get; set; }
        public int EggCount { get; set; }
        public int LevelId { get; set; }
        public bool IsHomeworld { get; set; }
        public bool IsBoss { get; set; }
        public int GemCount { get; set; }
        // 0-indexed indices of all bit masks in the gem section of RAM that are unused.
        public int[] GemBitSkipIndices {  get; set; }
        public uint GemBitStartAddress { get; set; }
        public int NumberOfGemChecks { get; set; }
        public string[] SkillPoints { get; set; }
        public LevelData(string name, int levelId, int eggCount, bool isHomeworld, bool isBoss, int gemCount, string[] skillPoints, int[] gemBitSkipIndices, uint gemBitStartAddress = 0x00, int numberOfGemChecks = 0)
        {
            Name = name;
            EggCount = eggCount;
            LevelId = levelId;
            IsHomeworld = isHomeworld;
            IsBoss = isBoss;
            GemCount = gemCount;
            SkillPoints = skillPoints;
            GemBitSkipIndices = gemBitSkipIndices;
            GemBitStartAddress = gemBitStartAddress;
            NumberOfGemChecks = numberOfGemChecks;
        }
    }
}
