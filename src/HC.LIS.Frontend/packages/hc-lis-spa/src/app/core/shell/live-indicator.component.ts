import { Component, computed, inject } from '@angular/core';
import { RealtimeClient } from '../infrastructure/realtime/realtime-client';

/**
 * Small ambient badge reflecting the live-feed connection. Injecting the {@link RealtimeClient}
 * here is also what brings the connection up: the client opens the stream once a user is
 * authenticated (the shell only renders when they are). Hidden while idle (disconnected).
 */
@Component({
  selector: 'app-live-indicator',
  standalone: true,
  template: `
    @if (status() !== 'idle') {
      <span
        class="live-indicator"
        [attr.data-status]="status()"
        data-testid="live-indicator"
        role="status"
        [attr.aria-label]="label()"
        [title]="label()"
      >
        <span class="dot" aria-hidden="true"></span>
        <span class="label">{{ label() }}</span>
      </span>
    }
  `,
  styleUrl: './live-indicator.component.css',
})
export class LiveIndicatorComponent {
  private readonly realtime = inject(RealtimeClient);

  protected readonly status = this.realtime.status;

  protected readonly label = computed(() =>
    this.status() === 'reconnecting' ? 'Reconnecting…' : 'Live',
  );
}
