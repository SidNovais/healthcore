import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { OrdersService } from './orders.service';
import { RequestExamFormComponent } from './request-exam-form.component';
import { AuthService } from '../../core/application/auth.service';
import type { RequestExamParams } from '../../core/application/i-orders-port';

@Component({
  selector: 'app-new-order',
  standalone: true,
  imports: [FormsModule, RequestExamFormComponent],
  template: `
    <div class="page">
      <h1 class="page-title">New Test Order</h1>

      @if (!ordersService.order()) {
        <section class="order-create-section">
          <form (ngSubmit)="createOrder()">
            <div class="field">
              <label for="patient-id">Patient ID</label>
              <input
                id="patient-id"
                data-testid="patient-id-input"
                type="text"
                [(ngModel)]="patientId"
                name="patientId"
                required
                placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
                class="mono-input"
                autocomplete="off"
              />
            </div>
            @if (createError()) {
              <p role="alert" class="error-text">{{ createError() }}</p>
            }
            <button
              type="submit"
              data-testid="create-order-btn"
              class="btn-primary"
              [disabled]="!patientId() || creating()"
            >
              {{ creating() ? 'Creating…' : 'Create Order' }}
            </button>
          </form>
        </section>
      }

      @if (ordersService.order(); as order) {
        <section data-testid="exam-section" class="exam-section">
          <div class="order-badge">
            Order <span class="order-id">{{ order.orderId }}</span> created
          </div>

          <h2 class="section-title">Add Exam</h2>
          <app-request-exam-form (examSubmitted)="onExamSubmit($event)" />

          @if (lastExamAdded()) {
            <div data-testid="exam-added-confirmation" class="confirmation">
              ✓ Exam <strong>{{ lastExamAdded() }}</strong> added to order
            </div>
          }

          @if (examError()) {
            <p role="alert" class="error-text">{{ examError() }}</p>
          }
        </section>
      }
    </div>
  `,
  styles: [`
    .page { max-width: 600px; margin: 3rem auto; padding: 0 1.5rem; font-family: system-ui, sans-serif; }
    .page-title { font-size: 1.5rem; font-weight: 600; color: #111; margin-bottom: 2rem; }
    .order-create-section, .exam-section { display: flex; flex-direction: column; gap: 1.25rem; }
    .field { display: flex; flex-direction: column; gap: 0.25rem; }
    label { font-size: 0.75rem; text-transform: uppercase; letter-spacing: 0.08em; color: #6b7280; }
    .mono-input {
      font-family: 'JetBrains Mono', 'IBM Plex Mono', monospace;
      padding: 0.5rem 0.75rem;
      border: 1px solid #d1d5db;
      border-radius: 0;
      font-size: 0.9rem;
      width: 100%;
      box-sizing: border-box;
      background: #fff;
    }
    .mono-input:focus { outline: 2px solid #00897B; outline-offset: -1px; }
    .btn-primary {
      align-self: flex-start;
      padding: 0.5rem 1.5rem;
      background: #00897B;
      color: #fff;
      border: none;
      border-radius: 2px;
      font-size: 0.875rem;
      cursor: pointer;
      font-weight: 500;
    }
    .btn-primary:disabled { opacity: 0.45; cursor: not-allowed; }
    .order-badge {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.4rem 0.75rem;
      background: #e6f4f1;
      border-left: 3px solid #00897B;
      font-size: 0.8rem;
      color: #004d40;
    }
    .order-id { font-family: 'JetBrains Mono', 'IBM Plex Mono', monospace; font-size: 0.75rem; }
    .section-title { font-size: 1rem; font-weight: 600; color: #374151; margin: 0.5rem 0; }
    .confirmation {
      padding: 0.6rem 0.75rem;
      background: #ecfdf5;
      border-left: 3px solid #10b981;
      font-size: 0.875rem;
      color: #065f46;
    }
    .error-text { color: #b91c1c; font-size: 0.875rem; margin: 0; }
  `],
})
export class NewOrderComponent {
  protected readonly ordersService = inject(OrdersService);
  private readonly authService = inject(AuthService);

  protected patientId = signal('');
  protected creating = signal(false);
  protected createError = signal<string | null>(null);
  protected lastExamAdded = signal<string | null>(null);
  protected examError = signal<string | null>(null);

  protected async createOrder(): Promise<void> {
    this.creating.set(true);
    this.createError.set(null);
    try {
      await this.ordersService.createOrder({
        patientId: this.patientId(),
        requestedBy: this.authService.currentUser()?.userId ?? '',
      });
    } catch (err) {
      this.createError.set(err instanceof Error ? err.message : 'Failed to create order');
    } finally {
      this.creating.set(false);
    }
  }

  protected async onExamSubmit(params: RequestExamParams): Promise<void> {
    this.examError.set(null);
    this.lastExamAdded.set(null);
    const orderId = this.ordersService.order()?.orderId ?? '';
    try {
      await this.ordersService.requestExam(orderId, params);
      this.lastExamAdded.set(params.examMnemonic);
    } catch (err) {
      this.examError.set(err instanceof Error ? err.message : 'Failed to add exam');
    }
  }
}
