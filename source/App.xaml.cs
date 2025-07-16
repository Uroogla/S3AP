using Archipelago.Core;
using Archipelago.Core.GameClients;
using Archipelago.Core.MauiGUI;
using Archipelago.Core.MauiGUI.Models;
using Archipelago.Core.MauiGUI.ViewModels;
using Archipelago.Core.Models;
using Archipelago.Core.Traps;
using Archipelago.Core.Util;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Microsoft.Maui.Devices.Sensors;
using Newtonsoft.Json;
using Serilog;
using static S3AP.Models.Enums;
using Color = Microsoft.Maui.Graphics.Color;
using Location = Archipelago.Core.Models.Location;

namespace S3AP
{
    public partial class App : Application
    {
        public static MainPageViewModel Context;
        public static ArchipelagoClient Client { get; set; }
        public static List<Location> GameLocations { get; set; }
        private static readonly object _lockObject = new object();
        public App()
        {
            InitializeComponent();
            Context = new MainPageViewModel("0.6.1");
            Context.ConnectClicked += Context_ConnectClicked;
            Context.CommandReceived += (e, a) =>
            {
                Client?.SendMessage(a.Command);
                if (a.Command == "clearSpyroGameState")
                {
                    Log.Logger.Information("Clearing the game state.  Please reconnect to the server while in game to refresh received items.");
                    Client.ForceReloadAllItems();
                }
            };
            MainPage = new MainPage(Context);
            Context.ConnectButtonEnabled = true;
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
                Log.Logger.Warning("duckstation not running, open duckstation and launch the game before connecting!");
                return;
            }
            Client = new ArchipelagoClient(client);

            Memory.GlobalOffset = Memory.GetDuckstationOffset();

            Client.Connected += OnConnected;
            Client.Disconnected += OnDisconnected;

            await Client.Connect(e.Host, "Spyro 3");
            GameLocations = Helpers.BuildLocationList();
            Client.LocationCompleted += Client_LocationCompleted;
            Client.CurrentSession.Locations.CheckedLocationsUpdated += Locations_CheckedLocationsUpdated;
            Client.MessageReceived += Client_MessageReceived;
            Client.ItemReceived += ItemReceived;
            Client.EnableLocationsCondition = () => Helpers.IsInGame();
            await Client.Login(e.Slot, !string.IsNullOrWhiteSpace(e.Password) ? e.Password : null);
            Client.MonitorLocations(GameLocations);

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
                case "Extra Life":
                    var currentLives = Memory.ReadShort(Addresses.PlayerLives);
                    Memory.Write(Addresses.PlayerLives, (short)(Math.Min(99, currentLives + 1)));
                    break;
                case "Lag Trap":
                    RunLagTrap();
                    break;
                case "Big Head Mode":
                    ActivateBigHeadMode();
                    break;
                case "Flat Spyro Mode":
                    ActivateFlatSpyroMode();
                    break;
                case "(Over)heal Sparx":
                    // Collecting a skill point provides a full heal, so wait for that to complete first.
                    await Task.Run(async () =>
                    {
                        await Task.Delay(3000);
                        currentHealth = Memory.ReadByte(Addresses.PlayerHealth);
                        // Going too high creates too many particles for the game to handle.
                        Memory.Write(Addresses.PlayerHealth, (byte)(Math.Min(5, currentHealth + 1)));
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
                case "Invincibility (15 seconds)":
                    ActivateInvincibility(15);
                    break;
                case "Invincibility (30 seconds)":
                    ActivateInvincibility(30);
                    break;
            }
        }
        private static void CheckGoalCondition()
        {
            var currentEggs = CalculateCurrentEggs();
            int goal = int.Parse(Client.Options?.GetValueOrDefault("goal", 0).ToString());
            // TODO: Don't hard code IDs.
            if ((CompletionGoal)goal == CompletionGoal.Sorceress1)
            {
                if (currentEggs >= 100 && Client.CurrentSession.Locations.AllLocationsChecked.Any(x => GameLocations.First(y => y.Id == x).Id == 1264000))
                {
                    Client.SendGoalCompletion();
                }
            }
            else if ((CompletionGoal)goal == CompletionGoal.EggForSale)
            {
                if (Client.CurrentSession.Locations.AllLocationsChecked.Any(x => GameLocations.First(y => y.Id == x).Id == 1257005))
                {
                    Client.SendGoalCompletion();
                }
            }
            else if ((CompletionGoal)goal == CompletionGoal.Sorceress2)
            {
                if (Client.CurrentSession.Locations.AllLocationsChecked.Any(x => GameLocations.First(y => y.Id == x).Id == 1266000))
                {
                    Client.SendGoalCompletion();
                }
            }
            // Test goal for ease of debugging
            /*else if ((CompletionGoal)goal == CompletionGoal.SunnyVilla)
            {
                if (Client.CurrentSession.Locations.AllLocationsChecked.Any(x => GameLocations.First(y => y.Id == x).Id == 1231000))
                {
                    Client.SendGoalCompletion();
                }
            }*/
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
        private static void OnConnected(object sender, EventArgs args)
        {
            Log.Logger.Information("Connected to Archipelago");
            Log.Logger.Information($"Playing {Client.CurrentSession.ConnectionInfo.Game} as {Client.CurrentSession.Players.GetPlayerName(Client.CurrentSession.ConnectionInfo.Slot)}");
        }

        private static void OnDisconnected(object sender, EventArgs args)
        {
            Log.Logger.Information("Disconnected from Archipelago");
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
