using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3AP
{
    public static class Addresses
    {
        public const uint TotalEggAddress = 0x0006C740;
        public const uint EggStartAddress = 0x000703E0;
        public const uint IsInDemoMode = 0x0006c758;
        public const uint GameStatus = 0x0006e424;
        // The values at this and the following 3 bytes seem to be 0 only on reset.
        public const uint ResetCheckAddress = 0x0006e434;
        public const uint PlayerLives = 0x0006c864;
        public const uint PlayerHealth = 0x00070688;

        public const uint SunriseSpringGems = 0x071af0;
        public const uint SunnyVillaGems = 0x071af4;
        public const uint CloudSpireLevelGems = 0x071af8;
        public const uint MoltenCraterGems = 0x071afc;
        public const uint SeashellShoreGems = 0x071b00;
        public const uint MushroomSpeedwayGems = 0x071b04;
        public const uint SheilaAlpGems = 0x071b08;
        public const uint BuzzDungeonGems = 0x071b0c;
        public const uint CrawdadFarmGems = 0x071b10;
        public const uint MiddayGardenGems = 0x071b14;
        public const uint IcyPeakGems = 0x071b18;
        public const uint EnchantedTowersGems = 0x071b1c;
        public const uint SpookySwampGems = 0x071b20;
        public const uint BambooTerraceGems = 0x071b24;
        public const uint CountrySpeedwayGems = 0x071b28;
        public const uint SgtByrdBaseGems = 0x071b2c;
        public const uint SpikesArenaGems = 0x071b30;
        public const uint SpiderTownGems = 0x071b34;
        public const uint EveningLakeGems = 0x071b38;
        public const uint FrozenAltarsGems = 0x071b3c;
        public const uint LostFleetGems = 0x071b40;
        public const uint FireworksFactoryGems = 0x071b44;
        public const uint CharmedRidgeGems = 0x071b48;
        public const uint HoneySpeedwayGems = 0x071b4c;
        public const uint BentleyOutpostGems = 0x071b50;
        public const uint ScorchPitGems = 0x071b54;
        public const uint StarfishReefGems = 0x071b58;
        public const uint MidnightMountainGems = 0x071b5c;
        public const uint CrystalIslandsGems = 0x071b60;
        public const uint DesertRuinsGems = 0x071b64;
        public const uint HauntedTombGems = 0x071b68;
        public const uint DinoMinesGems = 0x071b6c;
        public const uint HarborSpeedwayGems = 0x071b70;
        public const uint AgentNineLabGems = 0x071b74;
        public const uint SorcererLairGems = 0x071b78;
        public const uint BugbotFactoryGems = 0x071b7c;
        public const uint SuperBonusRoundGems = 0x071b80;

        public const uint SkillPointAddress = 0x066ca0;

        public const uint BigHeadMode = 0x06fc76;
        public const uint SpyroWidth = 0x06fc79;
        public const uint SpyroHeight = 0x06fc7d;
        public const uint SpyroLength = 0x06fc81;
        public const uint SpyroColorAddress = 0x06fc84;
        public const uint InvincibilityDurationAddress = 0x0705d4;

    }
}
