import { Component, output } from '@angular/core';
import { FormsModule } from '@angular/forms';

export interface CollectSampleFormData {
  tubeType: string;
  barcodeValue: string;
  patientName: string;
  patientBirthdate: string;
  patientGender: string;
}

@Component({
  selector: 'app-collect-sample-form',
  standalone: true,
  imports: [FormsModule],
  template: `
    <form data-testid="collect-sample-form" class="collect-form" (ngSubmit)="submit()">
      <div class="form-row">
        <div class="field">
          <label for="tube-type">Tube Type</label>
          <input id="tube-type" data-testid="tube-type-input" type="text"
            [(ngModel)]="tubeType" name="tubeType" required placeholder="e.g. EDTA" class="mono-input" />
        </div>
        <div class="field">
          <label for="barcode-value">Barcode</label>
          <input id="barcode-value" data-testid="barcode-value-input" type="text"
            [(ngModel)]="barcodeValue" name="barcodeValue" required placeholder="Scan or type" class="mono-input" />
        </div>
      </div>
      <div class="form-row">
        <div class="field">
          <label for="patient-name">Patient Name</label>
          <input id="patient-name" data-testid="patient-name-input" type="text"
            [(ngModel)]="patientName" name="patientName" required class="mono-input" />
        </div>
        <div class="field">
          <label for="patient-birthdate">Date of Birth</label>
          <input id="patient-birthdate" data-testid="patient-birthdate-input" type="date"
            [(ngModel)]="patientBirthdate" name="patientBirthdate" required class="mono-input" />
        </div>
        <div class="field">
          <label for="patient-gender">Gender</label>
          <select id="patient-gender" data-testid="patient-gender-select"
            [(ngModel)]="patientGender" name="patientGender" required class="mono-input">
            <option value="">—</option>
            <option value="M">M</option>
            <option value="F">F</option>
            <option value="Other">Other</option>
          </select>
        </div>
      </div>
      <button type="submit" data-testid="collect-submit-btn" class="btn-primary"
        [disabled]="!tubeType || !barcodeValue || !patientName || !patientBirthdate || !patientGender">
        Record Collection
      </button>
    </form>
  `,
  styles: [`
    .collect-form { display: flex; flex-direction: column; gap: 0.75rem; padding: 1rem; background: #f9fafb; border: 1px solid #e5e7eb; margin-top: 0.5rem; }
    .form-row { display: flex; gap: 0.75rem; flex-wrap: wrap; }
    .field { display: flex; flex-direction: column; gap: 0.2rem; flex: 1; min-width: 120px; }
    label { font-size: 0.7rem; text-transform: uppercase; letter-spacing: 0.08em; color: #6b7280; }
    .mono-input { font-family: 'JetBrains Mono', 'IBM Plex Mono', monospace; padding: 0.35rem 0.5rem; border: 1px solid #d1d5db; border-radius: 0; font-size: 0.85rem; width: 100%; box-sizing: border-box; }
    .mono-input:focus { outline: 2px solid #0284c7; outline-offset: -1px; }
    .btn-primary { align-self: flex-start; padding: 0.4rem 1rem; background: #0284c7; color: #fff; border: none; border-radius: 2px; font-size: 0.8rem; cursor: pointer; font-weight: 500; }
    .btn-primary:disabled { opacity: 0.45; cursor: not-allowed; }
  `],
})
export class CollectSampleFormComponent {
  readonly formSubmitted = output<CollectSampleFormData>();

  protected tubeType = '';
  protected barcodeValue = '';
  protected patientName = '';
  protected patientBirthdate = '';
  protected patientGender = '';

  protected submit(): void {
    this.formSubmitted.emit({
      tubeType: this.tubeType,
      barcodeValue: this.barcodeValue,
      patientName: this.patientName,
      patientBirthdate: this.patientBirthdate,
      patientGender: this.patientGender,
    });
  }
}
