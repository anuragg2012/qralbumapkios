using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Options;
using QRAlbums.API.Options;

namespace QRAlbums.API.Services;

public sealed class BunnyService : IBunnyService
{
    private readonly HttpClient _http;
    private readonly BunnyOptions _opts;

    public BunnyService(HttpClient http, IOptions<BunnyOptions> opts)
    {
        _http = http;
        _opts = opts.Value;
        if (_http.BaseAddress is null && !string.IsNullOrWhiteSpace(_opts.StorageZone))
            _http.BaseAddress = new Uri($"https://sg.storage.bunnycdn.com/{_opts.StorageZone}/");
        if (!_http.DefaultRequestHeaders.Contains("AccessKey") && !string.IsNullOrWhiteSpace(_opts.AccessKey))
            _http.DefaultRequestHeaders.Add("AccessKey", _opts.AccessKey);
    }

    public async Task<bool> PutAsync(string path, Stream content, string? contentType = null)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("path required");
        using var sc = new StreamContent(content);
        if (!string.IsNullOrWhiteSpace(contentType))
            sc.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        var res = await _http.PutAsync(path.TrimStart('/'), sc);
        return res.IsSuccessStatusCode;
    }

    public string BuildCdnUrl(string path)
        => string.IsNullOrWhiteSpace(_opts.CdnBase) ? path : $"{_opts.CdnBase.TrimEnd('/')}/{path.TrimStart('/')}";
}

