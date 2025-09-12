export interface User {
  id: string;
  email: string;
  createdAt: string;
}

export interface AuthResponse {
  token: string;
  user: User;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
}

export interface Project {
  id: number;
  name: string;
  key: string;
  createdAt: string;
  albumCount: number;
}

export interface ProjectDetail {
  id: number;
  name: string;
  key: string;
  createdAt: string;
  albums: AlbumSummary[];
}

export interface CreateProjectRequest {
  name: string;
}

export enum AlbumVersion {
  RAW = 'RAW',
  FINAL = 'FINAL'
}

export enum AlbumStatus {
  ACTIVE = 'ACTIVE',
  ARCHIVED = 'ARCHIVED'
}

export interface AlbumSummary {
  id: number;
  slug: string;
  title: string;
  version: AlbumVersion;
  status: AlbumStatus;
  itemCount: number;
  createdAt: string;
}

export interface Album {
  id: number;
  projectId: number;
  slug: string;
  title: string;
  version: AlbumVersion;
  allowSelection: boolean;
  selectionLimit: number;
  status: AlbumStatus;
  createdAt: string;
  shareUrl: string;
  qrPngBase64: string;
}

export interface AlbumDetail {
  id: number;
  projectId: number;
  slug: string;
  title: string;
  version: AlbumVersion;
  allowSelection: boolean;
  selectionLimit: number;
  status: AlbumStatus;
  createdAt: string;
  items: AlbumItem[];
}

export interface CreateAlbumRequest {
  title: string;
  selectionLimit?: number;
}

export enum ItemKind {
  IMAGE = 'IMAGE',
  VIDEO = 'VIDEO'
}

export interface AlbumItem {
  id: number;
  projectId: number;
  albumId: number;
  serialNo: number;
  kind: ItemKind;
  srcUrl: string;
  wmUrl?: string;
  thumbUrl?: string;
  width?: number;
  height?: number;
  bytes?: number;
  sortOrder: number;
  createdAt: string;
}

export interface UploadResponse {
  itemId: number;
  projectId: number;
  albumId: number;
  serialNo: number;
  srcUrl: string;
  wmUrl?: string;
  thumbUrl?: string;
}

export interface SelectionSummary {
  itemId: number;
  serialNo: number;
  thumbUrl: string;
  picksCount: number;
}

export interface FinalizeAlbumRequest {
  itemIds: number[];
}