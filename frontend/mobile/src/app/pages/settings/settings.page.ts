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
  IonButtons,
  IonBackButton,
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
    IonNote,
    IonButtons,
    IonBackButton
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

  async changePassword() {
    const alert = await this.alertController.create({
      header: 'Change Password',
      inputs: [
        { name: 'currentPassword', type: 'password', placeholder: 'Current Password' },
        { name: 'newPassword', type: 'password', placeholder: 'New Password' },
        { name: 'confirmPassword', type: 'password', placeholder: 'Confirm New Password' }
      ],
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Change',
          handler: async (data) => {
            if (data.newPassword !== data.confirmPassword) {
              const mismatch = await this.alertController.create({
                message: 'Passwords do not match',
                buttons: ['OK']
              });
              await mismatch.present();
              return false;
            }
            try {
              await this.authService.changePassword(data.currentPassword, data.newPassword).toPromise();
              const success = await this.alertController.create({
                message: 'Password changed successfully',
                buttons: ['OK']
              });
              await success.present();
            } catch (err) {
              const failed = await this.alertController.create({
                message: err.error?.message || 'Failed to change password',
                buttons: ['OK']
              });
              await failed.present();
            }
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
