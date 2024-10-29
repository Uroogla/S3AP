using Archipelago.Core;
using Archipelago.Core.GUI;
using Archipelago.Core.Models;
using Archipelago.Core.Util;
using Archipelago.ePSXe;
using Newtonsoft.Json;
using Serilog;

namespace S3AP
{
    internal static class Program
    {
        public static MainForm MainForm;
        public static ArchipelagoClient Client { get; set; }
        public static List<Location> GameLocations { get; set; }
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            var options = new GuiDesignOptions
            {
                BackgroundColor = Color.Purple,
                ButtonColor = Color.DarkSlateBlue,
                ButtonTextColor = Color.FromArgb(192, 192, 0),
                TextColor = Color.FromArgb(192, 192, 0),
            };
            MainForm = new MainForm(options);
            MainForm.ConnectClicked += MainForm_ConnectClicked;
            Application.Run(MainForm);
        }
        private static int CalculateCurrentEggs() 
        {
            var eggList = Helpers.BuildEggLocationList();
            var count = eggList.Count(x => Client.CurrentSession.Items.AllItemsReceived.Any(y => y.LocationId == x.Id));
            Memory.WriteByte(Addresses.TotalEggAddress, (byte)(count));
            return count;
        }

        private static async void MainForm_ConnectClicked(object? sender, ConnectClickedEventArgs e)
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
            Client.ItemReceived += (e, args) =>
            {
                Log.Logger.Information($"Item Received: {JsonConvert.SerializeObject(args.Item)}");
                if (args.Item.Name == "Egg")
                {
                    CalculateCurrentEggs();
                }
            };
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

        private static void Locations_CheckedLocationsUpdated(System.Collections.ObjectModel.ReadOnlyCollection<long> newCheckedLocations)
        {
            foreach (var locationId in newCheckedLocations)
            {
                var locationName = Client.CurrentSession.Locations.GetLocationNameFromId(locationId);
                var isLocalLocation = GameLocations.Any(x => x.Id == locationId);
                if (isLocalLocation)
                {
                    var location = GameLocations.First(x => x.Id == locationId);
                    var currentEggs = CalculateCurrentEggs();
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
    }
}