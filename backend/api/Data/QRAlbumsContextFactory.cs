using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

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
        var serverVersion = ServerVersion.AutoDetect(cs);
        builder.UseMySql(cs, serverVersion);
        return new QRAlbumsContext(builder.Options);
    }
}

