import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { Album, AlbumDetail, CreateAlbumRequest, SelectionSummary, FinalizeAlbumRequest } from '../models/types';

@Injectable({
  providedIn: 'root'
})
export class AlbumsService {
  constructor(private http: HttpClient) {}

  createAlbum(projectId: number, request: CreateAlbumRequest): Observable<Album> {
    return this.http
      .post<Album>(`${environment.apiUrl}/projects/${projectId}/albums`, request)
      .pipe(
        map((album) => ({
          ...album,
          shareUrl: `${environment.viewerBaseUrl}/a/${album.slug}`,
          qrPngBase64: '',
        }))
      );
  }

  getAlbumDetail(albumId: number): Observable<AlbumDetail> {
    return this.http.get<AlbumDetail>(`${environment.apiUrl}/albums/${albumId}`);
  }

  getSelectionSummary(albumId: number): Observable<SelectionSummary[]> {
    return this.http.get<SelectionSummary[]>(`${environment.apiUrl}/albums/${albumId}/selections/summary`);
  }

  finalizeAlbum(albumId: number, request: FinalizeAlbumRequest): Observable<Album> {
    return this.http.post<Album>(`${environment.apiUrl}/albums/${albumId}/finalize`, request);
  }

  deleteAlbum(albumId: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/albums/${albumId}`);
  }
}