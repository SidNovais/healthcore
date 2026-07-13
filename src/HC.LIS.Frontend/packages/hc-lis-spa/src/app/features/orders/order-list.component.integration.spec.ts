import { TestBed } from '@angular/core/testing';
import { ComponentFixture } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { signal } from '@angular/core';
import { OrderListComponent } from './order-list.component';
import { OrdersService } from './orders.service';
import type { OrderListItem } from '../../core/domain/order-list-item';

describe('OrderListComponent (integration)', () => {
  let fixture: ComponentFixture<OrderListComponent>;
  let mockService: Partial<OrdersService>;
  let orderListSignal: ReturnType<typeof signal<OrderListItem[]>>;
  let loadingSignal: ReturnType<typeof signal<boolean>>;

  const twoOrders: OrderListItem[] = [
    { orderId: 'o-1', patientId: 'p-1', patientName: 'Ana Souza', requestedBy: 'u-1', orderPriority: 'Routine', requestedAt: '2026-05-11T00:00:00Z', itemCount: 2 },
    { orderId: 'o-2', patientId: 'p-2', patientName: 'João Lima', requestedBy: 'u-1', orderPriority: 'Urgent', requestedAt: '2026-05-12T00:00:00Z', itemCount: 1 },
  ];

  beforeEach(async () => {
    orderListSignal = signal<OrderListItem[]>([]);
    loadingSignal = signal(false);

    mockService = {
      orderList: orderListSignal,
      loadingList: loadingSignal,
      loadOrderList: vi.fn().mockResolvedValue(undefined),
    };

    await TestBed.configureTestingModule({
      imports: [OrderListComponent],
      providers: [
        { provide: OrdersService, useValue: mockService },
        provideRouter([]),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(OrderListComponent);
    fixture.detectChanges();
  });

  afterEach(() => TestBed.resetTestingModule());

  function host(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  it('shows skeleton rows while the list is loading', () => {
    loadingSignal.set(true);
    fixture.detectChanges();

    const skeletons = host().querySelectorAll('[data-testid="order-list-skeleton-row"]');
    expect(skeletons.length).toBeGreaterThan(0);
    expect(host().querySelectorAll('[data-testid="order-list-row"]')).toHaveLength(0);
  });

  it('does not show skeleton rows once loading completes with data', () => {
    loadingSignal.set(false);
    orderListSignal.set(twoOrders);
    fixture.detectChanges();

    expect(host().querySelectorAll('[data-testid="order-list-skeleton-row"]')).toHaveLength(0);
    expect(host().querySelectorAll('[data-testid="order-list-row"]')).toHaveLength(2);
  });

  it('shows the empty-state (not skeletons) once loading completes with no data', () => {
    loadingSignal.set(false);
    orderListSignal.set([]);
    fixture.detectChanges();

    expect(host().querySelectorAll('[data-testid="order-list-skeleton-row"]')).toHaveLength(0);
    expect(host().textContent).toContain('No orders found.');
  });
});
