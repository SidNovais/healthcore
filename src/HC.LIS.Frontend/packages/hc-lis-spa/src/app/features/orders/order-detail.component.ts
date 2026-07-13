import { Component, ElementRef, NgZone, OnInit, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { gsap } from 'gsap';
import { OrdersService } from './orders.service';
import { HcAlert } from '../../ui/alert/alert';
import { HcBadge, type HcBadgeVariant } from '../../ui/badge/badge';
import { HcButton } from '../../ui/button/button';
import { HcCard } from '../../ui/card/card';
import { HcEmpty } from '../../ui/empty/empty';
import { HcInput } from '../../ui/input/input';
import { HcTable } from '../../ui/table/table';
import { ToastService } from '../../ui/toast/toast.service';
import { MOTION, prefersReducedMotion } from '../../ui/motion/motion';

const STATUS_VARIANTS: Record<string, HcBadgeVariant> = {
  Accepted: 'success',
  Completed: 'success',
  PartiallyCompleted: 'success',
  InProgress: 'accent',
  OnHold: 'warning',
  Rejected: 'error',
  Canceled: 'neutral',
  Requested: 'neutral',
};

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [FormsModule, RouterLink, HcAlert, HcBadge, HcButton, HcCard, HcEmpty, HcInput, HcTable],
  templateUrl: './order-detail.component.html',
  styleUrl: './order-detail.component.css',
})
export class OrderDetailComponent implements OnInit {
  protected readonly ordersService = inject(OrdersService);
  private readonly route = inject(ActivatedRoute);
  private readonly ngZone = inject(NgZone);
  private readonly toast = inject(ToastService);
  private readonly host = inject(ElementRef).nativeElement as HTMLElement;

  protected readonly activeRejectItemId = signal<string | null>(null);
  protected readonly activeOnHoldItemId = signal<string | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected rejectReason = '';
  protected onHoldReason = '';

  constructor() {
    // Stagger exam-item rows in whenever a fresh set renders.
    effect(() => {
      const count = this.ordersService.orderDetails()?.items.length ?? 0;
      if (count === 0 || prefersReducedMotion()) return;
      requestAnimationFrame(() => {
        const rows = this.host.querySelectorAll('[data-testid="exam-item-row"]');
        if (rows.length === 0) return;
        gsap.from(rows, { autoAlpha: 0, y: 8, duration: MOTION.fast, stagger: 0.03, overwrite: true });
      });
    });
  }

  protected statusVariant(status: string): HcBadgeVariant {
    return STATUS_VARIANTS[status] ?? 'neutral';
  }

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
      this.toast.show('Exam accepted', { variant: 'success' });
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
      this.toast.show('Exam rejected', { variant: 'success' });
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
      this.toast.show('Exam canceled', { variant: 'success' });
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
      this.toast.show('Exam placed on hold', { variant: 'success' });
      this.activeOnHoldItemId.set(null);
      this.onHoldReason = '';
      this.scheduleReload(orderId);
    } catch (err: unknown) {
      this.errorMessage.set(err instanceof Error ? err.message : 'Unexpected error.');
    }
  }
}
