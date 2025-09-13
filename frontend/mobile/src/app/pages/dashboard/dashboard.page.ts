import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  IonContent, IonHeader, IonTitle, IonToolbar, IonList, IonItem, IonLabel,
  IonToggle, IonCard, IonCardContent
} from '@ionic/angular/standalone';
import { ThemeService } from '../../services/theme.service';
import { ProjectsService } from '../../services/projects.service';
import { DashboardStats } from '../../models/types';
import { ViewWillEnter } from '@ionic/angular';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.page.html',
  styleUrls: ['./dashboard.page.scss'],
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    IonContent, IonHeader, IonTitle, IonToolbar, IonList, IonItem, IonLabel,
    IonToggle, IonCard, IonCardContent
  ]
})
export class DashboardPage implements ViewWillEnter {
  darkMode = false;
  stats: DashboardStats | null = null;

  constructor(
    private themeService: ThemeService,
    private projectsService: ProjectsService
  ) {
    this.darkMode = this.themeService.isDarkMode();
  }

  ionViewWillEnter() {
    this.loadStats();
  }

  toggleTheme(ev: CustomEvent) {
    const isDark = ev.detail.checked;
    this.themeService.setDarkMode(isDark);
    this.darkMode = isDark;
  }

  private loadStats() {
    this.projectsService.getStats().subscribe({
      next: (stats) => (this.stats = stats),
      error: (err) => console.error('Failed to load stats:', err)
    });
  }
}
