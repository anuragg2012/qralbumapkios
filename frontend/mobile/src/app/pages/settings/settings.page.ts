import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Preferences } from '@capacitor/preferences';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonList,
  IonItem,
  IonLabel,
  IonNote,
  AlertController
} from '@ionic/angular/standalone';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.page.html',
  styleUrls: ['./settings.page.scss'],
  standalone: true,
  imports: [
    CommonModule,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonList,
    IonItem,
    IonLabel,
    IonNote
  ]
})
export class SettingsPage {
  watermarkText = '';

  constructor(
    private alertController: AlertController,
    private authService: AuthService,
    private router: Router
  ) {
    this.loadWatermark();
  }

  async loadWatermark() {
    const { value } = await Preferences.get({ key: 'watermark_text' });
    this.watermarkText = value || '';
  }

  async setWatermark() {
    const alert = await this.alertController.create({
      header: 'Set Watermark',
      inputs: [
        {
          name: 'watermark',
          type: 'text',
          value: this.watermarkText,
          placeholder: 'Watermark text'
        }
      ],
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Save',
          handler: async (data) => {
            this.watermarkText = data.watermark;
            await Preferences.set({ key: 'watermark_text', value: this.watermarkText });
          }
        }
      ]
    });

    await alert.present();
  }

  async logout() {
    await this.authService.logout();
    this.router.navigate(['/auth']);
  }
}
