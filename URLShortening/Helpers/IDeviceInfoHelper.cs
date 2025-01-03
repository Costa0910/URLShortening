using URLShortening.Models;

namespace URLShortening.Helpers;

public interface IDeviceInfoHelper
{
    DeviceInfoModel GetDeviceInfo(string userAgent);
}
