using Bluetooth.Infos;
using CriticalData;
using CriticalData.Interfaces;
using FileSystem;
using FileSystem.Interfaces;
using Lights.Core.Keepsmile;
using System.Drawing;
using System.Text;

namespace Lights.Cli
{
    internal class Program
    {

        protected static CriticalDataManager? criticalDataManager;

        protected static IFileSystemController? fileSystemController;

        protected static KeepsmileController? keepsmileController;

        protected static List<BluetoothDeviceInfo>? scannedDevices;

        protected const string dataFileName = "KeepsmileDeviceData";
        static async Task Main(string[] args)
        {
            keepsmileController = new KeepsmileController(Console.WriteLine);
            fileSystemController = new FileSystemController();
            criticalDataManager = new CriticalDataManager(dataFileName, fileSystemController, new List<ICriticalData>() { keepsmileController });

            PrintHelpCommands();
            string? input = null;
            bool exit = false;
            while (!exit)
            {
                Console.Write("input> ");
                input = Console.ReadLine();

                switch (input)
                {
                    case "help":
                        PrintHelpCommands();
                        break;
                    case "exit":
                        exit = true;
                        break;
                    case "scan":
                        scannedDevices = await keepsmileController.FindDevicesAsync(2f);
                        PrintScannedDevices();
                        break;
                    case "connect new":
                        await ConnectNewAsync();
                        break;
                    case "connect remembered":
                        await ConnectRememberedAsync();
                        break;
                    case "disconnect":
                        await keepsmileController.DisconnectFromDeviceAsync();
                        break;
                    case "remember":
                        keepsmileController.RememberDevice = true;
                        break;
                    case "forget":
                        keepsmileController.RememberDevice = false;
                        break;
                    case "set color":
                        await SetColorAsync();
                        break;
                    case "set speed":
                        await SetSpeedAsync();
                        break;
                    case "set brightness":
                        await SetBrightnessAsync();
                        break;
                    case "turn on":
                        await ToggleOnOff(true);
                        break;
                    case "turn off":
                        await ToggleOnOff(false);
                        break;
                    default:
                        Console.WriteLine("enter a valid input, type \"help\" for valid inputs.");
                        break;

                }
            }
        }

        protected static async Task ToggleOnOff(bool on)
        {
            if (keepsmileController == null)
                return;

            string onoff = on ? "on" : "off"; 

            if (!keepsmileController.DeviceConnected)
            {
                Console.WriteLine($"No connected device, connect before turning {onoff}");
                return;
            }

            await (on ? keepsmileController.TurnLightsOnAsync(true) : keepsmileController.TurnLightsOffAsync(true));
        }

        protected static async Task ConnectNewAsync()
        {
            Console.Write("selected index> ");
            string input = Console.ReadLine();
            if (!int.TryParse(input, out int index))
            {
                Console.WriteLine("failed, enter a valid number");
                return;
            }

            if (scannedDevices == null || scannedDevices.Count - 1 < index)
            {
                Console.WriteLine("failed, enter a valid index, ensure scan has been called");
                return;
            }

            bool result = await keepsmileController.ConnectToDeviceAsync(scannedDevices[index]);
            Console.WriteLine("Connection success: " + result);
        }

        protected static async Task SetBrightnessAsync()
        {
            if (keepsmileController == null)
                return;

            if (!keepsmileController.DeviceConnected)
            {
                Console.WriteLine("No connected device, connect before setting brightness");
                return;
            }

            Console.Write("brightness (0-100): ");
            string brightness = Console.ReadLine();
            if (!int.TryParse(brightness, out int Brightness))
            {
                Console.WriteLine("invalid brightness detected, defaulting to previous brightness");
                Brightness = -1;
            }
            await keepsmileController.SetBrightnessAsync(Brightness, true);
        }

        protected static async Task SetSpeedAsync()
        {
            if (keepsmileController == null)
                return;

            if (!keepsmileController.DeviceConnected)
            {
                Console.WriteLine("No connected device, connect before setting speed");
                return;
            }

            if (keepsmileController.CurrentLightsSequence == KeepsmileConstants.LightsSequence.Invalid)
            {
                Console.WriteLine("can't set speed while a color sequence isn't playing");
                return;
            }

            Console.Write("speed (0-100): ");
            string speed = Console.ReadLine();
            if (!int.TryParse(speed, out int Speed))
            {
                Console.WriteLine("invalid speed detected, defaulting to previous speed");
                Speed = -1;
            }
            await keepsmileController.SetSpeedAsync(Speed, true);


        }

        protected static async Task SetColorAsync()
        {
            if (keepsmileController == null)
                return;

            if (!keepsmileController.DeviceConnected)
            { 
                Console.WriteLine("No connected device, connect before setting a color");
                return;
            }

            PrintColorCommands();
            Console.Write("selected color> ");
            string input = Console.ReadLine();
            if  (int.TryParse(input, out int number))
            {
                int counter = 0;
                foreach (KeepsmileConstants.LightsSequence sequence in Enum.GetValues<KeepsmileConstants.LightsSequence>())
                {
                    if (counter == number && sequence != KeepsmileConstants.LightsSequence.Invalid)
                    {
                        await keepsmileController.SetLightsSequenceAsync(sequence, force: true);
                        return;
                    }
                    counter++;
                }
                Console.WriteLine("invalid number, try again");
            }
            else
            { 
                switch (input)
                {

                    case "red":
                        await keepsmileController.SetColorAsync(Color.Red);
                        break;
                    case "green":
                        await keepsmileController.SetColorAsync(Color.Green);
                        break;
                    case "blue":
                        await keepsmileController.SetColorAsync(Color.Blue);
                        break;
                    case "custom":
                        Console.Write("r value: ");
                        string rValue = Console.ReadLine();
                        if (!int.TryParse(rValue, out int RValue))
                        {
                            Console.WriteLine("failed, enter a valid r value (integer)");
                            return;
                        }
                        Console.Write("g value: ");
                        string gValue = Console.ReadLine();
                        if (!int.TryParse(gValue, out int GValue))
                        {
                            Console.WriteLine("failed, enter a valid g value (integer)");
                            return;
                        }
                        Console.Write("b value: ");
                        string bValue = Console.ReadLine();
                        if (!int.TryParse(bValue, out int BValue))
                        {
                            Console.WriteLine("failed, enter a valid b value (integer)");
                            return;
                        }

                        Color color = Color.FromArgb(RValue, GValue, BValue);
                        await keepsmileController.SetColorAsync(color, -1, true);

                        break;
                }
            }
        }

        protected static async Task ConnectRememberedAsync()
        {
            bool result = await keepsmileController.ConnectToSavedDeviceAsync();
            if (result)
                Console.WriteLine("sucessfully connected");
            else
                Console.WriteLine("failed to connect");
        }

        protected static void PrintColorCommands()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("custom - enter custom RGB values");
            sb.AppendLine("red");
            sb.AppendLine("green");
            sb.AppendLine("blue");

            int counter = 0;
            foreach (KeepsmileConstants.LightsSequence sequence in Enum.GetValues<KeepsmileConstants.LightsSequence>())
            {
                sb.AppendLine($"{counter} - {sequence}");
                counter++;
            }
            Console.WriteLine(sb.ToString());
        }

        protected static void PrintHelpCommands()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("help - prints out available commands");
            sb.AppendLine("exit - exits out of program");
            sb.AppendLine("scan - scans for available bluetooth devices");
            sb.AppendLine("connect new - connects to a new device (call after scanning)");
            sb.AppendLine("connect remembered - connects to the remembered device");
            sb.AppendLine("disconnect - disconnects from the courrently connected device");
            sb.AppendLine("remember - remember the connected device");
            sb.AppendLine("forget - forget the connected device");
            sb.AppendLine("set color - sets the color on the connected device");
            sb.AppendLine("set brightness - sets the brightness on the connected device.");
            sb.AppendLine("set speed - sets the speed on the connected device.");
            sb.AppendLine("turn on - turns the lights on");
            sb.AppendLine("turn off - turns the lights off");
            Console.WriteLine(sb.ToString());       
        }

        protected static void PrintScannedDevices()
        {
            if (scannedDevices == null)
                return;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < scannedDevices.Count; i++) 
            { 
                BluetoothDeviceInfo device = scannedDevices[i];
                sb.Append($"({i}) ");
                sb.AppendLine(device.ToString());
            }
            Console.WriteLine(sb.ToString());
        }
    }

}
