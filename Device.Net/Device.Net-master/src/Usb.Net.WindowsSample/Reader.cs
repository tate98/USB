using Device.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Usb.Net.Sample
{
    internal sealed class Reader : IDisposable
    {
        #region Fields
#if(LIBUSB)
        private const int PollMilliseconds = 6000;
#else
        private const int PollMilliseconds = 3000;
#endif
        //Define the types of devices to search for. This particular device can be connected to via USB, or Hid
        private readonly List<FilterDeviceDefinition> _DeviceDefinitions = new List<FilterDeviceDefinition>
        {
            new FilterDeviceDefinition{ DeviceType= DeviceType.Usb, VendorId= 0x046D, ProductId=0xC332, Label="USB device" }
        };
        #endregion

        #region Events
        public event EventHandler USBInitialized;
        public event EventHandler USBDisconnected;
        #endregion

        #region Public Properties
        public IDevice USBDevice { get; private set; }
        public DeviceListener DeviceListener { get; }
        #endregion

        #region Constructor
        public Reader()
        {
            DeviceListener = new DeviceListener(_DeviceDefinitions, PollMilliseconds) { Logger = new DebugLogger() };
        }
        #endregion

        #region Event Handlers
        private void DevicePoller_DeviceInitialized(object sender, DeviceEventArgs e)
        {
            USBDevice = e.Device;
            USBInitialized?.Invoke(this, new EventArgs());
        }

        private void DevicePoller_DeviceDisconnected(object sender, DeviceEventArgs e)
        {
            USBDevice = null;
            USBDisconnected?.Invoke(this, new EventArgs());
        }
        #endregion

        #region Public Methods
        public void StartListening()
        {
            USBDevice?.Close();
            DeviceListener.DeviceDisconnected += DevicePoller_DeviceDisconnected;
            DeviceListener.DeviceInitialized += DevicePoller_DeviceInitialized;
            DeviceListener.Start();
        }

        public async Task InitializeUSBAsync(ConnectedDeviceDefinition deviceDefinition)
        {
            //Get the first available device and connect to it
            USBDevice = DeviceManager.Current.GetDevice(deviceDefinition);
            Console.WriteLine(USBDevice);
            if (USBDevice == null) throw new Exception("There were no devices found");

            await USBDevice.InitializeAsync();
        }

        public async Task<byte[]> WriteAndReadFromDeviceAsync()
        {
            //Create a buffer with 3 bytes (initialize)
            var writeBuffer = new byte[64];
            writeBuffer[0] = 0x3f;
            writeBuffer[1] = 0x23;
            writeBuffer[2] = 0x23;
            
            //Read data from device
            //return await USBDevice.WriteAndReadAsync(writeBuffer);
            return await USBDevice.ReadAsync();
        }

        public void Dispose()
        {
            DeviceListener.DeviceDisconnected -= DevicePoller_DeviceDisconnected;
            DeviceListener.DeviceInitialized -= DevicePoller_DeviceInitialized;
            DeviceListener.Dispose();
            USBDevice?.Dispose();
        }
        #endregion
    }
}
