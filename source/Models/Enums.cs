using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Sorceress2 = 2//,
            //Test goal for ease of debugging
            //SunnyVilla = 3
        }
    }
}
