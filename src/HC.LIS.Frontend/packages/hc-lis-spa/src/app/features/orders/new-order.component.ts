import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { OrdersService } from './orders.service';
import { RequestExamFormComponent } from './request-exam-form.component';
import { PatientPickerComponent } from './patient-picker.component';
import { AuthService } from '../../core/application/auth.service';
import type { RequestExamParams } from '../../core/application/i-orders-port';
import type { PatientSearchResult } from '../../core/domain/patient-search-result';
import { HcAlert } from '../../ui/alert/alert';
import { HcButton } from '../../ui/button/button';
import { HcCard } from '../../ui/card/card';
import { HcField } from '../../ui/field/field';
import { HcLabel } from '../../ui/input/label';

@Component({
  selector: 'app-new-order',
  standalone: true,
  imports: [FormsModule, RequestExamFormComponent, PatientPickerComponent, HcAlert, HcButton, HcCard, HcField, HcLabel],
  templateUrl: './new-order.component.html',
  styleUrl: './new-order.component.css',
})
export class NewOrderComponent implements OnInit {
  protected readonly ordersService = inject(OrdersService);
  private readonly authService = inject(AuthService);

  protected selectedPatient = signal<PatientSearchResult | null>(null);
  protected creating = signal(false);
  protected createError = signal<string | null>(null);
  protected lastExamAdded = signal<string | null>(null);
  protected examError = signal<string | null>(null);

  ngOnInit(): void {
    this.ordersService.resetOrder();
  }

  protected async createOrder(): Promise<void> {
    const patient = this.selectedPatient();
    if (!patient) return;
    this.creating.set(true);
    this.createError.set(null);
    try {
      await this.ordersService.createOrder({
        patientId: patient.id,
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
