using Archipelago.Core;
using Archipelago.Core.AvaloniaGUI.Logging;
using Archipelago.Core.AvaloniaGUI.Models;
using Archipelago.Core.AvaloniaGUI.ViewModels;
using Archipelago.Core.AvaloniaGUI.Views;
using Archipelago.Core.GameClients;
using Archipelago.Core.Models;
using Archipelago.Core.Util;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Newtonsoft.Json;
using ReactiveUI;
using S3AP.Models;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Timers;
using System.Text.Json;
using System.Text.Json.Serialization;
using static S3AP.Models.Enums;
using System.IO;
using Silk.NET.Maths;

namespace S3AP;

public partial class App : Application
{
    // TODO: Remember to set this in S3AP.Desktop as well.
    public static string Version = "1.3.3";
    public static List<string> SupportedVersions = ["1.3.0", "1.3.1", "1.3.2", "1.3.3"];
    public static List<string> PartiallySupportedVersions = [];

    public static MainWindowViewModel Context;
    public static ArchipelagoClient Client { get; set; }
    public static List<ILocation> GameLocations { get; set; }
    private static readonly object _lockObject = new object();
    private static ConcurrentQueue<string> _cosmeticEffects { get; set; }
    private static Dictionary<string, string> _hintsList { get; set; }
    private static bool _hasSubmittedGoal { get; set; }
    private static bool _useQuietHints { get; set; }
    private static int _unlockedLevels { get; set; }
    private static List<string> _easyChallenges { get; set; }
    private static int _worldKeys { get; set; }
    private static int _progressiveBasketBreaks { get; set; }
    private static bool _checkNames { get; set; }
    private static bool _deathlinkImmunity { get; set; }
    // AP Options and slot data
    private static int _slot { get; set; }
    private static Dictionary<string, object> _slotData { get; set; }
    private static LevelLockOptions _levelLockOptions { get; set; }
    private static Dictionary<string, int> _eggRequirements { get; set; }
    private static Dictionary<string, int> _gemRequirements { get; set; }
    private static GemsanityOptions _gemsanityOption { get; set; }
    private static MoneybagsOptions _moneybagsOption { get; set; }
    private static PowerupLockOptions _powerupLockOption { get; set; }
    private static int _openWorld {  get; set; }
    private static int _sparxPowerShuffle { get; set; }
    private static CompletionGoal _goal { get; set; }
    private static ProgressiveSparxHealthOptions _sparxOption { get; set; }
    private static int _eggCountGoal { get; set; }
    private static int _sorceressDoorEggReq { get; set; }
    private static int _sbrDoorEggReq { get; set; }
    private static int _sbrDoorGemReq { get; set; }
    private static int _isWorldKeysOn { get; set; }
    // Timers
    private static Timer _loadGameTimer { get; set; }
    private static Timer _cosmeticsTimer { get; set; }
    private static Timer _gameLoopTimer { get; set; }
    private static Timer _minigamesTimer { get; set; }
    private static int _timerLoopCount { get; set; }
    private static bool _justDied { get; set; }
    private static bool _handleGemsanity = false;
    private static LevelInGameIDs _previousLevel { get; set; }
    private static DeathLinkService _deathLinkService { get; set; }
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

    public static Dictionary<String, String> GetLastConnectionDetails()
    {
        string connectionDetails = File.ReadAllText(@"./connection.json");
        return System.Text.Json.JsonSerializer.Deserialize<Dictionary<String, String>>(connectionDetails);
    }

    public static void SaveLastConnectionDetails(Dictionary<String, String> lastConnectionDetails)
    {
        string json = System.Text.Json.JsonSerializer.Serialize(lastConnectionDetails);
        File.WriteAllText(@"./connection.json", json);
    }

    public void Start()
    {
        Context = new MainWindowViewModel("0.6.1 or later");
        Context.ClientVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
        Context.ConnectClicked += Context_ConnectClicked;
        Context.CommandReceived += (e, a) =>
        {
            if (string.IsNullOrWhiteSpace(a.Command)) return;
            HandleCommand(a);
        };
        Dictionary<String, String> lastConnectionDetails = new Dictionary<string, string>();
        lastConnectionDetails["slot"] = "";
        lastConnectionDetails["host"] = "";
        // Don't save password.
        try
        {
            lastConnectionDetails = GetLastConnectionDetails();
            if (!lastConnectionDetails.ContainsKey("slot"))
            {
                lastConnectionDetails["slot"] = "";
            }
            if (!lastConnectionDetails.ContainsKey("host"))
            {
                lastConnectionDetails["host"] = "";
            }
        }
        catch (Exception ex)
        {
            Log.Logger.Verbose($"Could not load connection details file.\r\n{ex.ToString()}");
        }
        Context.Host = lastConnectionDetails["host"];
        Context.Slot = lastConnectionDetails["slot"];
        Context.ConnectButtonEnabled = true;
        Context.AutoscrollEnabled = true;
        _hintsList = null;
        _hasSubmittedGoal = false;
        _useQuietHints = true;
        _easyChallenges = new List<string>();
        _eggRequirements = new Dictionary<string, int>();
        _gemRequirements = new Dictionary<string, int>();
        _worldKeys = 0;
        _progressiveBasketBreaks = 0;
        // No level ID set
        _previousLevel = (LevelInGameIDs)255;
        Log.Logger.Information("This Archipelago Client is compatible only with the NTSC-U (North America)\r\n" +
            "releases of Spyro 3.");
        if (!IsRunningAsAdministrator())
        {
            Log.Logger.Warning("You do not appear to be running this client as an administrator.\r\n" +
                "This may result in errors or crashes when trying to connect to Duckstation.");
        }
    }

    private static void HandleDeathLink(DeathLink deathLink)
    {
        if (deathLink.Source == Client.CurrentSession.Players.ActivePlayer.Name)
        {
            return;
        }
        LevelInGameIDs currentLevel = (LevelInGameIDs)Memory.ReadByte(Addresses.GetVersionAddress(Addresses.CurrentLevelAddress));
        LevelInGameIDs[] deathLinkLevels = [
            LevelInGameIDs.SunriseSpring,
            LevelInGameIDs.SunnyVilla,
            LevelInGameIDs.CloudSpires,
            LevelInGameIDs.MoltenCrater,
            LevelInGameIDs.SeashellShore,
            LevelInGameIDs.SheilasAlp,
            LevelInGameIDs.BuzzsDungeon,
            LevelInGameIDs.CrawdadFarm,
            LevelInGameIDs.MiddayGardens,
            LevelInGameIDs.IcyPeak,
            LevelInGameIDs.EnchantedTowers,
            LevelInGameIDs.SpookySwamp,
            LevelInGameIDs.BambooTerrace,
            LevelInGameIDs.SgtByrdsBase,
            LevelInGameIDs.SpikesArena,
            LevelInGameIDs.SpiderTown,
            LevelInGameIDs.EveningLake,
            LevelInGameIDs.FrozenAltars,
            LevelInGameIDs.LostFleet,
            LevelInGameIDs.FireworksFactory,
            LevelInGameIDs.CharmedRidge,
            LevelInGameIDs.BentleysOutpost,
            LevelInGameIDs.ScorchsPit,
            LevelInGameIDs.StarfishReef,
            LevelInGameIDs.MidnightMountain,
            LevelInGameIDs.CrystalIslands,
            LevelInGameIDs.DesertRuins,
            LevelInGameIDs.HauntedTomb,
            LevelInGameIDs.DinoMines,
            LevelInGameIDs.Agent9sLab,
            LevelInGameIDs.SorceressLair,
            LevelInGameIDs.BugbotFactory,
            LevelInGameIDs.SuperBonusRound
        ];
        if (!deathLinkLevels.Contains(currentLevel))
        {
            string ignoreMessage = "Received DeathLink from " + deathLink.Source;
            if (deathLink.Cause != null)
            {
                ignoreMessage = ignoreMessage + " - " + deathLink.Cause +
                    "\r\nIgnored the DeathLink to avoid softlock in current level.";
            }
            Log.Logger.Information(ignoreMessage);
            return;
        }

        if (!_deathlinkImmunity)
        {
            Memory.Write(Addresses.GetVersionAddress(Addresses.SpyroPositionZ), 1);
            Task.Run(async () =>
            {
                _deathlinkImmunity = true;
                await Task.Delay(6000);
                _deathlinkImmunity = false;
            });
        }
        string message = "Received DeathLink from " + deathLink.Source;
        if (deathLink.Cause != null)
        {
            message = message + " - " + deathLink.Cause;
        }
        Log.Logger.Information(message);
    }

    private async void HandleCommand(ArchipelagoCommandEventArgs command)
    {
        if (Client == null || Client.ItemState == null || Client.CurrentSession == null) return;
        string commandText = command.Command;
        switch (commandText.ToLower())
        {
            case "!help":
                // This is also a server command, so send the message after.
                string commandsText = "";
                // !help is covered by the server response.
                // commandsText += "!help\r\n\tShow available commands.\r\n";
                commandsText += "!goal\r\n\tShows your current goal and progress towards it.\r\n";
                commandsText += "!options\r\n\tShows important options for your slot.\r\n";
                commandsText += "!unlockedLevels\r\n\tWhen level locks are keys, shows the levels you have unlocked.\r\n";
                commandsText += "!reloadItems\r\n\tIn case of a desync, sends all received AP Items again.\r\n\tRequires reconnecting.\r\n";
                commandsText += "!quietHints\r\n\tMakes hints easier to read by hiding items you have already found.\r\n";
                commandsText += "!verboseHints\r\n\tChanges hints to the default archipelago behavior,\r\n\tshowing items you have already found.\r\n";
                commandsText += "!debugInfo\r\n\tPrints information about your game to the screen.\r\n\tYou may be asked to screenshot this if there is an error.\r\n";
                Client?.SendMessage(command.Command);
                await Task.Delay(200);
                Log.Logger.Information(commandsText);
                break;
            case "!options":
                List<CompletionGoal> eggRequirementGoals = new List<CompletionGoal>() {
                    CompletionGoal.Sorceress1,
                    CompletionGoal.Scorch,
                    CompletionGoal.Spike,
                    CompletionGoal.Sorceress2,
                    CompletionGoal.EggHunt
                };
                string goalEggText = eggRequirementGoals.Contains(_goal) ? $" ({_eggCountGoal} eggs)" : "";
                string worldSettings = _openWorld != 0 ? "Open World" : (_isWorldKeysOn != 0 ? "World Keys" : "Vanilla Progression");
                string optionsString = $"\tGoal: {_goal}{goalEggText} - Use !goal for more info\r\n" +
                    $"\tWorld Settings: {worldSettings}\r\n" +
                    $"\tLevel Locks: {_levelLockOptions}\r\n" +
                    $"\tMoneybagssanity: {_moneybagsOption}\r\n" +
                    $"\tGemsanity: {_gemsanityOption}\r\n" +
                    $"\tPowerup Locks: {_powerupLockOption}\r\n" +
                    $"\tSorceress' Lair Requirement: {_sorceressDoorEggReq} egg(s)\r\n" +
                    $"\tSuper Bonus Round Requirements: {_sbrDoorEggReq} egg(s) and {_sbrDoorGemReq} gem(s)\r\n";
                Log.Logger.Information($"\r\n{command.Command}\r\n{optionsString}");
                break;
            case "!debuginfo":
                _slotData.TryGetValue("apworldVersion", out var hostVersionValue);
                LevelInGameIDs currentLevel = (LevelInGameIDs)Memory.ReadByte(Addresses.GetVersionAddress(Addresses.CurrentLevelAddress));
                byte currentSubArea = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.CurrentSubareaAddress));
                string debugString = $"\tGame Version: {Helpers.gameVersion}\r\n" +
                    $"\tHost APWorld Version: {hostVersionValue}\r\n" +
                    $"\tClient Version: {Version}\r\n" +
                    $"\tCurrent Level: {currentLevel} - Sub-area {currentSubArea}\r\n";
                // TODO: Add more information.
                Log.Logger.Information($"\r\n{command.Command}\r\n{debugString}Screenshot this if requested to help debug issues.\r\n");
                break;
            case "clearspyrogamestate":
            case "!reloaditems":
                Log.Logger.Information($"\r\n{command.Command}\r\n" +
                    "\tClearing the game state.  Please reconnect to the server while in game to refresh received items.\r\n");
                Client.ForceReloadAllItems();
                break;
            case "usequiethints":
            case "!quiethints":
                Log.Logger.Information($"\r\n{command.Command}\r\n" +
                    "\tHints for found locations will not be displayed.  Type 'useVerboseHints' to show them.\r\n");
                _useQuietHints = true;
                break;
            case "useverbosehints":
            case "!verbosehints":
                Log.Logger.Information($"\r\n{command.Command}\r\n" +
                    "\tHints for found locations will be displayed.  Type 'useQuietHints' to show them.\r\n");
                _useQuietHints = false;
                break;
            case "showunlockedlevels":
            case "!unlockedlevels":
                Log.Logger.Information($"\r\n{command.Command}");
                showUnlockedLevels(true);
                break;
            case "showgoal":
            case "!goal":
                Log.Logger.Information($"\r\n{command.Command}");
                string goalText = "";
                string progressText = "";
                int eggsNeeded = int.Parse(Client.Options?.GetValueOrDefault("egg_count", 150).ToString());
                int eggs = CalculateCurrentEggs();
                string eggsNeededText = eggsNeeded == 1 ? $"{eggsNeeded} egg" : $"{eggsNeeded} eggs";
                string eggsText = eggs == 1 ? $"{eggs} egg" : $"{eggs} eggs";
                int skillPoints = CalculateCurrentSkillPoints();
                string skillPointsText = skillPoints == 1 ? $"{skillPoints} Skill Point" : $"{skillPoints} Skill Points";
                bool beatenSpike = Client.CurrentSession?.Items.AllItemsReceived.Any(x => x != null && x.ItemName == "Spike Defeated") ?? false;
                bool beatenScorch = Client.CurrentSession?.Items.AllItemsReceived.Any(x => x != null && x.ItemName == "Scorch Defeated") ?? false;
                bool beatenSorceress = Client.CurrentSession?.Items.AllItemsReceived.Any(x => x != null && x.ItemName == "Sorceress Defeated") ?? false;
                bool beatenSorceressTwo = Client.CurrentSession?.Items.AllItemsReceived.Any(x => x != null && x.ItemName == "Super Bonus Round Complete") ?? false;
                string defeatedSpikeText = beatenSpike ? "have defeated Spike" : "have not defeated Spike";
                string defeatedScorchText = beatenScorch ? "have defeated Scorch" : "have not defeated Scorch";
                string defeatedSorceressText = beatenSorceress ? "have defeated the Sorceress in her lair" : "have not defeated the Sorceress in her lair";
                string defeatedSorceressTwoText = beatenSorceressTwo ? "have defeated the Sorceress in Super Bonus Round" : "have not defeated the Sorceress in Super Bonus Round";
                switch (_goal)
                {
                    case CompletionGoal.Sorceress1:
                        goalText = $"Collect {eggsNeededText} and defeat the Sorceress in Sorceress' Lair";
                        progressText = $"You have {eggsText} and {defeatedSorceressText}.";
                        break;
                    case CompletionGoal.EggForSale:
                        goalText = "Chase Moneybags in Midnight Mountain";
                        break;
                    case CompletionGoal.Sorceress2:
                        goalText = $"Collect {eggsNeededText} and defeat the Sorceress in Super Bonus Round";
                        progressText = $"You have {eggsText} and {defeatedSorceressTwoText}.";
                        break;
                    case CompletionGoal.AllSkillPoints:
                        goalText = "Collect all 20 Skill Points";
                        progressText = $"You have {skillPointsText}.";
                        break;
                    case CompletionGoal.Epilogue:
                        goalText = "Defeat the Sorceress in Sorceress' Lair and collect all 20 skill points";
                        progressText = $"You have {skillPointsText} and {defeatedSorceressText}.";
                        break;
                    case CompletionGoal.Spike:
                        goalText = $"Collect {eggsNeededText} and defeat Spike in Spike's Arena";
                        progressText = $"You have {eggsText} and {defeatedSpikeText}.";
                        break;
                    case CompletionGoal.Scorch:
                        goalText = $"Collect {eggsNeededText} and defeat Scorch in Scorch's Pit";
                        progressText = $"You have {eggsText} and {defeatedScorchText}.";
                        break;
                    case CompletionGoal.EggHunt:
                        goalText = $"Collect {eggsNeededText} from a reduced egg pool";
                        progressText = $"You have {eggsText}.";
                        break;
                    default:
                        Log.Logger.Error("Error finding your goal\r\n");
                        return;
                }
                Log.Logger.Information($"\tYour goal is: {goalText}\r\n\t{progressText}\r\n");
                break;
            default:
                Client?.SendMessage(command.Command);
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
            if (Client.CurrentSession != null)
            {
                Client.CurrentSession.Locations.CheckedLocationsUpdated -= Locations_CheckedLocationsUpdated;
            }
        }
        DuckstationClient? client = null;
        try
        {
            client = new DuckstationClient();
        }
        catch (ArgumentException ex)
        {
            Log.Logger.Warning("Duckstation not running, open Duckstation and launch the game before connecting!");
            OnDisconnected();
            return;
        }
        var DuckstationConnected = client.Connect();
        if (!DuckstationConnected)
        {
            Log.Logger.Warning("Duckstation not running, open Duckstation and launch the game before connecting!");
            OnDisconnected();
            return;
        }
        Client = new ArchipelagoClient(client);
        Client.ShouldSaveStateOnItemReceived = false;

        Memory.GlobalOffset = Memory.GetDuckstationOffset();

        Client.Connected += OnConnected;
        Client.Disconnected += OnDisconnected;

        await Client.Connect(e.Host.Trim(), "Spyro 3", "save1");
        if (!Client.IsConnected)
        {
            Log.Logger.Error("Your host seems to be invalid.  Please confirm that you have entered it correctly.");
            return;
        }
        _cosmeticEffects = new ConcurrentQueue<string>();
        Client.LocationCompleted += Client_LocationCompleted;
        Client.CurrentSession.Locations.CheckedLocationsUpdated += Locations_CheckedLocationsUpdated;
        Client.MessageReceived += Client_MessageReceived;
        Client.ItemReceived += ItemReceived;
        Client.EnableLocationsCondition = () => Helpers.IsInGame();
        // Password may start or end with whitepace, so do not trim it.
        await Client.Login(e.Slot.Trim(), !string.IsNullOrWhiteSpace(e.Password) ? e.Password : null);
        if (Client.Options?.Count > 0)
        {
            _gemsanityOption = (GemsanityOptions)int.Parse(Client.Options?.GetValueOrDefault("enable_gemsanity", "0").ToString());
            _moneybagsOption = (MoneybagsOptions)int.Parse(Client.Options?.GetValueOrDefault("moneybags_settings", "0").ToString());
            _powerupLockOption = (PowerupLockOptions)int.Parse(Client.Options?.GetValueOrDefault("powerup_lock_settings", "0").ToString());
            _openWorld = int.Parse(Client.Options?.GetValueOrDefault("open_world", "0").ToString());
            _sparxPowerShuffle = int.Parse(Client.Options?.GetValueOrDefault("sparx_power_settings", "0").ToString());
            _goal = (CompletionGoal)int.Parse(Client.Options?.GetValueOrDefault("goal", "0").ToString());
            _eggCountGoal = int.Parse(Client.Options?.GetValueOrDefault("egg_count", "0").ToString());
            _sorceressDoorEggReq = int.Parse(Client.Options?.GetValueOrDefault("sorceress_door_requirement", "100").ToString());
            _sbrDoorEggReq = int.Parse(Client.Options?.GetValueOrDefault("sbr_door_egg_requirement", "149").ToString());
            _sbrDoorGemReq = int.Parse(Client.Options?.GetValueOrDefault("sbr_door_gem_requirement", "15000").ToString());
            _isWorldKeysOn = int.Parse(Client.Options?.GetValueOrDefault("enable_world_keys", "0").ToString());
            _sparxOption = (ProgressiveSparxHealthOptions)int.Parse(Client.Options?.GetValueOrDefault("enable_progressive_sparx_health", "0").ToString());
            _levelLockOptions = (LevelLockOptions)int.Parse(Client.Options?.GetValueOrDefault("level_lock_option", "0").ToString());

            string[] easyModeOptions = [
                "easy_skateboarding",  // Support backwards compatibility
                "easy_skateboarding_lizards",
                "easy_skateboarding_points",
                "easy_skateboarding_lost_fleet",
                "easy_skateboarding_super_bonus_round",
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

            _slot = Client.CurrentSession.ConnectionInfo.Slot;
            _slotData = await Client.CurrentSession.DataStorage.GetSlotDataAsync(_slot);

            if (_slotData.TryGetValue("level_egg_requirements", out var levelEggRequirements))
            {
                if (levelEggRequirements != null)
                {
                    _eggRequirements = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(levelEggRequirements.ToString());
                }
            }
            if (_slotData.TryGetValue("level_gem_requirements", out var levelGemRequirements))
            {
                if (levelGemRequirements != null)
                {
                    _gemRequirements = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(levelGemRequirements.ToString());
                }
            }

            if (_slotData.TryGetValue("apworldVersion", out var versionValue))
            {
                if (versionValue != null && SupportedVersions.Contains(versionValue.ToString().ToLower()))
                {
                    Log.Logger.Information($"\r\nThe host's AP world version is {versionValue.ToString()} and the client version is {Version}.\r\n" +
                        "These versions are known to be compatible.\r\n");
                }
                else if (versionValue != null && PartiallySupportedVersions.Contains(versionValue.ToString().ToLower()))
                {
                    Log.Logger.Warning($"\r\nThe host's AP world version is {versionValue.ToString()} and the client version is {Version}.\r\n" +
                        "These versions are known to be mostly compatible but may bave some unexpected behavior.\r\n" +
                        "Please be sure to read the release notes to see what may break.\r\n");
                }
                else if (versionValue != null && versionValue.ToString().ToLower() != Version.ToLower())
                {
                    Log.Logger.Warning($"\r\nThe host's AP world version is {versionValue.ToString()} but the client version is {Version}.\r\n" +
                        "Please ensure these are compatible before proceeding.\r\n");
                }
                else if (versionValue == null)
                {
                    Log.Logger.Error($"\r\nThe host's AP world version predates 1.2.0, but the client version is {Version}.\r\n" +
                        "This will almost certainly result in errors.\r\n");
                }
            }
            else
            {
                Log.Logger.Error($"\r\nThe host's AP world version predates 1.2.0, but the client version is {Version}.\r\n" +
                    "This will almost certainly result in errors.\r\n");
            }

            _loadGameTimer = new Timer();
            _loadGameTimer.Elapsed += new ElapsedEventHandler(StartSpyroGame);
            _loadGameTimer.Interval = 5000;
            _loadGameTimer.Enabled = true;

            _cosmeticsTimer = new Timer();
            _cosmeticsTimer.Elapsed += new ElapsedEventHandler(HandleCosmeticQueue);
            _cosmeticsTimer.Interval = 5000;
            _cosmeticsTimer.Enabled = true;

            _progressiveBasketBreaks = (byte)(Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Progressive Sparx Basket Break").Count() ?? 0);
            _worldKeys = (byte)(Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "World Key").Count() ?? 0);

            _gameLoopTimer = new Timer();
            _gameLoopTimer.Elapsed += new ElapsedEventHandler(ModifyGameLoop);
            _gameLoopTimer.Interval = 100;
            _gameLoopTimer.Enabled = true;

            CheckGoalCondition();
        }
        else
        {
            Log.Logger.Error("Failed to login.  Please check your host, name, and password.");
        }
    }

    private void Client_LocationCompleted(object? sender, LocationCompletedEventArgs e)
    {
        try
        {
            if (Client.ItemState == null || Client.CurrentSession == null) return;
            var currentEggs = CalculateCurrentEggs();
            if (_gemsanityOption != GemsanityOptions.Off)
            {
                CalculateCurrentGems();
            }
            CheckGoalCondition();
        } catch (Exception ex)
        {
            Log.Logger.Warning("Encountered an error while completing a location.");
            Log.Logger.Warning(ex.ToString());
            Log.Logger.Warning("This is not necessarily a problem if it happens during release or collect.");
        }
    }

    private async void ItemReceived(object? o, ItemReceivedEventArgs args)
    {
        if (Client.ItemState == null || Client.CurrentSession == null) return;
        Log.Logger.Debug($"Item Received: {JsonConvert.SerializeObject(args.Item)}");
        UpdateItemLog();
        // Give items a moment to update before printing values.
        await Task.Delay(200);
        try
        {
            int currentHealth;
            switch (args.Item.Name)
            {
                case "Egg":
                    _checkNames = true;
                    int eggs = CalculateCurrentEggs();
                    CheckGoalCondition();
                    break;
                case "Skill Point":
                    CheckGoalCondition();
                    break;
                case "Extra Life":
                    var currentLives = Memory.ReadShort(Addresses.GetVersionAddress(Addresses.PlayerLives));
                    Memory.Write(Addresses.GetVersionAddress(Addresses.PlayerLives), (short)(Math.Min(99, currentLives + 1)));
                    break;
                case "(Over)heal Sparx":
                    // Collecting a skill point provides a full heal, so wait for that to complete first.
                    await Task.Run(async () =>
                    {
                        await Task.Delay(3000);
                        currentHealth = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.PlayerHealth));
                        // Going too high creates too many particles for the game to handle.
                        Memory.WriteByte(Addresses.GetVersionAddress(Addresses.PlayerHealth), (byte)(Math.Min(5, currentHealth + 1)));
                    });
                    break;
                case "Damage Sparx Trap":
                    // Collecting a skill point provides a full heal, so wait for that to complete first.
                    await Task.Run(async () =>
                    {
                        await Task.Delay(3000);
                        currentHealth = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.PlayerHealth));
                        Memory.WriteByte(Addresses.GetVersionAddress(Addresses.PlayerHealth), (byte)(Math.Max(currentHealth - 1, 0)));
                    });
                    break;
                case "Sparxless Trap":
                    // Collecting a skill point provides a full heal, so wait for that to complete first.
                    await Task.Run(async () =>
                    {
                        await Task.Delay(3000);
                        Memory.WriteByte(Addresses.GetVersionAddress(Addresses.PlayerHealth), (byte)(0));
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
                case "Normal Spyro":
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
                    Memory.Write(Addresses.GetVersionAddress(Addresses.CloudBellowsUnlock), 0);
                    Log.Logger.Information("You can talk to Moneybags to complete this unlock for free.");
                    break;
                case "Moneybags Unlock - Spooky Swamp Door":
                    UnlockMoneybags(Addresses.GetVersionAddress(Addresses.SpookyDoorUnlock));
                    break;
                case "Moneybags Unlock - Sheila":
                    UnlockMoneybags(Addresses.GetVersionAddress(Addresses.SheilaUnlock), Addresses.GetVersionAddress(Addresses.SheilaCutscene));
                    break;
                case "Moneybags Unlock - Icy Peak Nancy Door":
                    UnlockMoneybags(Addresses.GetVersionAddress(Addresses.IcyNancyUnlock));
                    break;
                case "Moneybags Unlock - Molten Crater Thieves Door":
                    UnlockMoneybags(Addresses.GetVersionAddress(Addresses.MoltenThievesUnlock));
                    break;
                case "Moneybags Unlock - Charmed Ridge Stairs":
                    UnlockMoneybags(Addresses.GetVersionAddress(Addresses.CharmedStairsUnlock));
                    break;
                case "Moneybags Unlock - Sgt. Byrd":
                    UnlockMoneybags(Addresses.GetVersionAddress(Addresses.SgtByrdUnlock), Addresses.GetVersionAddress(Addresses.ByrdCutscene));
                    break;
                case "Moneybags Unlock - Bentley":
                    UnlockMoneybags(Addresses.GetVersionAddress(Addresses.BentleyUnlock), Addresses.GetVersionAddress(Addresses.BentleyCutscene));
                    break;
                case "Moneybags Unlock - Desert Ruins Door":
                    UnlockMoneybags(Addresses.GetVersionAddress(Addresses.DesertDoorUnlock));
                    break;
                case "Moneybags Unlock - Agent 9":
                    UnlockMoneybags(Addresses.GetVersionAddress(Addresses.Agent9Unlock), Addresses.GetVersionAddress(Addresses.Agent9Cutscene));
                    break;
                case "Moneybags Unlock - Frozen Altars Cat Hockey Door":
                    UnlockMoneybags(Addresses.GetVersionAddress(Addresses.FrozenHockeyUnlock));
                    break;
                case "Moneybags Unlock - Crystal Islands Bridge":
                    UnlockMoneybags(Addresses.GetVersionAddress(Addresses.CrystalBridgeUnlock));
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
                case "Hint 12":
                case "Hint 13":
                    GetZoeHint(args.Item.Name);
                    break;
                case "Progressive Sparx Health Upgrade":
                    break;
                case "World Key":
                    _worldKeys++;
                    string prevWorld = "";
                    string nextWorld = "";
                    if (_worldKeys == 1)
                    {
                        prevWorld = "Sunrise Spring";
                        nextWorld = "Midday Gardens";
                    }
                    else if (_worldKeys == 2)
                    {
                        prevWorld = "Midday Gardens";
                        nextWorld = "Evening Lake";
                    }
                    else
                    {
                        prevWorld = "Evening Lake";
                        nextWorld = "Midnight Mountain";
                    }
                    Log.Logger.Information($"You will have access to {nextWorld} after completing\r\nall end of level eggs and the boss of {prevWorld}");
                    break;
                case "Increased Sparx Range":
                    Memory.Write(Addresses.GetVersionAddress(Addresses.SparxRange), (short)3072);
                    Memory.Write(Addresses.GetVersionAddress(Addresses.SparxRangeHelper1), (short)525);
                    Memory.Write(Addresses.GetVersionAddress(Addresses.SparxRangeHelper2), (short)960);
                    break;
                case "Sparx Gem Finder":
                    Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SparxGemFinder), 1);
                    break;
                case "Extra Hit Point":
                    Memory.WriteByte(Addresses.GetVersionAddress(Addresses.PlayerMaxHealth), 4);
                    Memory.WriteByte(Addresses.GetVersionAddress(Addresses.PlayerMaxHealthIsModded), 1);
                    break;
                case "Progressive Sparx Basket Break":
                    _progressiveBasketBreaks++;
                    Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SparxBreakBaskets), (byte)_progressiveBasketBreaks);
                    break;
            }
            if (args.Item.Name.EndsWith(" Defeated") || args.Item.Name.EndsWith(" Complete"))
            {
                CheckGoalCondition();
            }
            else if (args.Item.Name.EndsWith(" Gem") || args.Item.Name.EndsWith(" Gems"))
            {
                _checkNames = true;
                CheckGoalCondition();
            }
            else if (args.Item.Name.EndsWith(" Unlock"))
            {
                _checkNames = true;
                showUnlockedLevels();
            }
            else if (args.Item.Name.EndsWith(" Powerup"))
            {
                Log.Logger.Information("This will take effect the next time you enter the level(s).");
            }
        } catch (Exception ex)
        {
            Log.Logger.Warning("Encountered an error while receiving an item.\r\n" +
                ex.ToString() + "\r\n");
        }
    }

    private void showUnlockedLevels(bool alwaysShow = false)
    {
        if (_levelLockOptions == LevelLockOptions.Vanilla)
        {
            Log.Logger.Information("Levels have their vanilla unlock requirements.");
        }
        else if(_levelLockOptions != LevelLockOptions.Keys)
        {
            Log.Logger.Information("Levels are not locked by keys. You can view unlock requirements in the atlas.");
        }
        List<ItemInfo> unlockedLevels = (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName.EndsWith(" Unlock")).ToList() ?? new List<ItemInfo>());
        if (alwaysShow || unlockedLevels.Count > _unlockedLevels)
        {
            _unlockedLevels = unlockedLevels.Count;
        }
        else
        {
            return;
        }
        string unlockedLevelsString = "You have unlocked:";
        int levelCount = 0;
        foreach (ItemInfo unlockedLevel in unlockedLevels)
        {
            string newLevel = unlockedLevel.ItemName.Split(" ")[0];
            levelCount++;
            // Print 6 per line so it is easier to read.
            if (levelCount % 6 == 1)
            {
                unlockedLevelsString += "\r\n";
            }
            unlockedLevelsString += (newLevel + "; ");
        }
        if (unlockedLevelsString != "You have unlocked:")
        {
            unlockedLevelsString = unlockedLevelsString.Substring(0, unlockedLevelsString.Length - 2);
        }
        else
        {
            unlockedLevelsString += "\r\nNo levels yet";
        }
        Log.Logger.Information($"{unlockedLevelsString}\r\n");
    }

    private static async void GetZoeHint(string hintName)
    {
        if (_hintsList == null)
        {
            if (_slotData != null && _slotData.TryGetValue("hints", out var hints))
            {
                if (hints != null)
                {
                    _hintsList = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(hints.ToString());
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

    private static async void ModifyGameLoop(object source, ElapsedEventArgs e)
    {
        if (!Helpers.IsInGame() || Client.ItemState == null || Client.CurrentSession == null)
        {
            return;
        }
        try
        {
            GameStatus status = (GameStatus)Memory.ReadByte(Addresses.GetVersionAddress(Addresses.GameStatus));
            // Avoid inadvertantly messing with the Atlas and Options overlays when loaded in on the pause screen.
            // This can result in corrupted save files.
            if (status == GameStatus.Paused || status == GameStatus.LoadingWorld || status == GameStatus.LoadingHomeworld)
            {
                return;
            }
            _timerLoopCount = (_timerLoopCount + 1) % 5;
            if (_timerLoopCount == 1)
            {
                HandleSparxPowers(source, e);
            }
            CalculateCurrentEggs();
            CalculateCurrentGems();
            HandleWorldKeys(source, e);
            HandleLevelUnlocks(source, e);
            HandleMinigames(source, e);
        } catch (Exception ex)
        {
            Log.Logger.Warning("Encountered an error while managing the game loop.\r\n" +
                ex.ToString() + "\r\n" +
                "This is not necessarily a problem if it happens during release or collect.");
        }
    }

    private static async void HandleWorldKeys(object source, ElapsedEventArgs e)
    {
        if (!Helpers.IsInGame() || Client.ItemState == null || Client.CurrentSession == null)
        {
            return;
        }
        if (_isWorldKeysOn != 0 && _worldKeys < 3)
        {
            LevelInGameIDs currentLevel = (LevelInGameIDs)Memory.ReadByte(Addresses.GetVersionAddress(Addresses.CurrentLevelAddress));
            byte transportWarpLocation = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.TransportMenuAddress));
            byte nextWarpAddress = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.NextWarpAddress));
            if (currentLevel == LevelInGameIDs.BuzzsDungeon && _worldKeys < 1 && nextWarpAddress != (byte)LevelInGameIDs.SunriseSpring)
            {
                Log.Logger.Warning("You will not be able to progress to Midday Gardens without a world key.");
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.NextWarpAddress), (byte)LevelInGameIDs.SunriseSpring);
            }
            else if (currentLevel == LevelInGameIDs.SpikesArena && _worldKeys < 2 && nextWarpAddress != (byte)LevelInGameIDs.MiddayGardens)
            {
                Log.Logger.Warning("You will not be able to progress to Evening Lake without a second world key.");
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.NextWarpAddress), (byte)LevelInGameIDs.MiddayGardens);
            }
            else if (currentLevel == LevelInGameIDs.ScorchsPit && _worldKeys < 3 && nextWarpAddress != (byte)LevelInGameIDs.EveningLake)
            {
                Log.Logger.Warning("You will not be able to progress to Midnight Mountain without a third world key.");
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.NextWarpAddress), (byte)LevelInGameIDs.EveningLake);
            }
            if (transportWarpLocation == (byte)LevelInGameIDs.MiddayGardens && _worldKeys < 1)
            {
                byte isBuzzDefeated = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.BuzzDefeated));
                if (isBuzzDefeated == 1)
                {
                    Log.Logger.Warning("You will not be able to progress to Midday Gardens without a world key.");
                    Memory.WriteByte(Addresses.GetVersionAddress(Addresses.TransportMenuAddress), (byte)LevelInGameIDs.SunriseSpring);
                }
            }
            else if (transportWarpLocation == (byte)LevelInGameIDs.EveningLake && _worldKeys < 2)
            {
                byte isSpikeDefeated = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.SpikeDefeated));
                if (isSpikeDefeated == 1)
                {
                    Log.Logger.Warning("You will not be able to progress to Evening Lake without a second world key.");
                    Memory.WriteByte(Addresses.GetVersionAddress(Addresses.TransportMenuAddress), (byte)LevelInGameIDs.MiddayGardens);
                }
            }
            else if (transportWarpLocation == (byte)LevelInGameIDs.MidnightMountain && _worldKeys < 3)
            {
                byte isScorchDefeated = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.ScorchDefeated));
                if (isScorchDefeated == 1)
                {
                    Log.Logger.Warning("You will not be able to progress to Midnight Mountain without a third world key.");
                    Memory.WriteByte(Addresses.GetVersionAddress(Addresses.HunterRescuedCutscene), 1);
                    Memory.WriteByte(Addresses.GetVersionAddress(Addresses.TransportMenuAddress), (byte)LevelInGameIDs.EveningLake);
                }
            }
        }
    }

    private static async void HandleSparxPowers(object source, ElapsedEventArgs e)
    {
        if (!Helpers.IsInGame() || Client.ItemState == null || Client.CurrentSession == null)
        {
            return;
        }
        if (_sparxPowerShuffle != 0)
        {
            // We need to override the powerups given by Zoe for completing a Sparx Level.
            var extendedRange = Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Increased Sparx Range").Count() ?? 0;
            var gemFinder = Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Sparx Gem Finder").Count() ?? 0;
            var extraHitPoint = Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Extra Hit Point").Count() ?? 0;
            if (extendedRange == 0)
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.SparxRange), (short)2062);
                Memory.Write(Addresses.GetVersionAddress(Addresses.SparxRangeHelper1), (short)350);
                Memory.Write(Addresses.GetVersionAddress(Addresses.SparxRangeHelper2), (short)640);
            }
            else
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.SparxRange), (short)3072);
                Memory.Write(Addresses.GetVersionAddress(Addresses.SparxRangeHelper1), (short)525);
                Memory.Write(Addresses.GetVersionAddress(Addresses.SparxRangeHelper2), (short)960);
            }
            if (gemFinder == 0)
            {
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SparxGemFinder), 0);
            }
            else
            {
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SparxGemFinder), 1);
            }
            if (extraHitPoint == 0)
            {
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.PlayerMaxHealth), 3);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.PlayerMaxHealthIsModded), 0);
            }
            else
            {
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.PlayerMaxHealth), 4);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.PlayerMaxHealthIsModded), 1);
            }
            if (_progressiveBasketBreaks > 2) _progressiveBasketBreaks = 2;
            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SparxBreakBaskets), (byte)_progressiveBasketBreaks);
        }

        byte sparxHealth = (byte)(Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Progressive Sparx Health Upgrade").Count() ?? 0);
        if (_sparxOption == ProgressiveSparxHealthOptions.Blue)
        {
            sparxHealth += 2;
        }
        else if (_sparxOption == ProgressiveSparxHealthOptions.Green)
        {
            sparxHealth += 1;
        }
        if (_sparxOption != ProgressiveSparxHealthOptions.Off && sparxHealth < 3)
        {
            // TODO: Allow overheal to work for 15 seconds.
            byte currentHealth = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.PlayerHealth));
            sparxHealth += (byte)(Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Extra Hit Point").Count() ?? 0);
            if ((Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Starfish Reef Complete").Count() ?? 0) > 0 && _sparxPowerShuffle == 0)
            {
                sparxHealth++;
            }
            if (_sparxOption == ProgressiveSparxHealthOptions.TrueSparxless)
            {
                sparxHealth = 0;
            }
            if (currentHealth <= 128 && currentHealth > sparxHealth)
            {
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.PlayerHealth), sparxHealth);
            }
            LevelInGameIDs currentLevel = (LevelInGameIDs)Memory.ReadByte(Addresses.GetVersionAddress(Addresses.CurrentLevelAddress));
            byte maxSparxHealth = (byte)(Math.Min(sparxHealth * 2, 6));
            if (maxSparxHealth == 0)
            {
                maxSparxHealth = 1;
            }
            if (currentLevel == LevelInGameIDs.CrawdadFarm)
            {
                byte currentSparxHealth = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.PlayerHealthCrawdad));
                if (currentSparxHealth > maxSparxHealth)
                {
                    Memory.WriteByte(Addresses.GetVersionAddress(Addresses.PlayerHealthCrawdad), maxSparxHealth);
                }
            }
            else if (currentLevel == LevelInGameIDs.SpiderTown)
            {
                byte currentSparxHealth = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.PlayerHealthSpider));
                if (currentSparxHealth > maxSparxHealth)
                {
                    Memory.WriteByte(Addresses.GetVersionAddress(Addresses.PlayerHealthSpider), maxSparxHealth);
                }
            }
            else if (currentLevel == LevelInGameIDs.StarfishReef)
            {
                byte currentSparxHealth = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.PlayerHealthStarfish));
                if (currentSparxHealth > maxSparxHealth)
                {
                    Memory.WriteByte(Addresses.GetVersionAddress(Addresses.PlayerHealthStarfish), maxSparxHealth);
                }
            }
            else if (currentLevel == LevelInGameIDs.BugbotFactory)
            {
                byte currentSparxHealth = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.PlayerHealthBugbot));
                if (currentSparxHealth > maxSparxHealth)
                {
                    Memory.WriteByte(Addresses.GetVersionAddress(Addresses.PlayerHealthBugbot), maxSparxHealth);
                }
            }
        }
    }

    /**
     * Writes a block of text to memory. endAddress will generally be the null terminator and will not be written to.
     */
    private static void WriteStringToMemory(uint startAddress, uint endAddress, string stringToWrite, bool padWithSpaces=true)
    {
        uint address = startAddress;
        int stringIndex = 0;
        while (address < endAddress)
        {
            char charToWrite = ' ';
            if (!padWithSpaces)
            {
                charToWrite = '\0';
            }
            if (stringIndex < stringToWrite.Length)
            {
                charToWrite = stringToWrite[stringIndex];
            }
            Memory.WriteByte(address, (byte)charToWrite);
            stringIndex++;
            address++;
        }
    }

    private static void HandleMinigames(object source, ElapsedEventArgs e)
    {
        // NOTE: Be very careful here, as writing to the wrong address or at the wrong time can crash the game.
        if (!Helpers.IsInGame(false) || Client.ItemState == null || Client.CurrentSession == null)
        {
            return;
        }
        
        LevelInGameIDs currentLevel = (LevelInGameIDs)Memory.ReadByte(Addresses.GetVersionAddress(Addresses.CurrentLevelAddress));
        byte currentSubarea = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.CurrentSubareaAddress));
        GameStatus gameStatus = (GameStatus)Memory.ReadByte(Addresses.GetVersionAddress(Addresses.GameStatus));

        if (_openWorld != 0 && currentLevel == LevelInGameIDs.SpookySwamp)
        {
            if (!Memory.ReadBit(Addresses.GetVersionAddress(Addresses.SpookyOpenWorldGem), 7))
            {
                Memory.WriteBit(Addresses.GetVersionAddress(Addresses.SpookyOpenWorldGem), 7, true);
                if (_gemsanityOption == GemsanityOptions.Off)
                {
                    int spookyGemCount = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.SpookySwampGems));
                    int totalGemCount = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.TotalGemAddress));
                    Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SpookySwampGems), (byte)(spookyGemCount + 1));
                    Memory.WriteByte(Addresses.GetVersionAddress(Addresses.TotalGemAddress), (byte)(totalGemCount + 1));
                }
            }
        }

        if (currentLevel != _previousLevel && _handleGemsanity)
        {
            Helpers.UpdateLocationList(currentLevel, Client);
        }

        if (_deathLinkService != null && !_hasSubmittedGoal)
        {
            byte health = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.PlayerHealth));
            bool isInSparxLevel = currentLevel == LevelInGameIDs.CrawdadFarm || 
                currentLevel == LevelInGameIDs.SpiderTown || 
                currentLevel == LevelInGameIDs.StarfishReef || 
                currentLevel ==  LevelInGameIDs.BugbotFactory;
            if (currentLevel == LevelInGameIDs.CrawdadFarm)
            {
                health = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.PlayerHealthCrawdad));
            }
            else if (currentLevel == LevelInGameIDs.SpiderTown)
            {
                health = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.PlayerHealthSpider));
            }
            else if (currentLevel == LevelInGameIDs.StarfishReef)
            {
                health = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.PlayerHealthStarfish));
            }
            else if (currentLevel == LevelInGameIDs.BugbotFactory)
            {
                health = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.PlayerHealthBugbot));
            }
            int animationLength = Memory.ReadInt(Addresses.GetVersionAddress(Addresses.PlayerAnimationLength));
            int zPos = Memory.ReadInt(Addresses.GetVersionAddress(Addresses.SpyroPositionZ));
            byte spyroState = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.SpyroState2));
            byte spyroVelocityFlag = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.PlayerVelocityStatus));
            LevelInGameIDs[] deathLinkLevels = [
                LevelInGameIDs.SunriseSpring,
                LevelInGameIDs.SunnyVilla,
                LevelInGameIDs.CloudSpires,
                LevelInGameIDs.MoltenCrater,
                LevelInGameIDs.SeashellShore,
                LevelInGameIDs.SheilasAlp,
                LevelInGameIDs.BuzzsDungeon,
                LevelInGameIDs.CrawdadFarm,
                LevelInGameIDs.MiddayGardens,
                LevelInGameIDs.IcyPeak,
                LevelInGameIDs.EnchantedTowers,
                LevelInGameIDs.SpookySwamp,
                LevelInGameIDs.BambooTerrace,
                LevelInGameIDs.SgtByrdsBase,
                LevelInGameIDs.SpikesArena,
                LevelInGameIDs.SpiderTown,
                LevelInGameIDs.EveningLake,
                LevelInGameIDs.FrozenAltars,
                LevelInGameIDs.LostFleet,
                LevelInGameIDs.FireworksFactory,
                LevelInGameIDs.CharmedRidge,
                LevelInGameIDs.BentleysOutpost,
                LevelInGameIDs.ScorchsPit,
                LevelInGameIDs.StarfishReef,
                LevelInGameIDs.MidnightMountain,
                LevelInGameIDs.CrystalIslands,
                LevelInGameIDs.DesertRuins,
                LevelInGameIDs.HauntedTomb,
                LevelInGameIDs.DinoMines,
                LevelInGameIDs.Agent9sLab,
                LevelInGameIDs.SorceressLair,
                LevelInGameIDs.BugbotFactory,
                LevelInGameIDs.SuperBonusRound
            ];

            if (
                !_justDied &&
                Helpers.IsInGame() &&
                Client.ItemState != null &&
                Client.CurrentSession != null &&
                deathLinkLevels.Contains(currentLevel) &&
                gameStatus != GameStatus.Cutscene &&
                gameStatus != GameStatus.Loading &&
                gameStatus != GameStatus.TitleScreen &&
                (
                    !isInSparxLevel && health > 128 ||
                    isInSparxLevel && health == 0 ||
                    (spyroState == 3 && spyroVelocityFlag == 0x7f && 0x45 < animationLength) ||
                    zPos > 10 && zPos < 0x400
                ) &&
                !_deathlinkImmunity
            )
            {
                _justDied = true;
                string deathlinkCause = "Unknown";
                if (!isInSparxLevel && health > 128)
                {
                    deathlinkCause = "Damage or drowning";
                }
                else if (isInSparxLevel && health == 0)
                {
                    deathlinkCause = "Damage in Sparx level";
                }
                else if (spyroState == 3 && spyroVelocityFlag == 0x7f && 0x45 < animationLength || zPos > 10 && zPos < 0x400)
                {
                    deathlinkCause = "Fell to death";
                }
                Log.Logger.Information($"Sending DeathLink. Cause: {deathlinkCause}");
                if (deathlinkCause == "Unknown")
                {
                    deathlinkCause = Client.CurrentSession.Players.ActivePlayer.Name + " died in Spyro 3.";
                }
                else
                {
                    deathlinkCause = Client.CurrentSession.Players.ActivePlayer.Name + $" died in Spyro 3: {deathlinkCause}";
                }
                _deathLinkService.SendDeathLink(new DeathLink(Client.CurrentSession.Players.ActivePlayer.Name, cause: deathlinkCause));
            }
            else if (_justDied &&
                !(
                    !isInSparxLevel && health > 128 ||
                    isInSparxLevel && health == 0 ||
                    (spyroState == 3 && spyroVelocityFlag == 0x7f && 0x45 < animationLength) ||
                    zPos > 10 && zPos < 0x400
                )
            )
            {
                _justDied = false;
            }
        }
        if (_powerupLockOption != PowerupLockOptions.Vanilla)
        {
            if (currentLevel == LevelInGameIDs.SunriseSpring)
            {
                if (
                    _powerupLockOption == PowerupLockOptions.Type && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Superfly Powerup").Count() ?? 0) == 0 ||
                    _powerupLockOption == PowerupLockOptions.Individual && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Sunrise Superfly Powerup").Count() ?? 0) == 0
                )
                {
                    Memory.Write(Addresses.GetVersionAddress(Addresses.SunrisePowerup) + 20, 100000);
                }
            }
            else if (currentLevel == LevelInGameIDs.CloudSpires && currentSubarea == 2)  // Bell tower sub area
            {
                if (
                    _powerupLockOption == PowerupLockOptions.Type && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Superfly Powerup").Count() ?? 0) == 0 ||
                    _powerupLockOption == PowerupLockOptions.Individual && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Cloud Superfly Powerup").Count() ?? 0) == 0
                )
                {
                    Memory.Write(Addresses.GetVersionAddress(Addresses.CloudPowerup) + 20, 100000);
                }
            }
            else if (currentLevel == LevelInGameIDs.MiddayGardens)
            {
                if (
                    _powerupLockOption == PowerupLockOptions.Type && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Fireball Powerup").Count() ?? 0) == 0 ||
                    _powerupLockOption == PowerupLockOptions.Individual && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Midday Fireball Powerup").Count() ?? 0) == 0
                )
                {
                    Memory.Write(Addresses.GetVersionAddress(Addresses.MiddayPowerup) + 20, 100000);
                }
            }
            else if (currentLevel == LevelInGameIDs.BambooTerrace && currentSubarea == 1)  // Boat sub area
            {
                if (
                    _powerupLockOption == PowerupLockOptions.Type && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Fireball Powerup").Count() ?? 0) == 0 ||
                    _powerupLockOption == PowerupLockOptions.Individual && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Bamboo Fireball Powerup").Count() ?? 0) == 0
                )
                {
                    Memory.Write(Addresses.GetVersionAddress(Addresses.BambooPowerup) + 20, 100000);
                }
            }
            else if (currentLevel == LevelInGameIDs.EveningLake)
            {
                if (
                    _powerupLockOption == PowerupLockOptions.Type && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Invincibility Powerup").Count() ?? 0) == 0 ||
                    _powerupLockOption == PowerupLockOptions.Individual && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Evening Invincibility Powerup").Count() ?? 0) == 0
                )
                {
                    Memory.Write(Addresses.GetVersionAddress(Addresses.EveningPowerup) + 20, 100000);
                }
            }
            else if (currentLevel == LevelInGameIDs.FireworksFactory && currentSubarea == 1)  // Twin Dragons sub area
            {
                if (
                    _powerupLockOption == PowerupLockOptions.Type && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Fireball Powerup").Count() ?? 0) == 0 ||
                    _powerupLockOption == PowerupLockOptions.Type && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Superfly Powerup").Count() ?? 0) == 0 ||
                    _powerupLockOption == PowerupLockOptions.Individual && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Fireworks Combo Powerup").Count() ?? 0) == 0
                )
                {
                    Memory.Write(Addresses.GetVersionAddress(Addresses.FireworksPowerup) + 20, 100000);
                }
            }
            else if (currentLevel == LevelInGameIDs.LostFleet && currentSubarea == 0)  // Main area
            {
                if (
                    _powerupLockOption == PowerupLockOptions.Type && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Invincibility Powerup").Count() ?? 0) == 0 ||
                    _powerupLockOption == PowerupLockOptions.Individual && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Fleet Invincibility Powerup").Count() ?? 0) == 0
                )
                {
                    Memory.Write(Addresses.GetVersionAddress(Addresses.LostFleetPowerup1) + 20, 100000);
                }
            }
            else if (currentLevel == LevelInGameIDs.LostFleet && currentSubarea == 1)  // Subs
            {
                if (
                    _powerupLockOption == PowerupLockOptions.Type && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Invincibility Powerup").Count() ?? 0) == 0 ||
                    _powerupLockOption == PowerupLockOptions.Individual && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Fleet Invincibility Powerup").Count() ?? 0) == 0
                )
                {
                    Memory.Write(Addresses.GetVersionAddress(Addresses.LostFleetPowerup2) + 20, 100000);
                    Memory.Write(Addresses.GetVersionAddress(Addresses.LostFleetPowerup3) + 20, 100000);
                }
            }
            else if (currentLevel == LevelInGameIDs.CharmedRidge && currentSubarea == 0)  // Main area
            {
                if (
                    _powerupLockOption == PowerupLockOptions.Type && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Fireball Powerup").Count() ?? 0) == 0 ||
                    _powerupLockOption == PowerupLockOptions.Individual && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Charmed Fireball Powerup").Count() ?? 0) == 0
                )
                {
                    Memory.Write(Addresses.GetVersionAddress(Addresses.CharmedRidgePowerup) + 20, 100000);
                }
            }
            else if (currentLevel == LevelInGameIDs.CrystalIslands && currentSubarea == 0)  // Main area
            {
                if (
                    _powerupLockOption == PowerupLockOptions.Type && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Superfly Powerup").Count() ?? 0) == 0 ||
                    _powerupLockOption == PowerupLockOptions.Individual && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Crystal Superfly Powerup").Count() ?? 0) == 0
                )
                {
                    Memory.Write(Addresses.GetVersionAddress(Addresses.CrystalIslandsPowerup) + 20, 100000);
                }
            }
            else if (currentLevel == LevelInGameIDs.SuperBonusRound && currentSubarea == 0)  // Main area
            {
                if (
                    _powerupLockOption == PowerupLockOptions.Type && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Fireball Powerup").Count() ?? 0) == 0 ||
                    _powerupLockOption == PowerupLockOptions.Type && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Superfly Powerup").Count() ?? 0) == 0 ||
                    _powerupLockOption == PowerupLockOptions.Individual && (Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Super Bonus Round Combo Powerup").Count() ?? 0) == 0
                )
                {
                    Memory.Write(Addresses.GetVersionAddress(Addresses.SuperBonusRoundPowerup) + 20, 100000);
                }
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
            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.LocalDifficultySettingAddress), 0);
        }

        // For other challenges, we can set values in RAM to make the challenge easier.
        if (
            currentLevel == LevelInGameIDs.SunnyVilla && currentSubarea == 2              // Skatepark
        )
        {
            if (_easyChallenges.Contains("easy_skateboarding") || _easyChallenges.Contains("easy_skateboarding_lizards"))
            {
                byte currentLizards = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.SunnyLizardsCount));
                if (currentLizards < 14)
                {
                    Memory.Write(Addresses.GetVersionAddress(Addresses.SunnyLizardsCount), (byte)14);
                }
            }
            if (_easyChallenges.Contains("easy_skateboarding") || _easyChallenges.Contains("easy_skateboarding_points"))
            {
                short currentScore = Memory.ReadShort(Addresses.GetVersionAddress(Addresses.SunnySkateScore));
                if (currentScore < 3199)
                {
                    Memory.Write(Addresses.GetVersionAddress(Addresses.SunnySkateScore), (short)3199);
                }
            }
        }
        else if (
            _easyChallenges.Contains("easy_bluto") &&
            currentLevel == LevelInGameIDs.SeashellShore && currentSubarea == 2          // Bluto
        )
        {
            byte currentEnemyHealth = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.BlutoHealth));
            if (currentEnemyHealth > 1)
            {
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.BlutoHealth), (byte)1);
            }
        }
        else if (
            (_easyChallenges.Contains("easy_skateboarding") || _easyChallenges.Contains("easy_skateboarding_points")) &&
            currentLevel == LevelInGameIDs.EnchantedTowers && currentSubarea == 1        // Skatepark
        )
        {
            short currentScore = Memory.ReadShort(Addresses.GetVersionAddress(Addresses.EnchantedSkateScore));
            if (currentScore < 9999)
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.EnchantedSkateScore), (short)9999);
            }
        }
        else if (
            _easyChallenges.Contains("easy_sleepyhead") &&
            currentLevel == LevelInGameIDs.SpookySwamp && currentSubarea == 1            // Sleepyhead
        )
        {
            byte currentEnemyHealth = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.SleepyheadHealth));
            if (currentEnemyHealth > 1)
            {
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SleepyheadHealth), (byte)1);
            }
        }
        else if (
            _easyChallenges.Contains("easy_boxing") &&
            currentLevel == LevelInGameIDs.FrozenAltars && currentSubarea == 1           // Yeti Boxing
        )
        {
            byte currentEnemyHealth = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.YetiBoxingHealth));
            if (currentEnemyHealth > 1)
            {
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.YetiBoxingHealth), (byte)1);
            }
        }
        else if (
            _easyChallenges.Contains("easy_subs") &&
            currentLevel == LevelInGameIDs.LostFleet && currentSubarea == 1              // Subs
        )
        {
            foreach (uint address in Addresses.LostFleetSubAddresses)
            {
                Memory.WriteByte(Addresses.GetVersionAddress(address), (byte)19);
            }
        }
        else if (
            (_easyChallenges.Contains("easy_skateboarding") || _easyChallenges.Contains("easy_skateboarding_lost_fleet")) &&
            currentLevel == LevelInGameIDs.LostFleet && currentSubarea == 2              // Skatepark
        )
        {
            Memory.Write(Addresses.GetVersionAddress(Addresses.LostFleetNitro), (short)1000);
        }
        else if (
            _easyChallenges.Contains("easy_whackamole") &&
            currentLevel == LevelInGameIDs.CrystalIslands && currentSubarea == 2         // Whack-A-Mole
        )
        {
            byte currentMoleCount = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.WhackAMoleCount));
            if (currentMoleCount < 19)
            {
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.WhackAMoleCount), (byte)19);
            }
        }
        else if (
            _easyChallenges.Contains("easy_shark_riders") &&
            currentLevel == LevelInGameIDs.DesertRuins && currentSubarea == 2            // Shark Riders
        )
        {
            foreach (uint address in Addresses.DesertSharkAddresses)
            {
                Memory.WriteByte(Addresses.GetVersionAddress(address), (byte)253);
            }
        }
        else if (
            _easyChallenges.Contains("easy_tanks") &&
            currentLevel == LevelInGameIDs.HauntedTomb && currentSubarea == 1            // Tanks
        )
        {
            byte currentTankCount = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.TanksCount));
            byte maxTankCount = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.MaxTanksCount));
            if (maxTankCount == 4 && currentTankCount < 4)
            {
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.TanksCount), (byte)3);
            }
            else if (maxTankCount == 10 && currentTankCount < 10)
            {
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.TanksCount), (byte)9);
            }
        }
        else if (
            _easyChallenges.Contains("no_green_rockets") &&
            currentLevel == LevelInGameIDs.ScorchsPit
        )
        {
            byte greenRockets = Memory.ReadByte(Addresses.GetVersionAddress(Addresses.GreenRocketCount));
            int redRockets = (int)Memory.ReadByte(Addresses.GetVersionAddress(Addresses.RedRocketCount));
            if (greenRockets > 0)
            {
                redRockets = Math.Min(200, redRockets + 50);
                greenRockets--;
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.RedRocketCount), (byte)redRockets);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.GreenRocketCount), greenRockets);
                if (greenRockets == 0)
                {
                    Memory.WriteByte(Addresses.GetVersionAddress(Addresses.HasGreenRocketAddress), 0);
                }
            }
        }
        else if (
            (_easyChallenges.Contains("easy_skateboarding") || _easyChallenges.Contains("easy_skateboarding_super_bonus_round")) &&
            currentLevel == LevelInGameIDs.SuperBonusRound && currentSubarea == 2              // Skatepark
        )
        {
            Memory.Write(Addresses.GetVersionAddress(Addresses.SuperBonusRoundNitro), (short)1300);
        }
        else if (
            currentLevel == LevelInGameIDs.MidnightMountain && gameStatus == GameStatus.InGame  // Protect this write carefully or it crashes warping to Midnight.
        )
        {
            Memory.Write(Addresses.GetVersionAddress(Addresses.SorceressDoorReq1), (short)_sorceressDoorEggReq);
            Memory.Write(Addresses.GetVersionAddress(Addresses.SorceressDoorReq2), (short)_sorceressDoorEggReq);
            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SorceressDoorReqDisplay), (byte)_sorceressDoorEggReq);
            Memory.Write(Addresses.GetVersionAddress(Addresses.SBRGemReq1), (short)_sbrDoorGemReq);
            Memory.Write(Addresses.GetVersionAddress(Addresses.SBRGemReq2), (short)_sbrDoorGemReq);
            Memory.Write(Addresses.GetVersionAddress(Addresses.SBRGemReq3), (short)_sbrDoorGemReq);
            Memory.Write(Addresses.GetVersionAddress(Addresses.SBRGemReq4), (short)_sbrDoorGemReq);
            Memory.Write(Addresses.GetVersionAddress(Addresses.SBRGemReq5), (short)_sbrDoorGemReq);
            Memory.Write(Addresses.GetVersionAddress(Addresses.SBRGemReq6), (short)_sbrDoorGemReq);
            if (_sbrDoorGemReq >= 9001)
            {
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SBRGemDisplayFirstDigit), (byte)0x1d);
            }
            else
            {
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SBRGemDisplayFirstDigit), (byte)0x1c);
            }
            int sbrGemSecondDigit = (_sbrDoorGemReq + 999) / 1000;
            if (sbrGemSecondDigit > 9)
            {
                sbrGemSecondDigit -= 10;
            }
            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SBRGemDisplaySecondDigit), (byte)(0x1c + sbrGemSecondDigit));
            Memory.Write(Addresses.GetVersionAddress(Addresses.SBREggReq1), (short)_sbrDoorEggReq);
            Memory.Write(Addresses.GetVersionAddress(Addresses.SBREggReq2), (short)_sbrDoorEggReq);
            Memory.Write(Addresses.GetVersionAddress(Addresses.SBREggReq3), (short)_sbrDoorEggReq);
        }
        else if (
            currentLevel == LevelInGameIDs.SuperBonusRound && gameStatus == GameStatus.InGame  // Protect this write carefully or it crashes warping to SBR.
        )
        {
            Memory.Write(Addresses.GetVersionAddress(Addresses.SBRSubsGemReq), (short)(_sbrDoorGemReq + 1000));
            Memory.Write(Addresses.GetVersionAddress(Addresses.SBRSkateboardingGemReq), (short)(_sbrDoorGemReq + 2000));
            Memory.Write(Addresses.GetVersionAddress(Addresses.SBRUFOsGemReq), (short)(_sbrDoorGemReq + 3500));
            Memory.Write(Addresses.GetVersionAddress(Addresses.SBRSorcGemReq), (short)(_sbrDoorGemReq + 5000));
        }
        _previousLevel = currentLevel;
    }

    private static bool HandleKeyUnlock(string levelName, uint portalAddress, uint nameAddress, uint nameLength)
    {
        if ((Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == $"{levelName} Unlock").Count() ?? 0) > 0)
        {
            Memory.WriteByte(Addresses.GetVersionAddress(portalAddress), 6);
            return true;
        }
        else
        {
            Memory.WriteByte(Addresses.GetVersionAddress(portalAddress), 0);
            return false;
        }
    }

    private static bool HandleKeyName(string levelName, uint portalAddress, uint nameAddress, uint nameLength)
    {
        if ((Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == $"{levelName} Unlock").Count() ?? 0) > 0)
        {
            WriteStringToMemory(
                Addresses.GetVersionAddress(nameAddress),
                Addresses.GetVersionAddress(nameAddress) + nameLength,
                levelName,
                padWithSpaces: false
            );
            return true;
        }
        else
        {
            WriteStringToMemory(
                Addresses.GetVersionAddress(nameAddress),
                Addresses.GetVersionAddress(nameAddress) + nameLength,
                "LOCKED",
                padWithSpaces: false
            );
            return false;
        }
    }

    private static bool HandleEggGemUnlock(string levelName, uint portalAddress, uint nameAddress, uint nameLength, int eggCount, int gemCount, double multiplier)
    {
        int eggReq = (int)Math.Floor(_eggRequirements.GetValueOrDefault(levelName, 0) * multiplier);
        int gemReq = _gemRequirements.GetValueOrDefault(levelName, 0);
        if (eggCount >= eggReq && gemCount >= gemReq)
        {
            Memory.WriteByte(Addresses.GetVersionAddress(portalAddress), 6);
            return true;
        }
        else
        {
            Memory.WriteByte(Addresses.GetVersionAddress(portalAddress), 0);
            return false;
        }
    }

    private static bool HandleEggGemName(string levelName, uint portalAddress, uint nameAddress, uint nameLength, int eggCount, int gemCount, double multiplier)
    {
        int eggReq = (int)Math.Floor(_eggRequirements.GetValueOrDefault(levelName, 0) * multiplier);
        int gemReq = _gemRequirements.GetValueOrDefault(levelName, 0);
        if (eggCount >= eggReq && gemCount >= gemReq)
        {
            WriteStringToMemory(
                Addresses.GetVersionAddress(nameAddress),
                Addresses.GetVersionAddress(nameAddress) + nameLength,
                levelName,
                padWithSpaces: false
            );
            return true;
        }
        else
        {
            string req = $"Gem x{gemReq}";
            if (eggReq > 0)
            {
                req = $"Egg x{eggReq}";
            }
            WriteStringToMemory(
                Addresses.GetVersionAddress(nameAddress),
                Addresses.GetVersionAddress(nameAddress) + nameLength,
                req,
                padWithSpaces: false
            );
            return false;
        }
    }

    private static void HandleLevelUnlocks(object source, ElapsedEventArgs e)
    {
        LevelInGameIDs currentLevel = (LevelInGameIDs)Memory.ReadByte(Addresses.GetVersionAddress(Addresses.CurrentLevelAddress));
        double multiplier = 1.0;
        if (_goal == CompletionGoal.EggHunt)
        {
            multiplier = _eggCountGoal / 100.0;
            if (multiplier > 1.0)
            {
                multiplier = 1.0;
            }
        }
        int eggs = CalculateCurrentEggs();
        int gems = CalculateCurrentGems();
        if (_levelLockOptions == LevelLockOptions.Vanilla)
        {
            if (_openWorld != 0 && eggs >= Math.Floor(10 * multiplier))
            {
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.MoltenUnlocked), 1);
            }
            if (_openWorld != 0 && eggs >= Math.Floor(14 * multiplier))
            {
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SeashellUnlocked), 1);
            }
            if (currentLevel == LevelInGameIDs.SunriseSpring)
            {
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.MoltenEggReq), (byte)Math.Floor(10 * multiplier));
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SeashellEggReq), (byte)Math.Floor(14 * multiplier));
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.MushroomEggReq), (byte)Math.Floor(20 * multiplier));
            }
            else if (currentLevel == LevelInGameIDs.MiddayGardens)
            {
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SpookyEggReq), (byte)Math.Floor(25 * multiplier));
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.BambooEggReq), (byte)Math.Floor(30 * multiplier));
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.CountryEggReq), (byte)Math.Floor(36 * multiplier));
            }
            else if (currentLevel == LevelInGameIDs.EveningLake)
            {
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.FireworksEggReq), (byte)Math.Floor(50 * multiplier));
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.CharmedEggReq), (byte)Math.Floor(58 * multiplier));
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.HoneyEggReq), (byte)Math.Floor(65 * multiplier));
            }
            else if (currentLevel == LevelInGameIDs.MidnightMountain)
            {
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.HauntedEggReq), (byte)Math.Floor(70 * multiplier));
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.DinoEggReq), (byte)Math.Floor(80 * multiplier));
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.HarborEggReq), (byte)Math.Floor(90 * multiplier));
            }
        }
        else if (_levelLockOptions == LevelLockOptions.Keys)
        {
            if (_checkNames)
            {
                HandleKeyName("Sunny Villa", Addresses.SunnyVillaPortal, Addresses.SunnyVillaName, 12);
                HandleKeyName("Cloud Spires", Addresses.CloudSpiresPortal, Addresses.CloudSpiresName, 16);
                HandleKeyName("Molten Crater", Addresses.MoltenCraterPortal, Addresses.MoltenCraterName, 16);
                HandleKeyName("Seashell Shore", Addresses.SeashellShorePortal, Addresses.SeashellShoreName, 16);
                HandleKeyName("Mushroom Speedway", Addresses.MushroomSpeedwayPortal, Addresses.MushroomSpeedwayName, 20);
                HandleKeyName("Enchanted Towers", Addresses.EnchantedTowersPortal, Addresses.EnchantedTowersName, 20);
                HandleKeyName("Icy Peak", Addresses.IcyPeakPortal, Addresses.IcyPeakName, 12);
                HandleKeyName("Spooky Swamp", Addresses.SpookySwampPortal, Addresses.SpookySwampName, 16);
                HandleKeyName("Bamboo Terrace", Addresses.BambooTerracePortal, Addresses.BambooTerraceName, 16);
                HandleKeyName("Country Speedway", Addresses.CountrySpeedwayPortal, Addresses.CountrySpeedwayName, 20);
                HandleKeyName("Frozen Altars", Addresses.FrozenAltarsPortal, Addresses.FrozenAltarsName, 16);
                HandleKeyName("Lost Fleet", Addresses.LostFleetPortal, Addresses.LostFleetName, 12);
                HandleKeyName("Fireworks Factory", Addresses.FireworksFactoryPortal, Addresses.FireworksFactoryName, 20);
                HandleKeyName("Charmed Ridge", Addresses.CharmedRidgePortal, Addresses.CharmedRidgeName, 16);
                HandleKeyName("Honey Speedway", Addresses.HoneySpeedwayPortal, Addresses.HoneySpeedwayName, 16);
                HandleKeyName("Crystal Islands", Addresses.CrystalIslandsPortal, Addresses.CrystalIslandsName, 16);
                HandleKeyName("Desert Ruins", Addresses.DesertRuinsPortal, Addresses.DesertRuinsName, 16);
                HandleKeyName("Haunted Tomb", Addresses.HauntedTombPortal, Addresses.HauntedTombName, 16);
                HandleKeyName("Dino Mines", Addresses.DinoMinesPortal, Addresses.DinoMinesName, 12);
                HandleKeyName("Harbor Speedway", Addresses.HarborSpeedwayPortal, Addresses.HarborSpeedwayName, 16);
                _checkNames = false;
            }
            if (currentLevel == LevelInGameIDs.SunriseSpring)
            {
                HandleKeyUnlock("Sunny Villa", Addresses.SunnyVillaPortal, Addresses.SunnyVillaName, 12);
                HandleKeyUnlock("Cloud Spires", Addresses.CloudSpiresPortal, Addresses.CloudSpiresName, 16);
                HandleKeyUnlock("Molten Crater", Addresses.MoltenCraterPortal, Addresses.MoltenCraterName, 16);
                HandleKeyUnlock("Seashell Shore", Addresses.SeashellShorePortal, Addresses.SeashellShoreName, 16);
                HandleKeyUnlock("Mushroom Speedway", Addresses.MushroomSpeedwayPortal, Addresses.MushroomSpeedwayName, 20);

                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.MoltenEggReq), (byte)1);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SeashellEggReq), (byte)1);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.MushroomEggReq), (byte)1);
            }
            else if (currentLevel == LevelInGameIDs.MiddayGardens)
            {
                HandleKeyUnlock("Enchanted Towers", Addresses.EnchantedTowersPortal, Addresses.EnchantedTowersName, 20);
                HandleKeyUnlock("Icy Peak", Addresses.IcyPeakPortal, Addresses.IcyPeakName, 12);
                HandleKeyUnlock("Spooky Swamp", Addresses.SpookySwampPortal, Addresses.SpookySwampName, 16);
                HandleKeyUnlock("Bamboo Terrace", Addresses.BambooTerracePortal, Addresses.BambooTerraceName, 16);
                HandleKeyUnlock("Country Speedway", Addresses.CountrySpeedwayPortal, Addresses.CountrySpeedwayName, 20);

                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SpookyEggReq), (byte)1);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.BambooEggReq), (byte)1);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.CountryEggReq), (byte)1);
            }
            else if (currentLevel == LevelInGameIDs.EveningLake)
            {
                HandleKeyUnlock("Frozen Altars", Addresses.FrozenAltarsPortal, Addresses.FrozenAltarsName, 16);
                HandleKeyUnlock("Lost Fleet", Addresses.LostFleetPortal, Addresses.LostFleetName, 12);
                HandleKeyUnlock("Fireworks Factory", Addresses.FireworksFactoryPortal, Addresses.FireworksFactoryName, 20);
                HandleKeyUnlock("Charmed Ridge", Addresses.CharmedRidgePortal, Addresses.CharmedRidgeName, 16);
                HandleKeyUnlock("Honey Speedway", Addresses.HoneySpeedwayPortal, Addresses.HoneySpeedwayName, 16);

                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.FireworksEggReq), (byte)1);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.CharmedEggReq), (byte)1);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.HoneyEggReq), (byte)1);
            }
            else if (currentLevel == LevelInGameIDs.MidnightMountain)
            {
                HandleKeyUnlock("Crystal Islands", Addresses.CrystalIslandsPortal, Addresses.CrystalIslandsName, 16);
                HandleKeyUnlock("Desert Ruins", Addresses.DesertRuinsPortal, Addresses.DesertRuinsName, 16);
                HandleKeyUnlock("Haunted Tomb", Addresses.HauntedTombPortal, Addresses.HauntedTombName, 16);
                HandleKeyUnlock("Dino Mines", Addresses.DinoMinesPortal, Addresses.DinoMinesName, 12);
                HandleKeyUnlock("Harbor Speedway", Addresses.HarborSpeedwayPortal, Addresses.HarborSpeedwayName, 16);

                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.HauntedEggReq), (byte)1);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.DinoEggReq), (byte)1);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.HarborEggReq), (byte)1);
            }
        }
        else
        {
            if (_checkNames)
            {
                HandleEggGemName("Sunny Villa", Addresses.SunnyVillaPortal, Addresses.SunnyVillaName, 12, eggs, gems, multiplier);
                HandleEggGemName("Cloud Spires", Addresses.CloudSpiresPortal, Addresses.CloudSpiresName, 16, eggs, gems, multiplier);
                HandleEggGemName("Molten Crater", Addresses.MoltenCraterPortal, Addresses.MoltenCraterName, 16, eggs, gems, multiplier);
                HandleEggGemName("Seashell Shore", Addresses.SeashellShorePortal, Addresses.SeashellShoreName, 16, eggs, gems, multiplier);
                HandleEggGemName("Mushroom Speedway", Addresses.MushroomSpeedwayPortal, Addresses.MushroomSpeedwayName, 20, eggs, gems, multiplier);
                HandleEggGemName("Enchanted Towers", Addresses.EnchantedTowersPortal, Addresses.EnchantedTowersName, 20, eggs, gems, multiplier);
                HandleEggGemName("Icy Peak", Addresses.IcyPeakPortal, Addresses.IcyPeakName, 12, eggs, gems, multiplier);
                HandleEggGemName("Spooky Swamp", Addresses.SpookySwampPortal, Addresses.SpookySwampName, 16, eggs, gems, multiplier);
                HandleEggGemName("Bamboo Terrace", Addresses.BambooTerracePortal, Addresses.BambooTerraceName, 16, eggs, gems, multiplier);
                HandleEggGemName("Country Speedway", Addresses.CountrySpeedwayPortal, Addresses.CountrySpeedwayName, 20, eggs, gems, multiplier);
                HandleEggGemName("Frozen Altars", Addresses.FrozenAltarsPortal, Addresses.FrozenAltarsName, 16, eggs, gems, multiplier);
                HandleEggGemName("Lost Fleet", Addresses.LostFleetPortal, Addresses.LostFleetName, 12, eggs, gems, multiplier);
                HandleEggGemName("Fireworks Factory", Addresses.FireworksFactoryPortal, Addresses.FireworksFactoryName, 20, eggs, gems, multiplier);
                HandleEggGemName("Charmed Ridge", Addresses.CharmedRidgePortal, Addresses.CharmedRidgeName, 16, eggs, gems, multiplier);
                HandleEggGemName("Honey Speedway", Addresses.HoneySpeedwayPortal, Addresses.HoneySpeedwayName, 16, eggs, gems, multiplier);
                HandleEggGemName("Crystal Islands", Addresses.CrystalIslandsPortal, Addresses.CrystalIslandsName, 16, eggs, gems, multiplier);
                HandleEggGemName("Desert Ruins", Addresses.DesertRuinsPortal, Addresses.DesertRuinsName, 16, eggs, gems, multiplier);
                HandleEggGemName("Haunted Tomb", Addresses.HauntedTombPortal, Addresses.HauntedTombName, 16, eggs, gems, multiplier);
                HandleEggGemName("Dino Mines", Addresses.DinoMinesPortal, Addresses.DinoMinesName, 12, eggs, gems, multiplier);
                HandleEggGemName("Harbor Speedway", Addresses.HarborSpeedwayPortal, Addresses.HarborSpeedwayName, 16, eggs, gems, multiplier);

                _checkNames = false;
            }
            if (currentLevel == LevelInGameIDs.SunriseSpring)
            {
                HandleEggGemUnlock("Sunny Villa", Addresses.SunnyVillaPortal, Addresses.SunnyVillaName, 12, eggs, gems, multiplier);
                HandleEggGemUnlock("Cloud Spires", Addresses.CloudSpiresPortal, Addresses.CloudSpiresName, 16, eggs, gems, multiplier);
                HandleEggGemUnlock("Molten Crater", Addresses.MoltenCraterPortal, Addresses.MoltenCraterName, 16, eggs, gems, multiplier);
                HandleEggGemUnlock("Seashell Shore", Addresses.SeashellShorePortal, Addresses.SeashellShoreName, 16, eggs, gems, multiplier);
                HandleEggGemUnlock("Mushroom Speedway", Addresses.MushroomSpeedwayPortal, Addresses.MushroomSpeedwayName, 20, eggs, gems, multiplier);

                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.MoltenEggReq), (byte)1);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SeashellEggReq), (byte)1);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.MushroomEggReq), (byte)1);
            }
            else if (currentLevel == LevelInGameIDs.MiddayGardens)
            {
                HandleEggGemUnlock("Enchanted Towers", Addresses.EnchantedTowersPortal, Addresses.EnchantedTowersName, 20, eggs, gems, multiplier);
                HandleEggGemUnlock("Icy Peak", Addresses.IcyPeakPortal, Addresses.IcyPeakName, 12, eggs, gems, multiplier);
                HandleEggGemUnlock("Spooky Swamp", Addresses.SpookySwampPortal, Addresses.SpookySwampName, 16, eggs, gems, multiplier);
                HandleEggGemUnlock("Bamboo Terrace", Addresses.BambooTerracePortal, Addresses.BambooTerraceName, 16, eggs, gems, multiplier);
                HandleEggGemUnlock("Country Speedway", Addresses.CountrySpeedwayPortal, Addresses.CountrySpeedwayName, 20, eggs, gems, multiplier);

                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SpookyEggReq), (byte)1);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.BambooEggReq), (byte)1);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.CountryEggReq), (byte)1);
            }
            else if (currentLevel == LevelInGameIDs.EveningLake)
            {
                HandleEggGemUnlock("Frozen Altars", Addresses.FrozenAltarsPortal, Addresses.FrozenAltarsName, 16, eggs, gems, multiplier);
                HandleEggGemUnlock("Lost Fleet", Addresses.LostFleetPortal, Addresses.LostFleetName, 12, eggs, gems, multiplier);
                HandleEggGemUnlock("Fireworks Factory", Addresses.FireworksFactoryPortal, Addresses.FireworksFactoryName, 20, eggs, gems, multiplier);
                HandleEggGemUnlock("Charmed Ridge", Addresses.CharmedRidgePortal, Addresses.CharmedRidgeName, 16, eggs, gems, multiplier);
                HandleEggGemUnlock("Honey Speedway", Addresses.HoneySpeedwayPortal, Addresses.HoneySpeedwayName, 16, eggs, gems, multiplier);

                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.FireworksEggReq), (byte)1);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.CharmedEggReq), (byte)1);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.HoneyEggReq), (byte)1);
            }
            else if (currentLevel == LevelInGameIDs.MidnightMountain)
            {
                HandleEggGemUnlock("Crystal Islands", Addresses.CrystalIslandsPortal, Addresses.CrystalIslandsName, 16, eggs, gems, multiplier);
                HandleEggGemUnlock("Desert Ruins", Addresses.DesertRuinsPortal, Addresses.DesertRuinsName, 16, eggs, gems, multiplier);
                HandleEggGemUnlock("Haunted Tomb", Addresses.HauntedTombPortal, Addresses.HauntedTombName, 16, eggs, gems, multiplier);
                HandleEggGemUnlock("Dino Mines", Addresses.DinoMinesPortal, Addresses.DinoMinesName, 12, eggs, gems, multiplier);
                HandleEggGemUnlock("Harbor Speedway", Addresses.HarborSpeedwayPortal, Addresses.HarborSpeedwayName, 16, eggs, gems, multiplier);

                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.HauntedEggReq), (byte)1);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.DinoEggReq), (byte)1);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.HarborEggReq), (byte)1);
            }
        }
    }

    private static async void HandleCosmeticQueue(object source, ElapsedEventArgs e)
    {
        // Avoid overwhelming the game when many cosmetic effects are received at once by processing only 1
        // every 5 seconds.  This also lets the user see effects when logging in asynchronously.
        if (Client.ItemState == null || Client.CurrentSession == null) return;
        if (Memory.ReadShort(Addresses.GetVersionAddress(Addresses.GameStatus)) != (short)GameStatus.InGame) return;
        if (_cosmeticEffects.Count > 0 && _cosmeticEffects.TryDequeue(out string? effect))
        {
            switch (effect)
            {
                case "Normal Spyro":
                    TurnSpyroColor(SpyroColor.SpyroColorDefault);
                    DeactivateBigHeadMode();
                    break;
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

    private static async void StartSpyroGame(object source, ElapsedEventArgs e)
    {
        bool isInGame = Helpers.IsInGame();
        bool nullGameState = Client.ItemState == null;
        bool nullCurrentSession = Client.CurrentSession == null;
        if (!isInGame || nullGameState || nullCurrentSession)
        {
            if (nullGameState || nullCurrentSession)
            {
                Log.Logger.Warning("This client is not correctly connected to Archipelago.\r\n" +
                    "If this warning persists, please restart the client and try again to connect.");
            }
            else
            {
                Log.Logger.Information("Player is not yet in control of Spyro.");
            }
            return;
        }
        Log.Logger.Information("Player is in control of Spyro. Starting game!");
        _loadGameTimer.Enabled = false;
        List<int> gemsanityIDs = new List<int>();
        if (_slotData.TryGetValue("gemsanity_ids", out var gemIDs))
        {
            if (gemIDs != null)
            {
                gemsanityIDs = System.Text.Json.JsonSerializer.Deserialize<List<int>>(gemIDs.ToString());
            }
        }
        GameLocations = Helpers.BuildLocationList(includeGemsanity: _gemsanityOption != GemsanityOptions.Off, gemsanityIDs: gemsanityIDs);
        GameLocations = GameLocations.Where(x => x != null && !Client.CurrentSession.Locations.AllLocationsChecked.Contains(x.Id)).ToList();
        Client.MonitorLocations(GameLocations);
        _checkNames = true;
        bool deathLink = int.Parse(Client.Options?.GetValueOrDefault("death_link", "0").ToString()) > 0;
        if (deathLink)
        {
            _deathLinkService = Client.EnableDeathLink();
            _deathLinkService.OnDeathLinkReceived += new DeathLinkService.DeathLinkReceivedHandler(HandleDeathLink);
        }

        if (_moneybagsOption != MoneybagsOptions.Vanilla)
        {
            if ((Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Moneybags Unlock - Sheila").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.SheilaUnlock), 20001);
            }
            else
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.SheilaUnlock), 65536);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SheilaCutscene), 1);
            }
            if ((Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Moneybags Unlock - Sgt. Byrd").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.SgtByrdUnlock), 20001);
            }
            else
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.SgtByrdUnlock), 65536);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.ByrdCutscene), 1);
            }
            if ((Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Moneybags Unlock - Bentley").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.BentleyUnlock), 20001);
            }
            else
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.BentleyUnlock), 65536);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.BentleyCutscene), 1);
            }
            if ((Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Moneybags Unlock - Agent 9").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.Agent9Unlock), 20001);
            }
            else
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.Agent9Unlock), 65536);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.Agent9Cutscene), 1);
            }
        }
        if (_moneybagsOption == MoneybagsOptions.Moneybagssanity)
        {
            if ((Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Moneybags Unlock - Cloud Spires Bellows").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.CloudBellowsUnlock), 20001);
            }
            else
            {
                // Special case this to just reduce the price to 0, since otherwise a rhynoc despawns.
                Memory.Write(Addresses.GetVersionAddress(Addresses.CloudBellowsUnlock), 0);
            }
            if ((Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Moneybags Unlock - Spooky Swamp Door").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.SpookyDoorUnlock), 20001);
            }
            else
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.SpookyDoorUnlock), 65536);
            }
            if ((Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Moneybags Unlock - Icy Peak Nancy Door").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.IcyNancyUnlock), 20001);
            }
            else
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.IcyNancyUnlock), 65536);
            }
            if ((Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Moneybags Unlock - Molten Crater Thieves Door").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.MoltenThievesUnlock), 20001);
            }
            else
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.MoltenThievesUnlock), 65536);
            }
            if ((Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Moneybags Unlock - Charmed Ridge Stairs").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.CharmedStairsUnlock), 20001);
            }
            else
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.CharmedStairsUnlock), 65536);
            }
            if ((Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Moneybags Unlock - Desert Ruins Door").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.DesertDoorUnlock), 20001);
            }
            else
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.DesertDoorUnlock), 65536);
            }
            if ((Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Moneybags Unlock - Frozen Altars Cat Hockey Door").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.FrozenHockeyUnlock), 20001);
            }
            else
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.FrozenHockeyUnlock), 65536);
            }
            if ((Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Moneybags Unlock - Crystal Islands Bridge").Count() ?? 0) == 0)
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.CrystalBridgeUnlock), 20001);
            }
            else
            {
                Memory.Write(Addresses.GetVersionAddress(Addresses.CrystalBridgeUnlock), 65536);
            }
        }
        if (_moneybagsOption == MoneybagsOptions.Vanilla && (_gemsanityOption != GemsanityOptions.Off || _openWorld != 0))
        {
            Memory.Write(Addresses.GetVersionAddress(Addresses.SheilaUnlock), (short)0);
            Memory.Write(Addresses.GetVersionAddress(Addresses.SgtByrdUnlock), (short)0);
            Memory.Write(Addresses.GetVersionAddress(Addresses.BentleyUnlock), (short)0);
            Memory.Write(Addresses.GetVersionAddress(Addresses.Agent9Unlock), (short)0);
        }
        if (_moneybagsOption == MoneybagsOptions.Companionsanity && _gemsanityOption != GemsanityOptions.Off)
        {
            Memory.Write(Addresses.GetVersionAddress(Addresses.CloudBellowsUnlock), (short)0);
            Memory.Write(Addresses.GetVersionAddress(Addresses.SpookyDoorUnlock), (short)0);
            Memory.Write(Addresses.GetVersionAddress(Addresses.IcyNancyUnlock), (short)0);
            Memory.Write(Addresses.GetVersionAddress(Addresses.MoltenThievesUnlock), (short)0);
            Memory.Write(Addresses.GetVersionAddress(Addresses.CharmedStairsUnlock), (short)0);
            Memory.Write(Addresses.GetVersionAddress(Addresses.DesertDoorUnlock), (short)0);
            Memory.Write(Addresses.GetVersionAddress(Addresses.FrozenHockeyUnlock), (short)0);
            Memory.Write(Addresses.GetVersionAddress(Addresses.CrystalBridgeUnlock), (short)0);
        }
        LevelInGameIDs currentLevel = (LevelInGameIDs)Memory.ReadByte(Addresses.GetVersionAddress(Addresses.CurrentLevelAddress));
        if (_openWorld != 0)
        {
            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SunriseLevelsComplete), 1);
            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.MiddayLevelsComplete), 1);
            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.EveningLevelsComplete), 1);
            // Mark level as entered in atlas, which changes transport behavior.
            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.EveningAtlasUnlock), 1);
            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SunriseAtlasUnlock), 1);
            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.BuzzAtlasUnlock), 1);
            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.MiddayAtlasUnlock), 1);
            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.MidnightAtlasUnlock), 1);
            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.ScorchAtlasUnlock), 1);
            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SpikeAtlasUnlock), 1);
            // Triggered when getting end of level eggs in sunrise.
            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SunnyCompletedFlags), 2);
            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.CloudCompletedFlags), 2);
            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.MoltenCompletedFlags), 2);
            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SeashellCompletedFlags), 2);
            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SheilaCompletedFlags), 2);


            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.BuzzDefeated), 1);
            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SpikeDefeated), 1);
            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.ScorchDefeated), 1);
            Memory.WriteByte(Addresses.GetVersionAddress(Addresses.EveningBianca), 1);
            if (_levelLockOptions != LevelLockOptions.Vanilla)
            {
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.MoltenUnlocked), 1);
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SeashellUnlocked), 1);
            }
            uint eggAddress = Addresses.GetVersionAddress(Addresses.EggStartAddress);
            // Mark as collected the end of level eggs for the 15 "progression" levels and first 3 bosses.
            List<uint> collectedEggLevels = new List<uint> { 1, 2, 3, 4, 6, 7, 10, 11, 12, 13, 15, 16, 19, 20, 21, 22, 24, 25 };
            for (uint i = 0; i < 26; i++)
            {
                if (collectedEggLevels.Contains(i))
                {
                    Memory.WriteBit(eggAddress + i, 0, true);
                }
            }
            // Ensure the rocket appears in Sunrise.
            if (currentLevel == LevelInGameIDs.SunriseSpring)
            {
                var currentLives = Memory.ReadShort(Addresses.GetVersionAddress(Addresses.PlayerLives));
                Memory.Write(Addresses.GetVersionAddress(Addresses.PlayerLives), (short)(Math.Min(99, currentLives + 1)));
                Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SpyroState), (byte)SpyroState.Dying);
            }   
        }

        Helpers.UpdateLocationList(currentLevel, Client);
        _handleGemsanity = true;
    }

    private static void CheckGoalCondition()
    {
        if (_hasSubmittedGoal || Client.ItemState == null || Client.CurrentSession == null)
        {
            return;
        }
        var currentEggs = CalculateCurrentEggs();
        var currentSkillPoints = CalculateCurrentSkillPoints();
        if (_goal == CompletionGoal.Sorceress1)
        {
            if (currentEggs >= _eggCountGoal && (Client.CurrentSession?.Items.AllItemsReceived.Any(x => x != null && x.ItemName == "Sorceress Defeated") ?? false))
            {
                _hasSubmittedGoal = true;
                Task.Run(async () =>
                {
                    Client.SendGoalCompletion();
                });
            }
        }
        else if (_goal == CompletionGoal.EggForSale)
        {
            if ((Client.CurrentSession?.Items.AllItemsReceived.Any(x => x != null && x.ItemName == "Moneybags Chase Complete") ?? false))
            {
                _hasSubmittedGoal = true;
                Task.Run(async () =>
                {
                    Client.SendGoalCompletion();
                });
            }
        }
        else if (_goal == CompletionGoal.Sorceress2)
        {
            if (currentEggs >= _eggCountGoal && (Client.CurrentSession?.Items.AllItemsReceived.Any(x => x != null && x.ItemName == "Super Bonus Round Complete") ?? false))
            {
                _hasSubmittedGoal = true;
                Task.Run(async () =>
                {
                    Client.SendGoalCompletion();
                });
            }
        }
        else if (_goal == CompletionGoal.AllSkillPoints)
        {
            if (currentSkillPoints >= 20)
            {
                _hasSubmittedGoal = true;
                Task.Run(async () =>
                {
                    Client.SendGoalCompletion();
                });
            }
        }
        else if (_goal == CompletionGoal.Epilogue)
        {
            if (currentSkillPoints >= 20 && (Client.CurrentSession?.Items.AllItemsReceived.Any(x => x != null && x.ItemName == "Sorceress Defeated") ?? false))
            {
                _hasSubmittedGoal = true;
                Task.Run(async () =>
                {
                    Client.SendGoalCompletion();
                });
            }
        }
        else if (_goal == CompletionGoal.Spike)
        {
            if (currentEggs >= _eggCountGoal && (Client.CurrentSession?.Items.AllItemsReceived.Any(x => x != null && x.ItemName == "Spike Defeated") ?? false))
            {
                _hasSubmittedGoal = true;
                Task.Run(async () =>
                {
                    Client.SendGoalCompletion();
                });
            }
        }
        else if (_goal == CompletionGoal.Scorch)
        {
            if (currentEggs >= _eggCountGoal && (Client.CurrentSession?.Items.AllItemsReceived.Any(x => x != null && x.ItemName == "Scorch Defeated") ?? false))
            {
                _hasSubmittedGoal = true;
                Task.Run(async () =>
                {
                    Client.SendGoalCompletion();
                });
            }
        }
        else if (_goal == CompletionGoal.EggHunt)
        {
            if (currentEggs >= _eggCountGoal)
            {
                _hasSubmittedGoal = true;
                Task.Run(async () =>
                {
                    Client.SendGoalCompletion();
                });
            }
        }
    }

    private static async void DeactivateBigHeadMode()
    {
        Memory.Write(Addresses.GetVersionAddress(Addresses.BigHeadMode), (short)(0));
        Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SpyroHeight), (byte)(0));
        Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SpyroLength), (byte)(0));
        Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SpyroWidth), (byte)(0));
    }

    private static async void ActivateBigHeadMode()
    {
        Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SpyroHeight), (byte)(32));
        Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SpyroLength), (byte)(32));
        Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SpyroWidth), (byte)(32));
        Memory.Write(Addresses.GetVersionAddress(Addresses.BigHeadMode), (short)(1));
    }

    private static async void ActivateFlatSpyroMode()
    {
        Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SpyroHeight), (byte)(16));
        Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SpyroLength), (byte)(16));
        Memory.WriteByte(Addresses.GetVersionAddress(Addresses.SpyroWidth), (byte)(2));
        Memory.Write(Addresses.GetVersionAddress(Addresses.BigHeadMode), (short)(256));
    }

    private static async void TurnSpyroColor(SpyroColor colorEnum)
    {
        Memory.Write(Addresses.GetVersionAddress(Addresses.SpyroColorAddress), (short)colorEnum);
    }

    private static async void ActivateInvincibility(int seconds)
    {
        // Speedway time attacks won't end during invincibility. Prevent the effect in these levels.
        // TODO: Review the feedback on this change - invincibility also stops the timer, making the time attack easier.
        List<LevelInGameIDs> speedways = new List<LevelInGameIDs>() {
            LevelInGameIDs.MushroomSpeedway,
            LevelInGameIDs.CountrySpeedway,
            LevelInGameIDs.HoneySpeedway,
            LevelInGameIDs.HarborSpeedway
        };
        LevelInGameIDs currentLevel = (LevelInGameIDs)Memory.ReadByte(Addresses.GetVersionAddress(Addresses.CurrentLevelAddress));
        if (speedways.Contains(currentLevel)) {
            return;
        }
        // The counter ticks down every frame (60 fps for in game calcs)
        seconds = seconds * 60;
        // Collecting an egg removes invincibility, so try to avoid a user's own egg
        // from providing no benefits.

        await Task.Run(async () =>
        {
            await Task.Delay(6000);
            Memory.Write(Addresses.GetVersionAddress(Addresses.InvincibilityDurationAddress), (short)seconds);
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

    private static void UpdateItemLog()
    {
        Dictionary<string, int> itemCount = new Dictionary<string, int>();
        List<LogListItem> messagesToLog = new List<LogListItem>();
        if (Client != null && Client.CurrentSession != null && Client.CurrentSession.Items != null)
        {
            foreach (ItemInfo item in Client.CurrentSession.Items.AllItemsReceived)
            {
                string itemName = item.ItemName;
                if (itemCount.ContainsKey(itemName))
                {
                    itemCount[itemName] = itemCount[itemName] + 1;
                }
                else
                {
                    itemCount[itemName] = 1;
                }
            }
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                List<string> sortedItems = itemCount.Keys.ToList();
                sortedItems.Sort();
                foreach (string item in sortedItems)
                {
                    messagesToLog.Add(new LogListItem(
                        new List<TextSpan>() {
                            new TextSpan() { Text = $"{item}: ", TextColor = new SolidColorBrush(Avalonia.Media.Color.FromRgb(200, 255, 200)) },
                            new TextSpan() { Text = $"{itemCount[item]}", TextColor = new SolidColorBrush(Avalonia.Media.Color.FromRgb(200, 255, 200)) }
                        }
                    ));
                }
            });
        }
        lock (_lockObject)
        {
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                Context.ItemList.Clear();
                foreach (LogListItem messageToLog in messagesToLog)
                {
                    Context.ItemList.Add(messageToLog);
                }
            });
        }
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
                spans.Add(new TextSpan() { Text = part.Text, TextColor = new SolidColorBrush(Avalonia.Media.Color.FromRgb(part.Color.R, part.Color.G, part.Color.B)) });
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
        if (Client.ItemState == null || Client.CurrentSession == null) return;
        if (!Helpers.IsInGame())
        {
            Log.Logger.Error("Sending location while not in game.\r\n" +
                "If this is not during item release, please report this error in the Discord.");
        }
        foreach (long location in newCheckedLocations)
        {
            foreach (LevelInGameIDs levelID in Helpers.remainingGemsanityChecks.Keys)
            {
                if (Helpers.remainingGemsanityChecks[levelID].Keys.Contains((int)location))
                {
                    Helpers.remainingGemsanityChecks[levelID].Remove((int)location);
                }
            }
        }
        foreach (long location in newCheckedLocations)
        {
            foreach (LevelInGameIDs levelID in Helpers.remainingGemsanityChecks.Keys)
            {
                if (Helpers.remainingGemsanityChecks[levelID].Keys.Contains((int)location))
                {
                    Helpers.remainingGemsanityChecks[levelID].Remove((int)location);
                }
            }
        }
        var currentEggs = CalculateCurrentEggs();
        CheckGoalCondition();

    }

    private static int CalculateCurrentEggs()
    {
        var count = Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Egg").Count() ?? 0;
        count = Math.Min(count, 150);
        Memory.WriteByte(Addresses.GetVersionAddress(Addresses.TotalEggAddress), (byte)(count));
        return count;
    }

    private static int CalculateCurrentSkillPoints()
    {
        return Client.CurrentSession?.Items.AllItemsReceived.Where(x => x.ItemName == "Skill Point").Count() ?? 0;
    }

    private static int CalculateCurrentGems()
    {
        if (_gemsanityOption == GemsanityOptions.Off)
        {
            return Memory.ReadShort(Addresses.GetVersionAddress(Addresses.TotalGemAddress));
        }
        uint levelGemCountAddress = Addresses.GetVersionAddress(Addresses.SunriseSpringGems);
        int totalGems = 0;
        int i = 0;
        foreach (LevelData level in Helpers.GetLevelData())
        {
            if (level.Name != "Super Bonus Round")
            {
                string levelName = level.Name;
                int levelGemCount = 50 * (Client.CurrentSession?.Items.AllItemsReceived?.Where(x => x != null && x.ItemName == $"{levelName} 50 Gems").Count() ?? 0) +
                    100 * (Client.CurrentSession?.Items.AllItemsReceived?.Where(x => x != null && x.ItemName == $"{levelName} 100 Gems").Count() ?? 0);
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
        Memory.Write(Addresses.GetVersionAddress(Addresses.TotalGemAddress), totalGems);
        return totalGems;
    }

    private static void OnConnected(object sender, EventArgs args)
    {
        // Repopulate hint list.  There is likely a better way to do this using the Get network protocol
        // with keys=[$"hints_{team}_{slot}"].
        // Client.CurrentSession.ConnectionInfo has some of this information.
        Client?.SendMessage("!hint");
        UpdateItemLog();
        Dictionary<String, String> lastConnectionDetails = new Dictionary<string, string>();
        lastConnectionDetails["slot"] = Context.Slot;
        lastConnectionDetails["host"] = Context.Host;
        try
        {
            SaveLastConnectionDetails(lastConnectionDetails);
        }
        catch (Exception ex)
        {
            Log.Logger.Verbose($"Failed to write connection details\r\n{ex.ToString()}");
        }
    }

    private static void OnDisconnected(object sender=null, EventArgs args=null)
    {
        // Avoid ongoing timers and settings affecting a new game.
        _gemsanityOption = GemsanityOptions.Off;
        _moneybagsOption = MoneybagsOptions.Vanilla;
        _powerupLockOption = PowerupLockOptions.Vanilla;
        _openWorld = 0;
        _sparxPowerShuffle = 0;
        _slot = 0;
        _goal = CompletionGoal.NotLoaded;
        _eggCountGoal = 150;
        _sorceressDoorEggReq = 150;
        _sbrDoorEggReq = 150;
        _sbrDoorGemReq = 20000;
        _slotData = null;
        _hintsList = null;
        _hasSubmittedGoal = false;
        _useQuietHints = true;
        _unlockedLevels = 0;
        _easyChallenges = new List<string>();
        _handleGemsanity = false;
        _previousLevel = (LevelInGameIDs)255;
        _worldKeys = 0;
        _progressiveBasketBreaks = 0;
        _levelLockOptions = LevelLockOptions.Vanilla;
        _eggRequirements = new Dictionary<string, int>();
        _gemRequirements = new Dictionary<string, int>();

        if (_deathLinkService != null)
        {
            _deathLinkService = null;
        }
        if (_loadGameTimer != null)
        {
            _loadGameTimer.Enabled = false;
            _loadGameTimer = null;
        }
        _cosmeticEffects = new ConcurrentQueue<string>();
        if (_cosmeticsTimer != null)
        {
            _cosmeticsTimer.Enabled = false;
            _cosmeticsTimer = null;
        }
        if (_gameLoopTimer != null)
        {
            _gameLoopTimer.Enabled = false;
            _gameLoopTimer = null;
        }
    }
}
