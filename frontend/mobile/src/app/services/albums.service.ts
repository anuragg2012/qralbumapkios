import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, from } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { Album, AlbumDetail, CreateAlbumRequest, SelectionSummary, FinalizeAlbumRequest } from '../models/types';
import { SupabaseService } from './supabase.service';

@Injectable({
  providedIn: 'root'
})
export class AlbumsService {
  constructor(private http: HttpClient, private supabase: SupabaseService) {}

  createAlbum(projectId: number, request: CreateAlbumRequest): Observable<Album> {
    const slug = Math.random().toString(36).substring(2, 10);
    return from(
      this.supabase.client
        .from('albums')
        .insert({
          project_id: projectId,
          title: request.title,
          selection_limit: request.selectionLimit ?? 0,
          slug
        })
        .select('*')
        .single()
    ).pipe(
      map(({ data, error }) => {
        if (error) throw error;
        return {
          id: data.id,
          projectId: data.project_id,
          slug: data.slug,
          title: data.title,
          version: data.version,
          allowSelection: data.allow_selection,
          selectionLimit: data.selection_limit,
          status: data.status,
          createdAt: data.created_at,
          shareUrl: `${environment.viewerBaseUrl}/a/${data.slug}`,
          qrPngBase64: ''
        } as Album;
      })
    );
  }

  getAlbumDetail(albumId: number): Observable<AlbumDetail> {
    return from(
      this.supabase.client
        .from('albums')
        .select('*, album_items(*)')
        .eq('id', albumId)
        .single()
    ).pipe(
      map(({ data, error }) => {
        if (error) throw error;
        const album = data as any;
        return {
          id: album.id,
          projectId: album.project_id,
          slug: album.slug,
          title: album.title,
          version: album.version,
          allowSelection: album.allow_selection,
          selectionLimit: album.selection_limit,
          status: album.status,
          createdAt: album.created_at,
          items: (album.album_items || []).map((i: any) => ({
            id: i.id,
            projectId: i.project_id,
            albumId: i.album_id,
            serialNo: i.serial_no,
            kind: i.kind,
            srcUrl: i.src_url,
            wmUrl: i.wm_url || undefined,
            thumbUrl: i.thumb_url || undefined,
            width: i.width || undefined,
            height: i.height || undefined,
            bytes: i.bytes || undefined,
            sortOrder: i.sort_order,
            createdAt: i.created_at
          }))
        } as AlbumDetail;
      })
    );
  }

  getSelectionSummary(albumId: number): Observable<SelectionSummary[]> {
    return this.http.get<SelectionSummary[]>(`${environment.apiUrl}/albums/${albumId}/selections/summary`);
  }

  finalizeAlbum(albumId: number, request: FinalizeAlbumRequest): Observable<Album> {
    return this.http.post<Album>(`${environment.apiUrl}/albums/${albumId}/finalize`, request);
  }
}