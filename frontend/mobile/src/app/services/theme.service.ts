import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private dark = false;

  isDarkMode(): boolean {
    return this.dark;
  }

  setDarkMode(isDark: boolean) {
    this.dark = isDark;
    document.body.classList.toggle('dark', isDark);
  }
}
