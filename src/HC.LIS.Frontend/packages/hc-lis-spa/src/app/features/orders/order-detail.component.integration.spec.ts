import { TestBed, ComponentFixture } from '@angular/core/testing';
import { signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { OrderDetailComponent } from './order-detail.component';
import { OrdersService } from './orders.service';
import { ToastService } from '../../ui/toast/toast.service';
import type { OrderDetails, ExamItem } from '../../core/domain/order-details';

describe('OrderDetailComponent (integration)', () => {
  let fixture: ComponentFixture<OrderDetailComponent>;
  let mockService: Partial<OrdersService>;
  let mockToast: { show: ReturnType<typeof vi.fn> };
  let detailsSignal: ReturnType<typeof signal<OrderDetails | null>>;

  const orderId = 'order-abc-123';

  const requestedItem: ExamItem = {
    orderItemId: 'item-1',
    specimenMnemonic: 'BLD',
    materialType: 'Whole Blood',
    containerType: 'EDTA',
    additive: 'K2EDTA',
    processingType: 'Standard',
    storageCondition: 'RT',
    reasonForRejection: null,
    status: 'Requested',
    requestedAt: '2026-05-14T08:00:00Z',
    canceledAt: null,
    onHoldAt: null,
    acceptedAt: null,
    rejectedAt: null,
    inProgressAt: null,
    partiallyCompletedAt: null,
    completedAt: null,
  };

  const acceptedItem: ExamItem = {
    ...requestedItem,
    orderItemId: 'item-2',
    status: 'Accepted',
    acceptedAt: '2026-05-14T09:00:00Z',
  };

  const baseOrder: OrderDetails = {
    orderId,
    patientId: 'patient-1',
    requestedBy: 'Dr. Smith',
    orderPriority: 'Routine',
    requestedAt: '2026-05-14T07:00:00Z',
    items: [requestedItem, acceptedItem],
  };

  beforeEach(async () => {
    detailsSignal = signal<OrderDetails | null>(null);

    mockService = {
      orderDetails: detailsSignal,
      loadOrderDetails: vi.fn().mockResolvedValue(undefined),
      acceptExam: vi.fn().mockResolvedValue(undefined),
      cancelExam: vi.fn().mockResolvedValue(undefined),
      rejectExam: vi.fn().mockResolvedValue(undefined),
      placeExamOnHold: vi.fn().mockResolvedValue(undefined),
      applyExamStatus: vi.fn(),
    };

    mockToast = { show: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [OrderDetailComponent],
      providers: [
        { provide: OrdersService, useValue: mockService },
        { provide: ToastService, useValue: mockToast },
        { provide: ActivatedRoute, useValue: { snapshot: { params: { id: orderId } } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(OrderDetailComponent);
    fixture.detectChanges();
  });

  afterEach(() => TestBed.resetTestingModule());

  it('calls loadOrderDetails() with the route id on init', () => {
    expect(mockService.loadOrderDetails).toHaveBeenCalledWith(orderId);
  });

  it('renders the order-detail container', () => {
    const el = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="order-detail"]');
    expect(el).not.toBeNull();
  });

  it('renders exam-items-table when order details are loaded', () => {
    detailsSignal.set(baseOrder);
    fixture.detectChanges();

    const table = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="exam-items-table"]');
    expect(table).not.toBeNull();
  });

  it('renders one row per exam item', () => {
    detailsSignal.set(baseOrder);
    fixture.detectChanges();

    const rows = (fixture.nativeElement as HTMLElement).querySelectorAll('[data-testid="exam-item-row"]');
    expect(rows).toHaveLength(2);
  });

  it('shows an hc-empty in the empty-items cell when items array is empty', () => {
    detailsSignal.set({ ...baseOrder, items: [] });
    fixture.detectChanges();

    const empty = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="empty-items"]');
    expect(empty).not.toBeNull();
    // The bare string is replaced by the hc-empty primitive for a consistent empty state.
    expect(empty!.querySelector('hc-empty')).not.toBeNull();
  });

  /** Opens the first exam item's row action menu (actions live inside it now). */
  function openActionsMenu(): void {
    (fixture.nativeElement as HTMLElement)
      .querySelector<HTMLButtonElement>('[data-testid="exam-actions-trigger"]')!
      .click();
    fixture.detectChanges();
  }

  it('exposes the row actions inside a dropdown menu', () => {
    detailsSignal.set({ ...baseOrder, items: [requestedItem] });
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;
    const trigger = host.querySelector<HTMLButtonElement>('[data-testid="exam-actions-trigger"]');
    expect(trigger).not.toBeNull();
    expect(trigger!.getAttribute('aria-haspopup')).toBe('menu');
    // Items are only in the DOM once the menu opens.
    expect(host.querySelector('[data-testid="accept-btn"]')).toBeNull();

    openActionsMenu();
    expect(host.querySelector('[role="menu"]')).not.toBeNull();
    expect(host.querySelector('[data-testid="accept-btn"]')).not.toBeNull();
  });

  it('shows the Accept action for a Requested item', () => {
    detailsSignal.set({ ...baseOrder, items: [requestedItem] });
    fixture.detectChanges();
    openActionsMenu();

    expect((fixture.nativeElement as HTMLElement).querySelector('[data-testid="accept-btn"]')).not.toBeNull();
  });

  it('does not show the Accept action for an Accepted item', () => {
    detailsSignal.set({ ...baseOrder, items: [acceptedItem] });
    fixture.detectChanges();
    openActionsMenu();

    expect((fixture.nativeElement as HTMLElement).querySelector('[data-testid="accept-btn"]')).toBeNull();
  });

  it('clicking Accept calls ordersService.acceptExam with orderId and itemId', async () => {
    detailsSignal.set({ ...baseOrder, items: [requestedItem] });
    fixture.detectChanges();
    openActionsMenu();

    (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('[data-testid="accept-btn"]')!.click();
    await fixture.whenStable();

    expect(mockService.acceptExam).toHaveBeenCalledWith(orderId, 'item-1');
  });

  it('clicking Accept shows a success confirmation toast', async () => {
    detailsSignal.set({ ...baseOrder, items: [requestedItem] });
    fixture.detectChanges();
    openActionsMenu();

    (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('[data-testid="accept-btn"]')!.click();
    await fixture.whenStable();

    expect(mockToast.show).toHaveBeenCalledWith('Exam accepted', expect.objectContaining({ variant: 'success' }));
  });

  it('clicking Accept optimistically patches the item status without a reload', async () => {
    detailsSignal.set({ ...baseOrder, items: [requestedItem] });
    fixture.detectChanges();
    openActionsMenu();

    (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('[data-testid="accept-btn"]')!.click();
    await fixture.whenStable();

    expect(mockService.applyExamStatus).toHaveBeenCalledWith('item-1', 'Accepted');
    // Init-only load; the live feed carries subsequent changes, so no post-action re-fetch.
    expect(mockService.loadOrderDetails).toHaveBeenCalledTimes(1);
  });

  it('selecting Reject opens the reject-reason-form in a dialog', () => {
    detailsSignal.set({ ...baseOrder, items: [requestedItem] });
    fixture.detectChanges();
    openActionsMenu();

    (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('[data-testid="reject-btn"]')!.click();
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;
    expect(host.querySelector('[data-testid="reject-dialog"]')).not.toBeNull();
    expect(host.querySelector('[data-testid="reject-reason-form"]')).not.toBeNull();
  });

  it('clicking Confirm Reject calls ordersService.rejectExam with reason', async () => {
    detailsSignal.set({ ...baseOrder, items: [requestedItem] });
    fixture.detectChanges();
    openActionsMenu();

    (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('[data-testid="reject-btn"]')!.click();
    fixture.detectChanges();

    const input = (fixture.nativeElement as HTMLElement).querySelector<HTMLInputElement>('[data-testid="reject-reason-input"]');
    input!.value = 'Hemolyzed sample';
    input!.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('[data-testid="confirm-reject-btn"]')!.click();
    await fixture.whenStable();

    expect(mockService.rejectExam).toHaveBeenCalledWith(orderId, 'item-1', 'Hemolyzed sample');
  });

  it('selecting On Hold opens the on-hold-reason-form in a dialog', () => {
    detailsSignal.set({ ...baseOrder, items: [requestedItem] });
    fixture.detectChanges();
    openActionsMenu();

    (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('[data-testid="on-hold-btn"]')!.click();
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;
    expect(host.querySelector('[data-testid="on-hold-dialog"]')).not.toBeNull();
    expect(host.querySelector('[data-testid="on-hold-reason-form"]')).not.toBeNull();
  });

  it('trails back to the orders list via a breadcrumb', () => {
    const crumb = (fixture.nativeElement as HTMLElement).querySelector(
      '[data-testid="order-breadcrumb"]',
    );

    expect(crumb).not.toBeNull();
    expect(
      crumb!.querySelector('[data-testid="order-breadcrumb-link-0"]')?.getAttribute('href'),
    ).toBe('/orders');
    expect(
      crumb!.querySelector('[data-testid="order-breadcrumb-page"]')?.textContent?.trim(),
    ).toBe('Order Detail');
  });
});
