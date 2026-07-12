import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="centered-page">
      <div class="error-card">
        <div class="error-icon" aria-hidden="true">
          <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
            <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/>
            <path d="M7 11V7a5 5 0 0 1 10 0v4"/>
          </svg>
        </div>
        <h1 class="error-code">403</h1>
        <p class="error-title">Access Denied</p>
        <p class="error-message">You don't have permission to view this page.</p>
        <a routerLink="/" class="back-link">Go to home</a>
      </div>
    </div>
  `,
  styles: [`
    .centered-page {
      display: flex;
      align-items: center;
      justify-content: center;
      height: 100%;
      padding: var(--space-8);
    }
    .error-card {
      text-align: center;
      max-width: 360px;
    }
    .error-icon {
      color: var(--color-error);
      margin-bottom: var(--space-4);
      display: flex;
      justify-content: center;
    }
    .error-code {
      font-family: 'JetBrains Mono', monospace;
      font-size: 4rem;
      font-weight: 700;
      color: var(--color-error);
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
export class UnauthorizedComponent {}
