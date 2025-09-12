using QRAlbums.API.Models;

namespace QRAlbums.API.Services;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<User?> GetCurrentUserAsync(long userId);
    string GenerateJwtToken(User user);
}

public interface IProjectService
{
    Task<ProjectDto> CreateProjectAsync(long userId, CreateProjectRequest request);
    Task<List<ProjectDto>> GetUserProjectsAsync(long userId);
    Task<ProjectDetailDto?> GetProjectDetailAsync(long userId, long projectId);
    Task<long> AssignNextSerialAsync(long projectId);
}

public interface IAlbumService
{
    Task<AlbumDto> CreateAlbumAsync(long userId, long projectId, CreateAlbumRequest request);
    Task<AlbumDetailDto?> GetAlbumDetailAsync(long userId, long albumId);
    Task<List<SelectionSummaryDto>> GetSelectionSummaryAsync(long userId, long albumId);
    Task<AlbumDto> FinalizeAlbumAsync(long userId, long albumId, FinalizeAlbumRequest request);
}

public interface IMediaService
{
    Task<UploadResponse> UploadItemAsync(long albumId, IFormFile file, ItemKind kind, bool watermarkEnabled = false, string? watermarkText = null);
}

public interface ISelectionService
{
    Task<ViewerAlbumDto?> GetViewerAlbumAsync(string slug);
    Task<CreateSessionResponse> CreateSessionAsync(string slug);
    Task<SubmitSelectionsResponse> SubmitSelectionsAsync(string slug, SubmitSelectionsRequest request);
}
public interface IQRService
{
    string GenerateQRCodeBase64(string url, int pixelsPerModule = 20);
}