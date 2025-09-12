import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Project, ProjectDetail, CreateProjectRequest } from '../models/types';

@Injectable({
  providedIn: 'root'
})
export class ProjectsService {
  constructor(private http: HttpClient) {}

  createProject(request: CreateProjectRequest): Observable<Project> {
    return this.http.post<Project>(`${environment.apiUrl}/projects`, request);
  }

  getProjects(): Observable<Project[]> {
    return this.http.get<Project[]>(`${environment.apiUrl}/projects`);
  }

  getProjectDetail(id: number): Observable<ProjectDetail> {
    return this.http.get<ProjectDetail>(`${environment.apiUrl}/projects/${id}`);
  }
}