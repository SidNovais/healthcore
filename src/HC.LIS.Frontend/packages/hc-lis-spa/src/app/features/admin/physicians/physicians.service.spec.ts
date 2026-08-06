import { TestBed } from '@angular/core/testing';
import { PhysiciansService } from './physicians.service';
import { PHYSICIANS_PORT, type IPhysiciansPort } from '../../../core/application/i-physicians-port';
import type { PhysicianSearchResult } from '../../../core/domain/physician-search-result';

describe('PhysiciansService', () => {
  let service: PhysiciansService;
  let mockPort: IPhysiciansPort;

  const registry: PhysicianSearchResult[] = [
    { id: 'p-1', fullName: 'Ana Lima', licenceNumber: 'CRM-1234', status: 'Active' },
    { id: 'p-2', fullName: 'Bruno Costa', licenceNumber: null, status: 'Inactive' },
  ];

  beforeEach(() => {
    mockPort = {
      search: vi.fn(),
      list: vi.fn().mockResolvedValue([]),
      getDetails: vi.fn(),
      register: vi.fn().mockResolvedValue('p-3'),
      update: vi.fn().mockResolvedValue(undefined),
      deactivate: vi.fn().mockResolvedValue(undefined),
      reactivate: vi.fn().mockResolvedValue(undefined),
    };

    TestBed.configureTestingModule({
      providers: [PhysiciansService, { provide: PHYSICIANS_PORT, useValue: mockPort }],
    });

    service = TestBed.inject(PhysiciansService);
  });

  afterEach(() => TestBed.resetTestingModule());

  it('physicians signal starts as an empty array', () => {
    expect(service.physicians()).toEqual([]);
  });

  // The registry page is the only place an inactive physician can be reactivated from,
  // so it must ask for them — the picker's active-only search would hide the rows.
  it('listPhysicians() asks the port for inactive physicians too', async () => {
    vi.mocked(mockPort.list).mockResolvedValue(registry);

    await service.listPhysicians();

    expect(mockPort.list).toHaveBeenCalledWith(true);
    expect(service.physicians()).toEqual(registry);
  });

  it('loading signal starts as false', () => {
    expect(service.loading()).toBe(false);
  });

  it('listPhysicians() sets loading true while the request is in flight', async () => {
    let resolve!: (physicians: PhysicianSearchResult[]) => void;
    vi.mocked(mockPort.list).mockReturnValue(
      new Promise(r => {
        resolve = r;
      }),
    );

    const pending = service.listPhysicians();
    expect(service.loading()).toBe(true);

    resolve(registry);
    await pending;
    expect(service.loading()).toBe(false);
  });

  it('listPhysicians() resets loading to false when the port rejects', async () => {
    vi.mocked(mockPort.list).mockRejectedValue(new Error('boom'));

    await expect(service.listPhysicians()).rejects.toThrow('boom');

    expect(service.loading()).toBe(false);
  });

  it('register(params) registers the physician and refreshes the list', async () => {
    await service.register({ fullName: 'Carla Dias', licenceNumber: 'CRM-9999' });

    expect(mockPort.register).toHaveBeenCalledWith({
      fullName: 'Carla Dias',
      licenceNumber: 'CRM-9999',
    });
    expect(mockPort.list).toHaveBeenCalledWith(true);
  });

  it('update(id, params) updates the physician and refreshes the list', async () => {
    await service.update('p-1', { fullName: 'Ana Lima Souza', licenceNumber: 'CRM-1234' });

    expect(mockPort.update).toHaveBeenCalledWith('p-1', {
      fullName: 'Ana Lima Souza',
      licenceNumber: 'CRM-1234',
    });
    expect(mockPort.list).toHaveBeenCalledWith(true);
  });

  it('deactivate(id) deactivates the physician and refreshes the list', async () => {
    await service.deactivate('p-1');

    expect(mockPort.deactivate).toHaveBeenCalledWith('p-1');
    expect(mockPort.list).toHaveBeenCalledWith(true);
  });

  it('reactivate(id) reactivates the physician and refreshes the list', async () => {
    await service.reactivate('p-2');

    expect(mockPort.reactivate).toHaveBeenCalledWith('p-2');
    expect(mockPort.list).toHaveBeenCalledWith(true);
  });

  it('does not refresh the list when the mutation is rejected', async () => {
    vi.mocked(mockPort.deactivate).mockRejectedValue(new Error('409'));

    await expect(service.deactivate('p-1')).rejects.toThrow('409');

    expect(mockPort.list).not.toHaveBeenCalled();
  });
});
