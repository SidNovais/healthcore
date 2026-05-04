import { TestBed } from '@angular/core/testing';
import { CollectionRequestsService } from './collection-requests.service';
import { COLLECTION_REQUESTS_PORT, ICollectionRequestsPort } from '../../core/application/i-collection-requests-port';
import type { CollectionRequestSummary } from '../../core/domain/collection-request-summary';

describe('CollectionRequestsService', () => {
  let service: CollectionRequestsService;
  let mockPort: ICollectionRequestsPort;

  const queueItems: CollectionRequestSummary[] = [
    { collectionRequestId: 'cr-1', patientId: 'p-1', status: 'Waiting', arrivedAt: '2026-05-04T10:00:00Z' },
    { collectionRequestId: 'cr-2', patientId: 'p-2', status: 'Waiting', arrivedAt: '2026-05-04T10:05:00Z' },
  ];

  beforeEach(() => {
    mockPort = {
      loadQueue: vi.fn(),
      callPatient: vi.fn(),
      createBarcode: vi.fn(),
      recordCollection: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        CollectionRequestsService,
        { provide: COLLECTION_REQUESTS_PORT, useValue: mockPort },
      ],
    });

    service = TestBed.inject(CollectionRequestsService);
  });

  it('queue signal starts as empty array', () => {
    expect(service.queue()).toEqual([]);
  });

  it('loadQueue() calls port.loadQueue and sets queue signal', async () => {
    vi.mocked(mockPort.loadQueue).mockResolvedValue(queueItems);

    await service.loadQueue();

    expect(mockPort.loadQueue).toHaveBeenCalledOnce();
    expect(service.queue()).toEqual(queueItems);
  });

  it('loadQueue() clears queue signal if port returns empty array', async () => {
    vi.mocked(mockPort.loadQueue).mockResolvedValue(queueItems);
    await service.loadQueue();

    vi.mocked(mockPort.loadQueue).mockResolvedValue([]);
    await service.loadQueue();

    expect(service.queue()).toEqual([]);
  });

  it('callPatient() calls port.callPatient with the given id', async () => {
    vi.mocked(mockPort.callPatient).mockResolvedValue();

    await service.callPatient('cr-1');

    expect(mockPort.callPatient).toHaveBeenCalledWith('cr-1');
  });

  it('createBarcode() calls port.createBarcode with id and params', async () => {
    vi.mocked(mockPort.createBarcode).mockResolvedValue();

    await service.createBarcode('cr-1', { tubeType: 'EDTA', barcodeValue: 'BC-001', technicianId: 'tech-1' });

    expect(mockPort.createBarcode).toHaveBeenCalledWith('cr-1', {
      tubeType: 'EDTA',
      barcodeValue: 'BC-001',
      technicianId: 'tech-1',
    });
  });

  it('recordCollection() calls port.recordCollection with id and params', async () => {
    vi.mocked(mockPort.recordCollection).mockResolvedValue();

    await service.recordCollection('cr-1', {
      sampleId: 'sample-1',
      patientName: 'John Doe',
      patientBirthdate: '1990-01-01',
      patientGender: 'M',
      technicianId: 'tech-1',
    });

    expect(mockPort.recordCollection).toHaveBeenCalledWith('cr-1', {
      sampleId: 'sample-1',
      patientName: 'John Doe',
      patientBirthdate: '1990-01-01',
      patientGender: 'M',
      technicianId: 'tech-1',
    });
  });
});
