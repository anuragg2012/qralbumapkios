import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full',
  },
  {
    path: 'auth',
    loadComponent: () => import('./pages/auth/auth.page').then(m => m.AuthPage)
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/dashboard/dashboard.page').then(m => m.DashboardPage)
  },
  {
    path: 'project',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/projects/projects.page').then(m => m.ProjectsPage)
  },
  {
    path: 'project/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/project-detail/project-detail.page').then(m => m.ProjectDetailPage)
  },
  {
    path: 'albums/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/album-detail/album-detail.page').then(m => m.AlbumDetailPage)
  },
  {
    path: 'upload/:albumId',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/upload/upload.page').then(m => m.UploadPage)
  },
  {
    path: 'scanner',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/scanner/scanner.page').then(m => m.ScannerPage)
  },
  {
    path: 'settings',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/settings/settings.page').then(m => m.SettingsPage)
  },
  {
    path: 'plans',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/plans/plans.page').then(m => m.PlansPage)
  }
];