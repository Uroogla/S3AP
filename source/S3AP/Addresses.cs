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

        public const uint SunriseFirstZoeAddress = 0x0018ea95;
        public const uint SunriseSuperflyZoeAddress = 0x0018f4e5;
        public const uint SunriseCameraZoeAddress = 0x0018df95;
        public const uint SunnyBigRhynocZoeAddress = 0x00190161;
        public const uint SunnyCheckpointZoeAddress = 0x001910d9;
        public const uint CloudMetalArmorZoeAddress = 0x0018ec71;
        public const uint CloudGlideZoeAddress = 0x0018e8a9;
        public const uint MoltenFodderZoeAddress = 0x0014bcfd;
        public const uint SeashellAtlasZoeAddress = 0x0018b699;
        public const uint SeashellHoverZoeAddress = 0x0018a931;
        public const uint SheilaControlsZoeAddress = 0x0016796d;

        public const uint SheilaCutscene = 0x007166a;
        public const uint BuzzDefeated = 0x007166c;
        public const uint SpikeDefeated = 0x007166d;
        public const uint ScorchDefeated = 0x007166e;
        public const uint HunterRescuedCutscene = 0x7169a;
        public const uint ByrdCutscene = 0x0071661;
        public const uint BentleyCutscene = 0x0071662;
        public const uint Agent9Cutscene = 0x007165e;

        public const uint SunrisePondLifeBottleAddress = 0x0018fb96;
        public const uint SunriseSheilaLifeBottleAddress = 0x0018d3b6;
        public const uint SunnyFirstLifeBottleAddress = 0x00190812;
        public const uint SunnySecondLifeBottleAddress = 0x0019086a;
        public const uint CloudLifeBottleAddress = 0x0018cd52;
        public const uint SheilaLifeBottleAddress = 0x00167c57;
        public const uint MoltenLifeBottleAddress = 0x0012d206;
        public const uint SeashellLifeBottleAddress = 0x00189fba;
        public const uint ByrdLifeBottleAddress = 0x00188f6a;
        public const uint BambooBentleyLifeBottleAddress = 0x00186fc7;
        public const uint BambooFirstUnderwaterLifeBottleAddress = 0x00186b22;
        public const uint BambooSecondUnderwaterLifeBottleAddress = 0x00186b7a;
        public const uint BambooThirdUnderwaterLifeBottleAddress = 0x00186bd2;
        public const uint BambooFourthUnderwaterLifeBottleAddress = 0x00186c2a;
        public const uint EnchantedLifeBottleAddress = 0x00191b82;
        public const uint IcyLifeBottleAddress = 0x0018e43a;
        public const uint FireworksAgentLifeBottleAddress = 0x00187a16;
        public const uint FireworksOOBLifeBottleAddress = 0x0018dba2;
        public const uint FleetSkateboardingLifeBottleAddress = 0x0016d94a;
        public const uint FleetAcidLifeBottleAddress = 0x0018fb02;
        public const uint EveningLifeBottleAddress = 0x00192d6e;
        public const uint CharmedFirstLifeBottleAddress = 0x0018f5e2;
        public const uint CharmedSecondLifeBottleAddress = 0x0018f63a;
        public const uint DesertInsideLifeBottleAddress = 0x0018153e;
        public const uint DesertOutsideLifeBottleAddress = 0x001814e6;
        public const uint HauntedLifeBottleAddress = 0x0018f4be;
        public const uint DinoLifeBottleAddress = 0x0018f5ca;

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
    }
}
