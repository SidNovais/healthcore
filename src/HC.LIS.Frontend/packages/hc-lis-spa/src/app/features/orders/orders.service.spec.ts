import { TestBed } from '@angular/core/testing';
import { OrdersService } from './orders.service';
import { ORDERS_PORT, IOrdersPort } from '../../core/application/i-orders-port';
import { RealtimeClient } from '../../core/infrastructure/realtime/realtime-client';
import type { OrderSummary } from '../../core/domain/order-summary';
import type { OrderListItem } from '../../core/domain/order-list-item';
import type { OrderDetails } from '../../core/domain/order-details';

describe('OrdersService', () => {
  let service: OrdersService;
  let mockPort: IOrdersPort;
  let ordersHandler: (payload: unknown) => void;

  const createdOrder: OrderSummary = { orderId: 'order-uuid-1', patientId: 'patient-uuid-1' };

  const sampleOrderListItem: OrderListItem = {
    orderId: 'order-uuid-1',
    patientId: 'patient-uuid-1',
    patientName: 'Ana Souza',
    requestedBy: 'user-uuid-1',
    orderPriority: 'Routine',
    requestedAt: '2026-05-11T00:00:00Z',
    itemCount: 2,
  };

  const sampleOrderDetails: OrderDetails = {
    orderId: 'order-uuid-1',
    patientId: 'patient-uuid-1',
    requestedBy: 'user-uuid-1',
    orderPriority: 'Routine',
    requestedAt: '2026-05-11T00:00:00Z',
    items: [],
  };

  beforeEach(() => {
    mockPort = {
      createOrder: vi.fn(),
      requestExam: vi.fn(),
      getOrderList: vi.fn(),
      getOrderDetails: vi.fn(),
      acceptExam: vi.fn(),
      cancelExam: vi.fn(),
      rejectExam: vi.fn(),
      placeExamOnHold: vi.fn(),
    };

    const realtimeStub = {
      on: vi.fn((topic: string, handler: (p: unknown) => void) => {
        if (topic === 'orders') ordersHandler = handler;
        return () => undefined;
      }),
    };

    TestBed.configureTestingModule({
      providers: [
        OrdersService,
        { provide: ORDERS_PORT, useValue: mockPort },
        { provide: RealtimeClient, useValue: realtimeStub },
      ],
    });

    service = TestBed.inject(OrdersService);
  });

  it('order signal starts as null', () => {
    expect(service.order()).toBeNull();
  });

  it('createOrder() calls port.createOrder with patientId and requestedBy', async () => {
    vi.mocked(mockPort.createOrder).mockResolvedValue(createdOrder);

    await service.createOrder({ patientId: 'patient-uuid-1', requestedBy: 'user-uuid-1' });

    expect(mockPort.createOrder).toHaveBeenCalledWith({ patientId: 'patient-uuid-1', requestedBy: 'user-uuid-1' });
  });

  it('createOrder() sets order signal on success', async () => {
    vi.mocked(mockPort.createOrder).mockResolvedValue(createdOrder);

    await service.createOrder({ patientId: 'patient-uuid-1', requestedBy: 'user-uuid-1' });

    expect(service.order()).toEqual(createdOrder);
  });

  it('createOrder() propagates error without setting signal', async () => {
    vi.mocked(mockPort.createOrder).mockRejectedValue(new Error('conflict'));

    await expect(service.createOrder({ patientId: 'patient-uuid-1', requestedBy: 'user-uuid-1' })).rejects.toThrow('conflict');
    expect(service.order()).toBeNull();
  });

  it('requestExam() calls port.requestExam with orderId and exam params', async () => {
    vi.mocked(mockPort.requestExam).mockResolvedValue();

    await service.requestExam('order-uuid-1', {
      examMnemonic: 'GLU',
      specimenMnemonic: 'SER',
      materialType: 'Serum',
      containerType: 'RedTop',
      additive: 'None',
      processingType: 'Centrifuge',
      storageCondition: 'RoomTemp',
    });

    expect(mockPort.requestExam).toHaveBeenCalledWith('order-uuid-1', {
      examMnemonic: 'GLU',
      specimenMnemonic: 'SER',
      materialType: 'Serum',
      containerType: 'RedTop',
      additive: 'None',
      processingType: 'Centrifuge',
      storageCondition: 'RoomTemp',
    });
  });

  it('requestExam() does not change order signal', async () => {
    vi.mocked(mockPort.createOrder).mockResolvedValue(createdOrder);
    await service.createOrder({ patientId: 'patient-uuid-1', requestedBy: 'user-uuid-1' });
    vi.mocked(mockPort.requestExam).mockResolvedValue();

    await service.requestExam('order-uuid-1', {
      examMnemonic: 'GLU',
      specimenMnemonic: 'SER',
      materialType: 'Serum',
      containerType: 'RedTop',
      additive: 'None',
      processingType: 'Centrifuge',
      storageCondition: 'RoomTemp',
    });

    expect(service.order()).toEqual(createdOrder);
  });

  it('orderList signal starts as empty array', () => {
    expect(service.orderList()).toEqual([]);
  });

  it('loadOrderList() calls port.getOrderList and sets orderList signal', async () => {
    vi.mocked(mockPort.getOrderList).mockResolvedValue([sampleOrderListItem]);

    await service.loadOrderList();

    expect(mockPort.getOrderList).toHaveBeenCalled();
    expect(service.orderList()).toEqual([sampleOrderListItem]);
  });

  it('loadingList signal starts as false', () => {
    expect(service.loadingList()).toBe(false);
  });

  it('loadOrderList() sets loadingList true while the request is in flight', async () => {
    let resolve!: (items: OrderListItem[]) => void;
    vi.mocked(mockPort.getOrderList).mockReturnValue(new Promise((r) => { resolve = r; }));

    const pending = service.loadOrderList();
    expect(service.loadingList()).toBe(true);

    resolve([sampleOrderListItem]);
    await pending;
    expect(service.loadingList()).toBe(false);
  });

  it('loadOrderList() resets loadingList to false when the port rejects', async () => {
    vi.mocked(mockPort.getOrderList).mockRejectedValue(new Error('boom'));

    await expect(service.loadOrderList()).rejects.toThrow('boom');

    expect(service.loadingList()).toBe(false);
  });

  it('orderDetails signal starts as null', () => {
    expect(service.orderDetails()).toBeNull();
  });

  it('loadOrderDetails() calls port.getOrderDetails with orderId and sets orderDetails signal', async () => {
    vi.mocked(mockPort.getOrderDetails).mockResolvedValue(sampleOrderDetails);

    await service.loadOrderDetails('order-uuid-1');

    expect(mockPort.getOrderDetails).toHaveBeenCalledWith('order-uuid-1');
    expect(service.orderDetails()).toEqual(sampleOrderDetails);
  });

  it('acceptExam() calls port.acceptExam with orderId and itemId', async () => {
    vi.mocked(mockPort.acceptExam).mockResolvedValue();

    await service.acceptExam('order-uuid-1', 'item-uuid-1');

    expect(mockPort.acceptExam).toHaveBeenCalledWith('order-uuid-1', 'item-uuid-1');
  });

  it('cancelExam() calls port.cancelExam with orderId and itemId', async () => {
    vi.mocked(mockPort.cancelExam).mockResolvedValue();

    await service.cancelExam('order-uuid-1', 'item-uuid-1');

    expect(mockPort.cancelExam).toHaveBeenCalledWith('order-uuid-1', 'item-uuid-1');
  });

  it('rejectExam() calls port.rejectExam with orderId, itemId, and reason', async () => {
    vi.mocked(mockPort.rejectExam).mockResolvedValue();

    await service.rejectExam('order-uuid-1', 'item-uuid-1', 'Insufficient sample');

    expect(mockPort.rejectExam).toHaveBeenCalledWith('order-uuid-1', 'item-uuid-1', 'Insufficient sample');
  });

  it('placeExamOnHold() calls port.placeExamOnHold with orderId, itemId, and reason', async () => {
    vi.mocked(mockPort.placeExamOnHold).mockResolvedValue();

    await service.placeExamOnHold('order-uuid-1', 'item-uuid-1', 'Awaiting reagent');

    expect(mockPort.placeExamOnHold).toHaveBeenCalledWith('order-uuid-1', 'item-uuid-1', 'Awaiting reagent');
  });

  it('subscribes to the orders topic on construction', () => {
    expect(ordersHandler).toBeDefined();
  });

  it('applyExamStatus patches the matching exam item in place', () => {
    service.orderDetails.set({
      ...sampleOrderDetails,
      items: [
        { orderItemId: 'oi-1', specimenMnemonic: 'SER', materialType: 'Serum', containerType: 'Tube', additive: '', processingType: '', storageCondition: '', reasonForRejection: null, status: 'Requested', requestedAt: '2026-05-11T00:00:00Z', canceledAt: null, onHoldAt: null, acceptedAt: null, rejectedAt: null, inProgressAt: null, partiallyCompletedAt: null, completedAt: null },
        { orderItemId: 'oi-2', specimenMnemonic: 'SER', materialType: 'Serum', containerType: 'Tube', additive: '', processingType: '', storageCondition: '', reasonForRejection: null, status: 'Requested', requestedAt: '2026-05-11T00:00:00Z', canceledAt: null, onHoldAt: null, acceptedAt: null, rejectedAt: null, inProgressAt: null, partiallyCompletedAt: null, completedAt: null },
      ],
    });

    service.applyExamStatus('oi-2', 'Canceled');

    const items = service.orderDetails()!.items;
    expect(items.find((i) => i.orderItemId === 'oi-2')!.status).toBe('Canceled');
    expect(items.find((i) => i.orderItemId === 'oi-1')!.status).toBe('Requested');
  });

  it('applyExamStatus is a no-op when no order detail is loaded', () => {
    service.applyExamStatus('oi-1', 'Canceled');
    expect(service.orderDetails()).toBeNull();
  });

  it('applies a status frame pushed over the live feed', () => {
    service.orderDetails.set({
      ...sampleOrderDetails,
      items: [
        { orderItemId: 'oi-1', specimenMnemonic: 'SER', materialType: 'Serum', containerType: 'Tube', additive: '', processingType: '', storageCondition: '', reasonForRejection: null, status: 'Requested', requestedAt: '2026-05-11T00:00:00Z', canceledAt: null, onHoldAt: null, acceptedAt: null, rejectedAt: null, inProgressAt: null, partiallyCompletedAt: null, completedAt: null },
      ],
    });

    ordersHandler({ op: 'status', scope: 'exam', orderItemId: 'oi-1', status: 'Accepted' });

    expect(service.orderDetails()!.items[0].status).toBe('Accepted');
  });

  it('ignores live frames it does not understand', () => {
    service.orderDetails.set({
      ...sampleOrderDetails,
      items: [
        { orderItemId: 'oi-1', specimenMnemonic: 'SER', materialType: 'Serum', containerType: 'Tube', additive: '', processingType: '', storageCondition: '', reasonForRejection: null, status: 'Requested', requestedAt: '2026-05-11T00:00:00Z', canceledAt: null, onHoldAt: null, acceptedAt: null, rejectedAt: null, inProgressAt: null, partiallyCompletedAt: null, completedAt: null },
      ],
    });

    ordersHandler({ op: 'noise' });

    expect(service.orderDetails()!.items[0].status).toBe('Requested');
  });

});
