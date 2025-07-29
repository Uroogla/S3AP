using Archipelago.Core;
using Archipelago.Core.GameClients;
using Archipelago.Core.MauiGUI;
using Archipelago.Core.MauiGUI.Models;
using Archipelago.Core.MauiGUI.ViewModels;
using Archipelago.Core.Models;
using Archipelago.Core.Traps;
using Archipelago.Core.Util;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json;
using Serilog;
using System.Runtime.InteropServices;
using System.Timers;
using static S3AP.Models.Enums;
using Color = Microsoft.Maui.Graphics.Color;

namespace S3AP
{
    public partial class App : Application
    {
        public static MainPageViewModel Context;
        public static ArchipelagoClient Client { get; set; }
        public static List<Archipelago.Core.Models.Location> GameLocations { get; set; }
        private static readonly object _lockObject = new object();
        private static Queue<string> CosmeticEffects { get; set; }
        private static Dictionary<string, string> HintsList { get; set; }
        private static byte sparxUpgrades { get; set; }
        private static bool hasSubmittedGoal { get; set; }
        private static System.Timers.Timer sparxTimer { get; set; }
        public App()
        {
            InitializeComponent();
            Context = new MainPageViewModel("0.6.1");
            Context.ConnectClicked += Context_ConnectClicked;
            Context.CommandReceived += (e, a) =>
            {
                if (string.IsNullOrWhiteSpace(a.Command)) return;
                Client?.SendMessage(a.Command);
                HandleCommand(a.Command);
            };
            MainPage = new MainPage(Context);
            Context.ConnectButtonEnabled = true;
            HintsList = null;
            sparxUpgrades = 0;
            hasSubmittedGoal = false;
            Log.Logger.Information("This Archipelago Client is compatible only with the NTSC-U 1.1 release of Spyro 1 (North America Greatest Hits version).");
            Log.Logger.Information("Trying to play with a different version will not work and may release all of your locations at the start.");
        }
        private void HandleCommand(string command)
        {
            if (command == "clearSpyroGameState")
            {
                Log.Logger.Information("Clearing the game state.  Please reconnect to the server while in game to refresh received items.");
                Client.ForceReloadAllItems();
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
            DuckstationClient client = new DuckstationClient();
            var DuckstationConnected = client.Connect();
            if (!DuckstationConnected)
            {
                Log.Logger.Warning("Duckstation not running, open Duckstation and launch the game before connecting!");
                return;
            }
            Client = new ArchipelagoClient(client);

            Memory.GlobalOffset = Memory.GetDuckstationOffset();

            Client.Connected += OnConnected;
            Client.Disconnected += OnDisconnected;

            await Client.Connect(e.Host, "Spyro 3");
            GameLocations = Helpers.BuildLocationList();
            CosmeticEffects = new Queue<string>();
            Client.LocationCompleted += Client_LocationCompleted;
            Client.CurrentSession.Locations.CheckedLocationsUpdated += Locations_CheckedLocationsUpdated;
            Client.MessageReceived += Client_MessageReceived;
            Client.ItemReceived += ItemReceived;
            Client.EnableLocationsCondition = () => Helpers.IsInGame();
            await Client.Login(e.Slot, !string.IsNullOrWhiteSpace(e.Password) ? e.Password : null);
            Client.MonitorLocations(GameLocations);
            Log.Logger.Information("Warnings and errors above are okay if this is your first time connecting to this multiworld server.");

        }

        private void Client_LocationCompleted(object? sender, LocationCompletedEventArgs e)
        {
            if (Client.GameState == null) return;
            var currentEggs = CalculateCurrentEggs();
            CheckGoalCondition();
        }

        private async void ItemReceived(object? o, ItemReceivedEventArgs args)
        {
            Log.Logger.Information($"Item Received: {JsonConvert.SerializeObject(args.Item)}");
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
                    RunLagTrap();
                    break;
                case "(Over)heal Sparx":
                    // Collecting a skill point provides a full heal, so wait for that to complete first.
                    await Task.Run(async () =>
                    {
                        await Task.Delay(3000);
                        currentHealth = Memory.ReadByte(Addresses.PlayerHealth);
                        // Going too high creates too many particles for the game to handle.
                        Memory.Write(Addresses.PlayerHealth, (byte)(Math.Min(4, currentHealth + 1)));
                    });
                    break;
                case "Damage Sparx Trap":
                    // Collecting a skill point provides a full heal, so wait for that to complete first.
                    await Task.Run(async () =>
                    {
                        await Task.Delay(3000);
                        currentHealth = Memory.ReadByte(Addresses.PlayerHealth);
                        Memory.Write(Addresses.PlayerHealth, Byte.Max((byte)(currentHealth - 1), 0));
                    });
                    break;
                case "Sparxless Trap":
                    // Collecting a skill point provides a full heal, so wait for that to complete first.
                    await Task.Run(async () =>
                    {
                        await Task.Delay(3000);
                        Memory.Write(Addresses.PlayerHealth, (byte)(0));
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
                    CosmeticEffects.Enqueue(args.Item.Name);
                    break;
                case "Invincibility (15 seconds)":
                    ActivateInvincibility(15);
                    break;
                case "Invincibility (30 seconds)":
                    ActivateInvincibility(30);
                    break;
                case "Moneybags Unlock - Cloud Spires Bellows":
                    UnlockMoneybags(Addresses.CloudBellowsUnlock);
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
                    sparxUpgrades++;
                    break;
            }
        }
        private static async void GetZoeHint(string hintName)
        {
            if (HintsList == null)
            {
                int slot = Client.CurrentSession.ConnectionInfo.Slot;
                Dictionary<string, object> obj = await Client.CurrentSession.DataStorage.GetSlotDataAsync(slot);
                if (obj.TryGetValue("hints", out var value))
                {
                    if (value != null)
                    {
                        HintsList = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(value.ToString());
                    }
                }
            }
            if (HintsList != null)
            {
                Log.Logger.Information(HintsList.GetValueOrDefault($"{hintName} Text", "Zoe says that there's been an error."));
                int hintLocationID = int.Parse(HintsList.GetValueOrDefault($"{hintName} ID", "-1"));
                if (hintLocationID != -1)
                {
                    LocationScoutsPacket locationScoutsPacket = new LocationScoutsPacket();
                    locationScoutsPacket.Locations = [hintLocationID];
                    locationScoutsPacket.CreateAsHint = 2;
                    Client.CurrentSession.Socket.SendPacketAsync(locationScoutsPacket);
                }
            }
        }
        private static async void HandleMaxSparxHealth(object source, ElapsedEventArgs e)
        {
            byte currentHealth = Memory.ReadByte(Addresses.PlayerHealth);
            if (currentHealth > sparxUpgrades)
            {
                Memory.WriteByte(Addresses.PlayerHealth, sparxUpgrades);
            }
            if (sparxUpgrades == 3)
            {
                sparxTimer.Enabled = false;
            }
        }
        private static async void HandleCosmeticQueue(object source, ElapsedEventArgs e)
        {
            // Avoid overwhelming the game when many cosmetic effects are received at once by processing only 1
            // every 5 seconds.  This also lets the user see effects when logging in asynchronously.
            if (CosmeticEffects.Count > 0 && Memory.ReadShort(Addresses.GameStatus) == (short)GameStatus.InGame)
            {
                string effect = CosmeticEffects.Dequeue();
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
        private static void CheckGoalCondition()
        {
            if (hasSubmittedGoal)
            {
                return;
            }
            var currentEggs = CalculateCurrentEggs();
            var currentSkillPoints = CalculateCurrentSkillPoints();
            int goal = int.Parse(Client.Options?.GetValueOrDefault("goal", 0).ToString());
            if ((CompletionGoal)goal == CompletionGoal.Sorceress1)
            {
                if (currentEggs >= 100 && Client.CurrentSession.Locations.AllLocationsChecked.Any(x => GameLocations.First(y => y.Id == x).Id == (int)ImportantLocationIDs.SorceressEgg))
                {
                    Client.SendGoalCompletion();
                    hasSubmittedGoal = true;
                }
            }
            else if ((CompletionGoal)goal == CompletionGoal.EggForSale)
            {
                if (Client.CurrentSession.Locations.AllLocationsChecked.Any(x => GameLocations.First(y => y.Id == x).Id == (int)ImportantLocationIDs.EggForSale))
                {
                    Client.SendGoalCompletion();
                    hasSubmittedGoal = true;
                }
            }
            else if ((CompletionGoal)goal == CompletionGoal.Sorceress2)
            {
                if (Client.CurrentSession.Locations.AllLocationsChecked.Any(x => GameLocations.First(y => y.Id == x).Id == (int)ImportantLocationIDs.SuperBonusRoundEgg))
                {
                    Client.SendGoalCompletion();
                    hasSubmittedGoal = true;
                }
            }
            else if ((CompletionGoal)goal == CompletionGoal.AllSkillPoints)
            {
                if (currentSkillPoints >= 20)
                {
                    Client.SendGoalCompletion();
                    hasSubmittedGoal = true;
                }
            }
            else if ((CompletionGoal)goal == CompletionGoal.Epilogue)
            {
                if (currentSkillPoints >= 20 && Client.CurrentSession.Locations.AllLocationsChecked.Any(x => GameLocations.First(y => y.Id == x).Id == (int)ImportantLocationIDs.SorceressEgg))
                {
                    Client.SendGoalCompletion();
                    hasSubmittedGoal = true;
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
            Memory.Write(Addresses.SpyroHeight, (byte)(32));
            Memory.Write(Addresses.SpyroLength, (byte)(32));
            Memory.Write(Addresses.SpyroWidth, (byte)(32));
            Memory.Write(Addresses.BigHeadMode, (short)(1));
        }
        private static async void ActivateFlatSpyroMode()
        {
            Memory.Write(Addresses.SpyroHeight, (byte)(16));
            Memory.Write(Addresses.SpyroLength, (byte)(16));
            Memory.Write(Addresses.SpyroWidth, (byte)(2));
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
            var messageToLog = new LogListItem(new List<TextSpan>()
            {
                new TextSpan(){Text = $"[{item.Id.ToString()}] -", TextColor = Color.FromRgb(255, 255, 255)},
                new TextSpan(){Text = $"{item.Name}", TextColor = Color.FromRgb(200, 255, 200)}
            });
            lock (_lockObject)
            {
                Microsoft.Maui.Controls.Application.Current.Dispatcher.DispatchAsync(() =>
                {
                    Context.ItemList.Add(messageToLog);
                });
            }
        }

        private void Client_MessageReceived(object? sender, Archipelago.Core.Models.MessageReceivedEventArgs e)
        {
            if (e.Message.Parts.Any(x => x.Text == "[Hint]: "))
            {
                LogHint(e.Message);
            }
            Log.Logger.Information(JsonConvert.SerializeObject(e.Message));
        }
        private static void LogHint(LogMessage message)
        {
            var newMessage = message.Parts.Select(x => x.Text);

            if (Context.HintList.Any(x => x.TextSpans.Select(y => y.Text) == newMessage))
            {
                return; //Hint already in list
            }
            List<TextSpan> spans = new List<TextSpan>();
            foreach (var part in message.Parts)
            {
                spans.Add(new TextSpan() { Text = part.Text, TextColor = Color.FromRgb(part.Color.R, part.Color.G, part.Color.B) });
            }
            lock (_lockObject)
            {
                Microsoft.Maui.Controls.Application.Current.Dispatcher.DispatchAsync(() =>
                {
                    Context.HintList.Add(new LogListItem(spans));
                });
            }
        }
        private static void Locations_CheckedLocationsUpdated(System.Collections.ObjectModel.ReadOnlyCollection<long> newCheckedLocations)
        {
            if (Client.GameState == null) return;
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
        private static void OnConnected(object sender, EventArgs args)
        {
            Log.Logger.Information("Connected to Archipelago");
            Log.Logger.Information($"Playing {Client.CurrentSession.ConnectionInfo.Game} as {Client.CurrentSession.Players.GetPlayerName(Client.CurrentSession.ConnectionInfo.Slot)}");
            // Note: The player must be in game for this to work.
            MoneybagsOptions moneybagsOption = (MoneybagsOptions)int.Parse(Client.Options?.GetValueOrDefault("moneybags_settings", "0").ToString());
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
                    Memory.WriteByte(Addresses.ByrdCutscene, 1);
                }
                else
                {
                    Memory.Write(Addresses.SgtByrdUnlock, 65536);
                }
                if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Moneybags Unlock - Bentley").Count() ?? 0) == 0)
                {
                    Memory.Write(Addresses.BentleyUnlock, 20001);
                    Memory.WriteByte(Addresses.BentleyCutscene, 1);
                }
                else
                {
                    Memory.Write(Addresses.BentleyUnlock, 65536);
                }
                if ((Client.GameState?.ReceivedItems.Where(x => x.Name == "Moneybags Unlock - Agent 9").Count() ?? 0) == 0)
                {
                    Memory.Write(Addresses.Agent9Unlock, 20001);
                    Memory.WriteByte(Addresses.Agent9Cutscene, 1);
                }
                else
                {
                    Memory.Write(Addresses.Agent9Unlock, 65536);
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
                    Memory.Write(Addresses.CloudBellowsUnlock, 65536);
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

            System.Timers.Timer cosmeticsTimer = new System.Timers.Timer();
            cosmeticsTimer.Elapsed += new ElapsedEventHandler(HandleCosmeticQueue);
            cosmeticsTimer.Interval = 5000;
            cosmeticsTimer.Enabled = true;

            ProgressiveSparxHealthOptions sparxOption = (ProgressiveSparxHealthOptions)int.Parse(Client.Options?.GetValueOrDefault("enable_progressive_sparx_health", "0").ToString());
            if (sparxOption != ProgressiveSparxHealthOptions.Off)
            {
                sparxUpgrades = (byte)(Client.GameState?.ReceivedItems.Where(x => x.Name == "Progressive Sparx Health Upgrade").Count() ?? 0);
                if (sparxOption == ProgressiveSparxHealthOptions.Blue)
                {
                    sparxUpgrades += 2;
                }
                else if (sparxOption == ProgressiveSparxHealthOptions.Green)
                {
                    sparxUpgrades += 1;
                }
                sparxTimer = new System.Timers.Timer();
                sparxTimer.Elapsed += new ElapsedEventHandler(HandleMaxSparxHealth);
                sparxTimer.Interval = 500;
                sparxTimer.Enabled = true;
            }
        }

        private static void OnDisconnected(object sender, EventArgs args)
        {
            Log.Logger.Information("Disconnected from Archipelago");
            // Avoid queued cosmetic effects impacting another game.
            CosmeticEffects = new Queue<string>();
        }
        protected override Microsoft.Maui.Controls.Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);
            if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            {
                window.Title = "S3AP - Spyro 3 Archipelago Randomizer";
            }
            window.Width = 600;

            return window;
        }
    }

}
