using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NanoidDotNet;
using URLShortening.Data;
using URLShortening.Data.Repository;
using URLShortening.DTOs;
using URLShortening.Helpers;
using URLShortening.Models;

namespace URLShortening.Controllers;

[ApiController]
[ApiVersion("1.0")] // Specify the API version
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ShortenController(
    IUserHelper userHelper,
    IUrlRepository
        urlRepository,
    IAccessLogRepository accessLogRepository,
    IDeviceInfoHelper deviceInfoHelper,
    IGeoHelper geoLocationHelper,
    IMapper mapper) :
    ControllerBase
{
    private readonly IGeoHelper _geoLocationHelper = geoLocationHelper;

    [HttpGet("{shortUrl}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> Get(string shortUrl)
    {
        var url = await urlRepository.FindByShortUrl(shortUrl);
        if (url is null ||
            url.ExpiresAt is not null && url.ExpiresAt < DateTime.Now)
        {
            return NotFound();
        }

        var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                 Request.HttpContext.Connection.RemoteIpAddress?.ToString();
        var referer = Request.Headers["Referer"].FirstOrDefault() ??
                      Request.Headers["Origin"].FirstOrDefault();
        var userAgent = Request.Headers["User-Agent"].FirstOrDefault() ??
                        "Unknown";

        var accessLog = new AccessLog()
        {
            AccessedAt = DateTime.Now,
            IPAddress = ip,
            Ref = referer,
            UserAgent = userAgent
        };


        url.AccessLogs.Add(accessLog);
        await urlRepository.UpdateAsync(url);

        var dto = mapper.Map<urlDto>(url);
        return Ok(dto);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    public async Task<IActionResult> Post(
        [FromBody] UrlRequestDto urlRequestDto)
    {
        var newUrl = new Url()
        {
            LongUrl = urlRequestDto.Url,
            ShortId = GenerateShortUrlId(),
            ExpiresAt = urlRequestDto.ExpiresAt,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        await urlRepository.AddAsync(newUrl);

        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes
            .Email)?.Value;

        if (email != null)
        {
            var user = await userHelper.FindUserByEmailAsync(email);
            if (user != null)
            {
                user.Urls.Add(newUrl);
                await userHelper.UpdateUserAsync(user);
            }
        }

        var dto = mapper.Map<urlDto>(newUrl);
        return Created($"/api/v1/shorten/{newUrl.ShortId}", dto);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get()
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)
            ?.Value;
        if (email is null)
        {
            return BadRequest();
        }

        var user = await userHelper.FindUserByEmailIncludeUrlsAsync(email);
        if (user is null)
        {
            return NotFound();
        }

        var urlsDto = mapper.Map<IEnumerable<urlDto>>(user.Urls);
        return Ok(urlsDto);
    }

    [HttpPut("{url}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put([FromBody] UrlRequestDto urlRequestDto)
    {
        var url = await urlRepository.FindByLongUrl(urlRequestDto.Url);
        if (url is null)
        {
            return NotFound();
        }

        url.ShortId = GenerateShortUrlId();
        url.UpdatedAt = DateTime.Now;
        if (urlRequestDto.ExpiresAt is not null)
        {
            url.ExpiresAt = urlRequestDto.ExpiresAt;
        }

        await urlRepository.UpdateAsync(url);

        var dto = mapper.Map<urlDto>(url);
        return Ok(dto);
    }

    [HttpPut("expire")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Expire(updateUrlDto updateUrlDto)
    {
        var url = await urlRepository.FindByShortUrl(updateUrlDto.ShortCode);
        if (url is null)
        {
            return NotFound();
        }

        url.ExpiresAt = updateUrlDto.ExpiresAt;
        url.UpdatedAt = DateTime.Now;
        await urlRepository.UpdateAsync(url);

        var dto = mapper.Map<urlDto>(url);
        return Ok(dto);
    }

    [HttpPut("update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(updateUrlDto updateUrlDto)
    {
        var url = await urlRepository.FindByShortUrl(updateUrlDto.ShortCode);
        if (url is null)
        {
            return NotFound();
        }

        url.ShortId = GenerateShortUrlId();
        url.UpdatedAt = DateTime.Now;
        url.ExpiresAt = updateUrlDto.ExpiresAt;
        await urlRepository.UpdateAsync(url);

        var dto = mapper.Map<urlDto>(url);
        return Ok(dto);
    }

    [HttpDelete("{shortUrl}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string shortUrl)
    {
        var url = await urlRepository.FindByShortUrl(shortUrl);
        if (url is null)
            return NotFound();

        await urlRepository.DeleteAsync(url);
        return NoContent();
    }

    [HttpGet("{shortUrl}/stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStats(string shortUrl)
    {
        var url = await urlRepository.FindByShortUrl(shortUrl);
        if (url is null)
        {
            return NotFound();
        }

        var stats = await BuildStats(url);
        return Ok(stats);
    }


    [HttpGet("topUrls")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTopUrls()
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)
            ?.Value;
        if (email is null)
        {
            return BadRequest();
        }

        var user = await userHelper.FindUserByEmailIncludeUrlsAsync(email);
        if (user is null)
        {
            return NotFound();
        }

        var urls = user.Urls.OrderByDescending(u => u.AccessLogs.Count)
            .Take(5);

        var urlsDto = mapper.Map<IEnumerable<urlDto>>(urls);
        return Ok(urlsDto);
    }

    private string GenerateShortUrlId()
    {
        return Nanoid.Generate(Nanoid.Alphabets.LettersAndDigits, 8);
    }

    private async Task<object> BuildStats(Url url)
    {
        var totalAccessCount = url.AccessLogs.Count;
        var lastAccess = url.AccessLogs.OrderByDescending(log => log.AccessedAt)
            .FirstOrDefault();
        var uniqueIPs = url.AccessLogs.Select(log => log.IPAddress)
            .Where(ip => !string.IsNullOrEmpty(ip)).Distinct().Count();

        var locationStats
            = await GetGroupLocation(url.AccessLogs);
        var referrerStats
            = GetGroupedStats(url.AccessLogs, log => new Uri(log.Ref).Host);
        var osStats = GetGroupedStats(url.AccessLogs,
            log => deviceInfoHelper.GetDeviceInfo(log.UserAgent).OS);
        var deviceStats = GetGroupedStats(url.AccessLogs,
            log => deviceInfoHelper.GetDeviceInfo(log.UserAgent).DeviceType);
        var browserStats = GetGroupedStats(url.AccessLogs,
            log => deviceInfoHelper.GetDeviceInfo(log.UserAgent).Browser);

        var lastAccessDevice = lastAccess != null
            ? deviceInfoHelper.GetDeviceInfo(lastAccess.UserAgent)
            : null;

        return new
        {
            url.Id,
            url = url.LongUrl,
            shortCode = url.ShortId,
            url.CreatedAt,
            url.UpdatedAt,
            TotalAccessCount = totalAccessCount,
            LastAccessed = lastAccess?.AccessedAt,
            UniqueIPCount = uniqueIPs,
            LastAccessDevice = lastAccessDevice == null
                ? null
                : new
                {
                    lastAccessDevice.DeviceType,
                    lastAccessDevice.OS,
                    lastAccessDevice.Browser
                },
            LocationStats = locationStats,
            ReferrerStats = referrerStats,
            OSStats = osStats,
            DeviceStats = deviceStats,
            BrowserStats = browserStats
        };
    }

    private static IEnumerable<GroupStats> GetGroupedStats(
        IEnumerable<AccessLog> logs,
        Func<AccessLog, string> keySelector)
    {
        return logs
            .Where(log => !string.IsNullOrEmpty(keySelector(log)))
            .GroupBy(keySelector)
            .Select(group => new GroupStats
                { Key = group.Key, Count = group.Count() })
            .OrderByDescending(stat => stat.Count);
    }

    private async Task<IEnumerable<object>> GetGroupLocation(
        IEnumerable<AccessLog> logs)
    {
        var group = GetGroupedStats(logs, log => log.IPAddress);
        var result = new List<object>();

        foreach (var ip in group)
        {
            var location = await _geoLocationHelper.GetCountryAsync(ip.Key);
            if (location != null)
            {
                result.Add(new
                {
                    location.Country,
                    location.City,
                    Flags = location.Flag,
                    ip.Count
                });
            }
            else
            {
                result.Add(new
                {
                    Country = "Unknown",
                    Count = ip.Count
                });
            }
        }

        return result;
    }
}

public class GroupStats
{
    public string Key { get; set; }
    public int Count { get; set; }
}
