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

  function makeOrders(n: number): OrderListItem[] {
    return Array.from({ length: n }, (_, i) => ({
      orderId: `o-${i + 1}`,
      patientId: `p-${i + 1}`,
      patientName: `Patient ${i + 1}`,
      requestedBy: 'u-1',
      orderPriority: 'Routine',
      requestedAt: '2026-05-11T00:00:00Z',
      itemCount: 1,
    }));
  }

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

  function rows(): HTMLElement[] {
    return Array.from(host().querySelectorAll('[data-testid="order-list-row"]'));
  }

  function firstPatientNames(): string[] {
    return rows().map(
      r => r.querySelector('[data-testid="patient-name-cell"]')!.textContent!.trim(),
    );
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

  it('paginates the list, showing one page of rows at a time', () => {
    orderListSignal.set(makeOrders(23));
    fixture.detectChanges();

    expect(rows()).toHaveLength(10);
    expect(firstPatientNames()[0]).toBe('Patient 1');
    expect(host().querySelector('[data-testid="order-list-pagination"]')).not.toBeNull();

    host().querySelector<HTMLButtonElement>('[data-testid="order-list-pagination-next"]')!.click();
    fixture.detectChanges();
    expect(rows()).toHaveLength(10);
    expect(firstPatientNames()[0]).toBe('Patient 11');

    host().querySelector<HTMLButtonElement>('[data-testid="order-list-pagination-page-3"]')!.click();
    fixture.detectChanges();
    expect(rows()).toHaveLength(3);
    expect(firstPatientNames()[0]).toBe('Patient 21');
  });

  it('hides pagination when the list fits on a single page', () => {
    orderListSignal.set(twoOrders);
    fixture.detectChanges();

    expect(host().querySelector('[data-testid="order-list-pagination"]')).toBeNull();
  });

  it('sorts by patient name and toggles direction on repeated header clicks', () => {
    orderListSignal.set([
      { ...twoOrders[0], orderId: 'c', patientName: 'Cora' },
      { ...twoOrders[0], orderId: 'a', patientName: 'Ana' },
      { ...twoOrders[0], orderId: 'b', patientName: 'Bea' },
    ]);
    fixture.detectChanges();

    const header = host().querySelector<HTMLButtonElement>('[data-testid="order-list-sort-patient"]')!;
    const th = header.closest('th')!;

    header.click();
    fixture.detectChanges();
    expect(firstPatientNames()).toEqual(['Ana', 'Bea', 'Cora']);
    expect(th.getAttribute('aria-sort')).toBe('ascending');

    header.click();
    fixture.detectChanges();
    expect(firstPatientNames()).toEqual(['Cora', 'Bea', 'Ana']);
    expect(th.getAttribute('aria-sort')).toBe('descending');
  });

  it('exposes a per-row action menu whose trigger opens without navigating the row', () => {
    orderListSignal.set(twoOrders);
    fixture.detectChanges();

    const trigger = host().querySelector<HTMLButtonElement>('[data-testid="order-actions-trigger"]')!;
    expect(trigger).not.toBeNull();
    expect(trigger.getAttribute('aria-haspopup')).toBe('menu');

    trigger.click();
    fixture.detectChanges();

    // Menu opened in place (row not navigated away) and offers a View action.
    expect(host().querySelector('[role="menu"]')).not.toBeNull();
    expect(host().querySelector('[data-testid="order-action-view"]')).not.toBeNull();
    expect(rows()).toHaveLength(2);
  });
});
