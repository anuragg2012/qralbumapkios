# QR Albums - Production Monorepo

A comprehensive QR-based album management system with mobile app and API backend.

## Architecture

- **Mobile App**: Ionic + Angular with Capacitor (Android/iOS)
- **API**: .NET 8 Minimal API with EF Core and MySQL
- **Media**: Bunny Storage + CDN + Stream integration
- **Auth**: JWT-based authentication
- **QR**: Dynamic QR code generation for album sharing

## Quick Start

### Prerequisites

- Node.js 18+ LTS
- .NET 8 SDK
- MySQL 8.0+
- Android Studio (for Android builds)
- Xcode (for iOS builds, macOS only)

### Environment Setup

1. **Clone and install dependencies**:
```bash
git clone <your-repo>
cd qr-albums-monorepo
npm install
```

2. **Set up Bunny CDN**:
   - Create Storage Zone at bunny.net
   - Get Access Key from Account Settings
   - Optional: Set up Stream Library for video uploads

3. **Configure API environment** (`services/api/.env`):
```bash
ConnectionStrings__Default="Server=localhost;Database=qralbums;Uid=root;Pwd=yourpassword;"
Jwt__Secret="your-super-secret-jwt-key-min-32-chars"
Bunny__StorageZone="your-storage-zone"
Bunny__AccessKey="your-bunny-access-key"
Bunny__CdnBase="https://your-zone.b-cdn.net"
Bunny__StreamLibraryId="your-stream-library-id"
Frontend__ViewerBaseUrl="https://viewer.qralbums.app"
```

4. **Configure Mobile environment** (`apps/mobile/src/environments/environment.ts`):
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000',
  viewerBaseUrl: 'https://viewer.qralbums.app'
};
```

5. **Set up database**:
```bash
cd services/api
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Development

**Start API server**:
```bash
npm run dev:api
# API runs on http://localhost:5000
```

**Start mobile app**:
```bash
npm run dev:mobile
# Mobile app runs on http://localhost:8100
```

**Sync mobile platforms**:
```bash
npm run sync:android
npm run sync:ios
```

## Production Builds

### Android Release (AAB/APK)

1. **Create keystore**:
```bash
keytool -genkey -v -keystore qr-albums-release-key.keystore -alias qr-albums -keyalg RSA -keysize 2048 -validity 10000
```

2. **Configure signing** (`apps/mobile/android/app/build.gradle`):
```gradle
android {
    signingConfigs {
        release {
            storeFile file('../../../qr-albums-release-key.keystore')
            storePassword 'your_keystore_password'
            keyAlias 'qr-albums'
            keyPassword 'your_key_password'
        }
    }
    buildTypes {
        release {
            signingConfig signingConfigs.release
            minifyEnabled true
            proguardFiles getDefaultProguardFile('proguard-android.txt'), 'proguard-rules.pro'
        }
    }
}
```

3. **Build release**:
```bash
cd apps/mobile
ionic build --prod
npx cap sync android

# For AAB (Google Play)
cd android
./gradlew bundleRelease

# For APK (direct distribution)
./gradlew assembleRelease
```

4. **Outputs**:
   - AAB: `apps/mobile/android/app/build/outputs/bundle/release/app-release.aab`
   - APK: `apps/mobile/android/app/build/outputs/apk/release/app-release.apk`

### iOS Release (IPA)

1. **Configure bundle ID and signing** in Xcode:
   - Open `apps/mobile/ios/App/App.xcworkspace`
   - Set Bundle Identifier: `com.earthinfosystems.qralbums`
   - Configure Team and Provisioning Profile

2. **Add camera permission** (`apps/mobile/ios/App/App/Info.plist`):
```xml
<key>NSCameraUsageDescription</key>
<string>This app needs access to camera to capture photos for albums</string>
```

3. **Build for release**:
```bash
cd apps/mobile
ionic build --prod
npx cap sync ios
npx cap open ios
```

4. **Archive in Xcode**:
   - Select "Any iOS Device" as target
   - Product → Archive
   - Distribute App → App Store Connect
   - Export IPA for manual upload

### Google Play Store Checklist

- [ ] AAB file generated and signed
- [ ] App Bundle Explorer tested
- [ ] Target API Level 34+ (Android 14)
- [ ] Privacy Policy URL provided
- [ ] Screenshots (phone, tablet, wear if applicable)
- [ ] Feature Graphic (1024x500)
- [ ] Content rating completed
- [ ] Pricing and distribution set

### Apple App Store Checklist

- [ ] IPA file generated and signed
- [ ] App Store Connect app record created
- [ ] Screenshots for all supported devices
- [ ] App metadata and description
- [ ] Privacy Policy URL
- [ ] App Review Information completed
- [ ] Age Rating completed
- [ ] Pricing and availability set

## API Endpoints

### Authentication
- `POST /auth/register` - Register new user
- `POST /auth/login` - Login user

### Projects
- `POST /projects` - Create project
- `GET /projects` - List user's projects
- `GET /projects/{id}` - Get project details

### Albums
- `POST /projects/{projectId}/albums` - Create RAW album
- `GET /albums/{id}` - Get album details
- `GET /albums/{id}/selections/summary` - Get selection summary
- `POST /albums/{id}/finalize` - Create FINAL album from selections

### Media Upload
- `POST /albums/{id}/items` - Upload media with optional watermark

### Public Viewer (for external viewer app)
- `GET /a/{slug}` - Get album data for public viewing
- `POST /a/{slug}/sessions` - Create viewer session
- `POST /a/{slug}/selections` - Submit selections

## Project Structure

```
qr-albums-monorepo/
├── apps/
│   └── mobile/                 # Ionic Angular mobile app
├── services/
│   └── api/                    # .NET 8 Minimal API
├── infrastructure/
│   ├── sql/                    # Database schema and migrations
│   ├── postman/               # API testing collections
│   └── docs/                  # Additional documentation
├── docker/                     # Docker configurations
└── docs/                      # Project documentation
```

## Serial Number System

Each uploaded media file gets a unique serial number within its project:
- Atomic assignment using `project_counters` table with row-level locking
- Guaranteed uniqueness within each project
- Used for organizing and referencing media items

## Watermarking

Images support optional text watermarking:
- Toggle on/off per upload
- User-provided text (e.g., "© EarthInfo Systems")
- Applied with SixLabors ImageSharp
- Semi-transparent overlay (bottom-right positioning)
- Original files preserved separately

## Bunny CDN Integration

### Image Optimization Examples
```
# Resize to 1200px width, auto format/quality
https://media.qralbums.app/image.jpg?width=1200&format=auto&quality=auto

# Create thumbnail 400x400
https://media.qralbums.app/image.jpg?width=400&height=400&crop=true
```

### Storage Structure
```
u/{ownerId}/p/{projectId}/a/{albumId}/
├── original/{guid}.jpg         # Original uploads
├── wm/{guid}.jpg              # Watermarked versions
└── thumb/{guid}.jpg           # Thumbnails (~800px)
```

## Contributing

1. Follow the established code style (ESLint + Prettier)
2. Add tests for new API endpoints
3. Update documentation for new features
4. Test on both Android and iOS before submitting

## Security Notes

- Original files not publicly accessible unless explicitly shared
- RAW albums prefer watermarked versions for public viewing
- Selection limits enforced server-side
- JWT tokens for API authentication
- Rate limiting on selection submissions

## License

© 2024 EarthInfo Systems. All rights reserved.