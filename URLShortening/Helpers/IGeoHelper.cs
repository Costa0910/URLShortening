using URLShortening.Models;

namespace URLShortening.Helpers;

public interface IGeoHelper
{
    Task<LocationModel> GetCountryAsync(string ipAddress);
}
