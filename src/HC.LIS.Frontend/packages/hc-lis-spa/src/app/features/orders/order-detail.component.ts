import { Component, NgZone, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { OrdersService } from './orders.service';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [FormsModule, RouterLink],
  template: `
    <div class="page" data-testid="order-detail">
      <div class="page-header">
        <a class="back-link" routerLink="/orders">← Back to Orders</a>
        <h1 class="page-title">Order Detail</h1>
      </div>

      @if (ordersService.orderDetails(); as details) {
        <div class="order-meta" data-testid="order-meta">
          <p>Patient: <span data-testid="patient-id">{{ details.patientId }}</span></p>
          <p>Requested By: <span data-testid="requested-by">{{ details.requestedBy }}</span></p>
          <p>Priority: <span data-testid="order-priority">{{ details.orderPriority }}</span></p>
          <p>Requested At: <span data-testid="order-requested-at">{{ details.requestedAt }}</span></p>
        </div>

        @if (errorMessage(); as msg) {
          <div class="error-banner" data-testid="exam-action-error">{{ msg }}</div>
        }

        <h2 class="section-title">Exam Items</h2>

        <table data-testid="exam-items-table" class="items-table">
          <thead>
            <tr>
              <th>Specimen</th>
              <th>Material</th>
              <th>Container</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            @for (item of details.items; track item.orderItemId) {
              <tr data-testid="exam-item-row">
                <td>{{ item.specimenMnemonic }}</td>
                <td>{{ item.materialType }}</td>
                <td>{{ item.containerType }}</td>
                <td>
                  <span data-testid="item-status" class="status-badge">{{ item.status }}</span>
                </td>
                <td class="actions-cell">
                  @if (item.status === 'Requested' || item.status === 'OnHold') {
                    <button data-testid="accept-btn" (click)="onAccept(item.orderItemId)">Accept</button>
                  }
                  @if (item.status === 'Requested') {
                    <button data-testid="reject-btn" (click)="startReject(item.orderItemId)">Reject</button>
                  }
                  @if (item.status === 'Requested') {
                    <button data-testid="on-hold-btn" (click)="startOnHold(item.orderItemId)">On Hold</button>
                  }
                  @if (item.status !== 'Canceled' && item.status !== 'Rejected' && item.status !== 'Completed' && item.status !== 'PartiallyCompleted') {
                    <button data-testid="cancel-btn" (click)="onCancel(item.orderItemId)">Cancel</button>
                  }
                  @if (activeRejectItemId() === item.orderItemId) {
                    <div data-testid="reject-reason-form" class="reason-form">
                      <input
                        data-testid="reject-reason-input"
                        type="text"
                        [(ngModel)]="rejectReason"
                        placeholder="Reason for rejection"
                      />
                      <button
                        data-testid="confirm-reject-btn"
                        (click)="onReject(item.orderItemId)"
                        [disabled]="!rejectReason.trim()"
                      >Confirm Reject</button>
                      <button data-testid="cancel-reject-btn" (click)="activeRejectItemId.set(null)">Cancel</button>
                    </div>
                  }
                  @if (activeOnHoldItemId() === item.orderItemId) {
                    <div data-testid="on-hold-reason-form" class="reason-form">
                      <input
                        data-testid="on-hold-reason-input"
                        type="text"
                        [(ngModel)]="onHoldReason"
                        placeholder="Reason for hold"
                      />
                      <button
                        data-testid="confirm-on-hold-btn"
                        (click)="onPlaceOnHold(item.orderItemId)"
                        [disabled]="!onHoldReason.trim()"
                      >Confirm On Hold</button>
                      <button data-testid="cancel-on-hold-btn" (click)="activeOnHoldItemId.set(null)">Cancel</button>
                    </div>
                  }
                </td>
              </tr>
            } @empty {
              <tr>
                <td colspan="5" data-testid="empty-items" class="empty-cell">No exam items found.</td>
              </tr>
            }
          </tbody>
        </table>
      }
    </div>
  `,
  styles: [`
    .page { max-width: 1000px; margin: 2rem auto; padding: 0 1.5rem; font-family: system-ui, sans-serif; }
    .page-header { display: flex; align-items: baseline; gap: 1.5rem; margin-bottom: 1.5rem; }
    .page-title { font-size: 1.5rem; font-weight: 600; color: #111; margin: 0; }
    .back-link { font-size: 0.875rem; color: #2c7be5; text-decoration: none; }
    .back-link:hover { text-decoration: underline; }
    .order-meta { background: #f9fafb; border: 1px solid #e5e7eb; border-radius: 6px; padding: 1rem 1.25rem; margin-bottom: 1.5rem; }
    .order-meta p { margin: 0.25rem 0; font-size: 0.875rem; color: #374151; }
    .section-title { font-size: 1.1rem; font-weight: 600; margin-bottom: 0.75rem; }
    .items-table { width: 100%; border-collapse: collapse; }
    .items-table th { text-align: left; padding: 0.5rem 0.75rem; border-bottom: 2px solid #e5e7eb; font-size: 0.75rem; text-transform: uppercase; color: #6b7280; }
    .items-table td { padding: 0.75rem; border-bottom: 1px solid #f3f4f6; font-size: 0.875rem; vertical-align: top; }
    .status-badge { display: inline-block; padding: 0.2rem 0.6rem; border-radius: 9999px; font-size: 0.75rem; font-weight: 500; background: #e5e7eb; color: #374151; }
    .actions-cell { display: flex; flex-wrap: wrap; gap: 0.4rem; align-items: flex-start; }
    .actions-cell button { padding: 0.3rem 0.7rem; font-size: 0.8rem; border: 1px solid #d1d5db; border-radius: 4px; background: #fff; cursor: pointer; }
    .actions-cell button:hover { background: #f3f4f6; }
    .reason-form { display: flex; flex-direction: column; gap: 0.4rem; margin-top: 0.5rem; min-width: 220px; }
    .reason-form input { padding: 0.35rem 0.6rem; border: 1px solid #d1d5db; border-radius: 4px; font-size: 0.8rem; }
    .empty-cell { text-align: center; color: #9ca3af; padding: 2rem; }
    .error-banner { margin-bottom: 1rem; padding: 0.75rem 1rem; border-radius: 6px; background: #fef2f2; border: 1px solid #fca5a5; color: #b91c1c; font-size: 0.875rem; }
  `],
})
export class OrderDetailComponent implements OnInit {
  protected readonly ordersService = inject(OrdersService);
  private readonly route = inject(ActivatedRoute);
  private readonly ngZone = inject(NgZone);

  protected readonly activeRejectItemId = signal<string | null>(null);
  protected readonly activeOnHoldItemId = signal<string | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected rejectReason = '';
  protected onHoldReason = '';

  ngOnInit(): void {
    this.scheduleReload(this.route.snapshot.params['id'] as string);
  }

  // Fires an immediate reload and retries after 3 s outside Angular's zone so that
  // the outbox-driven projection (Quartz every 2 s) has time to commit before the
  // second GET fires. Running outside the zone prevents TestBed.whenStable() from
  // blocking on the timer in integration tests.
  private scheduleReload(orderId: string): void {
    void this.ordersService.loadOrderDetails(orderId);
    this.ngZone.runOutsideAngular(() => {
      setTimeout(() => {
        this.ngZone.run(() => void this.ordersService.loadOrderDetails(orderId));
      }, 3000);
    });
  }

  protected async onAccept(itemId: string): Promise<void> {
    const orderId = this.route.snapshot.params['id'] as string;
    this.errorMessage.set(null);
    try {
      await this.ordersService.acceptExam(orderId, itemId);
      this.scheduleReload(orderId);
    } catch (err: unknown) {
      this.errorMessage.set(err instanceof Error ? err.message : 'Unexpected error.');
    }
  }

  protected startReject(itemId: string): void {
    this.activeRejectItemId.set(itemId);
    this.rejectReason = '';
  }

  protected async onReject(itemId: string): Promise<void> {
    const orderId = this.route.snapshot.params['id'] as string;
    this.errorMessage.set(null);
    try {
      await this.ordersService.rejectExam(orderId, itemId, this.rejectReason.trim());
      this.activeRejectItemId.set(null);
      this.rejectReason = '';
      this.scheduleReload(orderId);
    } catch (err: unknown) {
      this.errorMessage.set(err instanceof Error ? err.message : 'Unexpected error.');
    }
  }

  protected async onCancel(itemId: string): Promise<void> {
    const orderId = this.route.snapshot.params['id'] as string;
    this.errorMessage.set(null);
    try {
      await this.ordersService.cancelExam(orderId, itemId);
      this.scheduleReload(orderId);
    } catch (err: unknown) {
      this.errorMessage.set(err instanceof Error ? err.message : 'Unexpected error.');
    }
  }

  protected startOnHold(itemId: string): void {
    this.activeOnHoldItemId.set(itemId);
    this.onHoldReason = '';
  }

  protected async onPlaceOnHold(itemId: string): Promise<void> {
    const orderId = this.route.snapshot.params['id'] as string;
    this.errorMessage.set(null);
    try {
      await this.ordersService.placeExamOnHold(orderId, itemId, this.onHoldReason.trim());
      this.activeOnHoldItemId.set(null);
      this.onHoldReason = '';
      this.scheduleReload(orderId);
    } catch (err: unknown) {
      this.errorMessage.set(err instanceof Error ? err.message : 'Unexpected error.');
    }
  }

}
