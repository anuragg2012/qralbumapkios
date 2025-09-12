using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using QRAlbums.API.Options;
using QRAlbums.API.Services;
using Xunit;

public class BunnyUrlSignerTests
{
    private readonly BunnyService _service;

    public BunnyUrlSignerTests()
    {
        var options = Options.Create(new BunnyOptions
        {
            PullZoneBaseUrl = "https://test.b-cdn.net",
            SecurityKey = "secret",
            URLExpirySeconds = 600
        });
        _service = new BunnyService(new ConfigurationBuilder().Build(), options, new HttpClient());
    }

    [Fact]
    public void Sign_GeneratesDifferentTokens_ForDifferentExpiry()
    {
        var path = "/image.jpg";
        var signed1 = _service.Sign(path, DateTimeOffset.FromUnixTimeSeconds(1000));
        var signed2 = _service.Sign(path, DateTimeOffset.FromUnixTimeSeconds(2000));
        Assert.NotEqual(signed1, signed2);
    }

    [Fact]
    public void Sign_PreservesQueryParameters()
    {
        var path = "/image.jpg?width=100";
        var expiry = DateTimeOffset.FromUnixTimeSeconds(1000);
        var signed = _service.Sign(path, expiry);
        Assert.Contains("width=100", signed);
        Assert.Contains("token=", signed);
        Assert.Contains("expires=1000", signed);
    }
}
