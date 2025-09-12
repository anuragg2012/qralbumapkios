using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace QRAlbums.API.Data;

public sealed class QRAlbumsContextFactory : IDesignTimeDbContextFactory<QRAlbumsContext>
{
    public QRAlbumsContext CreateDbContext(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var basePath = Directory.GetCurrentDirectory();

        var cfg = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cs = cfg.GetConnectionString("Default")
                 ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                 ?? throw new InvalidOperationException("ConnectionStrings:Default missing");

        var builder = new DbContextOptionsBuilder<QRAlbumsContext>();
        builder.UseNpgsql(cs);
        return new QRAlbumsContext(builder.Options);
    }
}

