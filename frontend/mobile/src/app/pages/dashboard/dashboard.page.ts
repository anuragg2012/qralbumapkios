import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  IonContent, IonHeader, IonTitle, IonToolbar, IonList, IonItem, IonLabel,
  IonToggle
} from '@ionic/angular/standalone';
import { ThemeService } from '../../services/theme.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.page.html',
  styleUrls: ['./dashboard.page.scss'],
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    IonContent, IonHeader, IonTitle, IonToolbar, IonList, IonItem, IonLabel,
    IonToggle
  ]
})
export class DashboardPage {
  darkMode = false;

  constructor(private themeService: ThemeService) {
    this.darkMode = this.themeService.isDarkMode();
  }

  toggleTheme(ev: CustomEvent) {
    const isDark = ev.detail.checked;
    this.themeService.setDarkMode(isDark);
    this.darkMode = isDark;
  }
}
