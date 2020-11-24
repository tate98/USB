using System;
using System.Text;
using LibUsbDotNet;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;

namespace Examples
{
    internal class MyRead
    {
        public static IUsbDevice MyUsbDevice;

        public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(0x046D, 0xC332);

        public static void Main(string[] args)
        {
            using (UsbContext context = new UsbContext())
            {
                // Find and open the usb device.
                MyUsbDevice = context.Find(MyUsbFinder);

                // If the device is open and ready
                if (MyUsbDevice == null) throw new Exception("Device Not Found.");
                // open read endpoint 1.
                var reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
                byte[] readBuffer = new byte[1024];
                while (true)
                {
                    int bytesRead;
                    reader.Read(readBuffer, 5000, out bytesRead);
                    Console.WriteLine("{0} bytes read", bytesRead);
                    // Write to the console.
                    Console.Write(Encoding.Default.GetString(readBuffer, 0, bytesRead));
                }
            }
        }
    }
}