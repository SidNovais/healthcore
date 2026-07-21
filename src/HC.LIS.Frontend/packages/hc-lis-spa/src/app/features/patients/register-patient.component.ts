import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { PatientFormComponent } from './patient-form.component';
import { PatientsService } from '../../core/application/patients.service';
import type { RegisterPatientParams } from '../../core/domain/register-patient-params';
import { HcAlert } from '../../ui/alert/alert';
import { HcPage } from '../../ui/page/page';

@Component({
  selector: 'app-register-patient',
  standalone: true,
  imports: [PatientFormComponent, HcAlert, HcPage],
  templateUrl: './register-patient.component.html',
  styleUrl: './register-patient.component.css',
})
export class RegisterPatientComponent {
  private readonly service = inject(PatientsService);
  private readonly router = inject(Router);

  protected readonly error = signal<string | null>(null);
  protected readonly submitting = signal(false);

  protected async onFormSubmit(data: RegisterPatientParams): Promise<void> {
    this.error.set(null);
    this.submitting.set(true);
    try {
      const newId = await this.service.register(data);
      await this.router.navigate(['/patients', newId]);
    } catch (err) {
      this.error.set(err instanceof Error ? err.message : 'Failed to register patient');
    } finally {
      this.submitting.set(false);
    }
  }
}
