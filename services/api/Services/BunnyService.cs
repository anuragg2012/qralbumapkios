using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using QRAlbums.API.Options;

namespace QRAlbums.API.Services;

public class BunnyService : IBunnyService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly BunnyOptions _options;

    public BunnyService(IConfiguration config, IOptions<BunnyOptions> options, HttpClient httpClient)
    {
        _config = config;
        _options = options.Value;
        _httpClient = httpClient;
    }

    public async Task<string> UploadFileAsync(byte[] content, string path, string contentType)
    {
        var storageZone = _config["Bunny:StorageZone"]!;
        var accessKey = _config["Bunny:AccessKey"]!;
        var url = $"https://storage.bunnycdn.com/{storageZone}/{path}";

        using var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Headers.Add("AccessKey", accessKey);
        request.Content = new ByteArrayContent(content);
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return GetCdnUrl(path);
    }

    public string GetCdnUrl(string path)
    {
        return $"{_options.PullZoneBaseUrl.TrimEnd('/')}/{path}";
    }

    public string GetImageUrl(string filePath, int? width = null, int? height = null, string? fit = null, int? quality = null, bool watermark = false, string? watermarkText = null)
    {
        if (!filePath.StartsWith('/')) filePath = "/" + filePath;

        var query = new Dictionary<string, string?>();

        if (_options.OptimizerEnabled)
        {
            if (width.HasValue) query["width"] = width.Value.ToString();
            if (height.HasValue) query["height"] = height.Value.ToString();
            if (!string.IsNullOrEmpty(fit)) query["fit"] = fit;
            query["quality"] = (quality ?? _options.DefaultQuality).ToString();
            query["format"] = "auto";

            if (watermark && _options.Watermark.Enabled)
            {
                var text = watermarkText ?? _options.Watermark.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    query["text"] = text;
                    query["textopacity"] = _options.Watermark.Opacity.ToString();
                    query["textsize"] = _options.Watermark.Size.ToString();
                }
            }
        }

        var pathWithQuery = QueryHelpers.AddQueryString(filePath, query!);
        var expires = DateTimeOffset.UtcNow.AddSeconds(_options.URLExpirySeconds);
        var signed = Sign(pathWithQuery, expires);
        return $"{_options.PullZoneBaseUrl.TrimEnd('/')}{signed}";
    }

    public string GetVideoUrl(string libraryId, string videoId)
    {
        if (string.IsNullOrEmpty(_options.StreamBaseUrl))
            return string.Empty;
        var path = $"/{libraryId}/{videoId}";
        var expires = DateTimeOffset.UtcNow.AddSeconds(_options.URLExpirySeconds);
        var signed = Sign(path, expires);
        return $"{_options.StreamBaseUrl!.TrimEnd('/')}{signed}";
    }

    public string Sign(string fullPathWithQuery, DateTimeOffset expiresAt)
    {
        var path = fullPathWithQuery.StartsWith('/') ? fullPathWithQuery : "/" + fullPathWithQuery;
        var expiryUnix = expiresAt.ToUnixTimeSeconds();
        using var md5 = MD5.Create();
        var input = Encoding.UTF8.GetBytes(_options.SecurityKey + path + expiryUnix);
        var hash = md5.ComputeHash(input);
        var token = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        var separator = path.Contains('?') ? '&' : '?';
        return $"{path}{separator}token={token}&expires={expiryUnix}";
    }
}
