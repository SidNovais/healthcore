import { TestBed } from '@angular/core/testing';
import { ComponentFixture } from '@angular/core/testing';
import { signal } from '@angular/core';
import { NewOrderComponent } from './new-order.component';
import { OrdersService } from './orders.service';
import { AuthService } from '../../core/application/auth.service';
import type { OrderSummary } from '../../core/domain/order-summary';
import type { UserSession } from '../../core/domain/user-session';

describe('NewOrderComponent (integration)', () => {
  let fixture: ComponentFixture<NewOrderComponent>;
  let mockOrdersService: Partial<OrdersService>;
  let mockAuthService: Partial<AuthService>;
  let orderSignal: ReturnType<typeof signal<OrderSummary | null>>;

  const receptionist: UserSession = { userId: 'user-uuid-1', userName: 'rcpt@hclis.local', role: 'Receptionist' };
  const createdOrder: OrderSummary = { orderId: 'order-uuid-1', patientId: 'patient-uuid-1' };

  beforeEach(async () => {
    orderSignal = signal<OrderSummary | null>(null);

    mockOrdersService = {
      order: orderSignal,
      createOrder: vi.fn(),
      requestExam: vi.fn(),
    };

    mockAuthService = {
      currentUser: signal<UserSession | null>(receptionist),
    };

    await TestBed.configureTestingModule({
      imports: [NewOrderComponent],
      providers: [
        { provide: OrdersService, useValue: mockOrdersService },
        { provide: AuthService, useValue: mockAuthService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(NewOrderComponent);
    fixture.detectChanges();
  });

  afterEach(() => TestBed.resetTestingModule());

  function getElement<T extends HTMLElement>(selector: string): T | null {
    return (fixture.nativeElement as HTMLElement).querySelector<T>(selector);
  }

  function fillPatientIdAndSubmit(patientId: string) {
    const input = getElement<HTMLInputElement>('[data-testid="patient-id-input"]')!;
    input.value = patientId;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    const btn = getElement<HTMLButtonElement>('[data-testid="create-order-btn"]')!;
    btn.click();
  }

  it('exam form section is not visible before order is created', () => {
    expect(getElement('[data-testid="exam-section"]')).toBeNull();
  });

  it('submitting patient ID calls createOrder() with the typed ID', async () => {
    vi.mocked(mockOrdersService.createOrder!).mockResolvedValue(undefined);

    fillPatientIdAndSubmit('patient-uuid-1');
    await fixture.whenStable();

    expect(mockOrdersService.createOrder).toHaveBeenCalledWith(
      expect.objectContaining({ patientId: 'patient-uuid-1' }),
    );
  });

  it('after createOrder(), exam form section becomes visible', async () => {
    vi.mocked(mockOrdersService.createOrder!).mockImplementation(async () => {
      orderSignal.set(createdOrder);
    });

    fillPatientIdAndSubmit('patient-uuid-1');
    await fixture.whenStable();
    fixture.detectChanges();

    expect(getElement('[data-testid="exam-section"]')).not.toBeNull();
  });

  it('submitting exam form calls requestExam() with orderId and exam data', async () => {
    vi.mocked(mockOrdersService.createOrder!).mockImplementation(async () => {
      orderSignal.set(createdOrder);
    });
    vi.mocked(mockOrdersService.requestExam!).mockResolvedValue(undefined);

    fillPatientIdAndSubmit('patient-uuid-1');
    await fixture.whenStable();
    fixture.detectChanges();

    const examMnemonicInput = getElement<HTMLInputElement>('[data-testid="exam-mnemonic-input"]')!;
    examMnemonicInput.value = 'GLU';
    examMnemonicInput.dispatchEvent(new Event('input'));

    const containerTypeInput = getElement<HTMLInputElement>('[data-testid="container-type-input"]')!;
    containerTypeInput.value = 'RedTop';
    containerTypeInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    const requestExamBtn = getElement<HTMLButtonElement>('[data-testid="request-exam-btn"]')!;
    requestExamBtn.click();
    await fixture.whenStable();

    expect(mockOrdersService.requestExam).toHaveBeenCalledWith(
      'order-uuid-1',
      expect.objectContaining({ examMnemonic: 'GLU' }),
    );
  });

  it('confirmation element is visible after requestExam() resolves', async () => {
    vi.mocked(mockOrdersService.createOrder!).mockImplementation(async () => {
      orderSignal.set(createdOrder);
    });
    vi.mocked(mockOrdersService.requestExam!).mockResolvedValue(undefined);

    fillPatientIdAndSubmit('patient-uuid-1');
    await fixture.whenStable();
    fixture.detectChanges();

    const examMnemonicInput = getElement<HTMLInputElement>('[data-testid="exam-mnemonic-input"]')!;
    examMnemonicInput.value = 'GLU';
    examMnemonicInput.dispatchEvent(new Event('input'));

    const containerTypeInput = getElement<HTMLInputElement>('[data-testid="container-type-input"]')!;
    containerTypeInput.value = 'RedTop';
    containerTypeInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    const requestExamBtn = getElement<HTMLButtonElement>('[data-testid="request-exam-btn"]')!;
    requestExamBtn.click();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(getElement('[data-testid="exam-added-confirmation"]')).not.toBeNull();
  });
});
