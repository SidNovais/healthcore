import { Component, Input, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { WorklistService } from './worklist.service';
import { AuthService } from '../../core/application/auth.service';
import type { WorklistItemDetails } from '../../core/domain/worklist-item-details';

@Component({
  selector: 'app-worklist-item-detail',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div data-testid="worklist-item-detail" class="detail-panel">
      <h3>{{ item.examCode }} — {{ item.sampleBarcode }}</h3>
      <p>Patient ID: {{ item.patientId }}</p>
      <p>Status: <span class="status-badge">{{ item.status }}</span></p>

      @if (item.analyteResults.length > 0) {
        <table class="analyte-table">
          <thead>
            <tr>
              <th>Analyte</th>
              <th>Result</th>
              <th>Unit</th>
              <th>Ref. Range</th>
              <th>Out of Range</th>
            </tr>
          </thead>
          <tbody>
            @for (r of item.analyteResults; track r.id) {
              <tr [class.out-of-range]="r.isOutOfRange">
                <td>{{ r.analyteCode }}</td>
                <td>{{ r.resultValue }}</td>
                <td>{{ r.resultUnit }}</td>
                <td>{{ r.referenceRange }}</td>
                <td>{{ r.isOutOfRange ? 'Yes' : 'No' }}</td>
              </tr>
            }
          </tbody>
        </table>
      }

      @if (!reportSigned()) {
        <div class="sign-form">
          <label for="signature-input">Signature</label>
          <input
            id="signature-input"
            data-testid="signature-input"
            type="text"
            [(ngModel)]="signature"
            placeholder="Your name"
          />
          <button
            data-testid="sign-report-btn"
            (click)="onSignReport()"
            [disabled]="!signature.trim()"
          >
            Sign Report
          </button>
          @if (error()) {
            <p class="error">{{ error() }}</p>
          }
        </div>
      }

      @if (reportSigned()) {
        <div data-testid="sign-report-confirmation" class="confirmation">
          Report signed successfully.
        </div>
      }
    </div>
  `,
  styles: [`
    .detail-panel { padding: 1.5rem; border-left: 1px solid var(--color-border, #e0e0e0); }
    .status-badge { background: #e8f4fd; padding: 0.2rem 0.5rem; border-radius: 4px; font-size: 0.85rem; }
    .analyte-table { width: 100%; border-collapse: collapse; margin: 1rem 0; }
    .analyte-table th, .analyte-table td { padding: 0.5rem; border: 1px solid #ddd; text-align: left; }
    .out-of-range td { color: #c0392b; font-weight: 600; }
    .sign-form { display: flex; flex-direction: column; gap: 0.5rem; max-width: 320px; margin-top: 1rem; }
    .sign-form input { padding: 0.5rem; border: 1px solid #ccc; border-radius: 4px; }
    .sign-form button { padding: 0.5rem 1rem; background: #2c7be5; color: #fff; border: none; border-radius: 4px; cursor: pointer; }
    .sign-form button:disabled { background: #9ab8e8; cursor: not-allowed; }
    .confirmation { padding: 0.75rem; background: #d4edda; border-radius: 4px; color: #155724; margin-top: 1rem; }
    .error { color: #c0392b; }
  `],
})
export class WorklistItemDetailComponent {
  @Input({ required: true }) item!: WorklistItemDetails;

  private readonly service = inject(WorklistService);
  private readonly auth = inject(AuthService);

  protected signature = '';
  protected readonly reportSigned = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async onSignReport(): Promise<void> {
    const userId = this.auth.currentUser()?.userId ?? '';
    try {
      await this.service.signReport(this.item.id, {
        signature: this.signature.trim(),
        signedBy: userId,
      });
      this.reportSigned.set(true);
      this.error.set(null);
    } catch {
      this.error.set('Failed to sign report. Please try again.');
    }
  }
}
