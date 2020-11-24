﻿using Android.App;
using Android.Hardware.Usb;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Device.Net;
using System;
using Usb.Net.Android;
using Usb.Net.Sample;

namespace Usb.Net.AndroidSample
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        #region Fields
        private readonly TrezorExample _TrezorExample = new TrezorExample();
        #endregion

        #region Protected Override Methods
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetContentView(Resource.Layout.activity_main);

                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                SetSupportActionBar(toolbar);

                var fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
                fab.Click += FabOnClick;
            }
            catch (Exception ex)
            {
                DisplayMessage($"Error Starting up: {ex.Message}");
            }
        }
        #endregion

        #region Public Override Methods
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }
        #endregion

        #region Event Handlers
        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                var usbManager = GetSystemService(UsbService) as UsbManager;
                if (usbManager == null) throw new Exception("UsbManager is null");

                //Register the factory for creating Usb devices. This only needs to be done once.
                AndroidUsbDeviceFactory.Register(usbManager, base.ApplicationContext, new DebugLogger(), new DebugTracer());

                _TrezorExample.TrezorDisconnected += _TrezorExample_TrezorDisconnected;
                _TrezorExample.TrezorInitialized += _TrezorExample_TrezorInitialized;
                _TrezorExample.StartListening();

                //var attachedReceiver = new UsbDeviceBroadcastReceiver(_TrezorExample.DeviceListener);
                //var detachedReceiver = new UsbDeviceBroadcastReceiver(_TrezorExample.DeviceListener);
                //RegisterReceiver(attachedReceiver, new IntentFilter(UsbManager.ActionUsbDeviceAttached));
                //RegisterReceiver(detachedReceiver, new IntentFilter(UsbManager.ActionUsbDeviceDetached));

                DisplayMessage("Waiting for device...");
            }
            catch (Exception ex)
            {
                DisplayMessage("Failed to start listener..." + ex.Message);
            }
        }

        private async void _TrezorExample_TrezorInitialized(object sender, EventArgs e)
        {
            try
            {
                var readBuffer = await _TrezorExample.WriteAndReadFromDeviceAsync();

                if (readBuffer != null && readBuffer.Length > 0)
                {
                    DisplayMessage($"All good. First three bytes {readBuffer[0]}, {readBuffer[1]}, {readBuffer[2]}");
                }
                else
                {
                    DisplayMessage("No good. No data returned.");
                }
            }
            catch (Exception ex)
            {
                DisplayMessage($"No good: {ex.Message}");
            }
        }

        private void _TrezorExample_TrezorDisconnected(object sender, EventArgs e)
        {
            DisplayMessage("Device disconnected. Waiting for device...");
        }
        #endregion

        #region Private Methods
        private void DisplayMessage(string message)
        {
            var fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            Snackbar.Make(fab, message, Snackbar.LengthLong).SetAction("Action", (View.IOnClickListener)null).Show();
        }
        #endregion
    }
}

