import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { 
  IonContent, IonHeader, IonTitle, IonToolbar, IonBackButton, IonButtons,
  IonFab, IonFabButton, IonIcon, IonList, IonItem, IonLabel, IonCard, 
  IonCardContent, IonSpinner, IonText, IonBadge, AlertController,
  IonButton
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { addOutline, imageOutline, videocamOutline, eyeOutline, checkmarkCircleOutline, createOutline } from 'ionicons/icons';
import { ProjectsService } from '../../services/projects.service';
import { AlbumsService } from '../../services/albums.service';
import { ProjectDetail, AlbumSummary, CreateAlbumRequest, AlbumVersion, UpdateProjectRequest } from '../../models/types';

@Component({
  selector: 'app-project-detail',
  templateUrl: './project-detail.page.html',
  styleUrls: ['./project-detail.page.scss'],
  standalone: true,
  imports: [
    CommonModule,
    IonContent, IonHeader, IonTitle, IonToolbar, IonBackButton, IonButtons,IonButton,
    IonFab, IonFabButton, IonIcon, IonList, IonCard,
    IonCardContent, IonSpinner, IonText, IonBadge
  ]
})
export class ProjectDetailPage implements OnInit {
  projectId!: number;
  project: ProjectDetail | null = null;
  loading = true;
  error = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private projectsService: ProjectsService,
    private albumsService: AlbumsService,
    private alertController: AlertController
  ) {
    addIcons({ addOutline, imageOutline, videocamOutline, eyeOutline, checkmarkCircleOutline, createOutline });
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.projectId = parseInt(id, 10);
      this.loadProject();
    }
  }

  loadProject() {
    this.loading = true;
    this.error = '';

    this.projectsService.getProjectDetail(this.projectId).subscribe({
      next: (project) => {
        this.project = project;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load project details';
        this.loading = false;
        console.error(error);
      }
    });
  }

  async editProject() {
    if (!this.project) return;

    const alert = await this.alertController.create({
      header: 'Edit Project',
      inputs: [
        {
          name: 'name',
          type: 'text',
          value: this.project.name,
        }
      ],
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Save',
          handler: (data) => {
            const req: UpdateProjectRequest = { name: data.name };
            this.projectsService.updateProject(this.projectId, req).subscribe({
              next: (proj) => {
                if (this.project) this.project.name = proj.name;
              },
              error: (err) => console.error('Failed to update project:', err)
            });
          }
        }
      ]
    });

    await alert.present();
  }

  async createAlbum() {
    const alert = await this.alertController.create({
      header: 'New RAW Album',
      message: 'Create a new album for collecting photos',
      inputs: [
        {
          name: 'title',
          type: 'text',
          placeholder: 'Album title'
        },
        {
          name: 'selectionLimit',
          type: 'number',
          placeholder: 'Selection limit (0 = unlimited)',
          value: 0
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
            if (data.title?.trim()) {
              this.doCreateAlbum(data.title.trim(), parseInt(data.selectionLimit) || 0);
            }
          }
        }
      ]
    });

    await alert.present();
  }

  private doCreateAlbum(title: string, selectionLimit: number) {
    const request: CreateAlbumRequest = { title, selectionLimit };
    
    this.albumsService.createAlbum(this.projectId, request).subscribe({
      next: (album) => {
        this.router.navigate(['/albums', album.id]);
      },
      error: (error) => {
        console.error('Failed to create album:', error);
      }
    });
  }

  openAlbum(album: AlbumSummary) {
    this.router.navigate(['/albums', album.id]);
  }

  getVersionBadgeColor(version: AlbumVersion): string {
    return version === AlbumVersion.RAW ? 'warning' : 'success';
  }

  getVersionIcon(version: AlbumVersion): string {
    return version === AlbumVersion.RAW ? 'eye-outline' : 'checkmark-circle-outline';
  }
}