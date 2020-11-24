﻿using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace Device.Net.Windows
{
    public class ApiService : IApiService
    {
        #region Public Properties
        public ILogger Logger { get; }
        #endregion

        #region Constructor
        public ApiService(ILogger logger)
        {
            Logger = logger;
        }
        #endregion

        #region Implementation
        public SafeFileHandle CreateWriteConnection(string deviceId)
        {
            return CreateConnection(deviceId, FileAccessRights.GenericRead | FileAccessRights.GenericWrite, APICalls.FileShareRead | APICalls.FileShareWrite, APICalls.OpenExisting);
        }

        public SafeFileHandle CreateReadConnection(string deviceId, FileAccessRights desiredAccess)
        {
            return CreateConnection(deviceId, desiredAccess, APICalls.FileShareRead | APICalls.FileShareWrite, APICalls.OpenExisting);
        }

        public bool AGetCommState(SafeFileHandle hFile, ref Dcb lpDCB) => GetCommState(hFile, ref lpDCB);
        public bool APurgeComm(SafeFileHandle hFile, int dwFlags) => PurgeComm(hFile, dwFlags);
        public bool ASetCommTimeouts(SafeFileHandle hFile, ref CommTimeouts lpCommTimeouts) => SetCommTimeouts(hFile, ref lpCommTimeouts);
        public bool AWriteFile(SafeFileHandle hFile, byte[] lpBuffer, int nNumberOfBytesToWrite, out int lpNumberOfBytesWritten, int lpOverlapped) => WriteFile(hFile, lpBuffer, nNumberOfBytesToWrite, out lpNumberOfBytesWritten, lpOverlapped);
        public bool AReadFile(SafeFileHandle hFile, byte[] lpBuffer, int nNumberOfBytesToRead, out uint lpNumberOfBytesRead, int lpOverlapped) => ReadFile(hFile, lpBuffer, nNumberOfBytesToRead, out lpNumberOfBytesRead, lpOverlapped);
        public bool ASetCommState(SafeFileHandle hFile, [In] ref Dcb lpDCB) => SetCommState(hFile, ref lpDCB);
        #endregion

        #region Private Methods
        private SafeFileHandle CreateConnection(string deviceId, FileAccessRights desiredAccess, uint shareMode, uint creationDisposition)
        {
            Logger?.Log($"Calling {nameof(APICalls.CreateFile)} for DeviceId: {deviceId}. Desired Access: {desiredAccess}. Share mode: {shareMode}. Creation Disposition: {creationDisposition}", nameof(ApiService), null, LogLevel.Information);
            return APICalls.CreateFile(deviceId, desiredAccess, shareMode, IntPtr.Zero, creationDisposition, 0, IntPtr.Zero);
        }
        #endregion

        #region DLL Imports
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool PurgeComm(SafeFileHandle hFile, int dwFlags);
        [DllImport("kernel32.dll")]
        private static extern bool GetCommState(SafeFileHandle hFile, ref Dcb lpDCB);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetCommTimeouts(SafeFileHandle hFile, ref CommTimeouts lpCommTimeouts);
        [DllImport("kernel32.dll")]
        private static extern bool SetCommState(SafeFileHandle hFile, [In] ref Dcb lpDCB);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteFile(SafeFileHandle hFile, byte[] lpBuffer, int nNumberOfBytesToWrite, out int lpNumberOfBytesWritten, int lpOverlapped);
        [DllImport("kernel32.dll")]
        private static extern bool ReadFile(SafeFileHandle hFile, byte[] lpBuffer, int nNumberOfBytesToRead, out uint lpNumberOfBytesRead, int lpOverlapped);
        #endregion
    }
}
