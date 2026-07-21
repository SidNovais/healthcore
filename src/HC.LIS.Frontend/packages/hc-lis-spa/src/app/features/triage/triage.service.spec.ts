import { TestBed } from '@angular/core/testing';
import { TriageService } from './triage.service';
import { COLLECTION_REQUESTS_PORT, type ICollectionRequestsPort } from '../../core/application/i-collection-requests-port';
import { RealtimeClient } from '../../core/infrastructure/realtime/realtime-client';
import type { CollectionRequestSummary } from '../../core/domain/collection-request-summary';

describe('TriageService live updates', () => {
  let service: TriageService;
  let triageHandler: (payload: unknown) => void;
  let mockPort: ICollectionRequestsPort;

  const row = (id: string, status: string): CollectionRequestSummary => ({
    collectionRequestId: id,
    patientId: `p-${id}`,
    status,
    arrivedAt: '2026-05-05T08:00:00Z',
  });

  beforeEach(() => {
    mockPort = {
      loadArrived: vi.fn(),
      loadQueue: vi.fn(),
      loadCalled: vi.fn(),
      moveToWaiting: vi.fn(),
      callPatient: vi.fn(),
      recordCollection: vi.fn(),
      getSamples: vi.fn(),
    };

    const realtimeStub = {
      on: vi.fn((topic: string, handler: (p: unknown) => void) => {
        if (topic === 'triage') triageHandler = handler;
        return () => undefined;
      }),
    };

    TestBed.configureTestingModule({
      providers: [
        TriageService,
        { provide: COLLECTION_REQUESTS_PORT, useValue: mockPort },
        { provide: RealtimeClient, useValue: realtimeStub },
      ],
    });

    service = TestBed.inject(TriageService);
  });

  it('subscribes to the triage topic on construction', () => {
    expect(triageHandler).toBeDefined();
  });

  it('loading signal starts as false', () => {
    expect(service.loading()).toBe(false);
  });

  it('refreshAll() sets loading true while the queues are in flight and populates them', async () => {
    let resolveArrived!: (rows: CollectionRequestSummary[]) => void;
    vi.mocked(mockPort.loadArrived).mockReturnValue(new Promise((r) => { resolveArrived = r; }));
    vi.mocked(mockPort.loadQueue).mockResolvedValue([]);
    vi.mocked(mockPort.loadCalled).mockResolvedValue([]);

    const pending = service.refreshAll();
    expect(service.loading()).toBe(true);

    resolveArrived([row('1', 'Arrived')]);
    await pending;

    expect(service.loading()).toBe(false);
    expect(service.arrived()).toHaveLength(1);
  });

  it('refreshAll() resets loading to false when a queue load rejects', async () => {
    vi.mocked(mockPort.loadArrived).mockRejectedValue(new Error('boom'));
    vi.mocked(mockPort.loadQueue).mockResolvedValue([]);
    vi.mocked(mockPort.loadCalled).mockResolvedValue([]);

    await expect(service.refreshAll()).rejects.toThrow('boom');

    expect(service.loading()).toBe(false);
  });

  it('add inserts a new row into the arrived queue', () => {
    triageHandler({ op: 'add', queue: 'arrived', entity: row('cr-1', 'Arrived') });

    expect(service.arrived().map((r) => r.collectionRequestId)).toEqual(['cr-1']);
  });

  it('add does not duplicate an existing request', () => {
    service.arrived.set([row('cr-1', 'Arrived')]);

    triageHandler({ op: 'add', queue: 'arrived', entity: row('cr-1', 'Arrived') });

    expect(service.arrived()).toHaveLength(1);
  });

  it('move relocates the row to the next queue and updates its status', () => {
    service.arrived.set([row('cr-1', 'Arrived')]);

    triageHandler({ op: 'move', queue: 'waiting', collectionRequestId: 'cr-1', status: 'Waiting' });

    expect(service.arrived()).toHaveLength(0);
    expect(service.waiting()).toHaveLength(1);
    expect(service.waiting()[0].status).toBe('Waiting');
  });

  it('move is a no-op when the row is not currently loaded', () => {
    triageHandler({ op: 'move', queue: 'called', collectionRequestId: 'ghost', status: 'Called' });

    expect(service.called()).toHaveLength(0);
  });

  it('remove drops the request from whichever queue holds it', () => {
    service.called.set([row('cr-1', 'Called'), row('cr-2', 'Called')]);

    triageHandler({ op: 'remove', collectionRequestId: 'cr-1' });

    expect(service.called().map((r) => r.collectionRequestId)).toEqual(['cr-2']);
  });
});
