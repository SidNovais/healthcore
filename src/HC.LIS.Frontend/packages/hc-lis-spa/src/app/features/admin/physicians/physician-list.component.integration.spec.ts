import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { signal } from '@angular/core';
import { PhysicianListComponent } from './physician-list.component';
import { PhysiciansService } from './physicians.service';
import { ToastService } from '../../../ui/toast/toast.service';
import type { PhysicianSearchResult } from '../../../core/domain/physician-search-result';

describe('PhysicianListComponent (integration)', () => {
  let fixture: ComponentFixture<PhysicianListComponent>;
  let mockService: Partial<PhysiciansService>;
  let physiciansSignal: ReturnType<typeof signal<PhysicianSearchResult[]>>;
  let loadingSignal: ReturnType<typeof signal<boolean>>;

  const activeAndInactive: PhysicianSearchResult[] = [
    { id: 'p-1', fullName: 'Ana Lima', licenceNumber: 'CRM-1234', status: 'Active' },
    { id: 'p-2', fullName: 'Bruno Costa', licenceNumber: null, status: 'Inactive' },
  ];

  function makePhysicians(n: number): PhysicianSearchResult[] {
    return Array.from({ length: n }, (_, i) => ({
      id: `p-${i + 1}`,
      fullName: `Physician ${i + 1}`,
      licenceNumber: `CRM-${i + 1}`,
      status: 'Active' as const,
    }));
  }

  function host(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  function rows(): HTMLElement[] {
    return Array.from(host().querySelectorAll('[data-testid="physician-list-row"]'));
  }

  function click(selector: string, within: ParentNode = host()): void {
    within.querySelector<HTMLButtonElement>(selector)!.click();
    fixture.detectChanges();
  }

  /** Opens the row's action menu — its items only exist in the DOM while it is open. */
  function openActions(row: HTMLElement): void {
    click('[data-testid="physician-actions-trigger"]', row);
  }

  beforeEach(async () => {
    physiciansSignal = signal<PhysicianSearchResult[]>([]);
    loadingSignal = signal(false);

    mockService = {
      physicians: physiciansSignal,
      loading: loadingSignal,
      listPhysicians: vi.fn().mockResolvedValue(undefined),
      register: vi.fn().mockResolvedValue(undefined),
      update: vi.fn().mockResolvedValue(undefined),
      deactivate: vi.fn().mockResolvedValue(undefined),
      reactivate: vi.fn().mockResolvedValue(undefined),
    };

    await TestBed.configureTestingModule({
      imports: [PhysicianListComponent],
      providers: [{ provide: PhysiciansService, useValue: mockService }],
    }).compileComponents();

    fixture = TestBed.createComponent(PhysicianListComponent);
    fixture.detectChanges();
  });

  afterEach(() => TestBed.resetTestingModule());

  it('shows skeleton rows while the registry is loading', () => {
    loadingSignal.set(true);
    fixture.detectChanges();

    expect(
      host().querySelectorAll('[data-testid="physicians-skeleton-row"]').length,
    ).toBeGreaterThan(0);
    expect(rows()).toHaveLength(0);
    expect(host().querySelector('[data-testid="empty-state"]')).toBeNull();
  });

  it('shows the empty-state (not skeletons) once loading completes with no data', () => {
    loadingSignal.set(false);
    physiciansSignal.set([]);
    fixture.detectChanges();

    expect(host().querySelectorAll('[data-testid="physicians-skeleton-row"]')).toHaveLength(0);
    expect(host().querySelector('[data-testid="empty-state"]')).not.toBeNull();
  });

  it('lists every physician with its status, inactive ones included', () => {
    physiciansSignal.set(activeAndInactive);
    fixture.detectChanges();

    expect(rows()).toHaveLength(2);
    expect(rows()[0].textContent).toContain('Ana Lima');
    expect(rows()[0].textContent).toContain('CRM-1234');
    expect(
      rows()[1].querySelector('[data-testid="physician-status-badge"]')?.textContent?.trim(),
    ).toBe('Inactive');
  });

  it('renders a placeholder rather than a blank cell for a physician with no licence', () => {
    physiciansSignal.set(activeAndInactive);
    fixture.detectChanges();

    expect(
      rows()[1].querySelector('[data-testid="physician-licence-cell"]')?.textContent?.trim(),
    ).toBe('—');
  });

  it('paginates the registry, one page at a time', () => {
    physiciansSignal.set(makePhysicians(23));
    fixture.detectChanges();

    expect(rows()).toHaveLength(10);
    click('[data-testid="physicians-pagination-next"]');

    expect(rows()).toHaveLength(10);
    expect(rows()[0].textContent).toContain('Physician 11');
  });

  it('hides pagination when a single page of physicians fits', () => {
    physiciansSignal.set(activeAndInactive);
    fixture.detectChanges();

    expect(host().querySelector('[data-testid="physicians-pagination"]')).toBeNull();
  });

  it('opens an empty registration form inside a dialog', () => {
    click('[data-testid="create-physician-btn"]');

    const form = host().querySelector('[data-testid="physician-form"]')!;
    expect(form).not.toBeNull();
    expect(
      form.querySelector<HTMLInputElement>('[data-testid="physician-full-name-input"]')!.value,
    ).toBe('');
  });

  it('opens the form prefilled with the row being edited', async () => {
    physiciansSignal.set(activeAndInactive);
    fixture.detectChanges();

    openActions(rows()[0]);
    click('[data-testid="physician-action-edit"]');
    // NgModel pushes the initial value to the DOM in a microtask, not during the pass
    // that created the control.
    await fixture.whenStable();

    const form = host().querySelector('[data-testid="physician-form"]')!;
    expect(
      form.querySelector<HTMLInputElement>('[data-testid="physician-full-name-input"]')!.value,
    ).toBe('Ana Lima');
    expect(
      form.querySelector<HTMLInputElement>('[data-testid="physician-licence-input"]')!.value,
    ).toBe('CRM-1234');
  });

  it('updates the edited physician instead of registering a new one', async () => {
    physiciansSignal.set(activeAndInactive);
    fixture.detectChanges();

    openActions(rows()[0]);
    click('[data-testid="physician-action-edit"]');
    click('[data-testid="physician-form-submit-btn"]');
    await fixture.whenStable();

    expect(mockService.update).toHaveBeenCalledWith('p-1', {
      fullName: 'Ana Lima',
      licenceNumber: 'CRM-1234',
    });
    expect(mockService.register).not.toHaveBeenCalled();
  });

  // Deactivate and reactivate are opposite ends of one switch — offering both at once
  // would let an admin fire the transition the row is not in a state to accept.
  it('offers deactivate on an active physician and reactivate on an inactive one', () => {
    physiciansSignal.set(activeAndInactive);
    fixture.detectChanges();

    openActions(rows()[0]);
    expect(host().querySelector('[data-testid="physician-action-deactivate"]')).not.toBeNull();
    expect(host().querySelector('[data-testid="physician-action-reactivate"]')).toBeNull();

    openActions(rows()[0]);
    openActions(rows()[1]);
    expect(host().querySelector('[data-testid="physician-action-reactivate"]')).not.toBeNull();
    expect(host().querySelector('[data-testid="physician-action-deactivate"]')).toBeNull();
  });

  it('confirms before deactivating a physician', () => {
    physiciansSignal.set(activeAndInactive);
    fixture.detectChanges();

    openActions(rows()[0]);
    click('[data-testid="physician-action-deactivate"]');

    const dialog = host().querySelector('[data-testid="physician-status-dialog"]')!;
    expect(dialog.textContent).toContain('Ana Lima');
    expect(mockService.deactivate).not.toHaveBeenCalled();
  });

  it('deactivates the physician once the confirmation is accepted', async () => {
    physiciansSignal.set(activeAndInactive);
    fixture.detectChanges();

    openActions(rows()[0]);
    click('[data-testid="physician-action-deactivate"]');
    click('[data-testid="confirm-physician-status-btn"]');
    await fixture.whenStable();

    expect(mockService.deactivate).toHaveBeenCalledWith('p-1');
  });

  it('reactivates the physician once the confirmation is accepted', async () => {
    physiciansSignal.set(activeAndInactive);
    fixture.detectChanges();

    openActions(rows()[1]);
    click('[data-testid="physician-action-reactivate"]');
    click('[data-testid="confirm-physician-status-btn"]');
    await fixture.whenStable();

    expect(mockService.reactivate).toHaveBeenCalledWith('p-2');
  });

  it('does not change the status when the confirmation is cancelled', async () => {
    physiciansSignal.set(activeAndInactive);
    fixture.detectChanges();

    openActions(rows()[0]);
    click('[data-testid="physician-action-deactivate"]');
    click('[data-testid="cancel-physician-status-btn"]');
    await fixture.whenStable();

    expect(mockService.deactivate).not.toHaveBeenCalled();
  });

  // The same trap the user list fell into on its first live run: an unhandled rejection
  // closed the dialog and a refused change looked exactly like a successful one.
  it('surfaces an error toast when the status change is rejected', async () => {
    mockService.deactivate = vi.fn().mockRejectedValue(new Error('Physician is already inactive'));
    physiciansSignal.set(activeAndInactive);
    fixture.detectChanges();

    openActions(rows()[0]);
    click('[data-testid="physician-action-deactivate"]');
    click('[data-testid="confirm-physician-status-btn"]');
    await fixture.whenStable();

    const toasts = TestBed.inject(ToastService).toasts();
    expect(toasts.find(t => t.testId === 'physician-status-error-toast')?.variant).toBe('error');
    expect(toasts.some(t => t.testId === 'physician-status-toast')).toBe(false);
  });

  it('confirms a completed status change with a success toast', async () => {
    physiciansSignal.set(activeAndInactive);
    fixture.detectChanges();

    openActions(rows()[0]);
    click('[data-testid="physician-action-deactivate"]');
    click('[data-testid="confirm-physician-status-btn"]');
    await fixture.whenStable();

    const toasts = TestBed.inject(ToastService).toasts();
    expect(toasts.find(t => t.testId === 'physician-status-toast')?.variant).toBe('success');
  });
});
