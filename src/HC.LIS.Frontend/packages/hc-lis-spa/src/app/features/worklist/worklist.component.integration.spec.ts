import { TestBed } from '@angular/core/testing';
import { ComponentFixture } from '@angular/core/testing';
import { signal } from '@angular/core';
import { WorklistComponent } from './worklist.component';
import { WorklistService } from './worklist.service';
import { AuthService } from '../../core/application/auth.service';
import type { WorklistItemSummary } from '../../core/domain/worklist-item-summary';
import type { UserSession } from '../../core/domain/user-session';

describe('WorklistComponent (integration)', () => {
  let fixture: ComponentFixture<WorklistComponent>;
  let mockService: Partial<WorklistService>;
  let mockAuthService: Partial<AuthService>;
  let itemsSignal: ReturnType<typeof signal<WorklistItemSummary[]>>;

  const physician: UserSession = { userId: 'physician-1', userName: 'physician@hclis.local', role: 'Physician' };

  const twoItems: WorklistItemSummary[] = [
    { id: 'wi-1', sampleBarcode: 'BC-001', examCode: 'HGB', patientId: 'p-1', status: 'InProgress', createdAt: '2026-05-05T08:00:00Z' },
    { id: 'wi-2', sampleBarcode: 'BC-002', examCode: 'WBC', patientId: 'p-2', status: 'Completed', createdAt: '2026-05-05T09:00:00Z' },
  ];

  beforeEach(async () => {
    itemsSignal = signal<WorklistItemSummary[]>([]);

    mockService = {
      items: itemsSignal,
      selectedItem: signal(null),
      loadItems: vi.fn().mockResolvedValue(undefined),
      getItemDetails: vi.fn().mockResolvedValue(undefined),
      signReport: vi.fn().mockResolvedValue(undefined),
    };

    mockAuthService = {
      currentUser: signal<UserSession | null>(physician),
    };

    await TestBed.configureTestingModule({
      imports: [WorklistComponent],
      providers: [
        { provide: WorklistService, useValue: mockService },
        { provide: AuthService, useValue: mockAuthService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(WorklistComponent);
    fixture.detectChanges();
  });

  afterEach(() => TestBed.resetTestingModule());

  it('calls loadItems() on init', () => {
    expect(mockService.loadItems).toHaveBeenCalledOnce();
  });

  it('renders table rows with status badge for each worklist item', async () => {
    itemsSignal.set(twoItems);
    fixture.detectChanges();

    const rows = (fixture.nativeElement as HTMLElement).querySelectorAll('[data-testid="worklist-row"]');
    expect(rows).toHaveLength(2);
  });

  it('shows empty-state element when items list is empty', () => {
    itemsSignal.set([]);
    fixture.detectChanges();

    const empty = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="empty-state"]');
    expect(empty).not.toBeNull();
  });

  it('does not show empty-state when items list has entries', () => {
    itemsSignal.set(twoItems);
    fixture.detectChanges();

    const empty = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="empty-state"]');
    expect(empty).toBeNull();
  });

  it('clicking a row calls getItemDetails with the item id', async () => {
    itemsSignal.set(twoItems);
    fixture.detectChanges();

    const firstRow = (fixture.nativeElement as HTMLElement).querySelector<HTMLElement>('[data-testid="worklist-row"]');
    firstRow!.click();
    fixture.detectChanges();

    expect(mockService.getItemDetails).toHaveBeenCalledWith('wi-1');
  });
});
