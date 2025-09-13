import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { UploadResponse, ItemKind } from '../models/types';

@Injectable({
  providedIn: 'root'
})
export class UploadService {
  constructor(private http: HttpClient) {}

  uploadFile(
    albumId: number,
    file: File,
    kind: ItemKind,
    watermarkEnabled: boolean = false,
    watermarkText: string = ''
  ): Observable<HttpEvent<UploadResponse>> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('kind', kind);
    formData.append('watermarkEnabled', watermarkEnabled.toString());
    formData.append('watermarkText', watermarkText);

    return this.http.post<UploadResponse>(`${environment.apiUrl}/albums/${albumId}/items`, formData, {
      reportProgress: true,
      observe: 'events',
    });
  }
}