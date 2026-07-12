import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ThemeService } from './core/application/theme.service';
import { HcToaster } from './ui/toast/toaster';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HcToaster],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  constructor() {
    inject(ThemeService).init();
  }
}
