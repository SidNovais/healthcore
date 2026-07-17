import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ErrorPageComponent } from './error-page.component';

/** Routed inside the app shell, so the card fills the outlet rather than the viewport. */
@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [ErrorPageComponent],
  template: `
    <app-error-page
      code="403"
      heading="Access Denied"
      message="You don't have permission to view this page."
      icon="lock"
      tone="error"
      fill="main"
      testId="error-card"
    />
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UnauthorizedComponent {}
