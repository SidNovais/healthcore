import { Component, OnInit, computed, effect, inject, input, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PatientFormComponent } from './patient-form.component';
import { PatientsService } from '../../core/application/patients.service';
import { AuthService } from '../../core/application/auth.service';
import type { PatientDetails } from '../../core/domain/patient-details';
import type { RegisterPatientParams, UpdatePatientParams } from '../../core/domain/register-patient-params';
import { HcAlert } from '../../ui/alert/alert';
import { HcBadge } from '../../ui/badge/badge';
import { HcBreadcrumb, type HcBreadcrumbItem } from '../../ui/breadcrumb/breadcrumb';
import { HcButton } from '../../ui/button/button';
import { HcCard } from '../../ui/card/card';
import { HcDatePipe } from '../../ui/date/hc-date.pipe';
import { HcDateTimePipe } from '../../ui/date/hc-datetime.pipe';
import { HcDialog } from '../../ui/dialog/dialog';

@Component({
  selector: 'app-patient-detail',
  standalone: true,
  imports: [PatientFormComponent, HcAlert, HcBadge, HcBreadcrumb, HcButton, HcCard, HcDatePipe, HcDateTimePipe, HcDialog],
  templateUrl: './patient-detail.component.html',
  styleUrl: './patient-detail.component.css',
})
export class PatientDetailComponent implements OnInit {
  protected readonly patientsService = inject(PatientsService);
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);

  /**
   * When mounted inside the patient-search slide-over the id is supplied here;
   * on the routed /patients/:id page it is absent and the route param is used.
   */
  readonly patientId = input<string | undefined>(undefined);

  protected readonly isEditMode = signal(false);
  protected readonly showAnonymizeConfirm = signal(false);
  protected readonly error = signal<string | null>(null);

  /**
   * Null in slide-over mode: the sheet sits on top of /patients, so there is no route
   * hierarchy to trail — the list is still behind it.
   */
  protected readonly breadcrumbs = computed<HcBreadcrumbItem[] | null>(() => {
    if (this.patientId()) {
      return null;
    }
    const patient = this.patientsService.patient();
    return [{ label: 'Patients', route: '/patients' }, { label: patient?.fullName ?? 'Patient' }];
  });

  constructor() {
    // Slide-over mode: (re)load whenever the bound id changes. On the routed page
    // patientId() is undefined so this is inert and ngOnInit drives the load.
    effect(() => {
      if (this.patientId()) {
        void this.loadWithRetry();
      }
    });
  }

  private get effectivePatientId(): string {
    return this.patientId() ?? (this.route.snapshot.params['id'] as string);
  }

  protected get isITAdmin(): boolean {
    return this.authService.currentUser()?.role === 'ITAdmin';
  }

  protected toFormValues(patient: PatientDetails): RegisterPatientParams {
    return {
      fullName: patient.fullName,
      dateOfBirth: patient.dateOfBirth,
      gender: patient.gender ?? undefined,
      mothersFullName: patient.mothersFullName ?? undefined,
      documentId: patient.documentId ?? undefined,
      phone: patient.phone ?? undefined,
      email: patient.email ?? undefined,
    };
  }

  ngOnInit(): void {
    // Slide-over mode loads via the input-driven effect; avoid a double load here.
    if (!this.patientId()) {
      void this.loadWithRetry();
    }
  }

  // The API may transiently return stale data immediately after a write
  // (read model settles asynchronously). Retry up to 10 × 1 s until fresh.
  private async loadWithRetry(validate?: (p: PatientDetails) => boolean): Promise<void> {
    for (let attempt = 0; attempt < 10; attempt++) {
      await this.patientsService.loadDetails(this.effectivePatientId);
      const p = this.patientsService.patient();
      if (p !== null && (!validate || validate(p))) return;
      await new Promise<void>(r => setTimeout(r, 1_000));
    }
  }

  protected toggleEdit(): void {
    this.isEditMode.update(v => !v);
  }

  protected startAnonymize(): void {
    this.showAnonymizeConfirm.set(true);
  }

  protected async confirmAnonymize(): Promise<void> {
    this.error.set(null);
    try {
      await this.patientsService.anonymize(this.effectivePatientId);
      this.showAnonymizeConfirm.set(false);
      await this.loadWithRetry(p => p.status === 'Anonymized');
    } catch (err) {
      this.error.set(err instanceof Error ? err.message : 'Failed to anonymize patient');
    }
  }

  protected async onFormSubmit(data: UpdatePatientParams): Promise<void> {
    this.error.set(null);
    try {
      await this.patientsService.update(this.effectivePatientId, data);
      this.isEditMode.set(false);
      await this.loadWithRetry(p => p.fullName === data.fullName && p.dateOfBirth === data.dateOfBirth);
    } catch (err) {
      this.error.set(err instanceof Error ? err.message : 'Failed to update patient');
    }
  }
}
