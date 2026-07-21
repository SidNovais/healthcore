import { TestBed } from '@angular/core/testing';
import { PatientsService } from './patients.service';
import { PATIENTS_PORT, IPatientsPort } from './i-patients-port';
import type { PatientSearchResult } from '../domain/patient-search-result';
import type { PatientDetails } from '../domain/patient-details';
import type { RegisterPatientParams } from '../domain/register-patient-params';
import { ApiError } from '@hc-lis/api-client';

describe('PatientsService', () => {
  let service: PatientsService;
  let mockPort: IPatientsPort;

  const searchResults: PatientSearchResult[] = [
    { id: 'p1', fullName: 'John Doe', dateOfBirth: '1990-01-01', documentId: null, status: 'Active' },
  ];

  const registerParams: RegisterPatientParams = {
    fullName: 'Jane Doe',
    dateOfBirth: '1985-05-15',
  };

  const patientDetails: PatientDetails = {
    id: 'p1',
    fullName: 'John Doe',
    dateOfBirth: '1990-01-01',
    gender: null,
    mothersFullName: null,
    documentId: null,
    phone: null,
    email: null,
    status: 'Active',
    registeredAt: '2026-01-01T00:00:00Z',
    anonymizedAt: null,
  };

  beforeEach(() => {
    mockPort = {
      search: vi.fn(),
      getDetails: vi.fn(),
      register: vi.fn(),
      update: vi.fn(),
      anonymize: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [PatientsService, { provide: PATIENTS_PORT, useValue: mockPort }],
    });

    service = TestBed.inject(PatientsService);
  });

  it('quickSearch() returns matches without touching the page-level search state', async () => {
    vi.mocked(mockPort.search).mockResolvedValue(searchResults);
    service.searchResults.set([]);

    const results = await service.quickSearch('John');

    expect(results).toEqual(searchResults);
    // The command palette shares this service with the /patients page — its results
    // must never overwrite the list the page is rendering behind the palette.
    expect(service.searchResults()).toEqual([]);
    expect(service.searching()).toBe(false);
  });

  it('search() updates searchResults signal', async () => {
    vi.mocked(mockPort.search).mockResolvedValue(searchResults);

    await service.search('John');

    expect(mockPort.search).toHaveBeenCalledWith('John');
    expect(service.searchResults()).toEqual(searchResults);
  });

  it('searching signal starts as false', () => {
    expect(service.searching()).toBe(false);
  });

  it('search() sets searching true while the request is in flight', async () => {
    let resolve!: (results: PatientSearchResult[]) => void;
    vi.mocked(mockPort.search).mockReturnValue(new Promise((r) => { resolve = r; }));

    const pending = service.search('John');
    expect(service.searching()).toBe(true);

    resolve(searchResults);
    await pending;
    expect(service.searching()).toBe(false);
  });

  it('search() resets searching to false when the port rejects', async () => {
    vi.mocked(mockPort.search).mockRejectedValue(new ApiError(400, 'Bad request'));

    await service.search('fail');

    expect(service.searching()).toBe(false);
  });

  it('loadingDetails signal starts as false', () => {
    expect(service.loadingDetails()).toBe(false);
  });

  it('loadDetails() sets loadingDetails true while the request is in flight', async () => {
    let resolve!: (details: PatientDetails) => void;
    vi.mocked(mockPort.getDetails).mockReturnValue(new Promise((r) => { resolve = r; }));

    const pending = service.loadDetails('p1');
    expect(service.loadingDetails()).toBe(true);

    resolve(patientDetails);
    await pending;
    expect(service.loadingDetails()).toBe(false);
    expect(service.patient()).toEqual(patientDetails);
  });

  it('loadDetails() resets loadingDetails to false when the port rejects', async () => {
    vi.mocked(mockPort.getDetails).mockRejectedValue(new ApiError(404, 'Not found'));

    await service.loadDetails('p1');

    expect(service.loadingDetails()).toBe(false);
  });

  it('register() returns new patient id', async () => {
    vi.mocked(mockPort.register).mockResolvedValue('new-patient-id');

    const id = await service.register(registerParams);

    expect(mockPort.register).toHaveBeenCalledWith(registerParams);
    expect(id).toBe('new-patient-id');
  });

  it('update() calls adapter with id and params', async () => {
    vi.mocked(mockPort.update).mockResolvedValue();

    await service.update('p1', registerParams);

    expect(mockPort.update).toHaveBeenCalledWith('p1', registerParams);
  });

  it('anonymize() calls adapter with id', async () => {
    vi.mocked(mockPort.anonymize).mockResolvedValue();

    await service.anonymize('p1');

    expect(mockPort.anonymize).toHaveBeenCalledWith('p1');
  });

  it('error signal set on adapter rejection', async () => {
    vi.mocked(mockPort.search).mockRejectedValue(new ApiError(400, 'Bad request'));

    await service.search('fail');

    expect(service.error()).toBe('Bad request');
  });
});
