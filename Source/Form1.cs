using Archipelago.Core;
using Archipelago.Core.Models;
using Archipelago.Core.Util;
using Archipelago.ePSXe;
using Newtonsoft.Json;
using System.Text;
using System.Threading;
namespace S3AP
{
    public partial class Form1 : Form
    {
        public static ArchipelagoClient Client { get; set; }
        public static List<Location> GameLocations { get; set; }
        public Form1()
        {
            InitializeComponent();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ThreadPool.SetMinThreads(500, 500);
        }
        public void WriteLine(string output)
        {
            Invoke(() =>
            {
                outputTextbox.Text += output;
                outputTextbox.Text += System.Environment.NewLine;

                System.Diagnostics.Debug.WriteLine(output + System.Environment.NewLine);
            });
        }
        public async Task Connect()
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
                WriteLine("ePSXE not running, open ePSXe and launch the game before connecting!");
                return;
            }
            Client = new ArchipelagoClient(client);

            Client.Connected += OnConnected;
            Client.Disconnected += OnDisconnected;

            await Client.Connect(hostTextbox.Text, "Spyro 3");
            GameLocations = Helpers.BuildEggLocationList();
            await Client.Login(slotTextbox.Text, !string.IsNullOrWhiteSpace(passwordTextbox.Text) ? passwordTextbox.Text : null);
            Client.PopulateLocations(GameLocations);
            Client.CurrentSession.Locations.CheckedLocationsUpdated += Locations_CheckedLocationsUpdated;
            Client.ItemReceived += (e, args) =>
            {
                WriteLine($"Item Received: {JsonConvert.SerializeObject(args.Item)}");
                if (args.Item.Name == "Egg")
                {
                    var currentEggs = Memory.ReadByte(Addresses.TotalEggAddress);
                    Memory.WriteByte(Addresses.TotalEggAddress, (byte)(currentEggs + 1));
                }
            };
        }

        private void Locations_CheckedLocationsUpdated(System.Collections.ObjectModel.ReadOnlyCollection<long> newCheckedLocations)
        {
            foreach (var location in newCheckedLocations)
            {
                var locationName = Client.CurrentSession.Locations.GetLocationNameFromId(location);
                var isLocalLocation = GameLocations.Any(x => x.Id == location);
                if (isLocalLocation)
                {
                    if (locationName.StartsWith("Egg"))
                    {
                        var currentEggs = Memory.ReadByte(Addresses.TotalEggAddress);
                        Memory.WriteByte(Addresses.TotalEggAddress, (byte)(currentEggs - 1));
                    }
                }
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (Client == null || !(Client?.IsConnected ?? false))
            {
                var valid = ValidateSettings();
                if (!valid)
                {
                    WriteLine("Invalid settings, please check your input and try again.");
                    return;
                }
                Connect().ConfigureAwait(false);
            }
            else if (Client != null)
            {
                WriteLine("Disconnecting...");
                Client.Disconnect();
            }
        }
        private bool ValidateSettings()
        {
            var valid = !string.IsNullOrWhiteSpace(hostTextbox.Text) && !string.IsNullOrWhiteSpace(slotTextbox.Text);
            return valid;
        }
        private void OnConnected(object sender, EventArgs args)
        {
            WriteLine("Connected to Archipelago");
            WriteLine($"Playing {Client.CurrentSession.ConnectionInfo.Game} as {Client.CurrentSession.Players.GetPlayerName(Client.CurrentSession.ConnectionInfo.Slot)}");
        }

        private void OnDisconnected(object sender, EventArgs args)
        {
            WriteLine("Disconnected from Archipelago");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            WriteLine("S3AP - Spyro 3 Archipelago Randomiser");
            WriteLine("-- By ArsonAssassin --");


            WriteLine("Ready to connect!");
        }
    }
}
