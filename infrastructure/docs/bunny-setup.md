# Bunny CDN Setup Guide

This guide covers setting up Bunny Storage, CDN, and Stream for the QR Albums project.

## 1. Bunny Storage Setup

### Create Storage Zone
1. Go to [Bunny CDN Dashboard](https://panel.bunny.net/)
2. Navigate to **Storage** → **Storage Zones**
3. Click **Add Storage Zone**
4. Configure:
   - **Name**: `qr-albums-media` (or your preferred name)
   - **Region**: Choose closest to your users
   - **Replication**: Enable if needed for global distribution

### Get Access Key
1. Go to **Account** → **Account Settings**
2. Navigate to **FTP & API Access**
3. Copy your **Storage API Key**

## 2. Bunny CDN Pull Zone

### Create Pull Zone
1. Navigate to **CDN** → **Pull Zones**
2. Click **Add Pull Zone**
3. Configure:
   - **Name**: `qr-albums-cdn`
   - **Origin URL**: Your storage zone URL (e.g., `https://storage.bunnycdn.com/qr-albums-media/`)
   - **Custom CDN Hostname**: `media.qralbums.app` (optional)

### CDN Settings
- **Cache Settings**: 
  - Browser Cache: 1 day
  - CDN Cache: 7 days
- **Security**:
  - Enable Token Authentication (optional)
  - Set up allowed referrers if needed

## 3. Bunny Stream (Optional)

### Create Stream Library
1. Navigate to **Stream** → **Stream Libraries**
2. Click **Add Stream Library**
3. Configure:
   - **Name**: `qr-albums-videos`
   - **Allowed Origins**: Your domains
   - **Player Options**: Configure as needed

### Get Library ID
- Copy the **Library ID** from the Stream library dashboard

## 4. Environment Configuration

Update your API `.env` file:

```bash
# Bunny CDN Configuration
Bunny__StorageZone=qr-albums-media
Bunny__AccessKey=your-storage-api-key-here
Bunny__CdnBase=https://qr-albums-cdn.b-cdn.net
Bunny__StreamLibraryId=your-stream-library-id

# Alternative with custom domain
# Bunny__CdnBase=https://media.qralbums.app
```

## 5. File Organization

The API organizes files in this structure:
```
u/{userId}/p/{projectId}/a/{albumId}/
├── original/{guid}.jpg     # Original uploads
├── wm/{guid}.jpg          # Watermarked versions  
├── thumb/{guid}.jpg       # Thumbnails
└── video/{guid}.mp4       # Video uploads
```

## 6. Image Optimization

Bunny CDN provides automatic image optimization. Use these URL parameters:

```
# Basic optimization
https://media.qralbums.app/image.jpg?width=1200&format=auto&quality=auto

# Thumbnail with crop
https://media.qralbums.app/image.jpg?width=400&height=400&crop=center

# WebP conversion
https://media.qralbums.app/image.jpg?format=webp

# Quality control
https://media.qralbums.app/image.jpg?quality=85
```

## 7. Security Considerations

### Token Authentication (Recommended for Production)
1. Enable **Token Authentication** in Pull Zone settings
2. Generate signing key
3. Implement token generation in your API:

```csharp
public string GenerateSecureUrl(string path, int expiryMinutes = 60)
{
    var expiry = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes).ToUnixTimeSeconds();
    var toSign = $"{signingKey}{path}{expiry}";
    var hash = SHA256.HashData(Encoding.UTF8.GetBytes(toSign));
    var token = Convert.ToBase64String(hash);
    
    return $"{cdnBase}{path}?token={token}&expires={expiry}";
}
```

### CORS Configuration
Set up CORS headers in Pull Zone settings:
- **Access-Control-Allow-Origin**: Your app domains
- **Access-Control-Allow-Methods**: GET, OPTIONS
- **Access-Control-Allow-Headers**: Content-Type

## 8. Cost Optimization

### Storage Optimization
- Use appropriate image quality (85-90% for photos)
- Generate multiple thumbnail sizes
- Consider WebP format for supported browsers

### CDN Optimization
- Set appropriate cache headers
- Use compression (Brotli/Gzip)
- Enable HTTP/2 and HTTP/3

### Monitoring
- Set up usage alerts in Bunny dashboard
- Monitor bandwidth and storage usage
- Use analytics to optimize content delivery

## 9. Testing Configuration

Test your setup:

```bash
# Test storage upload (replace with your zone and key)
curl -X PUT \
  "https://storage.bunnycdn.com/your-zone/test.txt" \
  -H "AccessKey: your-access-key" \
  -d "Hello World"

# Test CDN access
curl "https://your-zone.b-cdn.net/test.txt"
```

## 10. Troubleshooting

### Common Issues
- **403 Forbidden**: Check AccessKey and permissions
- **CORS Errors**: Verify CORS settings in Pull Zone
- **Slow Loading**: Check CDN cache settings and origin configuration
- **High Costs**: Review usage patterns and optimize file sizes

### Support Resources
- [Bunny CDN Documentation](https://docs.bunny.net/)
- [Bunny CDN Community](https://community.bunny.net/)
- Support tickets through Bunny dashboard

## Production Checklist

- [ ] Storage Zone created and configured
- [ ] Pull Zone configured with proper caching
- [ ] Custom domain configured (if using)
- [ ] SSL certificate configured
- [ ] CORS settings configured
- [ ] Token authentication enabled (recommended)
- [ ] Usage alerts configured
- [ ] Image optimization parameters tested
- [ ] Backup strategy implemented