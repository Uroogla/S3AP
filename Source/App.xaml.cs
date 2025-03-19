using Archipelago.Core;
using Archipelago.Core.GameClients;
using Archipelago.Core.MauiGUI;
using Archipelago.Core.MauiGUI.Models;
using Archipelago.Core.MauiGUI.ViewModels;
using Archipelago.Core.Models;
using Archipelago.Core.Util;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Newtonsoft.Json;
using Serilog;
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
            Context = new MainPageViewModel();
            Context.ConnectClicked += Context_ConnectClicked;
            Context.CommandReceived += (e, a) =>
            {
                Client?.SendMessage(a.Command);
            };
            MainPage = new MainPage(Context);
            Context.ConnectButtonEnabled = true;
        }

        private async void Context_ConnectClicked(object? sender, ConnectClickedEventArgs e)
        {
            if (Client != null)
            {
                Client.Connected -= OnConnected;
                Client.Disconnected -= OnDisconnected;
            }
            DuckstationClient client = new DuckstationClient();
            var ePSXeConnected = client.Connect();
            if (!ePSXeConnected)
            {
                Log.Logger.Warning("duckstation not running, open duckstation and launch the game before connecting!");
                return;
            }
            Client = new ArchipelagoClient(client);

            Addresses.DuckstationOffset = Helpers.GetDuckstationOffset();
            Memory.GlobalOffset = Addresses.DuckstationOffset;

            Client.Connected += OnConnected;
            Client.Disconnected += OnDisconnected;

            await Client.Connect(e.Host, "Spyro 3");
            GameLocations = Helpers.BuildEggLocationList();
            await Client.Login(e.Slot, !string.IsNullOrWhiteSpace(e.Password) ? e.Password : null);
            Client.PopulateLocations(GameLocations);
            Client.CurrentSession.Locations.CheckedLocationsUpdated += Locations_CheckedLocationsUpdated;
            Client.MessageReceived += Client_MessageReceived;
            Client.ItemReceived += (e, args) =>
            {
                Log.Logger.Information($"Item Received: {JsonConvert.SerializeObject(args.Item)}");
                if (args.Item.Name == "Egg")
                {
                    CalculateCurrentEggs();
                }
            };
        }
        private static void LogItem(Item item)
        {
            var messageToLog = new LogListItem(new List<TextSpan>()
            {
                new TextSpan(){Text = $"[{item.Id.ToString()}] -", TextColor = Color.FromRgb(255, 255, 255)},
                new TextSpan(){Text = $"{item.Name}", TextColor = Color.FromRgb(200, 255, 200)},
                new TextSpan(){Text = $"x{item.Quantity.ToString()}", TextColor = Color.FromRgb(200, 255, 200)}
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
            var currentEggs = CalculateCurrentEggs();
            foreach (var locationId in newCheckedLocations)
            {
                var locationName = Client.CurrentSession.Locations.GetLocationNameFromId(locationId);
                var isLocalLocation = GameLocations.Any(x => x.Id == locationId);
                if (isLocalLocation)
                {
                    var location = GameLocations.First(x => x.Id == locationId);
                    currentEggs = CalculateCurrentEggs();
                    if (location.Category == "Egg")
                    {
                        if (currentEggs >= 100 && Client.CurrentSession.Locations.AllLocationsChecked.Any(x => GameLocations.First(y => y.Id == x).Name == "Sorceress Defeated"))
                        {
                            Client.SendGoalCompletion();
                        }
                    }
                    if (locationName == "Sorceress Defeated" && currentEggs >= 100)
                    {
                        Client.SendGoalCompletion();
                    }
                }
            }
        }
        private static int CalculateCurrentEggs()
        {
            var eggList = Helpers.BuildEggLocationList();
            Log.Logger.Debug($"Known egg count: {eggList.Count}");
            Log.Logger.Debug($"Received item count: {Client.CurrentSession.Items.AllItemsReceived.Count}");
            var count = eggList.Count(x => Client.CurrentSession.Items.AllItemsReceived.Any(y => y.LocationId == x.Id) && x.Category == "Egg");
            Log.Logger.Debug($"Received Egg count: {count}");
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
