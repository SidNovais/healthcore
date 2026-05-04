import { TestBed } from '@angular/core/testing';
import { OrdersService } from './orders.service';
import { ORDERS_PORT, IOrdersPort } from '../../core/application/i-orders-port';
import type { OrderSummary } from '../../core/domain/order-summary';

describe('OrdersService', () => {
  let service: OrdersService;
  let mockPort: IOrdersPort;

  const createdOrder: OrderSummary = { orderId: 'order-uuid-1', patientId: 'patient-uuid-1' };

  beforeEach(() => {
    mockPort = {
      createOrder: vi.fn(),
      requestExam: vi.fn(),
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
});
