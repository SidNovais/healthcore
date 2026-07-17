import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ErrorPageComponent } from './error-page.component';

/** Routed outside the app shell (the ** catch-all), so it owns the whole viewport. */
@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [ErrorPageComponent],
  template: `
    <app-error-page
      code="404"
      heading="Page Not Found"
      message="The page you're looking for doesn't exist."
      fill="viewport"
    />
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotFoundComponent {}
