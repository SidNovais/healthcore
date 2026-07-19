import { Component, ElementRef, OnInit, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { gsap } from 'gsap';
import { OrdersService } from './orders.service';
import { PATIENTS_PORT } from '../../core/application/i-patients-port';
import { HcAlert } from '../../ui/alert/alert';
import { HcBadge, type HcBadgeVariant } from '../../ui/badge/badge';
import { type HcBreadcrumbItem } from '../../ui/breadcrumb/breadcrumb';
import { HcButton } from '../../ui/button/button';
import { HcCard, HcCardContent } from '../../ui/card/card';
import { HcDateTimePipe } from '../../ui/date/hc-datetime.pipe';
import { HcPage } from '../../ui/page/page';
import { HcDialog } from '../../ui/dialog/dialog';
import {
  HcDropdownMenu,
  HcDropdownMenuItem,
  HcDropdownMenuTrigger,
} from '../../ui/dropdown-menu/dropdown-menu';
import { HcEmpty } from '../../ui/empty/empty';
import { HcIcon } from '../../ui/icon/icon';
import { HcInput } from '../../ui/input/input';
import { HcTable } from '../../ui/table/table';
import { ToastService } from '../../ui/toast/toast.service';
import { MOTION, prefersReducedMotion } from '../../ui/motion/motion';

const TERMINAL_STATUSES = new Set(['Canceled', 'Rejected', 'Completed', 'PartiallyCompleted']);

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
  imports: [
    FormsModule,
    HcAlert,
    HcBadge,
    HcButton,
    HcCard,
    HcCardContent,
    HcDateTimePipe,
    HcDialog,
    HcDropdownMenu,
    HcDropdownMenuTrigger,
    HcDropdownMenuItem,
    HcEmpty,
    HcIcon,
    HcInput,
    HcPage,
    HcTable,
  ],
  templateUrl: './order-detail.component.html',
  styleUrl: './order-detail.component.css',
})
export class OrderDetailComponent implements OnInit {
  protected readonly ordersService = inject(OrdersService);
  private readonly patientsPort = inject(PATIENTS_PORT);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);
  private readonly host = inject(ElementRef).nativeElement as HTMLElement;

  /** Resolved patient name for the order; null until the lookup completes (template shows a placeholder). */
  protected readonly patientName = signal<string | null>(null);
  private resolvedPatientId: string | null = null;

  protected readonly breadcrumbs: HcBreadcrumbItem[] = [
    { label: 'Orders', route: '/orders' },
    { label: 'Order Detail' },
  ];

  protected readonly activeRejectItemId = signal<string | null>(null);
  protected readonly activeOnHoldItemId = signal<string | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected rejectReason = '';
  protected onHoldReason = '';

  constructor() {
    // Resolve the patient's name for display so the UI never shows a raw patient id.
    effect(() => {
      const patientId = this.ordersService.orderDetails()?.patientId;
      if (!patientId || patientId === this.resolvedPatientId) return;
      this.resolvedPatientId = patientId;
      void this.resolvePatientName(patientId);
    });

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

  /** Looks up the patient's display name; leaves the placeholder in place if the lookup fails. */
  private async resolvePatientName(patientId: string): Promise<void> {
    try {
      const patient = await this.patientsPort.getDetails(patientId);
      this.patientName.set(patient.fullName);
    } catch {
      this.patientName.set(null);
    }
  }

  protected statusVariant(status: string): HcBadgeVariant {
    return STATUS_VARIANTS[status] ?? 'neutral';
  }

  /** An item exposes at least one action (Cancel) while it is not in a terminal state. */
  protected hasActions(status: string): boolean {
    return !TERMINAL_STATUSES.has(status);
  }

  ngOnInit(): void {
    void this.ordersService.loadOrderDetails(this.route.snapshot.params['id'] as string);
  }

  protected async onAccept(itemId: string): Promise<void> {
    const orderId = this.route.snapshot.params['id'] as string;
    this.errorMessage.set(null);
    try {
      await this.ordersService.acceptExam(orderId, itemId);
      this.toast.show('Exam accepted', { variant: 'success' });
      this.ordersService.applyExamStatus(itemId, 'Accepted');
    } catch (err: unknown) {
      this.errorMessage.set(err instanceof Error ? err.message : 'Unexpected error.');
    }
  }

  protected startReject(itemId: string): void {
    this.activeRejectItemId.set(itemId);
    this.rejectReason = '';
  }

  /** Reset the pending reject when the dialog is dismissed (Esc/backdrop/Cancel). */
  protected onRejectDialogOpenChange(open: boolean): void {
    if (!open) {
      this.activeRejectItemId.set(null);
      this.rejectReason = '';
    }
  }

  protected async onReject(itemId: string): Promise<void> {
    const orderId = this.route.snapshot.params['id'] as string;
    this.errorMessage.set(null);
    try {
      await this.ordersService.rejectExam(orderId, itemId, this.rejectReason.trim());
      this.toast.show('Exam rejected', { variant: 'success' });
      this.activeRejectItemId.set(null);
      this.rejectReason = '';
      this.ordersService.applyExamStatus(itemId, 'Rejected');
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
      this.ordersService.applyExamStatus(itemId, 'Canceled');
    } catch (err: unknown) {
      this.errorMessage.set(err instanceof Error ? err.message : 'Unexpected error.');
    }
  }

  protected startOnHold(itemId: string): void {
    this.activeOnHoldItemId.set(itemId);
    this.onHoldReason = '';
  }

  /** Reset the pending hold when the dialog is dismissed (Esc/backdrop/Cancel). */
  protected onOnHoldDialogOpenChange(open: boolean): void {
    if (!open) {
      this.activeOnHoldItemId.set(null);
      this.onHoldReason = '';
    }
  }

  protected async onPlaceOnHold(itemId: string): Promise<void> {
    const orderId = this.route.snapshot.params['id'] as string;
    this.errorMessage.set(null);
    try {
      await this.ordersService.placeExamOnHold(orderId, itemId, this.onHoldReason.trim());
      this.toast.show('Exam placed on hold', { variant: 'success' });
      this.activeOnHoldItemId.set(null);
      this.onHoldReason = '';
      this.ordersService.applyExamStatus(itemId, 'OnHold');
    } catch (err: unknown) {
      this.errorMessage.set(err instanceof Error ? err.message : 'Unexpected error.');
    }
  }
}
