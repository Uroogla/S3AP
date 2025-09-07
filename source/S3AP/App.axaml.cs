using Archipelago.Core;
using Archipelago.Core.AvaloniaGUI.Models;
using Archipelago.Core.AvaloniaGUI.ViewModels;
using Archipelago.Core.AvaloniaGUI.Views;
using Archipelago.Core.GameClients;
using Archipelago.Core.Models;
using Archipelago.Core.Traps;
using Archipelago.Core.Util;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Packets;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.OpenGL;
using Newtonsoft.Json;
using ReactiveUI;
using S3AP.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Timers;
using static S3AP.Models.Enums;

namespace S3AP;

public partial class App : Application
{
    public static MainWindowViewModel Context;
    public static ArchipelagoClient Client { get; set; }
    public static List<ILocation> GameLocations { get; set; }
    private static readonly object _lockObject = new object();
    private static Queue<string> _cosmeticEffects { get; set; }
    private static Dictionary<string, string> _hintsList { get; set; }
    private static bool _hasSubmittedGoal { get; set; }
    private static bool _useQuietHints { get; set; }
    private static List<string> _easyChallenges { get; set; }
    private static ProgressiveSparxHealthOptions _sparxOption {  get; set; }
    private static Timer _sparxTimer { get; set; }
    private static Timer _loadGameTimer { get; set; }
    private static int _worldKeys { get; set; }
    private static Timer _worldKeysTimer { get; set; }
    private static int _progressiveBasketBreaks { get; set; }
    private static Timer _cosmeticsTimer { get; set; }
    private static Timer _minigamesTimer { get; set; }
    private static Timer _sparxPowerTimer { get; set; }
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    private static bool IsRunningAsAdministrator()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Start();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Context
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainWindow
            {
                DataContext = Context
            };
        }
        base.OnFrameworkInitializationCompleted();
    }
    public void Start()
    {
        Context = new MainWindowViewModel("0.6.2");
        Context.ClientVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
        Context.ConnectClicked += Context_ConnectClicked;
        Context.CommandReceived += (e, a) =>
        {
            if (string.IsNullOrWhiteSpace(a.Command)) return;
            Client?.SendMessage(a.Command);
            HandleCommand(a.Command);
        };
        Context.ConnectButtonEnabled = true;
        _hintsList = null;
        _hasSubmittedGoal = false;
        _useQuietHints = true;
        _easyChallenges = new List<string>();
        _worldKeys = 0;
        _progressiveBasketBreaks = 0;
        Log.Logger.Information("This Archipelago Client is compatible only with the NTSC-U 1.1 release of Spyro 3 (North America Greatest Hits version).");
        Log.Logger.Information("Trying to play with a different version will not work and may release all of your locations at the start.");
        if (!IsRunningAsAdministrator())
        {
            Log.Logger.Warning("You do not appear to be running this client as an administrator.");
            Log.Logger.Warning("This may result in errors or crashes when trying to connect to Duckstation.");
        }
    }
    private void HandleCommand(string command)
    {
        if (Client == null || Client.GameState == null || Client.CurrentSession == null) return;
        switch (command)
        {
            case "clearSpyroGameState":
                Log.Logger.Information("Clearing the game state.  Please reconnect to the server while in game to refresh received items.");
                Client.ForceReloadAllItems();
                break;
            case "useQuietHints":
                Log.Logger.Information("Hints for found locations will not be displayed.  Type 'useVerboseHints' to show them.");
                _useQuietHints = true;
                break;
            case "useVerboseHints":
                Log.Logger.Information("Hints for found locations will be displayed.  Type 'useQuietHints' to show them.");
                _useQuietHints = false;
                break;
            case "showGoal":
                CompletionGoal goal = (CompletionGoal)(int.Parse(Client.Options?.GetValueOrDefault("goal", 0).ToString()));
                string goalText = "";
                switch (goal)
                {
                    case CompletionGoal.Sorceress1:
                        goalText = "Collect 100 eggs and defeat the Sorceress in Sorceress' Lair";
                        break;
                    case CompletionGoal.EggForSale:
                        goalText = "Chase Moneybags in Midnight Mountain";
                        break;
                    case CompletionGoal.Sorceress2:
                        goalText = "Defeat the Sorceress in Super Bonus Round";
                        break;
                    case CompletionGoal.AllSkillPoints:
                        goalText = "Collect all 20 Skill Points";
                        break;
                    case CompletionGoal.Epilogue:
                        goalText = "Defeat the Sorceress in Sorceress' Lair and collect all 20 skill points";
                        break;
                    case CompletionGoal.Spike:
                        goalText = "Defeat Spike in Spike's Arena and collect 36 eggs";
                        break;
                    case CompletionGoal.Scorch:
                        goalText = "Defeat Scorch in Scorch's Pit and collect 65 eggs";
                        break;
                    case CompletionGoal.EggHunt:
                        int eggsNeeded = int.Parse(Client.Options?.GetValueOrDefault("egg_count", 0).ToString());
                        goalText = $"Collect {eggsNeeded} eggs from a reduced egg pool";
                        break;
                    default:
                        goalText = "Error finding your goal";
                        break;
                }
                Log.Logger.Information($"Your goal is: {goalText}");
                break;
        }
    }
    private async void Context_ConnectClicked(object? sender, ConnectClickedEventArgs e)
    {
        if (Client != null)
        {
            Client.CancelMonitors();
            Client.Connected -= OnConnected;
            Client.Disconnected -= OnDisconnected;
            Client.ItemReceived -= ItemReceived;
            Client.MessageReceived -= Client_MessageReceived;
            Client.LocationCompleted -= Client_LocationCompleted;
            Client.CurrentSession.Locations.CheckedLocationsUpdated -= Locations_CheckedLocationsUpdated;
        }
        DuckstationClient? client = null;
        try
        {
            client = new DuckstationClient();
        }
        catch (ArgumentException ex)
        {
            Log.Logger.Warning("Duckstation not running, open Duckstation and launch the game before connecting!");
            return;
        }
        var DuckstationConnected = client.Connect();
        if (!DuckstationConnected)
        {
            Log.Logger.Warning("Duckstation not running, open Duckstation and launch the game before connecting!");
            return;
        }
        Client = new ArchipelagoClient(client);
        Client.ShouldSaveStateOnItemReceived = false;

        Memory.GlobalOffset = Memory.GetDuckstationOffset();

        Client.Connected += OnConnected;
        Client.Disconnected += OnDisconnected;

        await Client.Connect(e.Host, "Spyro 3");
        if (!Client.IsConnected)
        {
            Log.Logger.Error("Your host seems to be invalid.  Please confirm that you have entered it correctly.");
            return;
        }
        _cosmeticEffects = new Queue<string>();
        Client.LocationCompleted += Client_LocationCompleted;
        Client.CurrentSession.Locations.CheckedLocationsUpdated += Locations_CheckedLocationsUpdated;
        Client.MessageReceived += Client_MessageReceived;
        Client.ItemReceived += ItemReceived;
        Client.EnableLocationsCondition = () => Helpers.IsInGame();
        await Client.Login(e.Slot, !string.IsNullOrWhiteSpace(e.Password) ? e.Password : null);
        if (Client.Options?.Count > 0)
        {
            GemsanityOptions gemsanityOption = (GemsanityOptions)int.Parse(Client.Options?.GetValueOrDefault("enable_gemsanity", "0").ToString());
            int slot = Client.CurrentSession.ConnectionInfo.Slot;
            Dictionary<string, object> obj = await Client.CurrentSession.DataStorage.GetSlotDataAsync(slot);
            List<int> gemsanityIDs = new List<int>();
            if (obj.TryGetValue("gemsanity_ids", out var value))
            {
                if (value != null)
                {
                    gemsanityIDs = System.Text.Json.JsonSerializer.Deserialize<List<int>>(value.ToString());
                }
            }
            GameLocations = Helpers.BuildLocationList(includeGemsanity: gemsanityOption != GemsanityOptions.Off, gemsanityIDs: gemsanityIDs);
            int eggs = CalculateCurrentEggs();
            int skillPoints = CalculateCurrentSkillPoints();
            bool beatenSorceress = Client.GameState?.ReceivedItems.Any(x => x != null && x.Name == "Sorceress Defeated") ?? false;
            string defeatedSorceressText = beatenSorceress ? "you have defeated the sorceress" : "you have not defeated the sorceress";
            Log.Logger.Information($"You have {eggs} eggs, {skillPoints} skill points, and {defeatedSorceressText}.");
            GameLocations = GameLocations.Where(x => x != null && !Client.CurrentSession.Locations.AllLocationsChecked.Contains(x.Id)).ToList();
            Client.MonitorLocations(GameLocations);
            Log.Logger.Information("Warnings and errors above are okay if this is your first time connecting to this multiworld server.");
        }
        else
        {
            Log.Logger.Error("Failed to login.  Please check your host, name, and password.");
        }
    }

    private void Client_LocationCompleted(object? sender, LocationCompletedEventArgs e)
    {
        if (Client.GameState == null || Client.CurrentSession == null) return;
        var currentEggs = CalculateCurrentEggs();
        GemsanityOptions gemsanityOption = (GemsanityOptions)int.Parse(Client.Options?.GetValueOrDefault("enable_gemsanity", "0").ToString());
        if (gemsanityOption != GemsanityOptions.Off)
        {
            CalculateCurrentGems();
        }
        CheckGoalCondition();
    }

    private async void ItemReceived(object? o, ItemReceivedEventArgs args)
    {
        if (Client.GameState == null || Client.CurrentSession == null) return;
        Log.Logger.Debug($"Item Received: {JsonConvert.SerializeObject(args.Item)}");
        int currentHealth;
        switch (args.Item.Name)
        {
            case "Egg":
                var currentEggs = CalculateCurrentEggs();
                CheckGoalCondition();
                break;
            case "Skill Point":
                CheckGoalCondition();
                break;
            case "Extra Life":
                var currentLives = Memory.ReadShort(Addresses.PlayerLives);
                Memory.Write(Addresses.PlayerLives, (short)(Math.Min(99, currentLives + 1)));
                break;
            case "Lag Trap":
                //RunLagTrap();
                break;
            case "(Over)heal Sparx":
                // Collecting a skill point provides a full heal, so wait for that to complete first.
                await Task.Run(async () =>
                {
                    await Task.Delay(3000);
                    currentHealth = Memory.ReadByte(Addresses.PlayerHealth);
                    // Going too high creates too many particles for the game to handle.
                    Memory.WriteByte(Addresses.PlayerHealth, (byte)(Math.Min(4, currentHealth + 1)));
                });
                break;
            case "Damage Sparx Trap":
                // Collecting a skill point provides a full heal, so wait for that to complete first.
                await Task.Run(async () =>
                {
                    await Task.Delay(3000);
                    currentHealth = Memory.ReadByte(Addresses.PlayerHealth);
                    Memory.WriteByte(Addresses.PlayerHealth, (byte)(Math.Max(currentHealth - 1, 0)));
                });
                break;
            case "Sparxless Trap":
                // Collecting a skill point provides a full heal, so wait for that to complete first.
                await Task.Run(async () =>
                {
                    await Task.Delay(3000);
                    Memory.WriteByte(Addresses.PlayerHealth, (byte)(0));
                });
                break;
            case "Big Head Mode":
            case "Flat Spyro Mode":
            case "Turn Spyro Red":
            case "Turn Spyro Blue":
            case "Turn Spyro Yellow":
            case "Turn Spyro Pink":
            case "Turn Spyro Green":
            case "Turn Spyro Black":
                _cosmeticEffects.Enqueue(args.Item.Name);
                break;
            case "Invincibility (15 seconds)":
                ActivateInvincibility(15);
                break;
            case "Invincibility (30 seconds)":
                ActivateInvincibility(30);
                break;
            case "Moneybags Unlock - Cloud Spires Bellows":
                // Special case this to prevent a rhynoc from spawning in the bellows.
                Memory.Write(Addresses.CloudBellowsUnlock, 0);
                Log.Logger.Information("You can talk to Moneybags to complete this unlock for free.");
                break;
            case "Moneybags Unlock - Spooky Swamp Door":
                UnlockMoneybags(Addresses.SpookyDoorUnlock);
                break;
            case "Moneybags Unlock - Sheila":
                UnlockMoneybags(Addresses.SheilaUnlock, Addresses.SheilaCutscene);
                break;
            case "Moneybags Unlock - Icy Peak Nancy Door":
                UnlockMoneybags(Addresses.IcyNancyUnlock);
                break;
            case "Moneybags Unlock - Molten Crater Thieves Door":
                UnlockMoneybags(Addresses.MoltenThievesUnlock);
                break;
            case "Moneybags Unlock - Charmed Ridge Stairs":
                UnlockMoneybags(Addresses.CharmedStairsUnlock);
                break;
            case "Moneybags Unlock - Sgt. Byrd":
                UnlockMoneybags(Addresses.SgtByrdUnlock, Addresses.ByrdCutscene);
                break;
            case "Moneybags Unlock - Bentley":
                UnlockMoneybags(Addresses.BentleyUnlock, Addresses.BentleyCutscene);
                break;
            case "Moneybags Unlock - Desert Ruins Door":
                UnlockMoneybags(Addresses.DesertDoorUnlock);
                break;
            case "Moneybags Unlock - Agent 9":
                UnlockMoneybags(Addresses.Agent9Unlock, Addresses.Agent9Cutscene);
                break;
            case "Moneybags Unlock - Frozen Altars Cat Hockey Door":
                UnlockMoneybags(Addresses.FrozenHockeyUnlock);
                break;
            case "Moneybags Unlock - Crystal Islands Bridge":
                UnlockMoneybags(Addresses.CrystalBridgeUnlock);
                break;
            case "Hint 1":
            case "Hint 2":
            case "Hint 3":
            case "Hint 4":
            case "Hint 5":
            case "Hint 6":
            case "Hint 7":
            case "Hint 8":
            case "Hint 9":
            case "Hint 10":
            case "Hint 11":
                GetZoeHint(args.Item.Name);
                break;
            case "Progressive Sparx Health Upgrade":
                break;
            case "World Key":
                _worldKeys++;
                break;
            case "Increased Sparx Range":
                Memory.Write(Addresses.SparxRange, (short)3072);
                Memory.Write(Addresses.SparxRangeHelper1, (short)525);
                Memory.Write(Addresses.SparxRangeHelper2, (short)960);
                break;
            case "Sparx Gem Finder":
                Memory.WriteByte(Addresses.SparxGemFinder, 1);
                break;
            case "Extra Hit Point":
                Memory.WriteByte(Addresses.PlayerMaxHealth, 4);
                Memory.WriteByte(Addresses.PlayerMaxHealthIsModded, 1);
                break;
            case "Progressive Sparx Basket Break":
                _progressiveBasketBreaks++;
                Memory.WriteByte(Addresses.SparxBreakBaskets, (byte)_progressiveBasketBreaks);
                break;
        }
        if (args.Item.Name.EndsWith(" Defeated")) {
            CheckGoalCondition();
        }
        else if (args.Item.Name.EndsWith(" Gem") || args.Item.Name.EndsWith(" Gems"))
        {
            CalculateCurrentGems();
            CheckGoalCondition();
        }
    }
    private static async void GetZoeHint(string hintName)
    {
        if (_hintsList == null)
        {
            int slot = Client.CurrentSession.ConnectionInfo.Slot;
            Dictionary<string, object> obj = await Client.CurrentSession.DataStorage.GetSlotDataAsync(slot);
            if (obj.TryGetValue("hints", out var value))
            {
                if (value != null)
                {
                    _hintsList = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(value.ToString());
                }
            }
        }
        if (_hintsList != null)
        {
            Log.Logger.Information(_hintsList.GetValueOrDefault($"{hintName} Text", "Zoe says that there's been an error."));
            int hintLocationID = int.Parse(_hintsList.GetValueOrDefault($"{hintName} ID", "-1"));
            if (hintLocationID != -1)
            {
                // In Archipelago 0.6.3, LocationScouts with CreateAsHint = 2 is superseded by CreateHints,
                // but this likely requires updating MultiClient within Archipelago.Core.
                // TODO: Support both options depending on function availability.
                LocationScoutsPacket locationScoutsPacket = new LocationScoutsPacket();
                locationScoutsPacket.Locations = [hintLocationID];
                locationScoutsPacket.CreateAsHint = 2;
                Client.CurrentSession.Socket.SendPacketAsync(locationScoutsPacket);
            }
        }
    }
    private static async void HandleMaxSparxHealth(object source, ElapsedEventArgs e)
    {
        if (!Helpers.IsInGame() || Client.GameState == null || Client.CurrentSession == null)
        {
            return;
        }
        byte currentHealth = Memory.ReadByte(Addresses.PlayerHealth);
        byte _sparxUpgrades = (byte)(Client.GameState?.ReceivedItems.Where(x => x.Name == "Progressive Sparx Health Upgrade").Count() ?? 0);
        _sparxUpgrades += (byte)(Client.GameState?.ReceivedItems.Where(x => x.Name == "Extra Hit Point").Count() ?? 0);
        if (
            (Client.GameState?.ReceivedItems.Where(x => x.Name == "Starfish Reef Complete").Count() ?? 0) > 0 &&
            (int.Parse(Client.Options?.GetValueOrDefault("sparx_power_settings", "0").ToString()) == 0) 
        )
        {
            _sparxUpgrades++;
        }
        if (_sparxOption == ProgressiveSparxHealthOptions.Blue)
        {
            _sparxUpgrades += 2;
        }
        else if (_sparxOption == ProgressiveSparxHealthOptions.Green)
        {
            _sparxUpgrades += 1;
        }
        else if (_sparxOption == ProgressiveSparxHealthOptions.TrueSparxless)
        {
            _sparxUpgrades = 0;
        }
        if (currentHealth > _sparxUpgrades)
        {
            Memory.WriteByte(Addresses.PlayerHealth, _sparxUpgrades);
        }
        LevelInGameIDs currentLevel = (LevelInGameIDs)Memory.ReadByte(Addresses.CurrentLevelAddress);
        byte maxSparxHealth = (byte)(Math.Min(_sparxUpgrades * 2, 6));
        if (maxSparxHealth == 0)
        {
            maxSparxHealth = 1;
        }
        if (currentLevel == LevelInGameIDs.CrawdadFarm)
        {
            byte currentSparxHealth = Memory.ReadByte(Addresses.PlayerHealthCrawdad);
            if (currentSparxHealth > maxSparxHealth)
            {
                Memory.WriteByte(Addresses.PlayerHealthCrawdad, maxSparxHealth);
            }
        }
        else if (currentLevel == LevelInGameIDs.SpiderTown)
        {
            byte currentSparxHealth = Memory.ReadByte(Addresses.PlayerHealthSpider);
            if (currentSparxHealth > maxSparxHealth)
            {
                Memory.WriteByte(Addresses.PlayerHealthSpider, maxSparxHealth);
            }
        }
        else if (currentLevel == LevelInGameIDs.StarfishReef)
        {
            byte currentSparxHealth = Memory.ReadByte(Addresses.PlayerHealthStarfish);
            if (currentSparxHealth > maxSparxHealth)
            {
                Memory.WriteByte(Addresses.PlayerHealthStarfish, maxSparxHealth);
            }
        }
        else if (currentLevel == LevelInGameIDs.BugbotFactory)
        {
            byte currentSparxHealth = Memory.ReadByte(Addresses.PlayerHealthBugbot);
            if (currentSparxHealth > maxSparxHealth)
            {
                Memory.WriteByte(Addresses.PlayerHealthBugbot, maxSparxHealth);
            }
        }
        if (_sparxUpgrades == 4)
        {
            _sparxTimer.Enabled = false;
        }
    }
    private static async void HandleWorldKeys(object source, ElapsedEventArgs e)
    {
        if (!Helpers.IsInGame() || Client.GameState == null || Client.CurrentSession == null)
        {
            return;
        }
        LevelInGameIDs currentLevel = (LevelInGameIDs)Memory.ReadByte(Addresses.CurrentLevelAddress);
        byte transportWarpLocation = Memory.ReadByte(Addresses.TransportMenuAddress);
        if (currentLevel == LevelInGameIDs.BuzzsDungeon && _worldKeys < 1)
        {
            Memory.WriteByte(Addresses.NextWarpAddress, (byte)LevelInGameIDs.SunriseSpring);
        }
        else if (currentLevel == LevelInGameIDs.SpikesArena && _worldKeys < 2)
        {
            Memory.WriteByte(Addresses.NextWarpAddress, (byte)LevelInGameIDs.MiddayGardens);
        }
        else if (currentLevel == LevelInGameIDs.ScorchsPit && _worldKeys < 3)
        {
            Memory.WriteByte(Addresses.NextWarpAddress, (byte)LevelInGameIDs.EveningLake);
        }
        if (transportWarpLocation == 20 && _worldKeys < 1)
        {
            byte isBuzzDefeated = Memory.ReadByte(Addresses.BuzzDefeated);
            if (isBuzzDefeated == 1)
            {
                Memory.WriteByte(Addresses.TransportMenuAddress, (byte)LevelInGameIDs.SunriseSpring);
            }
        }
        else if (transportWarpLocation == 30 && _worldKeys < 2)
        {
            byte isSpikeDefeated = Memory.ReadByte(Addresses.SpikeDefeated);
            if (isSpikeDefeated == 1)
            {
                Memory.WriteByte(Addresses.TransportMenuAddress, (byte)LevelInGameIDs.MiddayGardens);
            }
        }
        else if (transportWarpLocation == 40 && _worldKeys < 3)
        {
            byte isScorchDefeated = Memory.ReadByte(Addresses.ScorchDefeated);
            if (isScorchDefeated == 1)
            {
                Memory.WriteByte(Addresses.HunterRescuedCutscene, 1);
                Memory.WriteByte(Addresses.TransportMenuAddress, (byte)LevelInGameIDs.EveningLake);
            }
        }
        if (_worldKeys == 3)
        {
            _worldKeysTimer.Enabled = false;
        }
    }
    private static async void HandleSparxPowers(object source, ElapsedEventArgs e)
    {
        if (!Helpers.IsInGame() || Client.GameState == null || Client.CurrentSession == null)
        {
            return;
        }
        // We need to override the powerups given by Zoe for completing a Sparx Level.
        var extendedRange = Client.GameState?.ReceivedItems.Where(x => x.Name == "Increased Sparx Range").Count() ?? 0;
        var gemFinder = Client.GameState?.ReceivedItems.Where(x => x.Name == "Sparx Gem Finder").Count() ?? 0;
        var extraHitPoint = Client.GameState?.ReceivedItems.Where(x => x.Name == "Extra Hit Point").Count() ?? 0;
        if (extendedRange == 0)
        {
            Memory.Write(Addresses.SparxRange, (short)2062);
            Memory.Write(Addresses.SparxRangeHelper1, (short)350);
            Memory.Write(Addresses.SparxRangeHelper2, (short)640);
        } else
        {
            Memory.Write(Addresses.SparxRange, (short)3072);
            Memory.Write(Addresses.SparxRangeHelper1, (short)525);
            Memory.Write(Addresses.SparxRangeHelper2, (short)960);
        }
        if (gemFinder == 0)
        {
            Memory.WriteByte(Addresses.SparxGemFinder, 0);
        } else
        {
            Memory.WriteByte(Addresses.SparxGemFinder, 1);
        }
        if (extraHitPoint == 0)
        {
            Memory.WriteByte(Addresses.PlayerMaxHealth, 3);
            Memory.WriteByte(Addresses.PlayerMaxHealthIsModded, 0);
        } else
        {
            Memory.WriteByte(Addresses.PlayerMaxHealth, 4);
            Memory.WriteByte(Addresses.PlayerMaxHealthIsModded, 1);
        }
        Memory.WriteByte(Addresses.SparxBreakBaskets, (byte)_progressiveBasketBreaks);
    }
    private static void HandleMinigames(object source, ElapsedEventArgs e)
    {
        // NOTE: Be very careful here, as writing to the wrong address or at the wrong time can crash the game.
        if (!Helpers.IsInGame() || Client.GameState == null || Client.CurrentSession == null)
        {
            return;
        }
        int eggs = CalculateCurrentEggs();
        CalculateCurrentGems();
        LevelInGameIDs currentLevel = (LevelInGameIDs)Memory.ReadByte(Addresses.CurrentLevelAddress);
        byte currentSubarea = Memory.ReadByte(Addresses.CurrentSubareaAddress);
        CompletionGoal goal = (CompletionGoal)int.Parse(Client.Options?.GetValueOrDefault("goal", "0").ToString());
        int openWorld = int.Parse(Client.Options?.GetValueOrDefault("open_world", "0").ToString());
        double multiplier = 1.0;
        if (goal == CompletionGoal.EggHunt)
        {
            multiplier = int.Parse(Client.Options?.GetValueOrDefault("egg_count", "0").ToString()) / 100.0;
            if (openWorld == 0)
            {
                if (currentLevel == LevelInGameIDs.SunriseSpring)
                {
                    Memory.WriteByte(Addresses.MoltenEggReq, (byte)Math.Floor(10 * multiplier));
                    Memory.WriteByte(Addresses.SeashellEggReq, (byte)Math.Floor(14 * multiplier));
                    Memory.WriteByte(Addresses.MushroomEggReq, (byte)Math.Floor(20 * multiplier));
                }
                else if (currentLevel == LevelInGameIDs.MiddayGardens)
                {
                    Memory.WriteByte(Addresses.SpookyEggReq, (byte)Math.Floor(25 * multiplier));
                    Memory.WriteByte(Addresses.BambooEggReq, (byte)Math.Floor(30 * multiplier));
                    Memory.WriteByte(Addresses.CountryEggReq, (byte)Math.Floor(36 * multiplier));
                }
                else if (currentLevel == LevelInGameIDs.EveningLake)
                {
                    Memory.WriteByte(Addresses.FireworksEggReq, (byte)Math.Floor(50 * multiplier));
                    Memory.WriteByte(Addresses.CharmedEggReq, (byte)Math.Floor(58 * multiplier));
                    Memory.WriteByte(Addresses.HoneyEggReq, (byte)Math.Floor(65 * multiplier));
                }
                else if (currentLevel == LevelInGameIDs.MidnightMountain)
                {
                    Memory.WriteByte(Addresses.HauntedEggReq, (byte)Math.Floor(70 * multiplier));
                    Memory.WriteByte(Addresses.DinoEggReq, (byte)Math.Floor(80 * multiplier));
                    Memory.WriteByte(Addresses.HarborEggReq, (byte)Math.Floor(90 * multiplier));
                }
            }
        }
        if (openWorld > 0)
        {
            if (currentLevel == LevelInGameIDs.SunriseSpring)
            {
                if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Molten Crater Unlock").Count() ?? 0) > 0)
                {
                    Memory.WriteByte(Addresses.MoltenEggReq, (byte)1);
                }
                else
                {
                    Memory.WriteByte(Addresses.MoltenEggReq, (byte)151);
                }

                if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Seashell Shore Unlock").Count() ?? 0) > 0)
                {
                    Memory.WriteByte(Addresses.SeashellEggReq, (byte)1);
                }
                else
                {
                    Memory.WriteByte(Addresses.SeashellEggReq, (byte)151);
                }

                if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Mushroom Speedway Unlock").Count() ?? 0) > 0)
                {
                    Memory.WriteByte(Addresses.MushroomEggReq, (byte)1);
                }
                else
                {
                    Memory.WriteByte(Addresses.MushroomEggReq, (byte)151);
                }
            }
            else if (currentLevel == LevelInGameIDs.MiddayGardens)
            {
                if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Spooky Swamp Unlock").Count() ?? 0) > 0)
                {
                    Memory.WriteByte(Addresses.SpookyEggReq, (byte)1);
                }
                else
                {
                    Memory.WriteByte(Addresses.SpookyEggReq, (byte)151);
                }

                if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Bamboo Terrace Unlock").Count() ?? 0) > 0)
                {
                    Memory.WriteByte(Addresses.BambooEggReq, (byte)1);
                }
                else
                {
                    Memory.WriteByte(Addresses.BambooEggReq, (byte)151);
                }

                if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Country Speedway Unlock").Count() ?? 0) > 0)
                {
                    Memory.WriteByte(Addresses.CountryEggReq, (byte)1);
                }
                else
                {
                    Memory.WriteByte(Addresses.CountryEggReq, (byte)151);
                }
            }
            else if (currentLevel == LevelInGameIDs.EveningLake)
            {
                if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Fireworks Factory Unlock").Count() ?? 0) > 0)
                {
                    Memory.WriteByte(Addresses.FireworksEggReq, (byte)1);
                }
                else
                {
                    Memory.WriteByte(Addresses.FireworksEggReq, (byte)151);
                }

                if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Charmed Ridge Unlock").Count() ?? 0) > 0)
                {
                    Memory.WriteByte(Addresses.CharmedEggReq, (byte)1);
                }
                else
                {
                    Memory.WriteByte(Addresses.CharmedEggReq, (byte)151);
                }

                if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Honey Speedway Unlock").Count() ?? 0) > 0)
                {
                    Memory.WriteByte(Addresses.HoneyEggReq, (byte)1);
                }
                else
                {
                    Memory.WriteByte(Addresses.HoneyEggReq, (byte)151);
                }
            }
            else if (currentLevel == LevelInGameIDs.MidnightMountain)
            {
                if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Haunted Tomb Unlock").Count() ?? 0) > 0)
                {
                    Memory.WriteByte(Addresses.HauntedEggReq, (byte)1);
                }
                else
                {
                    Memory.WriteByte(Addresses.HauntedEggReq, (byte)151);
                }

                if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Dino Mines Unlock").Count() ?? 0) > 0)
                {
                    Memory.WriteByte(Addresses.DinoEggReq, (byte)1);
                }
                else
                {
                    Memory.WriteByte(Addresses.DinoEggReq, (byte)151);
                }

                if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Harbor Speedway Unlock").Count() ?? 0) > 0)
                {
                    Memory.WriteByte(Addresses.HarborEggReq, (byte)1);
                }
                else
                {
                    Memory.WriteByte(Addresses.HarborEggReq, (byte)151);
                }
            }
            if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Molten Crater Unlock").Count() ?? 0) > 0)
            {
                Memory.WriteByte(Addresses.MoltenUnlocked, 1);
            }
            if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Seashell Shore Unlock").Count() ?? 0) > 0)
            {
                Memory.WriteByte(Addresses.SeashellUnlocked, 1);
            }
        }


        // For some challenges, it makes more sense to adjust local adaptive difficulty than modify other values.
        if (
            _easyChallenges.Contains("easy_tunnels") &&
            (
                currentLevel == LevelInGameIDs.SeashellShore && currentSubarea == 3 ||    // Tunnel
                currentLevel == LevelInGameIDs.DinoMines && currentSubarea == 2           // Tunnel
            ) ||
            (
                _easyChallenges.Contains("easy_sheila_bombing") &&
                currentLevel == LevelInGameIDs.SpookySwamp && currentSubarea == 2         // Sheila Subarea
            )
        )
        {
            Memory.WriteByte(Addresses.LocalDifficultySettingAddress, 0);
        }

        // For other challenges, we can set values in RAM to make the challenge easier.
        if (
            _easyChallenges.Contains("easy_skateboarding") &&
            currentLevel == LevelInGameIDs.SunnyVilla && currentSubarea == 2              // Skatepark
        )
        {
            byte currentLizards = Memory.ReadByte(Addresses.SunnyLizardsCount);
            short currentScore = Memory.ReadShort(Addresses.SunnySkateScore);
            if (currentLizards < 14)
            {
                Memory.Write(Addresses.SunnyLizardsCount, (byte)14);
            }
            if (currentScore < 3199)
            {
                Memory.Write(Addresses.SunnySkateScore, (short)3199);
            }
        }
        else if (
            _easyChallenges.Contains("easy_bluto") &&
            currentLevel == LevelInGameIDs.SeashellShore && currentSubarea == 2          // Bluto
        )
        {
            byte currentEnemyHealth = Memory.ReadByte(Addresses.BlutoHealth);
            if (currentEnemyHealth > 1)
            {
                Memory.WriteByte(Addresses.BlutoHealth, (byte)1);
            }
        }
        else if (
            _easyChallenges.Contains("easy_skateboarding") &&
            currentLevel == LevelInGameIDs.EnchantedTowers && currentSubarea == 1        // Skatepark
        )
        {
            short currentScore = Memory.ReadShort(Addresses.EnchantedSkateScore);
            if (currentScore < 9999)
            {
                Memory.Write(Addresses.EnchantedSkateScore, (short)9999);
            }
        }
        else if (
            _easyChallenges.Contains("easy_sleepyhead") &&
            currentLevel == LevelInGameIDs.SpookySwamp && currentSubarea == 1            // Sleepyhead
        )
        {
            byte currentEnemyHealth = Memory.ReadByte(Addresses.SleepyheadHealth);
            if (currentEnemyHealth > 1)
            {
                Memory.WriteByte(Addresses.SleepyheadHealth, (byte)1);
            }
        }
        else if (
            _easyChallenges.Contains("easy_boxing") &&
            currentLevel == LevelInGameIDs.FrozenAltars && currentSubarea == 1           // Yeti Boxing
        )
        {
            byte currentEnemyHealth = Memory.ReadByte(Addresses.YetiBoxingHealth);
            if (currentEnemyHealth > 1)
            {
                Memory.WriteByte(Addresses.YetiBoxingHealth, (byte)1);
            }
        }
        else if (
            _easyChallenges.Contains("easy_subs") &&
            currentLevel == LevelInGameIDs.LostFleet && currentSubarea == 1              // Subs
        )
        {
            foreach (uint address in Addresses.LostFleetSubAddresses)
            {
                Memory.WriteByte(address, (byte)19);
            }
        }
        else if (
            _easyChallenges.Contains("easy_skateboarding") &&
            currentLevel == LevelInGameIDs.LostFleet && currentSubarea == 2              // Skatepark
        )
        {
            Memory.Write(Addresses.LostFleetNitro, (short)1000);
        }
        else if (
            _easyChallenges.Contains("easy_whackamole") &&
            currentLevel == LevelInGameIDs.CrystalIslands && currentSubarea == 2         // Whack-A-Mole
        )
        {
            byte currentMoleCount = Memory.ReadByte(Addresses.WhackAMoleCount);
            if (currentMoleCount < 19)
            {
                Memory.WriteByte(Addresses.WhackAMoleCount, (byte)19);
            }
        }
        else if (
            _easyChallenges.Contains("easy_shark_riders") &&
            currentLevel == LevelInGameIDs.DesertRuins && currentSubarea == 2            // Shark Riders
        )
        {
            foreach (uint address in Addresses.DesertSharkAddresses)
            {
                Memory.WriteByte(address, (byte)253);
            }
        }
        else if (
            _easyChallenges.Contains("easy_tanks") &&
            currentLevel == LevelInGameIDs.HauntedTomb && currentSubarea == 1            // Tanks
        )
        {
            byte currentTankCount = Memory.ReadByte(Addresses.TanksCount);
            byte maxTankCount = Memory.ReadByte(Addresses.MaxTanksCount);
            if (maxTankCount == 4 && currentTankCount < 4)
            {
                Memory.WriteByte(Addresses.TanksCount, (byte)3);
            }
            else if (maxTankCount == 10 && currentTankCount < 10)
            {
                Memory.WriteByte(Addresses.TanksCount, (byte)9);
            }
        }
        else if (
            _easyChallenges.Contains("no_green_rockets") &&
            currentLevel == LevelInGameIDs.ScorchsPit
        )
        {
            byte greenRockets = Memory.ReadByte(Addresses.GreenRocketCount);
            int redRockets = (int)Memory.ReadByte(Addresses.RedRocketCount);
            if (greenRockets > 0)
            {
                redRockets = Math.Min(200, redRockets + 50);
                greenRockets--;
                Memory.WriteByte(Addresses.RedRocketCount, (byte)redRockets);
                Memory.WriteByte(Addresses.GreenRocketCount, greenRockets);
                if (greenRockets == 0)
                {
                    Memory.WriteByte(Addresses.HasGreenRocketAddress, 0);
                }
            }
        }
        // TODO: Add SBR.
    }
    private static async void HandleCosmeticQueue(object source, ElapsedEventArgs e)
    {
        // Avoid overwhelming the game when many cosmetic effects are received at once by processing only 1
        // every 5 seconds.  This also lets the user see effects when logging in asynchronously.
        if (Client.GameState == null || Client.CurrentSession == null) return;
        if (Memory.ReadShort(Addresses.GameStatus) != (short)GameStatus.InGame) return;
        if (_cosmeticEffects.Count > 0)
        {
            string effect = _cosmeticEffects.Dequeue();
            switch (effect)
            {
                case "Big Head Mode":
                    ActivateBigHeadMode();
                    break;
                case "Flat Spyro Mode":
                    ActivateFlatSpyroMode();
                    break;
                case "Turn Spyro Red":
                    TurnSpyroColor(SpyroColor.SpyroColorRed);
                    break;
                case "Turn Spyro Blue":
                    TurnSpyroColor(SpyroColor.SpyroColorBlue);
                    break;
                case "Turn Spyro Yellow":
                    TurnSpyroColor(SpyroColor.SpyroColorYellow);
                    break;
                case "Turn Spyro Pink":
                    TurnSpyroColor(SpyroColor.SpyroColorPink);
                    break;
                case "Turn Spyro Green":
                    TurnSpyroColor(SpyroColor.SpyroColorGreen);
                    break;
                case "Turn Spyro Black":
                    TurnSpyroColor(SpyroColor.SpyroColorBlack);
                    break;
            }
        }
    }
    private static void StartSpyroGame(object source, ElapsedEventArgs e)
    {
        if (!Helpers.IsInGame() || Client.GameState == null || Client.CurrentSession == null)
        {
            Log.Logger.Information("Player is not yet in game.");
            return;
        }
        MoneybagsOptions moneybagsOption = (MoneybagsOptions)int.Parse(Client.Options?.GetValueOrDefault("moneybags_settings", "0").ToString());
        GemsanityOptions gemsanityOption = (GemsanityOptions)int.Parse(Client.Options?.GetValueOrDefault("enable_gemsanity", "0").ToString());
        if (moneybagsOption != MoneybagsOptions.Vanilla)
        {
            if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Moneybags Unlock - Sheila").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.SheilaUnlock, 20001);
            }
            else
            {
                Memory.Write(Addresses.SheilaUnlock, 65536);
                Memory.WriteByte(Addresses.SheilaCutscene, 1);
            }
            if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Moneybags Unlock - Sgt. Byrd").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.SgtByrdUnlock, 20001);
            }
            else
            {
                Memory.Write(Addresses.SgtByrdUnlock, 65536);
                Memory.WriteByte(Addresses.ByrdCutscene, 1);
            }
            if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Moneybags Unlock - Bentley").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.BentleyUnlock, 20001);
            }
            else
            {
                Memory.Write(Addresses.BentleyUnlock, 65536);
                Memory.WriteByte(Addresses.BentleyCutscene, 1);
            }
            if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Moneybags Unlock - Agent 9").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.Agent9Unlock, 20001);
            }
            else
            {
                Memory.Write(Addresses.Agent9Unlock, 65536);
                Memory.WriteByte(Addresses.Agent9Cutscene, 1);
            }
        }
        if (moneybagsOption == MoneybagsOptions.Moneybagssanity)
        {
            if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Moneybags Unlock - Cloud Spires Bellows").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.CloudBellowsUnlock, 20001);
            }
            else
            {
                // Special case this to just reduce the price to 0, since otherwise a rhynoc despawns.
                Memory.Write(Addresses.CloudBellowsUnlock, 0);
            }
            if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Moneybags Unlock - Spooky Swamp Door").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.SpookyDoorUnlock, 20001);
            }
            else
            {
                Memory.Write(Addresses.SpookyDoorUnlock, 65536);
            }
            if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Moneybags Unlock - Icy Peak Nancy Door").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.IcyNancyUnlock, 20001);
            }
            else
            {
                Memory.Write(Addresses.IcyNancyUnlock, 65536);
            }
            if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Moneybags Unlock - Molten Crater Thieves Door").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.MoltenThievesUnlock, 20001);
            }
            else
            {
                Memory.Write(Addresses.MoltenThievesUnlock, 65536);
            }
            if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Moneybags Unlock - Charmed Ridge Stairs").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.CharmedStairsUnlock, 20001);
            }
            else
            {
                Memory.Write(Addresses.CharmedStairsUnlock, 65536);
            }
            if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Moneybags Unlock - Desert Ruins Door").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.DesertDoorUnlock, 20001);
            }
            else
            {
                Memory.Write(Addresses.DesertDoorUnlock, 65536);
            }
            if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Moneybags Unlock - Frozen Altars Cat Hockey Door").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.FrozenHockeyUnlock, 20001);
            }
            else
            {
                Memory.Write(Addresses.FrozenHockeyUnlock, 65536);
            }
            if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Moneybags Unlock - Crystal Islands Bridge").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.CrystalBridgeUnlock, 20001);
            }
            else
            {
                Memory.Write(Addresses.CrystalBridgeUnlock, 65536);
            }
        }
        if (moneybagsOption == MoneybagsOptions.Vanilla && gemsanityOption != GemsanityOptions.Off)
        {
            Memory.WriteByte(Addresses.SheilaUnlock, 0);
            Memory.WriteByte(Addresses.SgtByrdUnlock, 0);
            Memory.WriteByte(Addresses.BentleyUnlock, 0);
            Memory.WriteByte(Addresses.Agent9Unlock, 0);
        }
        if (moneybagsOption == MoneybagsOptions.Companionsanity && gemsanityOption != GemsanityOptions.Off)
        {
            Memory.WriteByte(Addresses.CloudBellowsUnlock, 0);
            Memory.WriteByte(Addresses.SpookyDoorUnlock, 0);
            Memory.WriteByte(Addresses.IcyNancyUnlock, 0);
            Memory.WriteByte(Addresses.MoltenThievesUnlock, 0);
            Memory.WriteByte(Addresses.CharmedStairsUnlock, 0);
            Memory.WriteByte(Addresses.DesertDoorUnlock, 0);
            Memory.WriteByte(Addresses.FrozenHockeyUnlock, 0);
            Memory.WriteByte(Addresses.CrystalBridgeUnlock, 0);
        }
        int openWorldOption = int.Parse(Client.Options?.GetValueOrDefault("open_world", "0").ToString());
        if (openWorldOption != 0)
        {
            Memory.WriteByte(Addresses.SunriseLevelsComplete, 1);
            Memory.WriteByte(Addresses.MiddayLevelsComplete, 1);
            Memory.WriteByte(Addresses.EveningLevelsComplete, 1);
            // Mark level as entered in atlas, which changes transport behavior.
            Memory.WriteByte(0x720A2, 1);
            Memory.WriteByte(0x72090, 1);
            Memory.WriteByte(0x72097, 1);
            Memory.WriteByte(0x72099, 1);
            Memory.WriteByte(0x720AB, 1);
            Memory.WriteByte(0x720A9, 1);
            Memory.WriteByte(0x720A0, 1);
            Memory.WriteByte(0x72091, 1);
            Memory.WriteByte(0x72092, 1);
            Memory.WriteByte(0x72093, 1);
            Memory.WriteByte(0x72094, 1);
            Memory.WriteByte(0x72096, 1);
            Memory.WriteByte(0x7209a, 1);
            Memory.WriteByte(0x7209b, 1);
            Memory.WriteByte(0x7209c, 1);
            Memory.WriteByte(0x7209d, 1);
            Memory.WriteByte(0x7209f, 1);
            Memory.WriteByte(0x720a3, 1);
            Memory.WriteByte(0x720a4, 1);
            Memory.WriteByte(0x720a5, 1);
            Memory.WriteByte(0x720a6, 1);
            Memory.WriteByte(0x720a8, 1);
            // Triggered when getting end of level eggs in sunrise.
            Memory.WriteByte(0x716a2, 2);
            Memory.WriteByte(0x716a8, 2);
            Memory.WriteByte(0x716ae, 2);
            Memory.WriteByte(0x716b4, 2);
            Memory.WriteByte(0x716c0, 2);


            Memory.WriteByte(Addresses.BuzzDefeated, 1);
            Memory.WriteByte(Addresses.SpikeDefeated, 1);
            Memory.WriteByte(Addresses.ScorchDefeated, 1);
            Memory.WriteByte(Addresses.EveningBianca, 1);
            uint eggAddress = Addresses.EggStartAddress;
            List<uint> collectedEggs = new List<uint> { 1, 2, 3, 4, 6, 7, 10, 11, 12, 13, 15, 16, 19, 20, 21, 22, 24, 25 };
            for (uint i = 0; i < 26; i++)
            {
                if (collectedEggs.Contains(i))
                {
                    Memory.WriteBit(eggAddress + i, 0, true);
                }
            }
            var currentLives = Memory.ReadShort(Addresses.PlayerLives);
            Memory.Write(Addresses.PlayerLives, (short)(Math.Min(99, currentLives + 1)));
            Memory.WriteByte(Addresses.SpyroState, 31);  // Death
        }
        _loadGameTimer.Enabled = false;
    }
    private static void CheckGoalCondition()
    {
        if (_hasSubmittedGoal || Client.GameState == null || Client.CurrentSession == null)
        {
            return;
        }
        var currentEggs = CalculateCurrentEggs();
        var currentSkillPoints = CalculateCurrentSkillPoints();
        int goal = int.Parse(Client.Options?.GetValueOrDefault("goal", 0).ToString());
        if ((CompletionGoal)goal == CompletionGoal.Sorceress1)
        {
            if (currentEggs >= 100 && (Client.GameState?.ReceivedItems.Any(x => x != null && x.Name == "Sorceress Defeated") ?? false))
            {
                Client.SendGoalCompletion();
                _hasSubmittedGoal = true;
            }
        }
        else if ((CompletionGoal)goal == CompletionGoal.EggForSale)
        {
            if ((Client.GameState?.ReceivedItems.Any(x => x != null && x.Name == "Moneybags Chase Complete") ?? false))
            {
                Client.SendGoalCompletion();
                _hasSubmittedGoal = true;
            }
        }
        else if ((CompletionGoal)goal == CompletionGoal.Sorceress2)
        {
            if ((Client.GameState?.ReceivedItems.Any(x => x != null && x.Name == "Super Bonus Round Complete") ?? false))
            {
                Client.SendGoalCompletion();
                _hasSubmittedGoal = true;
            }
        }
        else if ((CompletionGoal)goal == CompletionGoal.AllSkillPoints)
        {
            if (currentSkillPoints >= 20)
            {
                Client.SendGoalCompletion();
                _hasSubmittedGoal = true;
            }
        }
        else if ((CompletionGoal)goal == CompletionGoal.Epilogue)
        {
            if (currentSkillPoints >= 20 && (Client.GameState?.ReceivedItems.Any(x => x != null && x.Name == "Sorceress Defeated") ?? false))
            {
                Client.SendGoalCompletion();
                _hasSubmittedGoal = true;
            }
        }
        else if ((CompletionGoal)goal == CompletionGoal.Spike)
        {
            if (currentEggs >= 36 && (Client.GameState?.ReceivedItems.Any(x => x != null && x.Name == "Spike Defeated") ?? false))
            {
                Client.SendGoalCompletion();
                _hasSubmittedGoal = true;
            }
        }
        else if ((CompletionGoal)goal == CompletionGoal.Scorch)
        {
            if (currentEggs >= 65 && (Client.GameState?.ReceivedItems.Any(x => x != null && x.Name == "Scorch Defeated") ?? false))
            {
                Client.SendGoalCompletion();
                _hasSubmittedGoal = true;
            }
        }
        else if ((CompletionGoal)goal == CompletionGoal.EggHunt)
        {
            if (currentEggs >= int.Parse(Client.Options?.GetValueOrDefault("egg_count", 0).ToString()))
            {
                Client.SendGoalCompletion();
                _hasSubmittedGoal = true;
            }
        }
    }
    private static async void RunLagTrap()
    {
        using (var lagTrap = new LagTrap(TimeSpan.FromSeconds(20)))
        {
            lagTrap.Start();
            await lagTrap.WaitForCompletionAsync();
        }
    }
    private static async void ActivateBigHeadMode()
    {
        Memory.WriteByte(Addresses.SpyroHeight, (byte)(32));
        Memory.WriteByte(Addresses.SpyroLength, (byte)(32));
        Memory.WriteByte(Addresses.SpyroWidth, (byte)(32));
        Memory.Write(Addresses.BigHeadMode, (short)(1));
    }
    private static async void ActivateFlatSpyroMode()
    {
        Memory.WriteByte(Addresses.SpyroHeight, (byte)(16));
        Memory.WriteByte(Addresses.SpyroLength, (byte)(16));
        Memory.WriteByte(Addresses.SpyroWidth, (byte)(2));
        Memory.Write(Addresses.BigHeadMode, (short)(256));
    }
    private static async void TurnSpyroColor(SpyroColor colorEnum)
    {
        Memory.Write(Addresses.SpyroColorAddress, (short)colorEnum);
    }
    private static async void ActivateInvincibility(int seconds)
    {
        // The counter ticks down every frame (60 fps for in game calcs)
        seconds = seconds * 60;
        // Collecting an egg removes invincibility, so try to avoid a user's own egg
        // from providing no benefits.

        await Task.Run(async () =>
        {
            await Task.Delay(6000);
            Memory.Write(Addresses.InvincibilityDurationAddress, (short)seconds);
        });
    }
    private static async void UnlockMoneybags(uint address, uint cutsceneTriggerAddress = 0x00)
    {
        // Flag the cutscene as completed to avoid issues with warping softlocks.
        if (cutsceneTriggerAddress != 0x00)
        {
            Memory.WriteByte(cutsceneTriggerAddress, 1);
        }
        // Flag the check as paid for, and set the price to 0.  Otherwise, we'll get back too many gems during the chase.
        Memory.Write(address, 65536);
        Log.Logger.Information("If you are in the same zone as Moneybags, you can talk to him to complete the unlock for free.");
    }
    private static void LogItem(Item item)
    {
        // Not supported at this time.
        /*var messageToLog = new LogListItem(new List<TextSpan>()
            {
                new TextSpan(){Text = $"[{item.Id.ToString()}] -", TextColor = new SolidColorBrush(Color.FromRgb(255, 255, 255))},
                new TextSpan(){Text = $"{item.Name}", TextColor = new SolidColorBrush(Color.FromRgb(200, 255, 200))}
            });
        lock (_lockObject)
        {
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                Context.ItemList.Add(messageToLog);
            });
        }*/
    }

    private void Client_MessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        // If the player requests it, don't show "found" hints in the main client.
        if (e.Message.Parts.Any(x => x.Text == "[Hint]: ") && (!_useQuietHints || !e.Message.Parts.Any(x => x.Text.Trim() == "(found)")))
        {
            LogHint(e.Message);
        }
        if (!e.Message.Parts.Any(x => x.Text == "[Hint]: ") || !_useQuietHints || !e.Message.Parts.Any(x => x.Text.Trim() == "(found)"))
        {
            Log.Logger.Information(JsonConvert.SerializeObject(e.Message));
        }
    }
    private static void LogHint(LogMessage message)
    {
        var newMessage = message.Parts.Select(x => x.Text);

        foreach (var hint in Context.HintList)
        {
            IEnumerable<string> hintText = hint.TextSpans.Select(y => y.Text);
            if (newMessage.Count() != hintText.Count())
            {
                continue;
            }
            bool isMatch = true;
            for (int i = 0; i < hintText.Count(); i++)
            {
                if (newMessage.ElementAt(i) != hintText.ElementAt(i))
                {
                    isMatch = false;
                    break;
                }
            }
            if (isMatch)
            {
                return; //Hint already in list
            }
        }
        List<TextSpan> spans = new List<TextSpan>();
        foreach (var part in message.Parts)
        {
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                spans.Add(new TextSpan() { Text = part.Text, TextColor = new SolidColorBrush(Color.FromRgb(part.Color.R, part.Color.G, part.Color.B)) });
            });
        }
        lock (_lockObject)
        {
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                Context.HintList.Add(new LogListItem(spans));
            });
        }
    }
    private static void Locations_CheckedLocationsUpdated(System.Collections.ObjectModel.ReadOnlyCollection<long> newCheckedLocations)
    {
        if (Client.GameState == null || Client.CurrentSession == null) return;
        if (!Helpers.IsInGame())
        {
            Log.Logger.Error("Sending location while not in game - please report this error in the Discord.");
        }
        var currentEggs = CalculateCurrentEggs();
        CheckGoalCondition();

    }
    private static int CalculateCurrentEggs()
    {
        var count = Client.GameState?.ReceivedItems.Where(x => x.Name == "Egg").Count() ?? 0;
        count = Math.Min(count, 150);
        Memory.WriteByte(Addresses.TotalEggAddress, (byte)(count));
        return count;
    }
    private static int CalculateCurrentSkillPoints()
    {
        return Client.GameState?.ReceivedItems.Where(x => x.Name == "Skill Point").Count() ?? 0;
    }
    private static int CalculateCurrentGems()
    {
        GemsanityOptions gemsanityOption = (GemsanityOptions)int.Parse(Client.Options?.GetValueOrDefault("enable_gemsanity", "0").ToString());
        if (gemsanityOption == GemsanityOptions.Off)
        {
            return Memory.ReadShort(Addresses.TotalGemAddress);
        }
        uint levelGemCountAddress = Addresses.SunriseSpringGems;
        int totalGems = 0;
        int i = 0;
        foreach (LevelData level in Helpers.GetLevelData())
        {
            if (level.Name != "Super Bonus Round")
            {
                string levelName = level.Name;
                int levelGemCount = 50 * (Client.GameState?.ReceivedItems?.Where(x => x != null && x.Name == $"{levelName} 50 Gems").Count() ?? 0) +
                    100 * (Client.GameState?.ReceivedItems?.Where(x => x != null && x.Name == $"{levelName} 100 Gems").Count() ?? 0);
                Memory.Write(levelGemCountAddress, levelGemCount);
                totalGems += levelGemCount;
            }
            else
            {
                totalGems += Memory.ReadInt(levelGemCountAddress);
            }
            i++;
            levelGemCountAddress += 4;
        }
        Memory.Write(Addresses.TotalGemAddress, totalGems);
        return totalGems;
    }
    private static void OnConnected(object sender, EventArgs args)
    {
        Log.Logger.Information("Connected to Archipelago");
        Log.Logger.Information($"Playing {Client.CurrentSession.ConnectionInfo.Game} as {Client.CurrentSession.Players.GetPlayerName(Client.CurrentSession.ConnectionInfo.Slot)}");

        // There is a tradeoff here when creating new threads.  Separate timers allow for better control over when
        // memory reads and writes will happen, but they take away threads for other client tasks.
        // This solution is fine with the current item pool size but won't scale with gemsanity.
        // TODO: Test which of these can be combined without impacting the end result.
        _loadGameTimer = new Timer();
        _loadGameTimer.Elapsed += new ElapsedEventHandler(StartSpyroGame);
        _loadGameTimer.Interval = 5000;
        _loadGameTimer.Enabled = true;

        _cosmeticsTimer = new Timer();
        _cosmeticsTimer.Elapsed += new ElapsedEventHandler(HandleCosmeticQueue);
        _cosmeticsTimer.Interval = 5000;
        _cosmeticsTimer.Enabled = true;

        string[] easyModeOptions = [
            "easy_skateboarding",
                "easy_boxing",
                "easy_sheila_bombing",
                "easy_tanks",
                "easy_subs",
                "easy_bluto",
                "easy_sleepyhead",
                "easy_shark_riders",
                "easy_whackamole",
                "easy_tunnels",
                "no_green_rockets"
        ];
        foreach (string optionName in easyModeOptions)
        {
            int isOptionOn = int.Parse(Client.Options?.GetValueOrDefault(optionName, "0").ToString());
            if (isOptionOn != 0)
            {
                _easyChallenges.Add(optionName);
            }
        }
        _minigamesTimer = new Timer();
        _minigamesTimer.Elapsed += new ElapsedEventHandler(HandleMinigames);
        _minigamesTimer.Interval = 100;
        _minigamesTimer.Enabled = true;

        int isWorldKeysOn = int.Parse(Client.Options?.GetValueOrDefault("enable_world_keys", "0").ToString());
        if (isWorldKeysOn != 0)
        {
            _worldKeys = (byte)(Client.GameState?.ReceivedItems.Where(x => x.Name == "World Key").Count() ?? 0);
            _worldKeysTimer = new Timer();
            _worldKeysTimer.Elapsed += new ElapsedEventHandler(HandleWorldKeys);
            _worldKeysTimer.Interval = 100; // Critical to update quickly enough to prevent loading crashes.
            _worldKeysTimer.Enabled = true;
        }

        int isSparxSanityOn = int.Parse(Client.Options?.GetValueOrDefault("sparx_power_settings", "0").ToString());
        if (isSparxSanityOn != 0)
        {
            _progressiveBasketBreaks = (byte)(Client.GameState?.ReceivedItems.Where(x => x.Name == "Progressive Sparx Basket Break").Count() ?? 0);
            _sparxPowerTimer = new Timer();
            _sparxPowerTimer.Elapsed += new ElapsedEventHandler(HandleSparxPowers);
            _sparxPowerTimer.Interval = 500;
            _sparxPowerTimer.Enabled = true;
        }

        _sparxOption = (ProgressiveSparxHealthOptions)int.Parse(Client.Options?.GetValueOrDefault("enable_progressive_sparx_health", "0").ToString());
        if (_sparxOption != ProgressiveSparxHealthOptions.Off)
        {
            _sparxTimer = new Timer();
            _sparxTimer.Elapsed += new ElapsedEventHandler(HandleMaxSparxHealth);
            _sparxTimer.Interval = 500;
            _sparxTimer.Enabled = true;
        }

        // Repopulate hint list.  There is likely a better way to do this using the Get network protocol
        // with keys=[$"hints_{team}_{slot}"].
        Client?.SendMessage("!hint");
    }

    private static void OnDisconnected(object sender, EventArgs args)
    {
        Log.Logger.Information("Disconnected from Archipelago");
        // Avoid ongoing timers affecting a new game.
        _hintsList = null;
        _hasSubmittedGoal = false;
        _useQuietHints = true;
        _easyChallenges = new List<string>();
        _worldKeys = 0;
        _progressiveBasketBreaks = 0;
        Log.Logger.Information("This Archipelago Client is compatible only with the NTSC-U 1.1 release of Spyro 3 (North America Greatest Hits version).");
        Log.Logger.Information("Trying to play with a different version will not work and may release all of your locations at the start.");

        if (_loadGameTimer != null)
        {
            _loadGameTimer.Enabled = false;
            _loadGameTimer = null;
        }
        _cosmeticEffects = new Queue<string>();
        if (_cosmeticsTimer != null)
        {
            _cosmeticsTimer.Enabled = false;
            _cosmeticsTimer = null;
        }
        if (_sparxTimer != null)
        {
            _sparxTimer.Enabled = false;
            _sparxTimer = null;
        }
        if (_worldKeysTimer != null)
        {
            _worldKeysTimer.Enabled = false;
            _worldKeysTimer = null;
        }
        if (_minigamesTimer != null)
        {
            _minigamesTimer.Enabled = false;
            _minigamesTimer = null;
        }
        if (_sparxPowerTimer != null)
        {
            _sparxPowerTimer.Enabled = false;
            _sparxPowerTimer = null;
        }
    }
}
