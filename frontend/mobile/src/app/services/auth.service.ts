import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, from } from 'rxjs';
import { map } from 'rxjs/operators';
import { Preferences } from '@capacitor/preferences';
import { AuthResponse, LoginRequest, RegisterRequest, User } from '../models/types';
import { SupabaseService } from './supabase.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private tokenSubject = new BehaviorSubject<string | null>(null);

  public currentUser$ = this.currentUserSubject.asObservable();
  public token$ = this.tokenSubject.asObservable();

  constructor(private supabase: SupabaseService) {
    this.loadStoredAuth();
  }

  private async loadStoredAuth() {
    try {
      const { value: token } = await Preferences.get({ key: 'auth_token' });
      const { value: userJson } = await Preferences.get({ key: 'current_user' });

      if (token && userJson) {
        const user = JSON.parse(userJson);
        this.tokenSubject.next(token);
        this.currentUserSubject.next(user);
      }
    } catch (error) {
      console.error('Error loading stored auth:', error);
    }
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return from(this.supabase.client.auth.signUp({
      email: request.email,
      password: request.password
    })).pipe(
      map(({ data, error }) => {
        if (error || !data.user || !data.session) {
          throw error || new Error('Registration failed');
        }
        const user: User = {
          id: data.user.id,
          email: data.user.email!,
          createdAt: data.user.created_at
        };
        const response: AuthResponse = {
          token: data.session.access_token,
          user
        };
        this.setAuth(response);
        return response;
      })
    );
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return from(this.supabase.client.auth.signInWithPassword({
      email: request.email,
      password: request.password
    })).pipe(
      map(({ data, error }) => {
        if (error || !data.user || !data.session) {
          throw error || new Error('Login failed');
        }
        const user: User = {
          id: data.user.id,
          email: data.user.email!,
          createdAt: data.user.created_at
        };
        const response: AuthResponse = {
          token: data.session.access_token,
          user
        };
        this.setAuth(response);
        return response;
      })
    );
  }

  async logout(): Promise<void> {
    await this.supabase.client.auth.signOut();
    await Preferences.remove({ key: 'auth_token' });
    await Preferences.remove({ key: 'current_user' });
    this.tokenSubject.next(null);
    this.currentUserSubject.next(null);
  }

  private async setAuth(authResponse: AuthResponse): Promise<void> {
    await Preferences.set({ key: 'auth_token', value: authResponse.token });
    await Preferences.set({ key: 'current_user', value: JSON.stringify(authResponse.user) });
    this.tokenSubject.next(authResponse.token);
    this.currentUserSubject.next(authResponse.user);
  }

  getToken(): string | null {
    return this.tokenSubject.value;
  }

  get isAuthenticated(): boolean {
    return !!this.tokenSubject.value;
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }
}