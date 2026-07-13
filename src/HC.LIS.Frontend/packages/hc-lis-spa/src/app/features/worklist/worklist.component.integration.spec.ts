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
  let loadingSignal: ReturnType<typeof signal<boolean>>;

  const physician: UserSession = { userId: 'physician-1', userName: 'physician@hclis.local', role: 'Physician' };

  const twoItems: WorklistItemSummary[] = [
    { id: 'wi-1', sampleBarcode: 'BC-001', examCode: 'HGB', patientId: 'p-1', patientName: 'Ana Souza', patientDateOfBirth: '1990-01-01', patientGender: 'Female', status: 'InProgress', createdAt: '2026-05-05T08:00:00Z' },
    { id: 'wi-2', sampleBarcode: 'BC-002', examCode: 'WBC', patientId: 'p-2', patientName: 'João Lima', patientDateOfBirth: '1985-05-20', patientGender: 'Male', status: 'Completed', createdAt: '2026-05-05T09:00:00Z' },
  ];

  function makeItems(n: number): WorklistItemSummary[] {
    return Array.from({ length: n }, (_, i) => ({
      ...twoItems[0],
      id: `wi-${i + 1}`,
      sampleBarcode: `BC-${String(i + 1).padStart(3, '0')}`,
      patientName: `Patient ${i + 1}`,
    }));
  }

  function rows(): HTMLElement[] {
    return Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll('[data-testid="worklist-row"]'),
    );
  }

  function patientNames(): string[] {
    return rows().map(
      r => r.querySelector('[data-testid="patient-name-cell"]')!.textContent!.trim(),
    );
  }

  beforeEach(async () => {
    itemsSignal = signal<WorklistItemSummary[]>([]);
    loadingSignal = signal(false);

    mockService = {
      items: itemsSignal,
      loading: loadingSignal,
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

  it('shows skeleton rows while loading, hiding the empty-state', () => {
    loadingSignal.set(true);
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;
    expect(host.querySelectorAll('[data-testid="worklist-skeleton-row"]').length).toBeGreaterThan(0);
    expect(host.querySelector('[data-testid="empty-state"]')).toBeNull();
  });

  it('does not show skeleton rows once loading completes', () => {
    loadingSignal.set(false);
    itemsSignal.set(twoItems);
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;
    expect(host.querySelectorAll('[data-testid="worklist-skeleton-row"]')).toHaveLength(0);
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

  it('paginates the worklist, one page at a time', () => {
    itemsSignal.set(makeItems(23));
    fixture.detectChanges();

    expect(rows()).toHaveLength(10);
    const host = fixture.nativeElement as HTMLElement;
    expect(host.querySelector('[data-testid="worklist-pagination"]')).not.toBeNull();

    host.querySelector<HTMLButtonElement>('[data-testid="worklist-pagination-next"]')!.click();
    fixture.detectChanges();
    expect(rows()).toHaveLength(10);
    expect(patientNames()[0]).toBe('Patient 11');
  });

  it('hides pagination when the worklist fits on one page', () => {
    itemsSignal.set(twoItems);
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).querySelector('[data-testid="worklist-pagination"]')).toBeNull();
  });

  it('sorts by patient name and toggles direction on repeated header clicks', () => {
    itemsSignal.set([
      { ...twoItems[0], id: 'c', patientName: 'Cora' },
      { ...twoItems[0], id: 'a', patientName: 'Ana' },
      { ...twoItems[0], id: 'b', patientName: 'Bea' },
    ]);
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;
    const header = host.querySelector<HTMLButtonElement>('[data-testid="worklist-sort-patient"]')!;
    const th = header.closest('th')!;

    header.click();
    fixture.detectChanges();
    expect(patientNames()).toEqual(['Ana', 'Bea', 'Cora']);
    expect(th.getAttribute('aria-sort')).toBe('ascending');

    header.click();
    fixture.detectChanges();
    expect(patientNames()).toEqual(['Cora', 'Bea', 'Ana']);
    expect(th.getAttribute('aria-sort')).toBe('descending');
  });

  it('offers a per-row action menu whose View selects the item without the trigger selecting it', () => {
    itemsSignal.set(twoItems);
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;
    const trigger = host.querySelector<HTMLButtonElement>('[data-testid="worklist-actions-trigger"]')!;
    expect(trigger.getAttribute('aria-haspopup')).toBe('menu');

    trigger.click();
    fixture.detectChanges();
    // Opening the menu must not itself select the row.
    expect(mockService.getItemDetails).not.toHaveBeenCalled();

    host.querySelector<HTMLButtonElement>('[data-testid="worklist-action-view"]')!.click();
    fixture.detectChanges();
    expect(mockService.getItemDetails).toHaveBeenCalledWith('wi-1');
  });
});
