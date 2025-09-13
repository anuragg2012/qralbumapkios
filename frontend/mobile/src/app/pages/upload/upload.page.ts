import { Component, OnInit, TrackByFunction, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Camera, CameraResultType, CameraSource } from '@capacitor/camera';
import { HttpEventType } from '@angular/common/http';
import {
  IonContent, IonHeader, IonTitle, IonToolbar, IonBackButton, IonButtons,
  IonList, IonItem, IonLabel, IonButton, IonIcon, IonToggle, IonInput,
  IonCard, IonCardContent, IonProgressBar, IonText, ActionSheetController,
  IonLoading
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { cameraOutline, imagesOutline, videocamOutline, cloudUploadOutline } from 'ionicons/icons';
import { UploadService } from '../../services/upload.service';
import { ItemKind } from '../../models/types';

interface UploadItem {
  id: string;
  file: File;
  kind: ItemKind;
  watermarkEnabled: boolean;
  watermarkText: string;
  uploading: boolean;
  uploaded: boolean;
  progress: number;
  error?: string;
}

@Component({
  selector: 'app-upload',
  templateUrl: './upload.page.html',
  styleUrls: ['./upload.page.scss'],
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    IonContent, IonHeader, IonTitle, IonToolbar, IonBackButton, IonButtons,
    IonList, IonItem, IonLabel, IonButton, IonIcon, IonToggle, IonInput,
    IonCard, IonCardContent, IonProgressBar, IonText, IonLoading
  ]
})
export class UploadPage implements OnInit {
  albumId!: number;
  items: UploadItem[] = [];
  uploading = false;
  trackByItemId!: TrackByFunction<UploadItem>;
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private uploadService: UploadService,
    private actionSheetController: ActionSheetController
  ) {
    addIcons({ cameraOutline, imagesOutline, videocamOutline, cloudUploadOutline });
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('albumId');
    if (id) {
      this.albumId = parseInt(id, 10);
    }
  }

  async addMedia() {
    const actionSheet = await this.actionSheetController.create({
      header: 'Select Media Source',
      buttons: [
        {
          text: 'Camera',
          icon: 'camera-outline',
          handler: () => {
            this.takePicture();
          }
        },
        {
          text: 'Photo Library',
          icon: 'images-outline',
          handler: () => {
            this.selectFromGallery();
          }
        },
        {
          text: 'Cancel',
          icon: 'close',
          role: 'cancel'
        }
      ]
    });

    await actionSheet.present();
  }

  async takePicture() {
    try {
      const image = await Camera.getPhoto({
        quality: 90,
        allowEditing: false,
        resultType: CameraResultType.DataUrl,
        source: CameraSource.Camera
      });

      if (image.dataUrl) {
        this.addFileFromDataUrl(image.dataUrl, 'image.jpg', ItemKind.IMAGE);
      }
    } catch (error) {
      console.error('Error taking picture:', error);
    }
  }

  async selectFromGallery() {
    this.fileInput.nativeElement.click();
  }

  onFilesSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files) return;

    const files = Array.from(input.files);
    const remaining = 100 - this.items.length;
    files.slice(0, remaining).forEach(file => {
      if (!file.type.startsWith('image')) return;
      const item: UploadItem = {
        id: Date.now().toString() + Math.random(),
        file,
        kind: ItemKind.IMAGE,
        watermarkEnabled: false,
        watermarkText: '© EarthInfo Systems',
        uploading: false,
        uploaded: false,
        progress: 0
      };
      this.items.push(item);
    });

    input.value = '';
  }

  private addFileFromDataUrl(dataUrl: string, filename: string, kind: ItemKind) {
    // Convert data URL to File
    const arr = dataUrl.split(',');
    const mime = arr[0].match(/:(.*?);/)![1];
    const bstr = atob(arr[1]);
    let n = bstr.length;
    const u8arr = new Uint8Array(n);

    while (n--) {
      u8arr[n] = bstr.charCodeAt(n);
    }

    const file = new File([u8arr], filename, { type: mime });
    
    if (this.items.length >= 100) return;

    const item: UploadItem = {
      id: Date.now().toString(),
      file,
      kind,
      watermarkEnabled: false,
      watermarkText: '© EarthInfo Systems',
      uploading: false,
      uploaded: false,
      progress: 0
    };

    this.items.push(item);
  }

  removeItem(itemId: string) {
    this.items = this.items.filter(item => item.id !== itemId);
  }

  async uploadAll() {
    if (this.items.length === 0 || this.uploading) return;

    this.uploading = true;

    for (const item of this.items) {
      if (item.uploaded) continue;

      item.uploading = true;
      item.progress = 0;
      item.error = undefined;

      try {
        await this.uploadItem(item);
        item.uploaded = true;
      } catch (error) {
        item.error = 'Upload failed';
        console.error('Upload error:', error);
      } finally {
        item.uploading = false;
      }
    }

    this.uploading = false;

    // Navigate back if all uploads successful
    if (this.items.every(item => item.uploaded)) {
      this.router.navigate(['/albums', this.albumId]);
    }
  }

  private async uploadItem(item: UploadItem): Promise<void> {
    return new Promise((resolve, reject) => {
      this.uploadService.uploadFile(
        this.albumId,
        item.file,
        item.kind,
        item.watermarkEnabled,
        item.watermarkText
      ).subscribe({
        next: (event) => {
          if (event.type === HttpEventType.UploadProgress) {
            const total = event.total ?? item.file.size;
            item.progress = Math.round(100 * event.loaded / total);
          } else if (event.type === HttpEventType.Response) {
            item.progress = 100;
            resolve();
          }
        },
        error: (error) => {
          reject(error);
        }
      });
    });
  }

  getFileSize(bytes: number): string {
    const sizes = ['B', 'KB', 'MB', 'GB'];
    if (bytes === 0) return '0 B';
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return Math.round(bytes / Math.pow(1024, i) * 100) / 100 + ' ' + sizes[i];
  }

  canUpload(): boolean {
    return this.items.length > 0 && !this.uploading && this.items.some(item => !item.uploaded);
  }

  get overallProgress(): number {
    const total = this.items.length * 100;
    const uploaded = this.items.reduce((acc, item) => acc + item.progress, 0);
    return total ? Math.round(uploaded / total) : 0;
  }
}