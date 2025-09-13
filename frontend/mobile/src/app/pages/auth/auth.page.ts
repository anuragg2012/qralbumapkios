import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { 
  IonContent, IonHeader, IonTitle, IonToolbar, IonCard, IonCardContent, 
  IonItem, IonLabel, IonInput, IonButton, IonSegment, IonSegmentButton,
  IonSpinner, IonText, IonIcon
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { mailOutline, lockClosedOutline, personOutline } from 'ionicons/icons';
import { AuthService } from '../../services/auth.service';
import { LoginRequest, RegisterRequest } from '../../models/types';

@Component({
  selector: 'app-auth',
  templateUrl: './auth.page.html',
  styleUrls: ['./auth.page.scss'],
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    IonContent, IonHeader, IonTitle, IonToolbar, IonCard, IonCardContent,
    IonItem, IonLabel, IonInput, IonButton, IonSegment, IonSegmentButton,
    IonSpinner, IonText, IonIcon
  ]
})
export class AuthPage {
  segmentValue = 'login';
  email = '';
  password = '';
  confirmPassword = '';
  loading = false;
  error = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    addIcons({ mailOutline, lockClosedOutline, personOutline });
    if (this.authService.isAuthenticated) {
      this.router.navigate(['/projects']);
    }
  }

  async onSubmit() {
    if (this.loading) return;
    
    this.error = '';
    this.loading = true;

    try {
      if (this.segmentValue === 'login') {
        const loginRequest: LoginRequest = { email: this.email, password: this.password };
        await this.authService.login(loginRequest).toPromise();
      } else {
        if (this.password !== this.confirmPassword) {
          this.error = 'Passwords do not match';
          this.loading = false;
          return;
        }
        const registerRequest: RegisterRequest = { email: this.email, password: this.password };
        await this.authService.register(registerRequest).toPromise();
      }
      
      this.router.navigate(['/projects']);
    } catch (error: any) {
      this.error = error.error?.message || 'Authentication failed';
    } finally {
      this.loading = false;
    }
  }

  segmentChanged(event: any) {
    this.segmentValue = event.detail.value;
    this.error = '';
  }
}