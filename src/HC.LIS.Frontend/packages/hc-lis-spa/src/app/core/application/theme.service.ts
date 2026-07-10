import { Injectable, signal } from '@angular/core';

type Theme = 'light' | 'dark';
const STORAGE_KEY = 'hc-lis-theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly theme = signal<Theme>(this.load());

  init(): void {
    this.apply(this.theme());
  }

  toggle(): void {
    this.apply(this.theme() === 'light' ? 'dark' : 'light');
  }

  private apply(t: Theme): void {
    document.documentElement.setAttribute('data-theme', t);
    localStorage.setItem(STORAGE_KEY, t);
    this.theme.set(t);
  }

  private load(): Theme {
    return localStorage.getItem(STORAGE_KEY) === 'dark' ? 'dark' : 'light';
  }
}
