import { Injectable } from '@angular/core';
import { createClient, SupabaseClient } from '@supabase/supabase-js';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SupabaseService {
  private _client: SupabaseClient;

  constructor() {
    this._client = createClient(environment.supabaseUrl, environment.supabaseAnonKey);
  }

  get client(): SupabaseClient {
    return this._client;
  }
}
