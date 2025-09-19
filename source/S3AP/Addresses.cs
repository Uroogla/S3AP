using System.Collections.Generic;

namespace S3AP
{
    public static class Addresses
    {
        public const uint TotalEggAddress = 0x0006C740;
        public const uint EggStartAddress = 0x000703E0;
        public const uint CurrentLevelAddress = 0x0006c69c;
        public const uint CurrentSubareaAddress = 0x0006c6a8; // 0 for main area, 1/2/3 for subareas.
        public const uint LocalDifficultySettingAddress = 0x0006c8a4; // byte
        public const uint GlobalDifficultySettingAddress = 0x0006c888; // byte
        public const uint IsInDemoMode = 0x0006c758;
        public const uint GameStatus = 0x0006e424;
        public const uint SpyroState = 0x70450;
        public const uint NextWarpAddress = 0x0006c8a8;
        public const uint TransportMenuAddress = 0x0007023a; // Where the balloon/rocket will take Spyro.
        // The values at this and the following 3 bytes seem to be 0 only on reset.
        public const uint ResetCheckAddress = 0x0006e434;
        public const uint PlayerLives = 0x0006c864;
        public const uint PlayerHealth = 0x00070688;
        public const uint PlayerMaxHealth = 0x000658e4;
        public const uint PlayerMaxHealthIsModded = 0x0006fc85;
        // Each Sparx level's health is in a different memory address.
        public const uint PlayerHealthCrawdad = 0x00143658;
        public const uint PlayerHealthSpider = 0x001036d0;
        public const uint PlayerHealthStarfish = 0x0013150c;
        public const uint PlayerHealthBugbot = 0x000f1bd0;
        // Unclear what exactly these represent, but probably something like range, speed and range between gems
        // Sparx will collect without returning directly to Spyro.
        public const uint SparxRange = 0x000658e8;
        public const uint SparxRangeHelper1 = 0x000658ec;
        public const uint SparxRangeHelper2 = 0x000658f0;
        public const uint SparxGemFinder = 0x000658f8;
        public const uint SparxBreakBaskets = 0x000658f4;

        public const uint TotalGemAddress = 0x0006c7fc;
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

        public const uint CloudBellowsUnlock = 0x00066f84;
        public const uint SpookyDoorUnlock = 0x00066f88;
        public const uint SheilaUnlock = 0x00066f8c;
        public const uint IcyNancyUnlock = 0x00066f90;
        public const uint MoltenThievesUnlock = 0x00066f94;
        public const uint CharmedStairsUnlock = 0x00066f98;
        public const uint SgtByrdUnlock = 0x00066f9c;
        public const uint BentleyUnlock = 0x00066fa0;
        public const uint DesertDoorUnlock = 0x00066fa4;
        public const uint Agent9Unlock = 0x00066fa8;
        public const uint FrozenHockeyUnlock = 0x00066fac;
        public const uint CrystalBridgeUnlock = 0x00066fb0;

        public static readonly List<uint> SunriseFirstZoeAddress = [0x00071b9b, 4];
        public static readonly List<uint> SunriseSuperflyZoeAddress = [0x00071b9f, 2];
        public static readonly List<uint> SunriseCameraZoeAddress = [0x00071b97, 4];
        public static readonly List<uint> SunnyBigRhynocZoeAddress = [0x00071bb1, 4];
        public static readonly List<uint> SunnyCheckpointZoeAddress = [0x00071bb7, 1];
        public static readonly List<uint> CloudMetalArmorZoeAddress = [0x00071bdb, 3];
        public static readonly List<uint> CloudGlideZoeAddress = [0x00071bda, 0];
        public static readonly List<uint> MoltenFodderZoeAddress = [0x00071bf8, 6];
        public static readonly List<uint> SeashellAtlasZoeAddress = [0x00071c1c, 2];
        public static readonly List<uint> SeashellHoverZoeAddress = [0x00071c17, 3];
        public static readonly List<uint> SheilaControlsZoeAddress = [0x00071c56, 1];

        public const uint SheilaCutscene = 0x007166a;
        public const uint BuzzDefeated = 0x007166c;
        public const uint SpikeDefeated = 0x007166d;
        public const uint ScorchDefeated = 0x007166e;
        public const uint HunterRescuedCutscene = 0x7169a;
        public const uint ByrdCutscene = 0x0071661;
        public const uint BentleyCutscene = 0x0071662;
        public const uint Agent9Cutscene = 0x007165e;

        public const uint MoltenUnlocked = 0x71652;
        public const uint SeashellUnlocked = 0x7165b;
        public const uint SunriseLevelsComplete = 0x71666;
        public const uint MiddayLevelsComplete = 0x71667;
        public const uint EveningLevelsComplete = 0x71668;
        public const uint EveningBianca = 0x71673;

        public static readonly List<uint> SunrisePondLifeBottleAddress = [0x00071ba1, 6];
        public static readonly List<uint> SunriseSheilaLifeBottleAddress = [0x00071b93, 2];
        public static readonly List<uint> SunnyFirstLifeBottleAddress = [0x00071bb4, 0];
        public static readonly List<uint> SunnySecondLifeBottleAddress = [0x00071bb4, 1];
        public static readonly List<uint> CloudLifeBottleAddress = [0x00071bd0, 1];
        public static readonly List<uint> SheilaLifeBottleAddress = [0x00071c57, 2];
        public static readonly List<uint> MoltenLifeBottleAddress = [0x00071c00, 2];
        public static readonly List<uint> SeashellLifeBottleAddress = [0x00071c14, 0];
        public static readonly List<uint> ByrdLifeBottleAddress = [0x00071d76, 2];
        public static readonly List<uint> BambooBentleyLifeBottleAddress = [0x00071d41, 5];
        public static readonly List<uint> BambooFirstUnderwaterLifeBottleAddress = [0x00071d36, 4];
        public static readonly List<uint> BambooSecondUnderwaterLifeBottleAddress = [0x00071d36, 5];
        public static readonly List<uint> BambooThirdUnderwaterLifeBottleAddress = [0x00071d36, 7];
        public static readonly List<uint> BambooFourthUnderwaterLifeBottleAddress = [0x00071d36, 6];
        public static readonly List<uint> EnchantedLifeBottleAddress = [0x00071cf6, 5];
        public static readonly List<uint> IcyLifeBottleAddress = [0x00071cd8, 7];
        public static readonly List<uint> FireworksAgentLifeBottleAddress = [0x00071e46, 0];
        public static readonly List<uint> FireworksOOBLifeBottleAddress = [0x00071e35, 5];
        public static readonly List<uint> FleetSkateboardingLifeBottleAddress = [0x00071e2d, 2];
        public static readonly List<uint> FleetAcidLifeBottleAddress = [0x00071e18, 4];
        public static readonly List<uint> EveningLifeBottleAddress = [0x00071dd3, 6];
        public static readonly List<uint> CharmedFirstLifeBottleAddress = [0x00071e57, 2];
        public static readonly List<uint> CharmedSecondLifeBottleAddress = [0x00071e57, 3];
        public static readonly List<uint> DesertInsideLifeBottleAddress = [0x00071f32, 4];
        public static readonly List<uint> DesertOutsideLifeBottleAddress = [0x00071f32, 3];
        public static readonly List<uint> HauntedLifeBottleAddress = [0x00071f50, 6];
        public static readonly List<uint> DinoLifeBottleAddress = [0x00071f75, 0];

        public const uint SunnyLizardsCount = 0x001865ec; // byte
        public const uint SunnySkateScore = 0x001876e4; // short
        public const uint BlutoHealth = 0x00162574; // byte
        public const uint SleepyheadHealth = 0x00159670; // byte
        public const uint EnchantedSkateScore = 0x00181428; // short
        public const uint YetiBoxingHealth = 0x0015d050; // byte
        public const uint LostFleetNitro = 0x00173944; // short
        public static readonly uint[] LostFleetSubAddresses = [
            0x0015b126, 0x0015b17e, 0x0015b286, 0x0015b3e6, 0x0015b43e, // Subs 1
            0x0015ae0e, 0x0015aebe, 0x0015af16, 0x0015afc6, 0x0015b01e, 0x0015b2de // Subs 2
        ]; // bytes
        public const uint WhackAMoleCount = 0x0016bd64; // byte
        public static readonly uint[] DesertSharkAddresses = [0x0016b72c, 0x0016b784, 0x0016b7dc, 0x0016b834, 0x0016b88c, 0x0016b93c, 0x0016b994];
        public const uint TanksCount = 0x00177964; // byte
        public const uint MaxTanksCount = 0x0006757a; // byte; also the "max" for any bottom right HUD value. Read-only.
        public const uint RedRocketCount = 0x000705f0;
        public const uint GreenRocketCount = 0x0015bf64;
        public const uint HasGreenRocketAddress = 0x0015bf68;

        public const uint SunriseGemMask = 0x00071b90;
        public const uint SunnyGemMask = 0x00071bb0;
        public const uint CloudGemMask = 0x00071bd0;
        public const uint MoltenGemMask = 0x00071bf0;
        public const uint SeashellGemMask = 0x00071c10;
        public const uint MushroomGemMask = 0x00071c30;
        public const uint SheilaGemMask = 0x00071c50;
        public const uint CrawdadGemMask = 0x00071c91;
        public const uint MiddayGemMask = 0x00071cb0;
        public const uint IcyGemMask = 0x00071cd0;
        public const uint EnchantedGemMask = 0x00071cf0;
        public const uint SpookyGemMask = 0x00071d10;
        public const uint BambooGemMask = 0x00071d30;
        public const uint CountryGemMask = 0x00071d50;
        public const uint ByrdGemMask = 0x00071d70;
        public const uint SpiderGemMask = 0x00071db0;
        public const uint EveningGemMask = 0x00071dd0;
        public const uint FrozenGemMask = 0x00071df0;
        public const uint FleetGemMask = 0x00071e10;
        public const uint FireworksGemMask = 0x00071e30;
        public const uint CharmedGemMask = 0x00071e50;
        public const uint HoneyGemMask = 0x00071e70;
        public const uint BentleyGemMask = 0x00071e90;
        public const uint StarfishGemMask = 0x00071ed2;
        public const uint MidnightGemMask = 0x00071ef0;
        public const uint CrystalGemMask = 0x00071f10;
        public const uint DesertGemMask = 0x00071f30;
        public const uint HauntedGemMask = 0x00071f50;
        public const uint DinoGemMask = 0x00071f70;
        public const uint HarborGemMask = 0x00071f90;
        public const uint Agent9GemMask = 0x00071fb0;
        public const uint BugbotGemMask = 0x00071ff0;

        public const uint MoltenEggReq = 0x19560c;
        public const uint SeashellEggReq = 0x194F1c;
        public const uint MushroomEggReq = 0x197cc0;
        public const uint SpookyEggReq = 0x199938;
        public const uint BambooEggReq = 0x199c70;
        public const uint CountryEggReq = 0x199c5c;
        public const uint FireworksEggReq = 0x19a088;
        public const uint CharmedEggReq = 0x19a09c;
        public const uint HoneyEggReq = 0x19a0b0;
        public const uint HauntedEggReq = 0x19bfe0;
        public const uint DinoEggReq = 0x19bea0;
        public const uint HarborEggReq = 0x19be8c;

        public const uint BlackLabelAtlasAddress = 0x0006c4d8;
        public const uint GreenLabelAtlasAddress = 0x0006c5b8;
    }
}
