import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Share } from '@capacitor/share';
import {
  IonContent, IonHeader, IonTitle, IonToolbar, IonBackButton, IonButtons,
  IonButton, IonIcon, IonCard, IonCardContent, IonSpinner, IonText,
  IonFab, IonFabButton, IonGrid, IonRow, IonCol, IonImg, IonBadge, AlertController
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { shareOutline, cloudUploadOutline, eyeOutline, checkmarkCircleOutline, imagesOutline, trashOutline } from 'ionicons/icons';
import { AlbumsService } from '../../services/albums.service';
import { AlbumDetail, AlbumVersion, ItemKind } from '../../models/types';

@Component({
  selector: 'app-album-detail',
  templateUrl: './album-detail.page.html',
  styleUrls: ['./album-detail.page.scss'],
  standalone: true,
  imports: [
    CommonModule,
    IonContent, IonHeader, IonTitle, IonToolbar, IonBackButton, IonButtons,
    IonButton, IonIcon, IonCard, IonCardContent, IonSpinner, IonText,
    IonFab, IonFabButton, IonGrid, IonRow, IonCol, IonImg, IonBadge
  ]
})
export class AlbumDetailPage implements OnInit {
  albumId!: number;
  album: AlbumDetail | null = null;
  loading = true;
  error = '';
  AlbumVersion = AlbumVersion;
  ItemKind = ItemKind;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private albumsService: AlbumsService,
    private alertController: AlertController
  ) {
    addIcons({ shareOutline, cloudUploadOutline, eyeOutline, checkmarkCircleOutline, imagesOutline, trashOutline });
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.albumId = parseInt(id, 10);
      this.loadAlbum();
    }
  }

  loadAlbum() {
    this.loading = true;
    this.error = '';

    this.albumsService.getAlbumDetail(this.albumId).subscribe({
      next: (album) => {
        this.album = album;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load album details';
        this.loading = false;
        console.error(error);
      }
    });
  }

  async shareAlbum() {
    if (!this.album) return;
    
    // Get album sharing info first
    this.albumsService.getAlbumDetail(this.albumId).subscribe(async (album) => {
      const shareUrl = `https://viewer.qralbums.app/a/${album.slug}`;
      
      await Share.share({
        title: album.title,
        text: `Check out this photo album: ${album.title}`,
        url: shareUrl,
        dialogTitle: 'Share Album'
      });
    });
  }

  navigateToUpload() {
    this.router.navigate(['/upload', this.albumId]);
  }

  getVersionBadgeColor(): string {
    return this.album?.version === AlbumVersion.RAW ? 'warning' : 'success';
  }

  getVersionIcon(): string {
    return this.album?.version === AlbumVersion.RAW ? 'eye-outline' : 'checkmark-circle-outline';
  }

  getDisplayUrl(item: any): string {
    // For RAW albums, prefer watermarked version
    if (this.album?.version === AlbumVersion.RAW && item.wmUrl) {
      return item.wmUrl;
    }
    return item.thumbUrl || item.srcUrl;
  }

  async deleteAlbum() {
    if (!this.album) return;

    const alert = await this.alertController.create({
      header: 'Delete Album',
      message: 'Delete this album and all its media? This cannot be undone.',
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Delete',
          role: 'destructive',
          handler: () => {
            this.albumsService.deleteAlbum(this.albumId).subscribe({
              next: () => this.router.navigate(['/projects', this.album!.projectId]),
              error: (err) => console.error('Failed to delete album:', err)
            });
          }
        }
      ]
    });

    await alert.present();
  }
}