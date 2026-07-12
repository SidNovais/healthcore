import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import type { RegisterPatientParams } from '../../core/domain/register-patient-params';
import { HcButton } from '../../ui/button/button';
import { HcField } from '../../ui/field/field';
import { HcInput } from '../../ui/input/input';
import { HcLabel } from '../../ui/input/label';
import { HcSelect } from '../../ui/select/select';

@Component({
  selector: 'app-patient-form',
  standalone: true,
  imports: [ReactiveFormsModule, HcButton, HcField, HcInput, HcLabel, HcSelect],
  templateUrl: './patient-form.component.html',
  styleUrl: './patient-form.component.css',
})
export class PatientFormComponent implements OnChanges {
  @Input() initialValues: RegisterPatientParams | null = null;
  @Output() readonly formSubmit = new EventEmitter<RegisterPatientParams>();

  protected readonly form = inject(FormBuilder).nonNullable.group({
    fullName: ['', Validators.required],
    dateOfBirth: ['', Validators.required],
    gender: [''],
    mothersFullName: [''],
    documentId: [''],
    phone: [''],
    email: [''],
  });

  // Zoneless: bridge each required control's event stream into a signal so the
  // error computeds re-evaluate on touched/value changes.
  private readonly fullNameEvents = toSignal(this.form.controls.fullName.events);
  private readonly dateOfBirthEvents = toSignal(this.form.controls.dateOfBirth.events);

  protected readonly fullNameError = computed(() => {
    this.fullNameEvents();
    const control = this.form.controls.fullName;
    return control.touched && control.invalid ? 'Full name is required' : null;
  });

  protected readonly dateOfBirthError = computed(() => {
    this.dateOfBirthEvents();
    const control = this.form.controls.dateOfBirth;
    return control.touched && control.invalid ? 'Date of birth is required' : null;
  });

  ngOnChanges(changes: SimpleChanges): void {
    const change = changes['initialValues'];
    if (change?.isFirstChange() && this.initialValues) {
      this.form.patchValue(this.initialValues);
    }
  }

  protected submit(): void {
    if (this.form.invalid) return;
    const v = this.form.getRawValue();
    this.formSubmit.emit({
      fullName: v.fullName,
      dateOfBirth: v.dateOfBirth,
      gender: v.gender || undefined,
      mothersFullName: v.mothersFullName || undefined,
      documentId: v.documentId || undefined,
      phone: v.phone || undefined,
      email: v.email || undefined,
    });
  }
}
