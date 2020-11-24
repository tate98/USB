﻿using Device.Net;
using Device.Net.UWP;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hid.Net.UWP
{
    public sealed class UWPHidDeviceFactory : UWPDeviceFactoryBase, IDeviceFactory, IDisposable
    {
        #region Fields
        private readonly SemaphoreSlim _TestConnectionSemaphore = new SemaphoreSlim(1, 1);
        private readonly Dictionary<string, ConnectionInfo> _ConnectionTestedDeviceIds = new Dictionary<string, ConnectionInfo>();
        private bool disposed;
        #endregion

        #region Public Override Properties
        public override DeviceType DeviceType => DeviceType.Hid;
        protected override string VendorFilterName => "System.DeviceInterface.Hid.VendorId";
        protected override string ProductFilterName => "System.DeviceInterface.Hid.ProductId";
        #endregion

        #region Protected Override Methods
        protected override string GetAqsFilter(uint? vendorId, uint? productId)
        {
            return $"{InterfaceEnabledPart} {GetVendorPart(vendorId)} {GetProductPart(productId)}";
        }
        #endregion

        #region Public Override Methods
        public override async Task<ConnectionInfo> TestConnection(string deviceId)
        {
            try
            {
                await _TestConnectionSemaphore.WaitAsync();

                if (_ConnectionTestedDeviceIds.TryGetValue(deviceId, out var connectionInfo)) return connectionInfo;

                using (var hidDevice = await UWPHidDevice.GetHidDevice(deviceId).AsTask())
                {
                    var canConnect = hidDevice != null;

                    if (!canConnect) return new ConnectionInfo { CanConnect = false };

                    Log($"Testing device connection. Id: {deviceId}. Can connect: {canConnect}", null);

                    connectionInfo = new ConnectionInfo { CanConnect = canConnect, UsagePage = hidDevice.UsagePage };

                    _ConnectionTestedDeviceIds.Add(deviceId, connectionInfo);

                    return connectionInfo;
                }
            }
            catch (Exception ex)
            {
                Log("Connection failed", ex);
                return new ConnectionInfo { CanConnect = false };
            }
            finally
            {
                _TestConnectionSemaphore.Release();
            }
        }
        #endregion

        #region Constructor
        public UWPHidDeviceFactory(ILogger logger, ITracer tracer) : base(logger, tracer)
        {

        }
        #endregion

        #region Public Methods
        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            if (deviceDefinition == null) throw new ArgumentNullException(nameof(deviceDefinition));

            return deviceDefinition.DeviceType == DeviceType.Usb ? null : new UWPHidDevice(deviceDefinition.DeviceId, Logger, Tracer);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            _TestConnectionSemaphore.Dispose();

            GC.SuppressFinalize(this);
        }
        #endregion

        #region Public Static Methods
        /// <summary>
        /// Register the factory for enumerating Hid devices on UWP.
        /// </summary>
        public static void Register(ILogger logger, ITracer tracer)
        {
            foreach (var deviceFactory in DeviceManager.Current.DeviceFactories)
            {
                if (deviceFactory is UWPHidDeviceFactory) return;
            }

            DeviceManager.Current.DeviceFactories.Add(new UWPHidDeviceFactory(logger, tracer));
        }
        #endregion

        #region Finalizer
        ~UWPHidDeviceFactory()
        {
            Dispose();
        }
        #endregion
    }
}