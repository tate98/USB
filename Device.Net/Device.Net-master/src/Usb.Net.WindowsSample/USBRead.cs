using System;
using System.Threading;
using System.Threading.Tasks;
using Usb.Net.Sample;
using Device.Net;
using System.Collections.Generic;
using System.Linq;

#if (!LIBUSB)
using Usb.Net.Windows;
using Hid.Net.Windows;
using SerialPort.Net.Windows;
#else
using Device.Net.LibUsb;
#endif

namespace Usb.Net.WindowsSample
{
    internal class USBRead
    {
        #region Fields
        private static readonly Reader _DeviceConnectionExample = new Reader();
        private static readonly DebugLogger Logger = new DebugLogger();
        private static readonly DebugTracer Tracer = new DebugTracer();
        #endregion

        #region Main
        private static void Main(string[] args)
        {
            //Register the factory for creating Usb devices. This only needs to be done once.
#if (LIBUSB)
            LibUsbUsbDeviceFactory.Register(Logger, Tracer);
#else
            WindowsUsbDeviceFactory.Register(Logger, Tracer);
            WindowsHidDeviceFactory.Register(Logger, Tracer);
            WindowsSerialPortDeviceFactory.Register(Logger, Tracer);
#endif

            _DeviceConnectionExample.USBInitialized += _DeviceConnectionExample_USBInitialized;
            _DeviceConnectionExample.USBDisconnected += _DeviceConnectionExample_USBDisconnected;

            Go();

            new ManualResetEvent(false).WaitOne();
        }

        private static async Task Go()
        {
            ConnectedDeviceDefinition menuOption = await Menu();

            try
            {
                await _DeviceConnectionExample.InitializeUSBAsync(menuOption);
                Console.WriteLine("Waiting for DisplayDataAsync()...");
                await DisplayDataAsync();
                _DeviceConnectionExample.Dispose();

                GC.Collect();

                await Task.Delay(10000);
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine(ex.ToString());
            }
            Console.ReadKey();
        }
        #endregion

        #region Event Handlers
        private static void _DeviceConnectionExample_USBDisconnected(object sender, EventArgs e)
        {
            Console.Clear();
            Console.WriteLine("Disconnected.");
            DisplayWaitMessage();
        }

        private static async void _DeviceConnectionExample_USBInitialized(object sender, EventArgs e)
        {
            try
            {
                Console.Clear();
                await DisplayDataAsync();
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine(ex.ToString());
            }
        }
        #endregion

        #region Private Methods
        private async static Task<ConnectedDeviceDefinition> Menu()
        {
            while (true)
            {
                Console.Clear();

                IEnumerable<ConnectedDeviceDefinition> devices = await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(null);
                Console.WriteLine("Currently connected devices: ");
                int i = 0;
                foreach (var device in devices)
                {
                    Console.WriteLine(i + ". " + device.DeviceId);
                    i++;
                }
                Console.WriteLine();

                Console.WriteLine();
                Console.WriteLine("Select number of device to connect to...");
                var consoleKey = Console.ReadKey();
                Console.WriteLine();
                var devicesList = devices.ToList();
                int deviceNumber = (int)Char.GetNumericValue(consoleKey.KeyChar);
                Console.WriteLine("Connecting to " + devicesList[deviceNumber].DeviceId);
                return devicesList[deviceNumber];
            }
        }

        private static async Task DisplayDataAsync()
        {
            var bytes = await _DeviceConnectionExample.WriteAndReadFromDeviceAsync();
            Console.Clear();
            Console.WriteLine("Device connected. Output:");
            DisplayData(bytes);
        }

        private static void DisplayData(byte[] readBuffer)
        {
            Console.WriteLine("Waiting for device...");
            Console.WriteLine(string.Join(' ', readBuffer));
            Console.ReadKey();
        }

        private static void DisplayWaitMessage()
        {
            Console.WriteLine("Waiting for device to be plugged in...");
        }
        #endregion
    }
}
