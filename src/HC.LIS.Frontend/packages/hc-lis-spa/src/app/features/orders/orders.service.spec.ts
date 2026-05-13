import { TestBed } from '@angular/core/testing';
import { OrdersService } from './orders.service';
import { ORDERS_PORT, IOrdersPort } from '../../core/application/i-orders-port';
import type { OrderSummary } from '../../core/domain/order-summary';
import type { OrderListItem } from '../../core/domain/order-list-item';
import type { OrderDetails } from '../../core/domain/order-details';

describe('OrdersService', () => {
  let service: OrdersService;
  let mockPort: IOrdersPort;

  const createdOrder: OrderSummary = { orderId: 'order-uuid-1', patientId: 'patient-uuid-1' };

  const sampleOrderListItem: OrderListItem = {
    orderId: 'order-uuid-1',
    patientId: 'patient-uuid-1',
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
      placeExamInProgress: vi.fn(),
      partiallyCompleteExam: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [OrdersService, { provide: ORDERS_PORT, useValue: mockPort }],
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

  it('placeExamInProgress() calls port.placeExamInProgress with orderId and itemId', async () => {
    vi.mocked(mockPort.placeExamInProgress).mockResolvedValue();

    await service.placeExamInProgress('order-uuid-1', 'item-uuid-1');

    expect(mockPort.placeExamInProgress).toHaveBeenCalledWith('order-uuid-1', 'item-uuid-1');
  });

  it('partiallyCompleteExam() calls port.partiallyCompleteExam with orderId and itemId', async () => {
    vi.mocked(mockPort.partiallyCompleteExam).mockResolvedValue();

    await service.partiallyCompleteExam('order-uuid-1', 'item-uuid-1');

    expect(mockPort.partiallyCompleteExam).toHaveBeenCalledWith('order-uuid-1', 'item-uuid-1');
  });
});
