import { TestBed } from '@angular/core/testing';
import { WorklistService } from './worklist.service';
import { WORKLIST_PORT, IWorklistPort } from '../../core/application/i-worklist-port';
import type { WorklistItemSummary } from '../../core/domain/worklist-item-summary';
import type { WorklistItemDetails } from '../../core/domain/worklist-item-details';

describe('WorklistService', () => {
  let service: WorklistService;
  let mockPort: IWorklistPort;

  const summaryItems: WorklistItemSummary[] = [
    { id: 'wi-1', sampleBarcode: 'BC-001', examCode: 'HGB', patientId: 'p-1', status: 'InProgress', createdAt: '2026-05-05T08:00:00Z' },
    { id: 'wi-2', sampleBarcode: 'BC-002', examCode: 'WBC', patientId: 'p-2', status: 'Completed', createdAt: '2026-05-05T09:00:00Z' },
  ];

  const itemDetails: WorklistItemDetails = {
    id: 'wi-1',
    sampleId: 's-1',
    sampleBarcode: 'BC-001',
    examCode: 'HGB',
    patientId: 'p-1',
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

    TestBed.configureTestingModule({
      providers: [
        WorklistService,
        { provide: WORKLIST_PORT, useValue: mockPort },
      ],
    });

    service = TestBed.inject(WorklistService);
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
