import { TestBed } from '@angular/core/testing';
import { ComponentFixture } from '@angular/core/testing';
import { signal } from '@angular/core';
import { WaitingRoomComponent } from './waiting-room.component';
import { CollectionRequestsService } from './collection-requests.service';
import { AuthService } from '../../core/application/auth.service';
import type { CollectionRequestSummary } from '../../core/domain/collection-request-summary';
import type { UserSession } from '../../core/domain/user-session';

describe('WaitingRoomComponent (integration)', () => {
  let fixture: ComponentFixture<WaitingRoomComponent>;
  let mockService: Partial<CollectionRequestsService>;
  let mockAuthService: Partial<AuthService>;
  let queueSignal: ReturnType<typeof signal<CollectionRequestSummary[]>>;

  const labTech: UserSession = { userId: 'tech-1', userName: 'lab@hclis.local', role: 'LabTechnician' };

  const twoItems: CollectionRequestSummary[] = [
    { collectionRequestId: 'cr-1', patientId: 'p-1', status: 'Waiting', arrivedAt: '2026-05-04T10:00:00Z' },
    { collectionRequestId: 'cr-2', patientId: 'p-2', status: 'Waiting', arrivedAt: '2026-05-04T10:05:00Z' },
  ];

  beforeEach(async () => {
    queueSignal = signal<CollectionRequestSummary[]>([]);

    mockService = {
      queue: queueSignal,
      loadQueue: vi.fn().mockResolvedValue(undefined),
      callPatient: vi.fn().mockResolvedValue(undefined),
      createBarcode: vi.fn().mockResolvedValue(undefined),
      recordCollection: vi.fn().mockResolvedValue(undefined),
    };

    mockAuthService = {
      currentUser: signal<UserSession | null>(labTech),
    };

    await TestBed.configureTestingModule({
      imports: [WaitingRoomComponent],
      providers: [
        { provide: CollectionRequestsService, useValue: mockService },
        { provide: AuthService, useValue: mockAuthService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(WaitingRoomComponent);
    fixture.detectChanges();
  });

  afterEach(() => TestBed.resetTestingModule());

  it('calls loadQueue() on init', () => {
    expect(mockService.loadQueue).toHaveBeenCalledOnce();
  });

  it('renders one row per queue item', async () => {
    queueSignal.set(twoItems);
    fixture.detectChanges();

    const rows = (fixture.nativeElement as HTMLElement).querySelectorAll('[data-testid="patient-card"]');
    expect(rows).toHaveLength(2);
  });

  it('shows empty-state element when queue is empty', () => {
    queueSignal.set([]);
    fixture.detectChanges();

    const empty = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="empty-state"]');
    expect(empty).not.toBeNull();
  });

  it('does not show empty-state when queue has items', () => {
    queueSignal.set(twoItems);
    fixture.detectChanges();

    const empty = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="empty-state"]');
    expect(empty).toBeNull();
  });
});
