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
        public string[] SkillPoints { get; set; }
        public LevelData(string name, int levelId, int eggCount, bool isHomeworld, bool isBoss, int gemCount, string[] skillPoints)
        {
            Name = name;
            EggCount = eggCount;
            LevelId = levelId;
            IsHomeworld = isHomeworld;
            IsBoss = isBoss;
            GemCount = gemCount;
            SkillPoints = skillPoints;
        }
    }
}
