import { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'com.earthinfosystems.qralbums',
  appName: 'QR Albums',
  webDir: 'dist/qr-albums-mobile',
  server: {
    androidScheme: 'https'
  },
  plugins: {
    Camera: {
      permissions: ['camera', 'photos']
    },
    BarcodeScanner: {
      cameraDirection: 'back'
    }
  }
};

export default config;