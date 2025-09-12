import { Injectable } from '@angular/core';
import { Observable, from } from 'rxjs';
import { map } from 'rxjs/operators';
import { Project, ProjectDetail, CreateProjectRequest, AlbumSummary } from '../models/types';
import { SupabaseService } from './supabase.service';

@Injectable({
  providedIn: 'root'
})
export class ProjectsService {
  constructor(private supabase: SupabaseService) {}

  createProject(request: CreateProjectRequest): Observable<Project> {
    return from(
      this.supabase.client
        .from('projects')
        .insert({ name: request.name })
        .select('*')
        .single()
    ).pipe(
      map(({ data, error }) => {
        if (error) throw error;
        return {
          id: data.id,
          name: data.name,
          key: data.key,
          createdAt: data.created_at,
          albumCount: 0
        } as Project;
      })
    );
  }

  getProjects(): Observable<Project[]> {
    return from(this.supabase.client.from('projects').select('*')).pipe(
      map(({ data, error }) => {
        if (error) throw error;
        return (data as any[]).map((p: any) => ({
          id: p.id,
          name: p.name,
          key: p.key,
          createdAt: p.created_at,
          albumCount: 0
        })) as Project[];
      })
    );
  }

  getProjectDetail(id: number): Observable<ProjectDetail> {
    return from(
      this.supabase.client
        .from('projects')
        .select('*, albums(*)')
        .eq('id', id)
        .single()
    ).pipe(
      map(({ data, error }) => {
        if (error) throw error;
        const project = data as any;
        return {
          id: project.id,
          name: project.name,
          key: project.key,
          createdAt: project.created_at,
          albums: (project.albums || []).map((a: any) => ({
            id: a.id,
            slug: a.slug,
            title: a.title,
            version: a.version,
            status: a.status,
            itemCount: 0,
            createdAt: a.created_at
          })) as AlbumSummary[]
        } as ProjectDetail;
      })
    );
  }
}