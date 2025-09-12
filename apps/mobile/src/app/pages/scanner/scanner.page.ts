import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { BarcodeScanner } from '@capacitor-community/barcode-scanner';
import { 
  IonContent, IonHeader, IonTitle, IonToolbar, IonBackButton, IonButtons,
  IonButton, IonIcon, IonText, AlertController
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { qrCodeOutline, flashlightOutline } from 'ionicons/icons';

@Component({
  selector: 'app-scanner',
  templateUrl: './scanner.page.html',
  styleUrls: ['./scanner.page.scss'],
  standalone: true,
  imports: [
    CommonModule,
    IonContent, IonHeader, IonTitle, IonToolbar, IonBackButton, IonButtons,
    IonButton, IonIcon, IonText
  ]
})
export class ScannerPage {
  scanning = false;

  constructor(
    private router: Router,
    private alertController: AlertController
  ) {
    addIcons({ qrCodeOutline, flashlightOutline });
  }

  async startScan() {
    // Check permission
    const allowed = await BarcodeScanner.checkPermission({ force: true });

    if (allowed.granted) {
      BarcodeScanner.hideBackground();
      this.scanning = true;
      
      const result = await BarcodeScanner.startScan();
      
      this.scanning = false;
      BarcodeScanner.showBackground();

      if (result.hasContent) {
        await this.handleScannedUrl(result.content);
      }
    } else {
      const alert = await this.alertController.create({
        header: 'Permission Required',
        message: 'Camera permission is required to scan QR codes.',
        buttons: ['OK']
      });
      await alert.present();
    }
  }

  stopScan() {
    BarcodeScanner.stopScan();
    BarcodeScanner.showBackground();
    this.scanning = false;
  }

  private async handleScannedUrl(url: string) {
    // Check if it's a QR Albums viewer URL
    const albumMatch = url.match(/\/a\/([a-z0-9]+)$/);
    
    if (albumMatch) {
      const slug = albumMatch[1];
      const alert = await this.alertController.create({
        header: 'Album Found',
        message: `Open album "${slug}" in the viewer?`,
        buttons: [
          {
            text: 'Cancel',
            role: 'cancel'
          },
          {
            text: 'Open',
            handler: () => {
              // In a real app, this would open the viewer app or website
              window.open(url, '_blank');
            }
          }
        ]
      });
      await alert.present();
    } else {
      const alert = await this.alertController.create({
        header: 'QR Code Scanned',
        message: `Content: ${url}`,
        buttons: ['OK']
      });
      await alert.present();
    }
  }
}