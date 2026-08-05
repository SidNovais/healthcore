import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ComponentFixture } from '@angular/core/testing';
import { signal } from '@angular/core';
import { NewOrderComponent } from './new-order.component';
import { PatientPickerComponent } from './patient-picker.component';
import { PhysicianPickerComponent } from './physician-picker.component';
import { OrdersService } from './orders.service';
import { ToastService } from '../../ui/toast/toast.service';
import type { OrderSummary } from '../../core/domain/order-summary';
import type { PatientSearchResult } from '../../core/domain/patient-search-result';
import type { PhysicianSearchResult } from '../../core/domain/physician-search-result';

@Component({ selector: 'app-patient-picker', standalone: true, template: '' })
class StubPatientPickerComponent {}

@Component({ selector: 'app-physician-picker', standalone: true, template: '' })
class StubPhysicianPickerComponent {}

describe('NewOrderComponent (integration)', () => {
  let fixture: ComponentFixture<NewOrderComponent>;
  let mockOrdersService: Partial<OrdersService>;
  let mockToastService: { show: ReturnType<typeof vi.fn> };
  let orderSignal: ReturnType<typeof signal<OrderSummary | null>>;

  const createdOrder: OrderSummary = { orderId: 'order-uuid-1', patientId: 'patient-uuid-1' };
  const testPatient: PatientSearchResult = {
    id: 'patient-uuid-1',
    fullName: 'Test Patient',
    dateOfBirth: '1990-01-01',
    documentId: null,
    status: 'Active',
  };
  const testPhysician: PhysicianSearchResult = {
    id: 'physician-uuid-1',
    fullName: 'Ana Lima',
    licenceNumber: 'CRM-12345',
    status: 'Active',
  };

  beforeEach(async () => {
    orderSignal = signal<OrderSummary | null>(null);

    mockOrdersService = {
      order: orderSignal,
      createOrder: vi.fn(),
      requestExam: vi.fn(),
      resetOrder: vi.fn().mockImplementation(() => orderSignal.set(null)),
    };

    mockToastService = { show: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [NewOrderComponent],
      providers: [
        { provide: OrdersService, useValue: mockOrdersService },
        { provide: ToastService, useValue: mockToastService },
      ],
    })
      .overrideComponent(NewOrderComponent, {
        remove: { imports: [PatientPickerComponent, PhysicianPickerComponent] },
        add: { imports: [StubPatientPickerComponent, StubPhysicianPickerComponent] },
      })
      .compileComponents();

    fixture = TestBed.createComponent(NewOrderComponent);
    fixture.detectChanges();
  });

  afterEach(() => TestBed.resetTestingModule());

  function getElement<T extends HTMLElement>(selector: string): T | null {
    return (fixture.nativeElement as HTMLElement).querySelector<T>(selector);
  }

  type PickerSignals = {
    selectedPatient: ReturnType<typeof signal<PatientSearchResult | null>>;
    selectedPhysician: ReturnType<typeof signal<PhysicianSearchResult | null>>;
  };

  function pickers(): PickerSignals {
    return fixture.componentInstance as unknown as PickerSignals;
  }

  function select(patient: PatientSearchResult | null, physician: PhysicianSearchResult | null): void {
    pickers().selectedPatient.set(patient);
    pickers().selectedPhysician.set(physician);
    fixture.detectChanges();
  }

  function submitOrder(): void {
    getElement<HTMLButtonElement>('[data-testid="create-order-submit-btn"]')!.click();
  }

  function selectAndSubmit(): void {
    select(testPatient, testPhysician);
    submitOrder();
  }

  it('exam form section is not visible before order is created', () => {
    expect(getElement('[data-testid="exam-section"]')).toBeNull();
  });

  it('shows the create-order form on init even when a previous order is already set in the service', async () => {
    orderSignal.set(createdOrder);

    const freshFixture = TestBed.createComponent(NewOrderComponent);
    freshFixture.detectChanges();
    await freshFixture.whenStable();
    freshFixture.detectChanges();

    const el = freshFixture.nativeElement as HTMLElement;
    expect(el.querySelector('[data-testid="create-order-submit-btn"]')).not.toBeNull();
    expect(el.querySelector('[data-testid="exam-section"]')).toBeNull();
  });

  it('submit stays disabled until both a patient and a physician are selected', () => {
    const submitBtn = getElement<HTMLButtonElement>('[data-testid="create-order-submit-btn"]')!;
    expect(submitBtn.disabled).toBe(true);

    select(testPatient, null);
    expect(submitBtn.disabled).toBe(true);

    select(null, testPhysician);
    expect(submitBtn.disabled).toBe(true);

    select(testPatient, testPhysician);
    expect(submitBtn.disabled).toBe(false);
  });

  it('submitting calls createOrder() with the patient ID and the selected physician ID', async () => {
    vi.mocked(mockOrdersService.createOrder!).mockResolvedValue(undefined);

    selectAndSubmit();
    await fixture.whenStable();

    expect(mockOrdersService.createOrder).toHaveBeenCalledWith({
      patientId: 'patient-uuid-1',
      requestedBy: 'physician-uuid-1',
    });
  });

  it('after createOrder(), exam form section becomes visible', async () => {
    vi.mocked(mockOrdersService.createOrder!).mockImplementation(async () => {
      orderSignal.set(createdOrder);
    });

    selectAndSubmit();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(getElement('[data-testid="exam-section"]')).not.toBeNull();
  });

  it('submitting exam form calls requestExam() with orderId and exam data', async () => {
    vi.mocked(mockOrdersService.createOrder!).mockImplementation(async () => {
      orderSignal.set(createdOrder);
    });
    vi.mocked(mockOrdersService.requestExam!).mockResolvedValue(undefined);

    selectAndSubmit();
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

  it('shows an exam-added confirmation toast after requestExam() resolves', async () => {
    vi.mocked(mockOrdersService.createOrder!).mockImplementation(async () => {
      orderSignal.set(createdOrder);
    });
    vi.mocked(mockOrdersService.requestExam!).mockResolvedValue(undefined);

    selectAndSubmit();
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

    // Confirmation is now a global toast (rendered by the app-root toaster) tagged
    // with the exam-added-confirmation testId so e2e can still target it.
    expect(mockToastService.show).toHaveBeenCalledWith(
      'Exam GLU added to order',
      expect.objectContaining({ variant: 'success', testId: 'exam-added-confirmation' }),
    );
  });
});
