import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import {
  IonContent, IonHeader, IonTitle, IonToolbar, IonFab, IonFabButton,
  IonIcon, IonList, IonItem, IonLabel, IonCard, IonCardContent,
  IonButton, IonButtons, IonSpinner, IonText, AlertController, IonBackButton
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { addOutline, folderOpenOutline, logOutOutline, qrCodeOutline } from 'ionicons/icons';
import { ProjectsService } from '../../services/projects.service';
import { AuthService } from '../../services/auth.service';
import { Project, CreateProjectRequest } from '../../models/types';
import { ViewWillEnter } from '@ionic/angular';

@Component({
  selector: 'app-projects',
  templateUrl: './projects.page.html',
  styleUrls: ['./projects.page.scss'],
  standalone: true,
  imports: [
    CommonModule,
    IonContent, IonHeader, IonTitle, IonToolbar, IonFab, IonFabButton,
    IonIcon, IonList, IonItem, IonLabel, IonCard, IonCardContent,
    IonButton, IonButtons, IonSpinner, IonText, IonBackButton
  ]
})
export class ProjectsPage implements OnInit, ViewWillEnter {
  projects: Project[] = [];
  loading = true;
  error = '';

  constructor(
    private projectsService: ProjectsService,
    private authService: AuthService,
    private router: Router,
    private alertController: AlertController
  ) {
    addIcons({ addOutline, folderOpenOutline, logOutOutline, qrCodeOutline });
  }

  ngOnInit() {}

  ionViewWillEnter() {
    this.loadProjects();
  }

  loadProjects() {
    this.loading = true;
    this.error = '';
    
    this.projectsService.getProjects().subscribe({
      next: (projects) => {
        this.projects = projects;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load projects';
        this.loading = false;
        console.error(error);
      }
    });
  }

  async createProject() {
    const alert = await this.alertController.create({
      header: 'New Project',
      message: 'Enter a name for your new project',
      inputs: [
        {
          name: 'name',
          type: 'text',
          placeholder: 'Project name'
        }
      ],
      buttons: [
        {
          text: 'Cancel',
          role: 'cancel'
        },
        {
          text: 'Create',
          handler: (data) => {
            if (data.name?.trim()) {
              this.doCreateProject(data.name.trim());
            }
          }
        }
      ]
    });

    await alert.present();
  }

  private doCreateProject(name: string) {
    const request: CreateProjectRequest = { name };
    
    this.projectsService.createProject(request).subscribe({
      next: (project) => {
        this.projects.unshift(project);
        this.router.navigate(['/project', project.id]);
      },
      error: (error) => {
        console.error('Failed to create project:', error);
      }
    });
  }

  openProject(project: Project) {
    this.router.navigate(['/project', project.id]);
  }

  async logout() {
    await this.authService.logout();
    this.router.navigate(['/auth']);
  }

  navigateToScanner() {
    this.router.navigate(['/scanner']);
  }
}