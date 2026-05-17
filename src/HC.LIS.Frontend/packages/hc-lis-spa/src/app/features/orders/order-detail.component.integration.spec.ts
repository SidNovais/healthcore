import { TestBed, ComponentFixture } from '@angular/core/testing';
import { signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { OrderDetailComponent } from './order-detail.component';
import { OrdersService } from './orders.service';
import type { OrderDetails, ExamItem } from '../../core/domain/order-details';

describe('OrderDetailComponent (integration)', () => {
  let fixture: ComponentFixture<OrderDetailComponent>;
  let mockService: Partial<OrdersService>;
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
    };

    await TestBed.configureTestingModule({
      imports: [OrderDetailComponent],
      providers: [
        { provide: OrdersService, useValue: mockService },
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

  it('shows empty-items cell when items array is empty', () => {
    detailsSignal.set({ ...baseOrder, items: [] });
    fixture.detectChanges();

    const empty = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="empty-items"]');
    expect(empty).not.toBeNull();
  });

  it('shows Accept button for a Requested item', () => {
    detailsSignal.set({ ...baseOrder, items: [requestedItem] });
    fixture.detectChanges();

    const btn = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="accept-btn"]');
    expect(btn).not.toBeNull();
  });

  it('does not show Accept button for an Accepted item', () => {
    detailsSignal.set({ ...baseOrder, items: [acceptedItem] });
    fixture.detectChanges();

    const btn = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="accept-btn"]');
    expect(btn).toBeNull();
  });

  it('clicking Accept calls ordersService.acceptExam with orderId and itemId', async () => {
    detailsSignal.set({ ...baseOrder, items: [requestedItem] });
    fixture.detectChanges();

    const btn = (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('[data-testid="accept-btn"]');
    btn!.click();
    await fixture.whenStable();

    expect(mockService.acceptExam).toHaveBeenCalledWith(orderId, 'item-1');
  });

  it('clicking Accept reloads order details', async () => {
    detailsSignal.set({ ...baseOrder, items: [requestedItem] });
    fixture.detectChanges();

    (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('[data-testid="accept-btn"]')!.click();
    await fixture.whenStable();

    expect(mockService.loadOrderDetails).toHaveBeenCalledTimes(2);
  });

  it('clicking Reject button shows reject-reason-form inline', () => {
    detailsSignal.set({ ...baseOrder, items: [requestedItem] });
    fixture.detectChanges();

    (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('[data-testid="reject-btn"]')!.click();
    fixture.detectChanges();

    const form = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="reject-reason-form"]');
    expect(form).not.toBeNull();
  });

  it('clicking Confirm Reject calls ordersService.rejectExam with reason', async () => {
    detailsSignal.set({ ...baseOrder, items: [requestedItem] });
    fixture.detectChanges();

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

});
