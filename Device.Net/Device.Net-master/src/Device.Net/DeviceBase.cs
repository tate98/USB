﻿using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net
{
    public abstract class DeviceBase : IDisposable
    {
        #region Fields
        private readonly SemaphoreSlim _WriteAndReadLock = new SemaphoreSlim(1, 1);
        private bool disposed;
        private string _LogRegion;
        #endregion

        #region Public Abstract Properties
        public abstract ushort WriteBufferSize { get; }
        public abstract ushort ReadBufferSize { get; }
        public abstract bool IsInitialized { get; }
        #endregion

        #region Public Properties
        public ConnectedDeviceDefinitionBase ConnectedDeviceDefinition { get; set; }
        public string DeviceId { get; }
        public ILogger Logger { get; }
        public ITracer Tracer { get; }
        #endregion

        #region Constructor
        protected DeviceBase(string deviceId, ILogger logger, ITracer tracer)
        {
            DeviceId = deviceId ?? throw new ArgumentNullException(nameof(deviceId));
            Tracer = tracer;
            Logger = logger;
        }
        #endregion

        #region Private Methods
        private void Log(string message, string region, Exception ex, LogLevel logLevel)
        {
            Logger?.Log(message, region, ex, logLevel);
        }

        private void Log(string message, Exception ex, LogLevel logLevel, [CallerMemberName] string callMemberName = null)
        {
            if (_LogRegion == null)
            {
                _LogRegion = GetType().Name;
            }

            Log(message, $"{_LogRegion} - {callMemberName}", ex, logLevel);
        }
        #endregion

        #region Protected Methods
        protected void Log(string message, [CallerMemberName] string callMemberName = null)
        {
            Log(message, null, LogLevel.Information, callMemberName);
        }

        protected void Log(string message, Exception ex, [CallerMemberName] string callMemberName = null)
        {
            Log(message, ex, LogLevel.Error, callMemberName);
        }

        protected void Log(string message, LogLevel logLevel, [CallerMemberName] string callMemberName = null)
        {
            Log(message, null, logLevel, callMemberName);
        }
        #endregion

        #region Public Abstract Methods
        //TODO: Why are these here?

        public abstract Task<ReadResult> ReadAsync();
        public abstract Task WriteAsync(byte[] data);
        #endregion

        #region Public Methods
        public virtual Task Flush()
        {
            throw new NotImplementedException(Messages.ErrorMessageFlushNotImplemented);
        }

        public async Task<ReadResult> WriteAndReadAsync(byte[] writeBuffer)
        {
            await _WriteAndReadLock.WaitAsync();

            try
            {
                await WriteAsync(writeBuffer);
                var retVal = await ReadAsync();
                Log(Messages.SuccessMessageWriteAndReadCalled);
                return retVal;
            }
            catch (Exception ex)
            {
                Log(Messages.ErrorMessageReadWrite, ex);
                throw;
            }
            finally
            {
                _WriteAndReadLock.Release();
            }
        }

        /// <summary> 
        /// Many Hid devices on Windows have a buffer size that is one byte larger than the logical buffer size. For compatibility with other platforms etc. we need to remove the first byte. See DataHasExtraByte
        /// </summary> 
        public static byte[] RemoveFirstByte(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            var length = bytes.Length - 1;
            var retVal = new byte[length];

            Array.Copy(bytes, 1, retVal, 0, length);

            return retVal;
        }

        public virtual void Dispose()
        {
            if (disposed) return;

            disposed = true;

            _WriteAndReadLock.Dispose();

            GC.SuppressFinalize(this);
        }
        #endregion

        #region Finalizer
        ~DeviceBase()
        {
            Dispose();
        }
        #endregion
    }
}
