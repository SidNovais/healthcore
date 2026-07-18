import { TestBed } from '@angular/core/testing';
import { WorklistService } from './worklist.service';
import { WORKLIST_PORT, IWorklistPort } from '../../core/application/i-worklist-port';
import { RealtimeClient } from '../../core/infrastructure/realtime/realtime-client';
import type { WorklistItemSummary } from '../../core/domain/worklist-item-summary';
import type { WorklistItemDetails } from '../../core/domain/worklist-item-details';

describe('WorklistService', () => {
  let service: WorklistService;
  let mockPort: IWorklistPort;
  let worklistHandler: (payload: unknown) => void;

  const summaryItems: WorklistItemSummary[] = [
    { id: 'wi-1', sampleBarcode: 'BC-001', examCode: 'HGB', patientId: 'p-1', patientName: 'Ana Souza', patientDateOfBirth: '1990-01-01', patientGender: 'Female', status: 'InProgress', createdAt: '2026-05-05T08:00:00Z' },
    { id: 'wi-2', sampleBarcode: 'BC-002', examCode: 'WBC', patientId: 'p-2', patientName: 'João Lima', patientDateOfBirth: '1985-05-20', patientGender: 'Male', status: 'Completed', createdAt: '2026-05-05T09:00:00Z' },
  ];

  const itemDetails: WorklistItemDetails = {
    id: 'wi-1',
    sampleId: 's-1',
    sampleBarcode: 'BC-001',
    examCode: 'HGB',
    patientId: 'p-1',
    patientName: 'Ana Souza',
    patientDateOfBirth: '1990-01-01',
    patientGender: 'Female',
    orderId: 'o-1',
    orderItemId: 'oi-1',
    status: 'Completed',
    analyteResults: [],
    reportPath: null,
    completionType: null,
    createdAt: '2026-05-05T08:00:00Z',
    completedAt: '2026-05-05T10:00:00Z',
  };

  beforeEach(() => {
    mockPort = {
      loadItems: vi.fn(),
      getItemDetails: vi.fn(),
      signReport: vi.fn(),
    };

    const realtimeStub = {
      on: vi.fn((topic: string, handler: (p: unknown) => void) => {
        if (topic === 'worklist') worklistHandler = handler;
        return () => undefined;
      }),
    };

    TestBed.configureTestingModule({
      providers: [
        WorklistService,
        { provide: WORKLIST_PORT, useValue: mockPort },
        { provide: RealtimeClient, useValue: realtimeStub },
      ],
    });

    service = TestBed.inject(WorklistService);
  });

  it('subscribes to the worklist topic on construction', () => {
    expect(worklistHandler).toBeDefined();
  });

  it('add inserts a new item pushed over the live feed', () => {
    worklistHandler({ op: 'add', entity: summaryItems[0] });

    expect(service.items().map((i) => i.id)).toEqual(['wi-1']);
  });

  it('update patches the status of a loaded item', () => {
    service.items.set([summaryItems[0]]);

    worklistHandler({ op: 'update', id: 'wi-1', status: 'Completed' });

    expect(service.items()[0].status).toBe('Completed');
  });

  it('remove drops a loaded item', () => {
    service.items.set([...summaryItems]);

    worklistHandler({ op: 'remove', id: 'wi-1' });

    expect(service.items().map((i) => i.id)).toEqual(['wi-2']);
  });

  it('items signal starts as empty array', () => {
    expect(service.items()).toEqual([]);
  });

  it('selectedItem signal starts as null', () => {
    expect(service.selectedItem()).toBeNull();
  });

  it('loadItems() calls port.loadItems and sets items signal', async () => {
    vi.mocked(mockPort.loadItems).mockResolvedValue(summaryItems);

    await service.loadItems();

    expect(mockPort.loadItems).toHaveBeenCalledOnce();
    expect(service.items()).toEqual(summaryItems);
  });

  it('loadItems() clears items signal if port returns empty array', async () => {
    vi.mocked(mockPort.loadItems).mockResolvedValue(summaryItems);
    await service.loadItems();

    vi.mocked(mockPort.loadItems).mockResolvedValue([]);
    await service.loadItems();

    expect(service.items()).toEqual([]);
  });

  it('loading signal starts as false', () => {
    expect(service.loading()).toBe(false);
  });

  it('loadItems() sets loading true while the request is in flight', async () => {
    let resolve!: (items: WorklistItemSummary[]) => void;
    vi.mocked(mockPort.loadItems).mockReturnValue(new Promise((r) => { resolve = r; }));

    const pending = service.loadItems();
    expect(service.loading()).toBe(true);

    resolve(summaryItems);
    await pending;
    expect(service.loading()).toBe(false);
  });

  it('loadItems() resets loading to false when the port rejects', async () => {
    vi.mocked(mockPort.loadItems).mockRejectedValue(new Error('boom'));

    await expect(service.loadItems()).rejects.toThrow('boom');

    expect(service.loading()).toBe(false);
  });

  it('getItemDetails(id) calls port.getItemDetails and sets selectedItem signal', async () => {
    vi.mocked(mockPort.getItemDetails).mockResolvedValue(itemDetails);

    await service.getItemDetails('wi-1');

    expect(mockPort.getItemDetails).toHaveBeenCalledWith('wi-1');
    expect(service.selectedItem()).toEqual(itemDetails);
  });

  it('signReport(id, data) calls port.signReport with id and params', async () => {
    vi.mocked(mockPort.signReport).mockResolvedValue();

    await service.signReport('wi-1', { signature: 'Dr. Smith', signedBy: 'physician-1' });

    expect(mockPort.signReport).toHaveBeenCalledWith('wi-1', {
      signature: 'Dr. Smith',
      signedBy: 'physician-1',
    });
  });
});
