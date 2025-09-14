import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  IonContent, IonHeader, IonTitle, IonToolbar, IonList, IonItem, IonLabel,
  IonCard, IonCardContent, IonButtons, IonButton, IonIcon
} from '@ionic/angular/standalone';
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
    IonCard, IonCardContent, IonButtons, IonButton, IonIcon
  ]
})
export class DashboardPage implements ViewWillEnter {
  stats: DashboardStats | null = null;

  constructor(
    private projectsService: ProjectsService
  ) {}

  ionViewWillEnter() {
    this.loadStats();
  }

  private loadStats() {
    this.projectsService.getStats().subscribe({
      next: (stats) => (this.stats = stats),
      error: (err) => console.error('Failed to load stats:', err)
    });
  }
}
