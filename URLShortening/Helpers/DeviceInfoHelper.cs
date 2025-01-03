using DeviceDetectorNET;
using URLShortening.Models;

namespace URLShortening.Helpers;

public class DeviceInfoHelper : IDeviceInfoHelper
{
    public DeviceInfoModel GetDeviceInfo(string userAgent)
    {
        var deviceDetector = new DeviceDetector(userAgent);
        deviceDetector.Parse();

        return new DeviceInfoModel()
        {
            DeviceType = deviceDetector.GetDeviceName() ?? "Unknown",
            OS = deviceDetector.GetOs()?.Match?.Name ?? "Unknown",
            Browser = deviceDetector.GetClient()?.Match?.Name ?? "Unknown"
        };
    }
}
