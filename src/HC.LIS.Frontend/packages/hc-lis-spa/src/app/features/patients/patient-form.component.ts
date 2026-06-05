import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import type { RegisterPatientParams } from '../../core/domain/register-patient-params';

@Component({
  selector: 'app-patient-form',
  standalone: true,
  imports: [ReactiveFormsModule],
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
