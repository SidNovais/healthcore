import { Injectable, InjectionToken, effect, inject, signal } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { AuthService } from '../../application/auth.service';

/** Live-feed connection state surfaced to the UI. */
export type RealtimeStatus = 'idle' | 'live' | 'reconnecting';

/** The real-time topics the server can push; each maps to a view. */
export const REALTIME_TOPICS = ['orders', 'triage', 'worklist'] as const;
export type RealtimeTopic = (typeof REALTIME_TOPICS)[number];

export type RealtimeHandler = (payload: unknown) => void;

/**
 * Factory for the browser `EventSource`, injected so tests can substitute a fake
 * (jsdom has no native `EventSource`).
 */
export type EventSourceFactory = (url: string, init?: EventSourceInit) => EventSource;

export const EVENT_SOURCE_FACTORY = new InjectionToken<EventSourceFactory>('EVENT_SOURCE_FACTORY', {
  providedIn: 'root',
  factory: () => (url, init) => new EventSource(url, init),
});

/**
 * Holds the single SSE connection to the API and fans server-pushed changes out to the feature
 * services. The connection follows the auth session: it opens once the user is authenticated and
 * closes on logout. Native `EventSource` reconnects on its own; we only reflect that in `status`.
 */
@Injectable({ providedIn: 'root' })
export class RealtimeClient {
  private readonly createSource = inject(EVENT_SOURCE_FACTORY);
  private readonly auth = inject(AuthService);

  readonly status = signal<RealtimeStatus>('idle');

  private source: EventSource | null = null;
  private readonly handlers = new Map<string, Set<RealtimeHandler>>();

  constructor() {
    // Bind the connection lifecycle to the auth session (login, logout, initial hydration).
    effect(() => {
      if (this.auth.currentUser()) this.connect();
      else this.disconnect();
    });
  }

  /** Registers a handler for a topic; returns an unsubscribe function. */
  on(topic: RealtimeTopic, handler: RealtimeHandler): () => void {
    let set = this.handlers.get(topic);
    if (!set) {
      set = new Set<RealtimeHandler>();
      this.handlers.set(topic, set);
    }
    set.add(handler);
    return () => set?.delete(handler);
  }

  connect(): void {
    if (this.source) return;

    const source = this.createSource(`${environment.apiUrl}/api/v1/events/stream`, {
      withCredentials: true,
    });
    this.source = source;

    source.onopen = () => this.status.set('live');
    source.onerror = () => this.status.set('reconnecting');

    for (const topic of REALTIME_TOPICS) {
      source.addEventListener(topic, (event) => this.dispatch(topic, (event as MessageEvent).data));
    }
  }

  disconnect(): void {
    this.source?.close();
    this.source = null;
    this.status.set('idle');
  }

  private dispatch(topic: RealtimeTopic, raw: string): void {
    let payload: unknown;
    try {
      payload = JSON.parse(raw);
    } catch {
      return;
    }
    this.handlers.get(topic)?.forEach((handler) => handler(payload));
  }
}
