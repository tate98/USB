﻿using System;

namespace Device.Net.Windows
{
    public static class WindowsDeviceConstants
    {
        public static Guid GUID_DEVINTERFACE_HID { get; } = new Guid("4D1E55B2-F16F-11CF-88CB-001111000030");
        public static Guid GUID_DEVINTERFACE_USB_DEVICE { get; } = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED");
        public static Guid WinUSBGuid { get; } = new Guid("dee824ef-729b-4a0e-9c14-b7117d33a817");
    }
}
