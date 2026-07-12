import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="centered-page">
      <div class="error-card">
        <p class="error-code">404</p>
        <p class="error-title">Page Not Found</p>
        <p class="error-message">The page you're looking for doesn't exist.</p>
        <a routerLink="/" class="back-link">Go to home</a>
      </div>
    </div>
  `,
  styles: [`
    .centered-page {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 100dvh;
      padding: var(--space-8);
    }
    .error-card { text-align: center; max-width: 360px; }
    .error-code {
      font-family: 'JetBrains Mono', monospace;
      font-size: 5rem;
      font-weight: 700;
      color: var(--color-text-subtle);
      margin: 0;
      line-height: 1;
    }
    .error-title {
      font-family: var(--font-heading);
      font-size: 1.25rem;
      font-weight: 600;
      color: var(--color-text);
      margin: var(--space-2) 0;
    }
    .error-message {
      color: var(--color-text-muted);
      margin: 0 0 var(--space-6);
      font-size: 0.875rem;
    }
    .back-link {
      color: var(--color-accent);
      text-decoration: none;
      font-weight: 500;
      font-size: 0.875rem;
    }
    .back-link:hover { text-decoration: underline; }
  `],
})
export class NotFoundComponent {}
