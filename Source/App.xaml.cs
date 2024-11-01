using Archipelago.Core;
using Archipelago.Core.MauiGUI;
using Archipelago.Core.MauiGUI.Models;
using Archipelago.Core.MauiGUI.ViewModels;
using Archipelago.Core.Util;
using Archipelago.ePSXe;
using Newtonsoft.Json;
using Serilog;
using Color = Microsoft.Maui.Graphics.Color;
using Location = Archipelago.Core.Models.Location;

namespace S3AP
{
    public partial class App : Application
    {
        public MainPageViewModel Context;
        public static ArchipelagoClient Client { get; set; }
        public static List<Location> GameLocations { get; set; }
        public App()
        {
            InitializeComponent();
            var options = new GuiDesignOptions
            {
                BackgroundColor = Color.FromArgb("FF800080"),
                ButtonColor = Color.FromArgb("FF483D8B"),
                ButtonTextColor = Color.FromRgb(192, 192, 0),
                TextColor = Color.FromRgb(192, 192, 0),
                Title = "S3AP - Spyro 3 Archipelago Client",
            };
            Context = new MainPageViewModel(options);
            Context.ConnectClicked += Context_ConnectClicked;
            MainPage = new MainPage(Context);
            MainPage = MainPage;
        }

        private async void Context_ConnectClicked(object? sender, ConnectClickedEventArgs e)
        {
            if (Client != null)
            {
                Client.Connected -= OnConnected;
                Client.Disconnected -= OnDisconnected;
            }
            ePSXeClient client = new ePSXeClient();
            var ePSXeConnected = client.Connect();
            if (!ePSXeConnected)
            {
                Log.Logger.Information("ePSXE not running, open ePSXe and launch the game before connecting!");
                return;
            }
            Client = new ArchipelagoClient(client);

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
        private void Client_MessageReceived(object? sender, Archipelago.Core.Models.MessageReceivedEventArgs e)
        {

            Log.Logger.Information(JsonConvert.SerializeObject(e.Message));
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
                            var status = Client.CurrentSession.DataStorage.GetClientStatus(Client.CurrentSession.ConnectionInfo.Slot);
                            if (!status.HasFlag(Archipelago.MultiClient.Net.Enums.ArchipelagoClientState.ClientGoal))
                            {
                                Client.SendGoalCompletion();
                            }
                        }
                    }
                    if (locationName == "Sorceress Defeated" && currentEggs >= 100)
                    {
                        var status = Client.CurrentSession.DataStorage.GetClientStatus(Client.CurrentSession.ConnectionInfo.Slot);
                        if (!status.HasFlag(Archipelago.MultiClient.Net.Enums.ArchipelagoClientState.ClientGoal))
                        {
                            Client.SendGoalCompletion();
                        }
                    }
                }
            }
        }
        private static int CalculateCurrentEggs()
        {
            var eggList = Helpers.BuildEggLocationList();
            Log.Logger.Information($"Known egg count: {eggList.Count}");
            Log.Logger.Information($"Received item count: {Client.CurrentSession.Items.AllItemsReceived.Count}");
            var count = eggList.Count(x => Client.CurrentSession.Items.AllItemsReceived.Any(y => y.LocationId == x.Id) && x.Category == "Egg");
            Log.Logger.Information($"Received Egg count: {count}");
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
    }

}
