using System.Collections.Generic;
using static S3AP.Models.Enums;

namespace S3AP.Models
{
    public class LevelData
    {
        public string Name { get; set; }
        public int EggCount { get; set; }
        public int LevelId { get; set; }
        public LevelInGameIDs LevelInGameId { get; set; }
        public bool IsHomeworld { get; set; }
        public bool IsBoss { get; set; }
        public int GemCount { get; set; }
        public string[] SkillPoints { get; set; }
        public List<uint>[] LifeBottles { get; set; }
        public List<uint>[] ZoeHintAddresses { get; set; }
        public uint GemMaskAddress { get; set; }
        public int TotalGemCount { get; set; }
        public int[] GemSkipIndices { get; set; }
        public LevelData(
            string name,
            int levelId,
            LevelInGameIDs levelInGameId,
            int eggCount,
            bool isHomeworld,
            bool isBoss,
            int gemCount,
            string[] skillPoints,
            List<uint>[] lifeBottleAddresses,
            List<uint>[] zoeHintAddresses,
            uint gemMaskAddress = 0x0,
            int totalGemCount = 0,
            int[] gemSkipIndices = null
        )
        {
            Name = name;
            EggCount = eggCount;
            LevelId = levelId;
            LevelInGameId = levelInGameId;
            IsHomeworld = isHomeworld;
            IsBoss = isBoss;
            GemCount = gemCount;
            SkillPoints = skillPoints;
            LifeBottles = lifeBottleAddresses;
            ZoeHintAddresses = zoeHintAddresses;
            GemMaskAddress = gemMaskAddress;
            TotalGemCount = totalGemCount;
            if (gemSkipIndices == null)
            {
                gemSkipIndices = [];
            }
            GemSkipIndices = gemSkipIndices;
        }
    }
}
