using System.IO;
using System.Threading.Tasks;

namespace QRAlbums.API.Services;

public interface IBunnyService
{
    Task<bool> PutAsync(string path, Stream content, string? contentType = null);
    string BuildCdnUrl(string path);
}

