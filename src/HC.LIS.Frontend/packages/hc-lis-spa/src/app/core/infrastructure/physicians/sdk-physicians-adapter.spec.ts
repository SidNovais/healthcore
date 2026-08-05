import { TestBed } from '@angular/core/testing';
import {
  searchPhysicians,
  registerPhysician,
  getPhysicianDetails,
  updatePhysician,
  deactivatePhysician,
  reactivatePhysician,
} from '@hc-lis/api-client';
import { SdkPhysiciansAdapter } from './sdk-physicians-adapter';

vi.mock('@hc-lis/api-client', () => ({
  searchPhysicians: vi.fn(),
  registerPhysician: vi.fn(),
  getPhysicianDetails: vi.fn(),
  updatePhysician: vi.fn(),
  deactivatePhysician: vi.fn(),
  reactivatePhysician: vi.fn(),
}));

describe('SdkPhysiciansAdapter', () => {
  let adapter: SdkPhysiciansAdapter;

  beforeEach(() => {
    vi.clearAllMocks();
    TestBed.configureTestingModule({ providers: [SdkPhysiciansAdapter] });
    adapter = TestBed.inject(SdkPhysiciansAdapter);
  });

  it('search() asks for active physicians only and normalises the DTO', async () => {
    vi.mocked(searchPhysicians).mockResolvedValue({
      data: [
        { id: 'ph1', fullName: 'Ana Lima', licenceNumber: 'CRM-1234', status: 'Active' },
        { id: 'ph2', fullName: null, licenceNumber: null, status: null },
      ],
    } as never);

    const results = await adapter.search('Ana');

    expect(searchPhysicians).toHaveBeenCalledWith({
      query: { search: 'Ana', includeInactive: false },
    });
    expect(results).toEqual([
      { id: 'ph1', fullName: 'Ana Lima', licenceNumber: 'CRM-1234', status: 'Active' },
      { id: 'ph2', fullName: '', licenceNumber: null, status: 'Active' },
    ]);
  });

  it('search() returns an empty list when the response carries no data', async () => {
    vi.mocked(searchPhysicians).mockResolvedValue({ data: undefined } as never);

    await expect(adapter.search('nobody')).resolves.toEqual([]);
  });

  it('list() sends a blank term so the registry page gets every physician', async () => {
    vi.mocked(searchPhysicians).mockResolvedValue({ data: [] } as never);

    await adapter.list(true);

    expect(searchPhysicians).toHaveBeenCalledWith({
      query: { search: '', includeInactive: true },
    });
  });

  it('getDetails() maps the full record including the optional timestamps', async () => {
    vi.mocked(getPhysicianDetails).mockResolvedValue({
      data: {
        id: 'ph1',
        fullName: 'Ana Lima',
        licenceNumber: 'CRM-1234',
        status: 'Inactive',
        registeredAt: '2026-08-01T10:00:00Z',
        updatedAt: '2026-08-02T10:00:00Z',
        deactivatedAt: '2026-08-03T10:00:00Z',
      },
    } as never);

    const details = await adapter.getDetails('ph1');

    expect(getPhysicianDetails).toHaveBeenCalledWith({ path: { id: 'ph1' } });
    expect(details).toEqual({
      id: 'ph1',
      fullName: 'Ana Lima',
      licenceNumber: 'CRM-1234',
      status: 'Inactive',
      registeredAt: '2026-08-01T10:00:00Z',
      updatedAt: '2026-08-02T10:00:00Z',
      deactivatedAt: '2026-08-03T10:00:00Z',
    });
  });

  it('register() posts the payload and returns the new id', async () => {
    vi.mocked(registerPhysician).mockResolvedValue({ data: { id: 'ph9' } } as never);

    const id = await adapter.register({ fullName: 'Ana Lima', licenceNumber: 'CRM-1234' });

    expect(registerPhysician).toHaveBeenCalledWith({
      body: { fullName: 'Ana Lima', licenceNumber: 'CRM-1234' },
    });
    expect(id).toBe('ph9');
  });

  it('update() puts the payload against the physician id', async () => {
    vi.mocked(updatePhysician).mockResolvedValue({ data: undefined } as never);

    await adapter.update('ph1', { fullName: 'Ana Lima Souza' });

    expect(updatePhysician).toHaveBeenCalledWith({
      path: { id: 'ph1' },
      body: { fullName: 'Ana Lima Souza', licenceNumber: undefined },
    });
  });

  it('deactivate() and reactivate() post to their lifecycle routes', async () => {
    vi.mocked(deactivatePhysician).mockResolvedValue({ data: undefined } as never);
    vi.mocked(reactivatePhysician).mockResolvedValue({ data: undefined } as never);

    await adapter.deactivate('ph1');
    await adapter.reactivate('ph1');

    expect(deactivatePhysician).toHaveBeenCalledWith({ path: { id: 'ph1' } });
    expect(reactivatePhysician).toHaveBeenCalledWith({ path: { id: 'ph1' } });
  });
});
