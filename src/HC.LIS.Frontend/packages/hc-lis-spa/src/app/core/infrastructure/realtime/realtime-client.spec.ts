import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AuthService } from '../../application/auth.service';
import type { UserSession } from '../../domain/user-session';
import { EVENT_SOURCE_FACTORY, RealtimeClient, type EventSourceFactory } from './realtime-client';

class FakeEventSource {
  onopen: (() => void) | null = null;
  onerror: (() => void) | null = null;
  closed = false;
  private readonly listeners = new Map<string, (event: MessageEvent) => void>();

  constructor(
    readonly url: string,
    readonly init?: EventSourceInit,
  ) {}

  addEventListener(type: string, cb: (event: MessageEvent) => void): void {
    this.listeners.set(type, cb);
  }

  close(): void {
    this.closed = true;
  }

  open(): void {
    this.onopen?.();
  }

  fail(): void {
    this.onerror?.();
  }

  emit(type: string, data: string): void {
    this.listeners.get(type)?.({ data } as MessageEvent);
  }
}

describe('RealtimeClient', () => {
  let sources: FakeEventSource[];
  let currentUser: ReturnType<typeof signal<UserSession | null>>;

  function setup(): RealtimeClient {
    sources = [];
    currentUser = signal<UserSession | null>(null);
    const factory: EventSourceFactory = (url, init) => {
      const source = new FakeEventSource(url, init);
      sources.push(source);
      return source as unknown as EventSource;
    };

    TestBed.configureTestingModule({
      providers: [
        RealtimeClient,
        { provide: EVENT_SOURCE_FACTORY, useValue: factory },
        { provide: AuthService, useValue: { currentUser } },
      ],
    });

    return TestBed.inject(RealtimeClient);
  }

  it('status starts idle', () => {
    const client = setup();
    expect(client.status()).toBe('idle');
  });

  it('connect() opens a credentialed stream and goes live on open', () => {
    const client = setup();
    client.connect();

    expect(sources).toHaveLength(1);
    expect(sources[0].url).toContain('/api/v1/events/stream');
    expect(sources[0].init).toEqual({ withCredentials: true });

    sources[0].open();
    expect(client.status()).toBe('live');
  });

  it('connect() is idempotent', () => {
    const client = setup();
    client.connect();
    client.connect();
    expect(sources).toHaveLength(1);
  });

  it('dispatches the parsed payload to a topic handler', () => {
    const client = setup();
    client.connect();
    const received: unknown[] = [];
    client.on('orders', (payload) => received.push(payload));

    sources[0].emit('orders', '{"op":"status","orderItemId":"oi-1","status":"Canceled"}');

    expect(received).toEqual([{ op: 'status', orderItemId: 'oi-1', status: 'Canceled' }]);
  });

  it('ignores malformed JSON frames', () => {
    const client = setup();
    client.connect();
    const received: unknown[] = [];
    client.on('orders', (payload) => received.push(payload));

    sources[0].emit('orders', 'not-json');

    expect(received).toEqual([]);
  });

  it('goes reconnecting on error', () => {
    const client = setup();
    client.connect();
    sources[0].fail();
    expect(client.status()).toBe('reconnecting');
  });

  it('disconnect() closes the stream and resets status', () => {
    const client = setup();
    client.connect();
    client.disconnect();

    expect(sources[0].closed).toBe(true);
    expect(client.status()).toBe('idle');
  });

  it('unsubscribe stops delivering to a handler', () => {
    const client = setup();
    client.connect();
    const received: unknown[] = [];
    const off = client.on('orders', (payload) => received.push(payload));

    off();
    sources[0].emit('orders', '{"op":"status"}');

    expect(received).toEqual([]);
  });

  it('connects when the user becomes authenticated and disconnects on logout', () => {
    const client = setup();

    currentUser.set({ userId: 'u-1', userName: 'a@b.c', role: 'Physician' } as UserSession);
    TestBed.flushEffects();
    expect(sources).toHaveLength(1);

    currentUser.set(null);
    TestBed.flushEffects();
    expect(sources[0].closed).toBe(true);
    expect(client.status()).toBe('idle');
  });
});
